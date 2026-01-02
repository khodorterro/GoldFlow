using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
        public  class CommissionDTO
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
            public bool Active { get; set; }
        }

        public class CommissionData
        {
            public static int AddCommission(CommissionDTO commission)
            {
                using var connection = new SqlConnection(ConnectionString.connectionstring);
            try
            {
                connection.Open();

                // Deactivate all existing commissions
                string deactivateQuery = "UPDATE commission SET db_active = 0";
                using (var deactivateCmd = new SqlCommand(deactivateQuery, connection))
                {
                    deactivateCmd.ExecuteNonQuery();
                }

                // Insert new commission with current date/time and active = true
                string insertQuery = @"
                INSERT INTO Commission (db_amount, db_date, db_active)
                VALUES (@amount, @date, 1);
                SELECT SCOPE_IDENTITY();";

                using var insertCmd = new SqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@amount", commission.Amount);
                // Use the commission.Date that will be set in the controller (typically DateTime.Now)
                insertCmd.Parameters.AddWithValue("@date", DateTime.Now);

                object result = insertCmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                connection.Close();
            }
            }


            public static bool UpdateCommissionStatusToFalse(int id)
            {
                const string query = @"
                                  UPDATE commission
                                  SET db_active = 0
                                  WHERE db_commission_id = @id";

                using var connection = new SqlConnection(ConnectionString.connectionstring);
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
            try
            {
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
                
            }

        public static bool UpdateCommissionStatusToTrue(int id)
        {
            using var connection = new SqlConnection(ConnectionString.connectionstring);

            try
            {
                connection.Open();

                // Deactivate all existing commissions
                string deactivateQuery = "UPDATE commission SET db_active = 0";
                using (var deactivateCmd = new SqlCommand(deactivateQuery, connection))
                {
                    deactivateCmd.ExecuteNonQuery();
                }

                // Activate the specified commission
                const string query = @"
                              UPDATE commission
                              SET db_active = 1
                              WHERE db_commission_id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                // Optionally log ex.Message
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public static bool DeleteCommission(int id)
        {
            const string query = "DELETE FROM commission WHERE db_commission_id = @id";

            using var connection = new SqlConnection(ConnectionString.connectionstring);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Rows affected: {rowsAffected}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete Error: " + ex.Message);
                return false;
            }
            finally
            {
                connection.Close();
            }
        }


        public static List<CommissionDTO> GetAllCommissions()
            {
                var list = new List<CommissionDTO>();
                const string query = "SELECT * FROM commission";

                using var connection = new SqlConnection(ConnectionString.connectionstring);
                using var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new CommissionDTO
                    {
                        Id = (int)reader["db_commission_id"],
                        Amount = (decimal)reader["db_amount"],
                        Date = (DateTime)reader["db_date"],
                        Active = (bool)reader["db_active"]
                    });
                }

                return list;
            }
            catch
            {
                return list;
            }
            finally
            {
                connection.Close();
            }
            }
        public static CommissionDTO? GetActiveCommission()
        {
            const string query = "SELECT TOP 1 * FROM Commission WHERE db_active = 1";

            using var connection = new SqlConnection(ConnectionString.connectionstring);
            using var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new CommissionDTO
                    {
                        Id = (int)reader["db_commission_id"],
                        Amount = (decimal)reader["db_amount"],
                        Date = (DateTime)reader["db_date"],
                        Active = (bool)reader["db_active"]
                    };
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

    }

}
