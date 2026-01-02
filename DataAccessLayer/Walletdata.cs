using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class WalletDTO
    {
        public string TraderID { get; set; }
        public string WalletID { get; set; }
        public int Liras { get; set; }
        public int Ounces { get; set; }
        public int HalfLiras { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ActionDTO
    {
        public int ActionID { get; set; }
        public string WalletID { get; set; }
        public int TypeOfActionID { get; set; }
        public int QuantityOfOunces { get; set; }
        public int QuantityOfLiras { get; set; }
        public int QuantityOfSemiLiras { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Walletdata
    {
        public static bool AddWallet(string walletid, int lirasChange, int ouncesChange, int semiLirasChange)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    // Update the wallet balance
                    string updateQuery = @"
                    UPDATE wallets 
                    SET 
                    db_liras = db_liras + @liras,
                    db_ounces = db_ounces + @ounces,
                    db_semiliras = db_semiliras + @semiliras
                    WHERE db_walletID = @walletid";

                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                    updateCmd.Parameters.Add("@liras", SqlDbType.Int).Value = lirasChange;
                    updateCmd.Parameters.Add("@ounces", SqlDbType.Int).Value = ouncesChange;
                    updateCmd.Parameters.Add("@semiliras", SqlDbType.Int).Value = semiLirasChange;
                    updateCmd.Parameters.Add("@walletid", SqlDbType.NVarChar).Value = walletid;

                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        string actionQuery = @"
                INSERT INTO actions (db_walletID, db_typeID, db_quantityofounces, db_quantityofliras, db_quantityofsemiliras,db_date)
                VALUES (@walletid, @actionType, @ounces, @liras, @semiliras,GETDATE())";

                        SqlCommand actionCmd = new SqlCommand(actionQuery, conn, transaction);
                        actionCmd.Parameters.Add("@walletid", SqlDbType.NVarChar).Value = walletid;
                        actionCmd.Parameters.Add("@actionType", SqlDbType.Int).Value = 1; 
                        actionCmd.Parameters.Add("@ounces", SqlDbType.Int).Value = ouncesChange;
                        actionCmd.Parameters.Add("@liras", SqlDbType.Int).Value = lirasChange;
                        actionCmd.Parameters.Add("@semiliras", SqlDbType.Int).Value = semiLirasChange;

                        actionCmd.ExecuteNonQuery();
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        Console.WriteLine("No wallet found with the provided wallet ID.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Console.WriteLine("Error in AddWallet: " + ex.Message);
                    return false;
                }
            }
        }


        public static bool ReduceFromWallet(string walletid, int lirasChange, int ouncesChange, int semiLirasChange)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string updateQuery = @"
            UPDATE wallets 
            SET 
            db_liras = db_liras - @liras,
            db_ounces = db_ounces - @ounces,
            db_semiliras = db_semiliras - @semiliras
            WHERE db_walletID = @walletid";

                    SqlCommand updateCmd = new SqlCommand(updateQuery, conn, transaction);
                    updateCmd.Parameters.Add("@liras", SqlDbType.Int).Value = lirasChange;
                    updateCmd.Parameters.Add("@ounces", SqlDbType.Int).Value = ouncesChange;
                    updateCmd.Parameters.Add("@semiliras", SqlDbType.Int).Value = semiLirasChange;
                    updateCmd.Parameters.Add("@walletid", SqlDbType.NVarChar).Value = walletid;

                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        string actionQuery = @"
                INSERT INTO actions (db_walletID, db_typeID, db_quantityofounces, db_quantityofliras, db_quantityofsemiliras,db_date)
                VALUES (@walletid, @actionType, @ounces, @liras, @semiliras,GETDATE())";

                        SqlCommand actionCmd = new SqlCommand(actionQuery, conn, transaction);
                        actionCmd.Parameters.Add("@walletid", SqlDbType.NVarChar).Value = walletid;
                        actionCmd.Parameters.Add("@actionType", SqlDbType.Int).Value = 2;
                        actionCmd.Parameters.Add("@ounces", SqlDbType.Int).Value = ouncesChange;
                        actionCmd.Parameters.Add("@liras", SqlDbType.Int).Value = lirasChange;
                        actionCmd.Parameters.Add("@semiliras", SqlDbType.Int).Value = semiLirasChange;

                        actionCmd.ExecuteNonQuery();
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        Console.WriteLine("No wallet found with the provided wallet ID.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Console.WriteLine("Error in ReduceWallet: " + ex.Message);
                    return false;
                }
            }
        }

        public static WalletDTO GetWalletByTraderID(string traderID)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = @"SELECT TraderID, db_walletID, db_liras, db_ounces, db_semiliras, db_createddate 
                                 FROM wallets WHERE TraderID = @traderID";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.Add("@traderID", SqlDbType.NVarChar).Value = traderID;

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new WalletDTO
                        {
                            TraderID = reader["TraderID"].ToString(),
                            WalletID = reader["db_walletID"].ToString(),
                            Liras = Convert.ToInt32(reader["db_liras"]),
                            Ounces = Convert.ToInt32(reader["db_ounces"]),
                            HalfLiras = Convert.ToInt32(reader["db_semiliras"]),
                            CreatedDate = Convert.ToDateTime(reader["db_createddate"])
                        };
                    }
                    else
                    {
                        Console.WriteLine($"No wallet found for TraderID {traderID}");
                    }
                }
            }
            return null;
        }

        public static List<ActionDTO> GetAllActionsOnWallet(string walletID)
        {
            List<ActionDTO> actions = new List<ActionDTO>();

            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                try
                {
                    string query = @"
                                SELECT *
                                FROM actions
                                WHERE db_walletID = @walletID
                                ORDER BY db_date DESC"; 

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@walletID", SqlDbType.NVarChar).Value = walletID;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actions.Add(new ActionDTO
                            {
                                ActionID = Convert.ToInt32(reader["actionID"]),
                                WalletID = reader["db_walletID"].ToString(),
                                TypeOfActionID = Convert.ToInt32(reader["db_typeID"]),
                                QuantityOfOunces = Convert.ToInt32(reader["db_quantityofounces"]),
                                QuantityOfLiras = Convert.ToInt32(reader["db_quantityofliras"]),
                                QuantityOfSemiLiras = Convert.ToInt32(reader["db_quantityofsemiliras"]),
                                Timestamp = Convert.ToDateTime(reader["db_date"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetAllActionsOnWallet: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return actions;
        }

        public static int GetNumberOfIncomes(string walletID)
        {
            int incomeCount = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                try
                {
                    string query = @"
                SELECT COUNT(*) 
                FROM actions
                WHERE db_walletID = @walletID AND db_typeID = 1";  // 1 = Income

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@walletID", SqlDbType.NVarChar).Value = walletID;

                    conn.Open();
                    incomeCount = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetNumberOfIncomes: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return incomeCount;
        }

        public static int GetNumberOfDecomes(string walletID)
        {
            int decomeCount = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                try
                {
                    string query = @"
                SELECT COUNT(*) 
                FROM actions
                WHERE db_walletID = @walletID AND db_typeID = 2";  // 2 = Decome/Outcome

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@walletID", SqlDbType.NVarChar).Value = walletID;

                    conn.Open();
                    decomeCount = (int)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetNumberOfDecomes: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return decomeCount;
        }


    }
}
