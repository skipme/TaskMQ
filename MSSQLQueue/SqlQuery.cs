using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MSSQLQueue
{
    public class SqlQuery
    {
        public static object QueryScalar(string sql, string connectionString, params KeyValuePair<string, object>[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = sql;
                foreach (KeyValuePair<string, object> kv in parameters)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value);
                }
                return command.ExecuteScalar();
            }
        }
    }
}
