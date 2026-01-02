using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class Traderidgenerator
    {
        public static string GenerateTraderId()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    int sequence = -1;

                    const string selectQuery = "SELECT db_seqeunce FROM seqeunce WITH (UPDLOCK, HOLDLOCK)";
                    using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection, transaction))
                    {
                        object result = selectCommand.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            throw new Exception("Sequence not initialized in database.");
                        }
                        sequence = (int)result;
                    }

                    const string updateQuery = "UPDATE seqeunce SET db_seqeunce = db_seqeunce + 1";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                    {
                        updateCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    string prefix = "TRD";
                    string datePart = DateTime.Now.ToString("yyyyMMdd");
                    string sequencePart = sequence.ToString("D5"); 

                    string traderId = $"{prefix}{datePart}{sequencePart}";
                    return traderId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error generating TraderID: " + ex.Message);
                    throw; 
                }
                finally
                {
                    connection.Close();
                }
            }
        }

    }
}
