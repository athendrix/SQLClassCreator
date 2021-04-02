using CSL.SQL;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;



namespace SQLClassCreator
{
    public class Column
    {
        public static readonly Dictionary<string, ColumnType> TypeMap = new Dictionary<string, ColumnType>()
        {
            {"BOOL",ColumnType.Boolean},
            {"BOOLEAN",ColumnType.Boolean},

            {"BYTE", ColumnType.Byte},

            {"CHAR",ColumnType.Char},

            {"SHORT", ColumnType.Short},
            {"INT16", ColumnType.Short},
            {"USHORT", ColumnType.Integer},
            {"UINT16", ColumnType.Integer},

            {"INT",ColumnType.Integer},
            {"INTEGER",ColumnType.Integer},
            {"INT32",ColumnType.Integer},
            {"UINT",ColumnType.Long},
            {"UINT32",ColumnType.Long},

            {"LONG",ColumnType.Long},
            {"INT64",ColumnType.Long},
            {"ULONG", ColumnType.UnsignedLong},
            {"UINT64", ColumnType.UnsignedLong},

            {"FLOAT", ColumnType.Float},
            {"DOUBLE", ColumnType.Double},
            {"DECIMAL", ColumnType.Decimal},
            {"NUMERIC", ColumnType.Decimal},

            {"CHAR[]",ColumnType.String},
            {"STRING",ColumnType.String},
            {"VARCHAR",ColumnType.String},
            {"TEXT",ColumnType.String},

            {"BYTE[]",ColumnType.ByteArray},
            {"BYTEA",ColumnType.ByteArray},

            {"GUID",ColumnType.Guid},
            {"UUID",ColumnType.Guid},

            {"DATETIME",ColumnType.DateTime},
            {"DT",ColumnType.DateTime},
            {"TIMESTAMP",ColumnType.DateTime},

            {"ENUM", ColumnType.Enum},

            {"STRUCT", ColumnType.Struct},

            {"CLASS", ColumnType.Class },
        };
        public static Regex LengthFinder = new Regex(@"\((\d+)\)",RegexOptions.Compiled);
        public readonly string ColumnName;
        public readonly ColumnType type;
        public readonly bool nullable;
        public readonly int length;

