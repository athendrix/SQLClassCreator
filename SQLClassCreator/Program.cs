using System;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace SQLClassCreator
{
    class Program
    {

        static void Main(string[] args)
        {
            string TableName;
            string NameSpace;
            List<Column> Columns = new List<Column>();
            List<Column> PrimaryKeys = new List<Column>();

            //TODO: YAML definitions so we can add Foreign Keys, Unique Indexes on columns, options to define classes/structs/enums elseware (so they don't get overwritten)
            //Table Truncation
            //YamlStream ys = new YamlStream();
            //using StreamReader = new StreamReader()

            #region UserPrompts
            
            Console.Clear();
            Console.WriteLine("Enter Table Name:");
            TableName = Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Enter Namespace Name:");
            NameSpace = Console.ReadLine();

            Console.Clear();
            while(true)
            {
                
                Console.WriteLine("Enter Key Column Name (Blank to end):");
                string keycolumnname = Console.ReadLine().Trim().Replace(" ", "");
                if(string.IsNullOrWhiteSpace(keycolumnname)) { break; }

                Console.WriteLine("Enter Key Column Type:");
                string keyusertype = Console.ReadLine().Trim().ToUpper();
                Column c = new Column(keycolumnname,keyusertype);
                if (c.type == ColumnType.Unknown)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Type. " + keyusertype + " was not found. Please try again.");
                }
                else
                {
                    PrimaryKeys.Add(c);
                    Columns.Add(c);
                    Console.Clear();
                }
            }
            Console.Clear();
            while (true)
            {
                
                Console.WriteLine("Enter Column Name (Blank to end):");
                string columnname = Console.ReadLine().Trim().Replace(" ", "");
                if (string.IsNullOrWhiteSpace(columnname)) { break; }

                Console.WriteLine("Enter Column Type:");
                string usertype = Console.ReadLine().Trim().ToUpper();
                Column c = new Column(columnname,usertype);
                if (c.type == ColumnType.Unknown)
                {
                    Console.Clear();
                    Console.WriteLine("Invalid Type. " + usertype + " was not found. Please try again.");
                }
                else
                {
                    Columns.Add(c);
                    Console.Clear();
                }
            }
            Console.Clear();
            
            #endregion 
            
            Generator gen = new Generator();
            gen.Libraries();
            gen.BlankLine();
            gen.BeginNamespace(NameSpace);
            gen.BeginFactory(TableName,PrimaryKeys);
            gen.CreateDB(TableName,PrimaryKeys,Columns,null);
            gen.GetEnumerator(TableName,PrimaryKeys);
            gen.Select(TableName,PrimaryKeys);
            gen.Delete(TableName,PrimaryKeys);
            gen.EndFactory();
            gen.BeginRowClass(TableName,PrimaryKeys);
            gen.Properties(PrimaryKeys,Columns);
            gen.Constructors(TableName,PrimaryKeys,Columns);
            gen.IDBSetFunctions(TableName,PrimaryKeys,Columns);
            gen.EndRowClass();
            gen.EnumsAndStructs(Columns);
            gen.EndNamespace();

            Console.WriteLine(gen.ToString());
        }
    }
}
