using System;
using System.Security.Permissions;

namespace TestAutomation.Tide.DataBase
{
    public class FieldNameAttribute : Attribute
    {

        public string FieldName { get; set; }
        public DataType DataType { get; set; }
        public bool IsNullable { get; set; }
    }
}