        public string CSharpTypeName 
        {
            get
            {
                return type switch
                {
                    ColumnType.Boolean => "bool" + (nullable?"?":""),
                    ColumnType.Byte => "byte" + (nullable?"?":""),
                    ColumnType.Char => "char" + (nullable?"?":""),
                    ColumnType.Short => "short" + (nullable?"?":""),
                    ColumnType.Integer => "int" + (nullable?"?":""),
                    ColumnType.Long => "long" + (nullable?"?":""),
                    ColumnType.UnsignedLong => "ulong" + (nullable?"?":""),
                    ColumnType.Float => "float" + (nullable?"?":""),
                    ColumnType.Double => "double" + (nullable?"?":""),
                    ColumnType.Decimal => "decimal" + (nullable?"?":""),
                    ColumnType.String => "string",
                    ColumnType.ByteArray => "byte[]",
                    ColumnType.Guid => "Guid" + (nullable?"?":""),
                    ColumnType.DateTime => "DateTime" + (nullable?"?":""),
                    ColumnType.Enum => ColumnName + (nullable?"?":""),
                    ColumnType.Struct => ColumnName  + (nullable?"?":""),
                    ColumnType.Class => ColumnName,
                    _ => "<FIXME>"
                };
            }
        }
        public string CSharpPrivateTypeName 
        {
            get
            {
                return type switch
                {
                    ColumnType.Byte => "byte[]",
                    ColumnType.UnsignedLong => "long" + (nullable?"?":""),
                    ColumnType.Enum => "long" + (nullable?"?":""),
                    ColumnType.Struct => "byte[]",
                    ColumnType.Class => "byte[]",
                    _ => CSharpTypeName
                };
            }
        }
        #region SpecialConversions
        public string CSharpConvertPrivatePrepend
        {
            get
            {
                return type switch
                {
                    ColumnType.Byte => "new byte[] {",
                    ColumnType.UnsignedLong => "(long"+ (nullable ? "?" : "") + ")",
                    ColumnType.Enum => "(long"+(nullable?"?":"")+")",
                    //ColumnType.Struct => "MemoryMarshal.AsBytes(new " + CSharpTypeName.TrimEnd('?') + "[]{",
                    _ => ""
                };
            }
        }
        public string CSharpConvertPrivateAppend
        {
            get
            {
                return type switch
                {
                    ColumnType.Byte => "}",
                    //ColumnType.UnsignedLong => (nullable?".Value":"") + ".ToLong()",
                    //ColumnType.Enum => ")" + (nullable?".Value":"") + ".ToLong()",
                    ColumnType.Struct => (nullable?".Value":"") + ".ToByteArray()",
                    ColumnType.Class => ".ToByteArray()",
                    _ => ""
                };
            }
        }
        public string CSharpConvertPublicPrepend
        {
            get
            {
                return type switch
                {
                    ColumnType.Enum => "(" + CSharpTypeName +")",
                    ColumnType.UnsignedLong => "(ulong"+ (nullable ? "?" : "") + ")",
                    //ColumnType.Struct => "MemoryMarshal.AsRef<" + CSharpTypeName.TrimEnd('?') + ">(",
                    ColumnType.Class => "new " + CSharpTypeName + "(",
                    _ => ""
                };
            }
        }
        public string CSharpConvertPublicAppend
        {
            get
            {
                return type switch
                {
                    ColumnType.Byte => "[0]",
                    //ColumnType.UnsignedLong => (nullable?"?":"") + ".ToUlong()",
                    //ColumnType.Enum => (nullable?"?":"") + ".ToUlong())",
                    ColumnType.Struct => ".ToStruct<" + CSharpTypeName.TrimEnd('?') + ">()",
                    ColumnType.Class => ")",
                    _ => ""
                };
            }
        }
        #endregion
        public string SQLTypeName 
        {
            get
            {
                return type switch
                {
                    ColumnType.Boolean => "BOOLEAN" + (nullable?"":" NOT NULL"),
                    ColumnType.Byte => "BYTEA" + (nullable?"":" NOT NULL"),
                    ColumnType.Char => "CHAR(1)" + (nullable?"":" NOT NULL"),
                    ColumnType.Short => "SMALLINT" + (nullable?"":" NOT NULL"),
                    ColumnType.Integer => "INTEGER" + (nullable?"":" NOT NULL"),
                    ColumnType.Long => "BIGINT" + (nullable?"":" NOT NULL"),
                    ColumnType.UnsignedLong => "BIGINT" + (nullable?"":" NOT NULL"),
                    ColumnType.Float => "FLOAT4" + (nullable?"":" NOT NULL"),
                    ColumnType.Double => "FLOAT8" + (nullable?"":" NOT NULL"),
                    ColumnType.Decimal => "NUMERIC" + (nullable?"":" NOT NULL"),
                    ColumnType.String => (length >= 0?"VARCHAR(" + length.ToString() + ")":"TEXT") + (nullable?"":" NOT NULL"),
                    ColumnType.ByteArray => "BYTEA" + (nullable?"":" NOT NULL"),//TODO: Add Length
                    ColumnType.Guid => "UUID" + (nullable?"":" NOT NULL"),
                    ColumnType.DateTime => "TIMESTAMP" + (nullable?"":" NOT NULL"),
                    ColumnType.Enum => "BIGINT" + (nullable?"":" NOT NULL"),
                    ColumnType.Struct => "BYTEA" + (nullable?"":" NOT NULL"),
                    ColumnType.Class => "BYTEA" + (nullable ? "" : " NOT NULL"),
                    _ => "<FIXME>"
                };
            }
        }

        public Column(string Name, string StringType)
        {
            ColumnName = Common.NameParser(Name).Replace(" ","");
            nullable = false;
            length = -1;
            StringType = StringType.ToUpper().Replace(" ","").Trim();
            if(StringType.Contains('?'))
            {
                nullable = true;
                StringType = StringType.Replace("?","");
            }
            Match m;
            if((m = LengthFinder.Match(StringType)).Success)
            {
                string StrMaxLength = m.Groups.Values.LastOrDefault()?.Value;
                StringType = LengthFinder.Replace(StringType,"");
                if(StrMaxLength != null)
                {
                    length = int.Parse(StrMaxLength);
                }
            }
            if(TypeMap.ContainsKey(StringType))
            {
                type = TypeMap[StringType];
            }
            else
            {
                type = ColumnType.Unknown;
            }
        }
    }
    public enum ColumnType
    {
        Boolean,               //BOOLEAN
        Byte,                  //BYTEA(1)
        Char,                  //CHAR(1) //Not a varchar, because there's only 1
        Short,                 //SMALLINT
        Integer,               //INTEGER
        Long,                  //BIGINT
        UnsignedLong,          //BIGINT with C# conversion
        Float,                 //FLOAT4
        Double,                //FLOAT8
        Decimal,               //NUMERIC
        
        String,                //VARCHAR with limit or TEXT without
        ByteArray,             //BYTEA
        Guid,                  //UUID
        DateTime,              //TIMESTAMP
        Enum, //Unsigned Long for maximum values. //BIGINT with C# conversion
        //A User defined datatype that won't be portable across archetectures.
        //x86 vs ARM
        //Dependant on the ability of Memory Marshal to do the conversion.
        Struct,                //BYTEA
        Class,
        Unknown
    }
}