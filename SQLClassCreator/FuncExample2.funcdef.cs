using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record FuncExample2(DateTime Now, string Version)
    {
        #region Static Functions
        public static Task<int> CreateFunction(SQLDB sql) => sql.ExecuteNonQuery(
            "CREATE OR REPLACE FUNCTION \"FuncExample2\" ( " +
            "\"Data\" TEXT " +
            ") " +
            "RETURNS TABLE(\"Now\" TIMESTAMP, \"Version\" TEXT) " +
            "AS " +
            "$$ " +
            "Select now(), version(), \"Data\"; " +
            "$$ " +
            "Language SQL " +
            "STABLE;");
        public static Task DropFunction(SQLDB sql) => 
            sql.ExecuteNonQuery("DROP FUNCTION \"FuncExample2\";");
        public static async Task<FuncExample2?> CallFunction(SQLDB sql, string Data)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"FuncExample2\"(@0);", Data))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        public static IEnumerable<FuncExample2> GetRecords(IDataReader dr)
        {
            while(dr.Read())
            {
                DateTime Now =  (DateTime)dr[0];
                string Version =  (string)dr[1];
                yield return new FuncExample2(Now, Version);
            }
            yield break;
        }
        #endregion
    }
}