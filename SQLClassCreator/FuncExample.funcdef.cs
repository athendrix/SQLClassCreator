using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record FuncExample(DateTime Now, string Version)
    {
        #region Static Functions
        public static Task<int> CreateFunction(SQLDB sql) => sql.ExecuteNonQuery(
            "CREATE OR REPLACE FUNCTION \"FuncExample\" ( " +
            ") " +
            "RETURNS TABLE(\"Now\" TIMESTAMP, \"Version\" TEXT) " +
            "AS " +
            "$$ " +
            "Select Now(), " +
            "version() " +
            "$$ " +
            "Language SQL " +
            "STABLE;");
        public static Task DropFunction(SQLDB sql) => 
            sql.ExecuteNonQuery("DROP FUNCTION \"FuncExample\";");
        public static async Task<FuncExample?> CallFunction(SQLDB sql)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"FuncExample\"();"))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        public static IEnumerable<FuncExample> GetRecords(IDataReader dr)
        {
            while(dr.Read())
            {
                DateTime Now =  (DateTime)dr[0];
                string Version =  (string)dr[1];
                yield return new FuncExample(Now, Version);
            }
            yield break;
        }
        #endregion
    }
}