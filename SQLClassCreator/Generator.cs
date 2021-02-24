using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLClassCreator
{
    public class Generator
    {
        private int CurrentIndentationLevel = 0;
        List<string> toReturn = new List<string>();
        #region Core
        public void Reset()
        {
            toReturn = new List<string>();
            CurrentIndentationLevel = 0;
        }
        public void BlankLine() => IndentAdd();
        public void Libraries()
        {
            IndentAdd("using System;");
            IndentAdd("using System.Collections.Generic;");
            IndentAdd("using System.Data;");
            IndentAdd("using System.Linq;");
            IndentAdd("using System.Runtime.InteropServices;");
            IndentAdd("using System.Threading.Tasks;");
            IndentAdd("using CSL.SQL;");
        }
        public void BeginNamespace(string namespacename)
        {
            IndentAdd("namespace " + namespacename);
            EnterBlock();
        }
        public void EndNamespace() => ExitBlock();
        public void Region(string RegionName) => IndentAdd("#region " + RegionName);
        public void EndRegion() => IndentAdd("#endregion");
        public override string ToString()
        {
            return string.Join(Environment.NewLine, toReturn);
        }
        #endregion
        #region Factory Functions
        public void BeginFactory(string TableName, List<Column> PrimaryKeys)
        {
            IndentAdd("public class " + TableName + "Factory : IDBSetFactory<" + string.Join(",",PrimaryKeys.Select((x) => x.CSharpTypeName)) + ">");
            EnterBlock();
        }
        public void EndFactory() => ExitBlock();
        public void CreateDB(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            IndentAdd("public Task<int> CreateDB(SQL sql)");
            EnterBlock();
            IndentAdd("return sql.ExecuteNonQuery(");
            IndentAdd($@"""CREATE TABLE IF NOT EXISTS \""{TableName}\"" ("" +");
            for(int i = 0; i < Columns.Count; i++)
            {
                IndentAdd($@"""\""{Columns[i].ColumnName}\"" {Columns[i].SQLTypeName}, "" +");
            }
            IndentAdd($@"""PRIMARY KEY(\""{string.Join(@"\"", \""",PrimaryKeys.Select((x) => x.ColumnName))}\""));"");");
            ExitBlock();
        }
        public void GetEnumerator(string TableName, List<Column> PrimaryKeys)
        {
            IndentAdd($"public IEnumerable<IDBSet<{string.Join(", ",PrimaryKeys.Select((x) => x.CSharpTypeName))}>> GetEnumerator(IDataReader dr)");
            EnterBlock();
            IndentAdd("while(dr.Read())");
            EnterBlock();
            IndentAdd($"yield return new {TableName}Row(dr);");
            ExitBlock();
            IndentAdd("yield break;");
            ExitBlock();
        }
        public void Select(string TableName, List<Column> PrimaryKeys)
        {
            string returnType = string.Join(",",PrimaryKeys.Select((x) => x.CSharpTypeName));
            Tuple<Column,int>[] CO = new Tuple<Column, int>[PrimaryKeys.Count];
            for(int i = 0; i < CO.Length; i++)
            {
                CO[i] = new Tuple<Column, int>(PrimaryKeys[i],i + 1);
            }
            
            Region("Select");
            IndentAdd("public async IAsyncEnumerable<IDBSet<" + returnType + ">> Select(SQL sql)");
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"";""))");
            EnterBlock();
            IndentAdd($"foreach ({TableName}Row item in GetEnumerator(dr))");
            EnterBlock();
            IndentAdd("yield return item;");
            ExitBlock();
            ExitBlock();
            ExitBlock();

            switch(CO.Length)
            {
                case 1:
                SelectHelper(TableName,returnType,false, CO[0]);
                break;
                case 2:
                SelectHelper(TableName,returnType,true,CO[0]);
                SelectHelper(TableName,returnType,true,CO[1]);
                SelectHelper(TableName,returnType,false,CO[0],CO[1]);
                break;
                case 3:
                SelectHelper(TableName,returnType,true,CO[0]);
                SelectHelper(TableName,returnType,true,CO[1]);
                SelectHelper(TableName,returnType,true,CO[2]);
                SelectHelper(TableName,returnType,true,CO[0],CO[1]);
                SelectHelper(TableName,returnType,true,CO[0],CO[2]);
                SelectHelper(TableName,returnType,true,CO[1],CO[2]);
                SelectHelper(TableName,returnType,false,CO[0],CO[1],CO[2]);
                break;
                case 4:
                SelectHelper(TableName,returnType,true,CO[0]);
                SelectHelper(TableName,returnType,true,CO[1]);
                SelectHelper(TableName,returnType,true,CO[2]);
                SelectHelper(TableName,returnType,true,CO[3]);
                SelectHelper(TableName,returnType,true,CO[0],CO[1]);
                SelectHelper(TableName,returnType,true,CO[0],CO[2]);
                SelectHelper(TableName,returnType,true,CO[0],CO[3]);
                SelectHelper(TableName,returnType,true,CO[1],CO[2]);
                SelectHelper(TableName,returnType,true,CO[1],CO[3]);
                SelectHelper(TableName,returnType,true,CO[2],CO[3]);
                SelectHelper(TableName,returnType,true,CO[0],CO[1],CO[2]);
                SelectHelper(TableName,returnType,true,CO[0],CO[1],CO[3]);
                SelectHelper(TableName,returnType,true,CO[0],CO[2],CO[3]);
                SelectHelper(TableName,returnType,true,CO[1],CO[2],CO[3]);
                SelectHelper(TableName,returnType,false,CO[0],CO[1],CO[2],CO[3]);
                break;
            }
            EndRegion();
        }
        private void SelectHelper(string TableName, string returnType, bool partial, params Tuple<Column,int>[] CO)
        {
            IndentAdd("public async " + (partial?"IAsyncEnumerable":"Task") + "<IDBSet<" + returnType + ">> " +
            "SelectByPK" + (partial?string.Join("",CO.Select((x)=> x.Item2)):"") + "(SQL sql, " + string.Join(", ", CO.Select((x) => x.Item1.CSharpTypeName + " PK" + x.Item2)) + ")" );
            EnterBlock();
            IndentAdd($@"using (IDataReader dr = await sql.ExecuteReader(""SELECT * FROM \""{TableName}\"" WHERE {string.Join(" AND ",CO.Select((x) => $@"\""{x.Item1.ColumnName}\"" = @PK{x.Item2}"))};"",");
            IndentAdd("new Dictionary<string,object>(){{"+string.Join("}, {",CO.Select((x) => " \"@PK" + x.Item2.ToString() + "\", "+x.Item1.CSharpConvertPrivatePrepend+"PK" + x.Item2.ToString() + x.Item1.CSharpConvertPrivateAppend +" "))+"}}))");
            EnterBlock();
            if(partial)
            {
                IndentAdd($"foreach ({TableName}Row item in GetEnumerator(dr))");
                EnterBlock();
                IndentAdd("yield return item;");
                ExitBlock();
            }
            else
            {
                IndentAdd("return GetEnumerator(dr).FirstOrDefault();");
            }
            ExitBlock();
            ExitBlock();
        }
        public void Delete(string TableName, List<Column> PrimaryKeys)
        {
            Region("Delete");
            Tuple<Column,int>[] CO = new Tuple<Column, int>[PrimaryKeys.Count];
            for(int i = 0; i < CO.Length; i++)
            {
                CO[i] = new Tuple<Column, int>(PrimaryKeys[i],i + 1);
            }
            switch(CO.Length)
            {
                case 1:
                DeleteHelper(TableName,false, CO[0]);
                break;
                case 2:
                DeleteHelper(TableName,true,CO[0]);
                DeleteHelper(TableName,true,CO[1]);
                DeleteHelper(TableName,false,CO[0],CO[1]);
                break;
                case 3:
                DeleteHelper(TableName,true,CO[0]);
                DeleteHelper(TableName,true,CO[1]);
                DeleteHelper(TableName,true,CO[2]);
                DeleteHelper(TableName,true,CO[0],CO[1]);
                DeleteHelper(TableName,true,CO[0],CO[2]);
                DeleteHelper(TableName,true,CO[1],CO[2]);
                DeleteHelper(TableName,false,CO[0],CO[1],CO[2]);
                break;
                case 4:
                DeleteHelper(TableName,true,CO[0]);
                DeleteHelper(TableName,true,CO[1]);
                DeleteHelper(TableName,true,CO[2]);
                DeleteHelper(TableName,true,CO[3]);
                DeleteHelper(TableName,true,CO[0],CO[1]);
                DeleteHelper(TableName,true,CO[0],CO[2]);
                DeleteHelper(TableName,true,CO[0],CO[3]);
                DeleteHelper(TableName,true,CO[1],CO[2]);
                DeleteHelper(TableName,true,CO[1],CO[3]);
                DeleteHelper(TableName,true,CO[2],CO[3]);
                DeleteHelper(TableName,true,CO[0],CO[1],CO[2]);
                DeleteHelper(TableName,true,CO[0],CO[1],CO[3]);
                DeleteHelper(TableName,true,CO[0],CO[2],CO[3]);
                DeleteHelper(TableName,true,CO[1],CO[2],CO[3]);
                DeleteHelper(TableName,false,CO[0],CO[1],CO[2],CO[3]);
                break;
            }
            EndRegion();
        }
        private void DeleteHelper(string TableName, bool partial, params Tuple<Column,int>[] CO)
        {
            IndentAdd("public Task<int> DeleteByPK" + (partial?string.Join("",CO.Select((x)=> x.Item2)):"") + "(SQL sql, "
            + string.Join(", ", CO.Select((x) => x.Item1.CSharpTypeName + " PK" + x.Item2)) + ")");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""DELETE FROM \""{TableName}\"" WHERE {string.Join(" AND ",CO.Select((x) => $@"\""{x.Item1.ColumnName}\"" = @PK{x.Item2}"))};"",");
            IndentAdd("new Dictionary<string,object>(){{"+string.Join("}, {",CO.Select((x) => " \"@PK" + x.Item2.ToString() + "\", " + x.Item1.CSharpConvertPrivatePrepend + "PK" + x.Item2.ToString() + x.Item1.CSharpConvertPrivateAppend + " "))+"}});");
            ExitBlock();
        }
        #endregion
        #region Row Functions
        public void BeginRowClass(string TableName, List<Column> PrimaryKeys)
        {
            IndentAdd("public class " + TableName + "Row : IDBSet<" + string.Join(",",PrimaryKeys.Select((x) => x.CSharpTypeName)) + ">");
            EnterBlock();
        }
        public void EndRowClass() => ExitBlock();
        public void Properties(List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("Properties");
            int i;
            for(i = 0; i < PrimaryKeys.Count; i++)
            {
                string PrivCSType = PrimaryKeys[i].CSharpPrivateTypeName;
                string CSType = PrimaryKeys[i].CSharpTypeName;
                string Name = PrimaryKeys[i].ColumnName;
                string pubpre = PrimaryKeys[i].CSharpConvertPublicPrepend;
                string pubapp = PrimaryKeys[i].CSharpConvertPublicAppend;
                bool nullable = PrimaryKeys[i].nullable;
                IndentAdd("private readonly " + PrivCSType + " _" + Name + ";");
                IndentAdd("public " + CSType + " " + Name + " => " + (nullable?"_" + Name + " == null?default:":"" ) + pubpre + "_" + Name + pubapp + ";");
                IndentAdd("public " + CSType + " PK" + (PrimaryKeys.Count == 1?"":(i+1).ToString()) + " => " + Name + ";");
                BlankLine();
            }
            for(;i< Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string CSType = Columns[i].CSharpTypeName;
                string Name = Columns[i].ColumnName;
                string pubpre = Columns[i].CSharpConvertPublicPrepend;
                string pubapp = Columns[i].CSharpConvertPublicAppend;
                string privpre = Columns[i].CSharpConvertPrivatePrepend;
                string privapp = Columns[i].CSharpConvertPrivateAppend;
                bool nullable = Columns[i].nullable;
                IndentAdd("private " + PrivCSType + " _" + Name + ";");
                IndentAdd("public " + CSType + " " + Name + " { get => "+ (nullable?"_" + Name + " == null?default:":"" ) + pubpre + "_" + Name + pubapp + "; set => _" + Name + " = " + privpre + "value" + privapp + ";}");
                BlankLine();
            }
            EndRegion();
        }
        public void Constructors(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("Constructors");
            IndentAdd($"public {TableName}Row(IDataReader dr)");
            EnterBlock();
            for(int i = 0; i < Columns.Count; i++)
            {
                string PrivCSType = Columns[i].CSharpPrivateTypeName;
                string Name = Columns[i].ColumnName;
                IndentAdd($"_{Name} = dr.IsDBNull({i.ToString()}) ? default : ({PrivCSType.TrimEnd('?')})dr[{i.ToString()}];");
            }
            ExitBlock();
            string FunctionParams = string.Join(", ",Columns.Select((x) => x.CSharpTypeName + " " + x.ColumnName));
            IndentAdd($"public {TableName}Row({FunctionParams})");
            EnterBlock();
            for(int i = 0; i < Columns.Count; i++)
            {
                string Name = Columns[i].ColumnName;
                string privpre = Columns[i].CSharpConvertPrivatePrepend;
                string privapp = Columns[i].CSharpConvertPrivateAppend;
                IndentAdd($"_{Name} = {(Columns[i].nullable?("(" + Name + " == null) ? default : "):"")}{privpre}{Name}{privapp};");
            }
            ExitBlock();
            EndRegion();
        }
        public void IDBSetFunctions(string TableName, List<Column> PrimaryKeys, List<Column> Columns)
        {
            Region("IDBSetFunctions");
            List<Column> DataColumns = new List<Column>();
            for(int i = PrimaryKeys.Count; i< Columns.Count; i++)
            {
                DataColumns.Add(Columns[i]);
            }
            string SQLCols = @"\""" + string.Join(@"\"", \""",Columns.Select((x) => x.ColumnName)) + @"\""";
            string SQLParams = "@" + string.Join(", @",Columns.Select((x) => x.ColumnName));
            string SetData = string.Join(", ",DataColumns.Select((x) => "\\\"" + x.ColumnName + "\\\" = @" + x.ColumnName));
            string WhereData = string.Join(" AND ",PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\" = @" + x.ColumnName));
            string ConflictKeys = string.Join(", ",PrimaryKeys.Select((x) => "\\\"" + x.ColumnName + "\\\""));
            IndentAdd("public Task<int> Insert(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams});\",ToDictionary());");
            ExitBlock();
            IndentAdd("public Task<int> Update(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""UPDATE \""{TableName}\"" "" +");
            IndentAdd($"\"SET {SetData} \" +");
            IndentAdd($"\"WHERE {WhereData};\",ToDictionary());");
            ExitBlock();
            IndentAdd("public Task<int> Upsert(SQL sql)");
            EnterBlock();
            IndentAdd($@"return sql.ExecuteNonQuery(""INSERT INTO \""{TableName}\"" ({SQLCols}) "" +");
            IndentAdd($"\"VALUES({SQLParams}) \" +");
            IndentAdd($"\"ON CONFLICT ({ConflictKeys}) DO UPDATE \" +");
            IndentAdd($"\"SET {SetData};\",ToDictionary());");
            ExitBlock();
            IndentAdd("public Dictionary<string,object> ToDictionary()");
            EnterBlock();
            IndentAdd("return new Dictionary<string, object>()");
            EnterBlock();
            for(int i = 0; i< Columns.Count; i++)
            {
                IndentAdd("{\"@" + Columns[i].ColumnName + "\", _" + Columns[i].ColumnName + "},");
            }
            ExitInlineBlock();
            ExitBlock();
            EndRegion();
        }
        #endregion
        #region Enums and Structs
        public void EnumsAndStructs(List<Column> Columns)
        {
            foreach(Column c in Columns)
            {
                switch(c.type)
                {
                    case ColumnType.Enum:
                    IndentAdd("[Flags]");
                    IndentAdd("public enum " + c.CSharpTypeName.TrimEnd('?') + " : ulong");
                    EnterBlock();
                    IndentAdd("NoFlags = 0,");
                    for(int i = 0; i < 16; i++)
                    {
                        IndentAdd("Flag" + (i + 1).ToString() + ((i+1) >= 10?"  ":"   ") + "= 1UL << " + i.ToString() + ",");
                    }
                    ExitBlock();
                    break;
                    case ColumnType.Struct:
                    IndentAdd("public struct " + c.CSharpTypeName.TrimEnd('?'));
                    EnterBlock();
                    IndentAdd("uint Dummy;");
                    ExitBlock();
                    break;
                    default: continue;
                }
            }
        }
        #endregion
        #region Helpers
        public void IndentAdd(params string[] toAdd)
        {
            toReturn.Add(new string(' ', CurrentIndentationLevel) + string.Join("",toAdd));
        }
        public void EnterBlock()
        {
            IndentAdd("{");
            CurrentIndentationLevel += 4;
        }
        public void ExitBlock()
        {
            CurrentIndentationLevel -= 4;
            IndentAdd("}");
        }
        public void ExitInlineBlock()
        {
            CurrentIndentationLevel -= 4;
            IndentAdd("};");
        }
        #endregion
    }
}