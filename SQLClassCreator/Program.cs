using System;
using System.Linq;
using System.Collections.Generic;
using CSL.SQL.ClassCreator;
using System.IO;

namespace SQLClassCreator
{
    class Program
    {

        static void Main(string[] args)
        {
            TextReader toRead;
            bool prompts;
            if(args != null && args.Length > 0 && File.Exists(args[0]))
            {
                prompts = false;
                toRead = new StringReader(File.ReadAllText(args[0]));
            }
            else
            {
                prompts = true;
                toRead = Console.In;
            }
            string TableName = null;
            string NameSpace = null;
            List<Column> Columns = new List<Column>();
            List<Column> PrimaryKeys = new List<Column>();
            List<string> ExtraSQL = new List<string>();

            #region UserPrompts
            bool ClearConsole = prompts;
            if (ClearConsole)
            {
                try
                {
                    Console.Clear();
                }
                catch (Exception)
                {
                    ClearConsole = false;
                }
            }
            if (prompts) { Console.Error.WriteLine("Enter Table Name:"); }
            while (string.IsNullOrWhiteSpace(TableName) || TableName.Trim().StartsWith('#'))
            {
                TableName = toRead.ReadLine();
            }
            if (ClearConsole) { Console.Clear(); }
            if (prompts) { Console.Error.WriteLine("Enter Namespace Name:"); }
            while (string.IsNullOrWhiteSpace(NameSpace) || NameSpace.Trim().StartsWith('#'))
            {
                NameSpace = toRead.ReadLine();
            }

            if (ClearConsole) { Console.Clear(); }
            while (true)
            {
                if (prompts) { Console.Error.WriteLine("Enter Key Column Name (Blank to end):"); }
                string keycolumnname = null;
                while(keycolumnname == null || keycolumnname.StartsWith('#'))
                {
                    keycolumnname = toRead.ReadLine().Trim().Replace(" ", "");
                }
                if(string.IsNullOrWhiteSpace(keycolumnname)) { break; }

                if (prompts) { Console.Error.WriteLine("Enter Key Column Type:"); }
                string keyusertype = null;
                while (keyusertype == null || keyusertype.StartsWith('#'))
                {
                    keyusertype = toRead.ReadLine().Trim().ToUpper();
                }
                if (string.IsNullOrWhiteSpace(keyusertype)) { continue; }

                Column c = new Column(keycolumnname,keyusertype);
                if (c.type == ColumnType.Unknown)
                {
                    if (ClearConsole) { Console.Clear(); }
                    Console.Error.WriteLine("Invalid Type. " + keyusertype + " was not found. Please try again.");
                }
                else
                {
                    PrimaryKeys.Add(c);
                    Columns.Add(c);
                    if (ClearConsole) { Console.Clear(); }
                }
            }
            if (ClearConsole) { Console.Clear(); }
            while (true)
            {
                if (prompts) { Console.Error.WriteLine("Enter Column Name (Blank to end):"); }
                string columnname = null;
                while (columnname == null || columnname.StartsWith('#'))
                {
                    columnname = toRead.ReadLine().Trim().Replace(" ", "");
                }
                if (string.IsNullOrWhiteSpace(columnname)) { break; }

                if (prompts) { Console.Error.WriteLine("Enter Column Type:"); }
                string usertype = null;
                while (usertype == null || usertype.StartsWith('#'))
                {
                    usertype = toRead.ReadLine().Trim().ToUpper();
                }
                if (string.IsNullOrWhiteSpace(usertype)) { continue; }

                Column c = new Column(columnname,usertype);
                if (c.type == ColumnType.Unknown)
                {
                    if (ClearConsole) { Console.Clear(); }
                    Console.Error.WriteLine("Invalid Type. " + usertype + " was not found. Please try again.");
                }
                else
                {
                    Columns.Add(c);
                    if (ClearConsole) { Console.Clear(); }
                }
            }
            if (ClearConsole) { Console.Clear(); }
            if (prompts)
            {
                Console.Error.WriteLine("Enter any Extra SQL lines to add to the Table Creation Command.");
                Console.Error.WriteLine("Things like Foreign Keys or unique Indexes would go well here.");
            }
            
            string sqlline = null;
            while (sqlline == null || !string.IsNullOrWhiteSpace(sqlline))
            {
                sqlline = toRead.ReadLine();
                if (string.IsNullOrWhiteSpace(sqlline)) { break; }
                ExtraSQL.Add(sqlline.Trim());
            }
            #endregion

            //Generator gen = new Generator();
            //gen.Libraries();
            //gen.BlankLine();
            //gen.BeginNamespace(NameSpace);
            //gen.BeginFactory(TableName,PrimaryKeys);
            //gen.CreateDB(TableName,PrimaryKeys,Columns,null);
            //gen.GetEnumerator(TableName,PrimaryKeys);
            //gen.Select(TableName,PrimaryKeys);
            //gen.Delete(TableName,PrimaryKeys);
            //gen.EndFactory();
            //gen.BeginRowClass(TableName,PrimaryKeys);
            //gen.Properties(PrimaryKeys,Columns);
            //gen.Constructors(TableName,PrimaryKeys,Columns);
            //gen.IDBSetFunctions(TableName,PrimaryKeys,Columns);
            //gen.EndRowClass();
            //gen.EnumsAndStructs(Columns);
            //gen.EndNamespace();

            Console.WriteLine(Generator.Generate(NameSpace, TableName, PrimaryKeys, Columns, ExtraSQL.ToArray()));
        }
    }
}
