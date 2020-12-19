using System;
using System.Collections.Generic;
using System.Data;

namespace TestAutomation.Tide.DataBase
{
    public enum DataType
    {
        bigint,
        numeric,
        bit,
        smallint,
        smallmoney,
        @int,
        tinyint,
        @decimal,
        money,
        @float,
        real,
        date,
        datetimeoffset,
        datetime2,
        smalldatetime,
        datetime,
        time,
        @char,
        varchar,
        text,
        nchar,
        nvarchar,
        ntext,
        binary,
        varbinary,
        image
    }

    public static class DataTypeHelper
    {
        public static Dictionary<DataType, Type> DbTypeToType = new Dictionary<DataType, Type>
        {
            [DataType.bigint] = typeof(long?),
            [DataType.bit] = typeof(bool?),
            [DataType.smallint] = typeof(short?),
            [DataType.tinyint] = typeof(short?),
            [DataType.@decimal] = typeof(decimal?),
            [DataType.@float] = typeof(float?),
            [DataType.text] = typeof(string),
            [DataType.nchar] = typeof(string),
            [DataType.nvarchar] = typeof(string),
            [DataType.varchar] = typeof(string),
            [DataType.@int] = typeof(int?),
            [DataType.@char] = typeof(char?),
            [DataType.date] = typeof(DateTime?),
            [DataType.datetime] = typeof(DateTime?),
            [DataType.datetimeoffset] = typeof(DateTimeOffset?),
            [DataType.datetime2] = typeof(DateTime?),
            [DataType.smalldatetime] = typeof(DateTime?),
            [DataType.time] = typeof(DateTime?),
            [DataType.real] = typeof(decimal?),
            [DataType.numeric] = typeof(int?),
            [DataType.money] = typeof(float?),
            [DataType.smallmoney] = typeof(float?),
        };

        public static Dictionary<DataType, SqlDbType> TypeEnumConversion = new Dictionary<DataType, SqlDbType>
        {
            [DataType.bigint] = SqlDbType.BigInt,
            [DataType.bit] = SqlDbType.Bit,
            [DataType.smallint] = SqlDbType.SmallInt,
            [DataType.tinyint] = SqlDbType.TinyInt,
            [DataType.@decimal] = SqlDbType.Decimal,
            [DataType.@float] = SqlDbType.Float,
            [DataType.text] = SqlDbType.Text,
            [DataType.nchar] = SqlDbType.NChar,
            [DataType.nvarchar] = SqlDbType.NVarChar,
            [DataType.varchar] = SqlDbType.VarChar,
            [DataType.@int] = SqlDbType.Int,
            [DataType.@char] = SqlDbType.Char,
            [DataType.date] = SqlDbType.Date,
            [DataType.datetime] = SqlDbType.DateTime,
            [DataType.datetimeoffset] = SqlDbType.DateTimeOffset,
            [DataType.datetime2] = SqlDbType.DateTime2,
            [DataType.smalldatetime] = SqlDbType.SmallDateTime,
            [DataType.time] = SqlDbType.Time,
            [DataType.real] = SqlDbType.Real,
            [DataType.money] = SqlDbType.Money,
            [DataType.smallmoney] = SqlDbType.SmallMoney,
        };

        public static Dictionary<DataType, Func<object, object>> ConvertInto =
            new Dictionary<DataType, Func<object, object>>
            {
                [DataType.bigint] = o =>
                {
                    try
                    {
                        if (o == null) return null;
                        if (long.TryParse(o.ToString(), out var parse))
                            return parse;
                        return null;
                    }
                    catch (Exception e)
                    {
                        Console.Write($"ConverterError: {DataType.bigint}\r\n{e.Message}");
                        return default(long);
                    }
                },
                [DataType.bit] = (o =>
                {
                    try
                    {
                        if (o == null) return null;
                        if (bool.TryParse(o.ToString(), out var parse))
                            return parse;
                        return null;
                    }
                    catch (Exception e)
                    {
                        Console.Write($"ConverterError: {DataType.bit}\r\n{e.Message}");
                        return false;
                    }
                }),
                [DataType.smallint] = o => Int16Converter(o, DataType.smallint),
                [DataType.tinyint] = o => Int16Converter(o, DataType.tinyint),
                [DataType.@decimal] = o => DecimalConverter(o, DataType.@decimal),
                [DataType.@float] = o => FloatConverter(o, DataType.@float),
                [DataType.text] = StringConverter,
                [DataType.nchar] = StringConverter,
                [DataType.nvarchar] = StringConverter,
                [DataType.varchar] = StringConverter,
                [DataType.@int] = o => Int32Converter(o, DataType.@int),
                [DataType.@char] = o =>
                {
                    try
                    {
                        if (o == null) return null;
                        if (char.TryParse(o.ToString(), out var parse))
                            return parse;
                        return null;
                    }
                    catch (Exception e)
                    {
                        Console.Write($"ConverterError: {DataType.@char}\r\n{e.Message}");
                        return 0;
                    }
                },
                [DataType.date] = o => DateTimeConverter(o, DataType.date),
                [DataType.datetime] = o => DateTimeConverter(o, DataType.datetime),
                [DataType.datetimeoffset] = o => DateTimeOffsetConverter(o, DataType.datetimeoffset),
                [DataType.datetime2] = o => DateTimeConverter(o, DataType.datetime2),
                [DataType.smalldatetime] = o => DateTimeConverter(o, DataType.smalldatetime),
                [DataType.time] = o => DateTimeConverter(o, DataType.time),
                [DataType.real] = o => DecimalConverter(o, DataType.real),
                [DataType.numeric] = o => Int32Converter(o, DataType.numeric),
                [DataType.money] = o => FloatConverter(o, DataType.money),
                [DataType.smallmoney] = o => FloatConverter(o, DataType.smallmoney),
            };

        private static object Int16Converter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (short.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return default(short);
            }
        }

        private static object Int32Converter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (int.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return default(int);
            }
        }

        private static object FloatConverter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (float.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return 0.0f;
            }
        }

        private static object DecimalConverter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (decimal.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return 0.0d;
            }
        }

        private static object DateTimeConverter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (DateTime.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return DateTime.MinValue;
            }
        }

        private static object DateTimeOffsetConverter(object o, DataType dataType)
        {
            try
            {
                if (o == null) return null;
                if (DateTimeOffset.TryParse(o.ToString(), out var parse))
                    return parse;
                return null;
            }
            catch (Exception e)
            {
                Console.Write($"ConverterError: {dataType}\r\n{e.Message}");
                return DateTimeOffset.MinValue;
            }
        }

        private static object StringConverter(object o) => o?.ToString();
    }
}