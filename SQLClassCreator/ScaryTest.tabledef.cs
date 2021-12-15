using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSL.SQL;

namespace ExampleNamespace.SomeSubNamespace
{
    public record Scary_Table(byte ID, Data29 Data29, string? Data30) : IDBSet
    {
        #region Static Functions
        public static Task<int> CreateDB(SQLDB sql) => sql.ExecuteNonQuery(
            "CREATE TABLE IF NOT EXISTS \"Scary Table\" (" +
            "\"ID\" BYTEA NOT NULL, " +
            "\"Data29\" BIGINT NOT NULL, " +
            "\"Data30\" TEXT, " +
            "PRIMARY KEY(\"ID\")" +
            ");");
        public static IEnumerable<Scary_Table> GetRecords(IDataReader dr)
        {
            while(dr.Read())
            {
                byte[] _ID =  (byte[])dr[0];
                byte ID = _ID[0];
                long _Data29 =  (long)dr[1];
                Data29 Data29 = (Data29)_Data29;
                string? Data30 = dr.IsDBNull(2) ? null : (string)dr[2];
                yield return new Scary_Table(ID, Data29, Data30);
            }
            yield break;
        }
        #region Select
        public static async Task<AutoClosingEnumerable<Scary_Table>> Select(SQLDB sql)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Scary Table\";");
            return new AutoClosingEnumerable<Scary_Table>(GetRecords(dr),dr);
        }
        public static async Task<AutoClosingEnumerable<Scary_Table>> Select(SQLDB sql, string query, params object[] parameters)
        {
            AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Scary Table\" WHERE \"" + query + "\" ;", parameters);
            return new AutoClosingEnumerable<Scary_Table>(GetRecords(dr),dr);
        }
        public static async Task<Scary_Table?> SelectBy_ID(SQLDB sql, byte ID)
        {
            using(AutoClosingDataReader dr = await sql.ExecuteReader("SELECT * FROM \"Scary Table\" WHERE \"ID\" = @0;", ID))
            {
                return GetRecords(dr).FirstOrDefault();
            }
        }
        #endregion
        #region Delete
        public static Task<int> DeleteBy_ID(SQLDB sql, byte ID) => sql.ExecuteNonQuery("DELETE FROM \"Scary Table\" WHERE \"ID\" = @0;", ID);
        #endregion
        #region Table Management
        public static Task Truncate(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"TRUNCATE \"Scary Table\"{(cascade?" CASCADE":"")};");
        public static Task Drop(SQLDB sql, bool cascade = false) => sql.ExecuteNonQuery($"DROP TABLE IF EXISTS \"Scary Table\"{(cascade?" CASCADE":"")};");
        #endregion
        #endregion
        #region Instance Functions
        public Task<int> Insert(SQLDB sql) =>
            sql.ExecuteNonQuery("INSERT INTO \"Scary Table\" (\"ID\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2);", ToArray());
        public Task<int> Update(SQLDB sql) =>
            sql.ExecuteNonQuery("UPDATE \"Scary Table\" " +
            "SET \"Data29\" = @1, \"Data30\" = @2 " +
            "WHERE \"ID\" = @0;", ToArray());
        public Task<int> Upsert(SQLDB sql) =>
            sql.ExecuteNonQuery("INSERT INTO \"Scary Table\" (\"ID\", \"Data29\", \"Data30\") " +
            "VALUES(@0, @1, @2) " +
            "ON CONFLICT (\"ID\") DO UPDATE " +
            "SET \"Data29\" = @1, \"Data30\" = @2;", ToArray());
        public object?[] ToArray()
        {
            byte[] _ID = new byte[] {ID};
            long _Data29 = (long)Data29;
            string? _Data30 = Data30 == null?default:Data30;
            return new object?[] { _ID, _Data29, _Data30 };
        }
        #endregion
    }
    #region Example Enums
    
    ////Example Enum
    //[Flags]
    ////Specifying ulong allows data to be auto converted for your convenience into the database.
    //public enum Data29 : ulong
    //{
        //NoFlags = 0,
        //Flag1   = 1UL << 0,
        //Flag2   = 1UL << 1,
        //Flag3   = 1UL << 2,
        //Flag4   = 1UL << 3,
        //Flag5   = 1UL << 4,
        //Flag6   = 1UL << 5,
        //Flag7   = 1UL << 6,
        //Flag8   = 1UL << 7,
        //Flag9   = 1UL << 8,
        //Flag10  = 1UL << 9,
        //Flag11  = 1UL << 10,
        //Flag12  = 1UL << 11,
        //Flag13  = 1UL << 12,
        //Flag14  = 1UL << 13,
        //Flag15  = 1UL << 14,
        //Flag16  = 1UL << 15,
        //Flag17  = 1UL << 16,
        //Flag18  = 1UL << 17,
        //Flag19  = 1UL << 18,
        //Flag20  = 1UL << 19,
        //Flag21  = 1UL << 20,
        //Flag22  = 1UL << 21,
        //Flag23  = 1UL << 22,
        //Flag24  = 1UL << 23,
        //Flag25  = 1UL << 24,
        //Flag26  = 1UL << 25,
        //Flag27  = 1UL << 26,
        //Flag28  = 1UL << 27,
        //Flag29  = 1UL << 28,
        //Flag30  = 1UL << 29,
        //Flag31  = 1UL << 30,
        //Flag32  = 1UL << 31,
        //Flag33  = 1UL << 32,
        //Flag34  = 1UL << 33,
        //Flag35  = 1UL << 34,
        //Flag36  = 1UL << 35,
        //Flag37  = 1UL << 36,
        //Flag38  = 1UL << 37,
        //Flag39  = 1UL << 38,
        //Flag40  = 1UL << 39,
        //Flag41  = 1UL << 40,
        //Flag42  = 1UL << 41,
        //Flag43  = 1UL << 42,
        //Flag44  = 1UL << 43,
        //Flag45  = 1UL << 44,
        //Flag46  = 1UL << 45,
        //Flag47  = 1UL << 46,
        //Flag48  = 1UL << 47,
        //Flag49  = 1UL << 48,
        //Flag50  = 1UL << 49,
        //Flag51  = 1UL << 50,
        //Flag52  = 1UL << 51,
        //Flag53  = 1UL << 52,
        //Flag54  = 1UL << 53,
        //Flag55  = 1UL << 54,
        //Flag56  = 1UL << 55,
        //Flag57  = 1UL << 56,
        //Flag58  = 1UL << 57,
        //Flag59  = 1UL << 58,
        //Flag60  = 1UL << 59,
        //Flag61  = 1UL << 60,
        //Flag62  = 1UL << 61,
        //Flag63  = 1UL << 62,
        //Flag64  = 1UL << 63,
    //}
    #endregion
}