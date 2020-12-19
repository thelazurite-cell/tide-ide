using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static TestAutomation.Tide.DataBase.DataTypeHelper;

namespace TestAutomation.Tide.DataBase
{
    public static class DtoCreator
    {
        public static List<Type> CreatedTypes { get; set; } = new List<Type>();

        public static DtoCollection CreateResultSet(DataView dataview)
        {
            var table = dataview.Table;
            var typeSignature = $"{table.DatabaseName}_{table.Schema}_" + table.Name.Replace(" ", "");
            var exists = CreatedTypes.SingleOrDefault(itm => itm.Name == typeSignature);

            var type = exists ?? CompileType(dataview);

            var results = new DtoCollection()
            {
                Table = dataview.Table,
                DtoType = type
            };

            foreach (var view in dataview.Results)
            {
                var resultantObject = Activator.CreateInstance(type) as DataTransferObject;
                foreach (var heading in dataview.Headers.OfType<TableColumn>())
                {
                    var value = view.FirstOrDefault(itm => itm.Key == heading.Name).Value;

                    var convertedValue = ConvertInto[heading.DataType](value)
                        .CastToReflected(DbTypeToType[heading.DataType]);

                    var prop = type.GetProperty(heading.Name.Replace(" ", ""));
                    prop?.SetValue(resultantObject, convertedValue);
                    var propOriginal = type.GetProperty("Original" + heading.Name.Replace(" ", ""));
                    propOriginal?.SetValue(resultantObject, convertedValue);
                }

                results.Dtos.Add(resultantObject);
            }

            return results;
        }

        private static Type CompileType(DataView dataview)
        {
            var tb = GetTypeBuilder(dataview);
            var constructor =
                tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName |
                                            MethodAttributes.RTSpecialName);

            foreach (var field in dataview.Headers)
            {
                if (field is TableColumn tc)
                    CreateProperty(tb, tc, tc.DataType);
            }

            var compileType = tb.CreateType();
            CreatedTypes.Add(compileType);
            return compileType;
        }

        private static void CreateProperty(TypeBuilder tb, TableColumn field, DataType type)
        {
            var nameNoSpaces = field.Name.Replace(" ", String.Empty);
            var dType = DbTypeToType[type];
            var fieldBuilder = tb.DefineField($"_{nameNoSpaces}", dType, FieldAttributes.Private);
            var propertyBuilder =
                tb.DefineProperty(nameNoSpaces, PropertyAttributes.HasDefault, dType, null);
            var attributeType = typeof(FieldNameAttribute);
            var cab = new CustomAttributeBuilder(attributeType.GetConstructor(Type.EmptyTypes), Type.EmptyTypes,
                attributeType.GetProperties()
                    .Where(itm => new[] {"FieldName", "DataType", "IsNullable"}.Any(c => c == itm.Name)).ToArray(),
                new object[] {field.Name, type, field.IsNullable});
            propertyBuilder.SetCustomAttribute(cab);
            var getPropMthdBuilder = tb.DefineMethod($"get_{nameNoSpaces}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, dType,
                Type.EmptyTypes);
            var getIl = getPropMthdBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);
            var setPropMthdBuilder = tb.DefineMethod($"set_{nameNoSpaces}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null,
                new[] {dType});
            var setIl = setPropMthdBuilder.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldstr, propertyBuilder.Name);
            MethodInfo raisePropertyChangedMethod = typeof(DataTransferObject).GetMethod("OnPropertyChanged",
                BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof(string)}, null);
            setIl.Emit(OpCodes.Callvirt, raisePropertyChangedMethod);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBuilder);
            propertyBuilder.SetSetMethod(setPropMthdBuilder);

            var originalFieldFb = tb.DefineField($"_Original{nameNoSpaces}", dType, FieldAttributes.Private);

            var originalFieldPb =
                tb.DefineProperty($"Original{nameNoSpaces}", PropertyAttributes.HasDefault, dType, null);

            var getOriginalMb = tb.DefineMethod($"get_Original{nameNoSpaces}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, dType,
                Type.EmptyTypes);
            var getOriginalOps = getOriginalMb.GetILGenerator();
            getOriginalOps.Emit(OpCodes.Ldarg_0);
            getOriginalOps.Emit(OpCodes.Ldfld, originalFieldFb);
            getOriginalOps.Emit(OpCodes.Ret);
            var setOriginalMb = tb.DefineMethod($"set_Original{nameNoSpaces}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null,
                new[] {dType});
            var setOriginalOps = setOriginalMb.GetILGenerator();
            var modifyOrProperty = setOriginalOps.DefineLabel();
            var exitOrSet = setOriginalOps.DefineLabel();

            setOriginalOps.MarkLabel(modifyOrProperty);

            setOriginalOps.Emit(OpCodes.Nop);
            setOriginalOps.Emit(OpCodes.Ldarg_0);
            setOriginalOps.Emit(OpCodes.Ldarg_1);
            setOriginalOps.Emit(OpCodes.Stfld, originalFieldFb);
            setOriginalOps.MarkLabel(exitOrSet);
            setOriginalOps.Emit(OpCodes.Ret);
            originalFieldPb.SetGetMethod(getOriginalMb);
            originalFieldPb.SetSetMethod(setOriginalMb);
        }

        private static TypeBuilder GetTypeBuilder(DataView dataview)
        {
            var table = dataview.Table;
            var typeSignature = $"{table.DatabaseName}_{table.Schema}_" + table.Name.Replace(" ", "");
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DataTransferObjects");
            var tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(DataTransferObject));
            return tb;
        }
    }
}