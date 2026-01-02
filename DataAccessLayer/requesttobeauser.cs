using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;

namespace DataAccessLayer
{

    public class requesttobeauser
    {
        public static bool Addrequesttobeauser(PersonDTO PDTO, string traderID, string password)
        {
            try
            {
                using (SqlConnection checkConn = new SqlConnection(ConnectionString.connectionstring))
                {
                    checkConn.Open();

                    // 🔍 Check if TraderID already exists in Traders
                    string checkTraderQuery = "SELECT COUNT(*) FROM Traders WHERE TraderID = @traderId";
                    using (SqlCommand cmd = new SqlCommand(checkTraderQuery, checkConn))
                    {
                        cmd.Parameters.AddWithValue("@traderId", traderID);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            Console.WriteLine("❌ Signup failed: TraderID already exists in Traders.");
                            return false;
                        }
                    }

                    // 🔍 Check if TraderID already exists in request_to_be_a_user
                    string checkRequestQuery = "SELECT COUNT(*) FROM request_to_be_a_user WHERE db_traderID = @traderId";
                    using (SqlCommand cmd = new SqlCommand(checkRequestQuery, checkConn))
                    {
                        cmd.Parameters.AddWithValue("@traderId", traderID);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            Console.WriteLine("❌ Signup failed: TraderID already has a pending request.");
                            return false;
                        }
                    }
                }

                // ✅ Add new Person
                int personid = PeopleData.AddPerson(PDTO);

                // ✅ Hash the password
                var hasher = new PasswordHasher<string>();
                string hashedPassword = hasher.HashPassword(null, password);

                using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
                {
                    const string query = @"
                    INSERT INTO request_to_be_a_user (db_personID, db_traderID, db_password)
                    VALUES (@personId, @traderId, @password);
                    SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@personId", personid);
                        cmd.Parameters.AddWithValue("@traderId", traderID);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);

                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int newId) && newId > 0)
                        {
                            Console.WriteLine("✅ Signup request added successfully.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("❌ Failed to insert signup request.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception in Addrequesttobeauser: " + ex.Message);
                return false;
            }
        }


        public static bool IsItGoodRequest(string traderID)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = "SELECT 1 FROM guildtraders WHERE TraderID = @traderid";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@traderid", traderID);

                    try
                    {
                        connection.Open();
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                            return false;

                        string query2 = "SELECT db_personID FROM request_to_be_a_user WHERE db_traderID = @traderid";
                        using (SqlCommand cmd2 = new SqlCommand(query2, connection))
                        {
                            cmd2.Parameters.AddWithValue("@traderid", traderID);
                            var result2 = cmd2.ExecuteScalar();

                            if (result2 == null)
                                return false;

                            PersonDTO checkedPerson = PeopleData.GetPersonByID((int)result2);

                            string query3 = "SELECT personID FROM guildtraders WHERE TraderID = @traderid";
                            using (SqlCommand cmd3 = new SqlCommand(query3, connection))
                            {
                                cmd3.Parameters.AddWithValue("@traderid", traderID);
                                var result3 = cmd3.ExecuteScalar();

                                if (result3 == null)
                                    return false;

                                PersonDTO traderPerson = PeopleData.GetPersonByID((int)result3);

                                if (traderPerson.Name != checkedPerson.Name || traderPerson.Email != checkedPerson.Email || traderPerson.Phonenumber != checkedPerson.Phonenumber)
                                    return false;

                                return true;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }


        private static  string GenerateShortWalletID()
        {
            // Generate a random number with fewer digits, for example, a 4-digit random number
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);

            // Get the current timestamp in seconds (Unix timestamp) and take the last 4 digits
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int shortTimestamp = (int)(timestamp % 10000);  // Taking last 4 digits

            // Combine the random number and short timestamp to create a very short unique wallet ID
            string walletID = $"{randomNumber}{shortTimestamp}"; // Shorter length by limiting to 8 characters

            return walletID;
        }


        public static bool AcceptRejectrequest(string traderID)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                SqlTransaction? transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    Console.WriteLine("Opened connection and started transaction.");

                    if (IsItGoodRequest(traderID))
                    {
                        Console.WriteLine("Request is valid.");


                        string query1 = "SELECT personID FROM guildtraders WHERE TraderID = @traderid";
                        SqlCommand cmd1 = new SqlCommand(query1, connection, transaction);
                        cmd1.Parameters.AddWithValue("@traderid", traderID);
                        var result1 = cmd1.ExecuteScalar();

                        if (result1 != null && int.TryParse(result1.ToString(), out int guildPersonID) && guildPersonID != 0)
                        {
                            Console.WriteLine($"Found guild person ID: {guildPersonID}");


                            string query2 = "SELECT db_password, db_personID FROM request_to_be_a_user WHERE db_traderID = @traderid";
                            SqlCommand cmd2 = new SqlCommand(query2, connection, transaction);
                            cmd2.Parameters.AddWithValue("@traderid", traderID);

                            using (SqlDataReader reader = cmd2.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string password = reader["db_password"].ToString();
                                    int requestPersonID = (int)reader["db_personID"];
                                    reader.Close();

                                    Console.WriteLine($"Got request person ID: {requestPersonID}, password exists: {!string.IsNullOrEmpty(password)}");


                                    SqlCommand cmd3 = new SqlCommand(
                                        "INSERT INTO Traders(TraderID, personID, db_password) VALUES (@traderid, @personid, @password)",
                                        connection, transaction);
                                    cmd3.Parameters.AddWithValue("@traderid", traderID);
                                    cmd3.Parameters.AddWithValue("@personid", guildPersonID); 
                                    cmd3.Parameters.AddWithValue("@password", password);
                                    cmd3.ExecuteNonQuery();

                                    string walletID = GenerateShortWalletID();

                                    SqlCommand cmd6 = new SqlCommand(
                                    "INSERT INTO wallets (TraderID, db_walletID, db_liras, db_ounces, db_semiliras, db_createddate) VALUES (@traderid, @walletid, 0, 0, 0, GETDATE())",
                                    connection, transaction);
                                    cmd6.Parameters.AddWithValue("@traderid", traderID);
                                    cmd6.Parameters.AddWithValue("@walletid", walletID);
                                    cmd6.ExecuteNonQuery();

                                    Console.WriteLine("Wallet created successfully with Wallet ID: " + walletID);


                                    SqlCommand cmd4 = new SqlCommand(
                                        "DELETE FROM request_to_be_a_user WHERE db_traderID = @traderid",
                                        connection, transaction);
                                    cmd4.Parameters.AddWithValue("@traderid", traderID);
                                    cmd4.ExecuteNonQuery();

                                    SqlCommand cmd5 = new SqlCommand(
                                        "DELETE FROM people WHERE personID = @personid",
                                        connection, transaction);
                                    cmd5.Parameters.AddWithValue("@personid", requestPersonID);
                                    cmd5.ExecuteNonQuery();

                                    transaction.Commit();
                                    Console.WriteLine("Transaction committed successfully.");
                                    return true;
                                }
                                else
                                {
                                    Console.WriteLine("No matching request found in request_to_be_a_user.");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Guild trader person ID not found.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Request is not valid. Deleting request...");

                        // ✅ Delete the invalid request
                        SqlCommand deleteCmd = new SqlCommand(
                            "DELETE FROM request_to_be_a_user WHERE db_traderID = @traderid",
                            connection, transaction);
                        deleteCmd.Parameters.AddWithValue("@traderid", traderID);
                        deleteCmd.ExecuteNonQuery();

                        transaction.Commit(); // commit deletion
                        return false;
                    }

                    transaction?.Rollback();
                    return false;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    Console.WriteLine($"Error in AcceptRejectrequest: {ex.Message}");
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public static List<(string TraderID, int PersonID)> GetAllRequests()
        {
            List<(string TraderID, int PersonID)> requests = new List<(string, int)>();

            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = "SELECT db_traderID, db_personID FROM request_to_be_a_user";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string traderID = reader["db_traderID"].ToString();
                                int personID = Convert.ToInt32(reader["db_personID"]);

                                requests.Add((traderID, personID));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in GetAllRequests: {ex.Message}");
                        // You can return empty list if error
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return requests;
        }




    }
}
