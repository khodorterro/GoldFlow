using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class TraderDTO
    {
        public string TraderID { get; set; }
        public string Password { get; set; } // maps to db_password
        public int PersonID { get; set; }
    }
    public class TraderDTO3:PersonDTO
    {
        public string TraderID { get; set; }
        public string Password { get; set; }
        public int PersonID { get; set; }

    }


    public class TraderDTO2:PersonDTO
    {
        public string TraderID { get; set; }
        public int Liras { get; set; }
        public int Ounces { get; set; }
        public int HalfLiras { get; set; }

    }
    public  class Traderdata
    {
        private string connectionString = ConnectionString.connectionstring;

        public TraderDTO Login(string traderID, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TraderID, db_password, personID 
                         FROM Traders 
                         WHERE TraderID = @TraderID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TraderID", traderID);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string storedHashedPassword = reader["db_password"].ToString();
                    var hasher = new PasswordHasher<string>();

                    var result = hasher.VerifyHashedPassword(null, storedHashedPassword, password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        return new TraderDTO
                        {
                            TraderID = reader["TraderID"].ToString(),
                            Password = storedHashedPassword, // hashed version
                            PersonID = Convert.ToInt32(reader["personID"])
                        };
                    }
                }

                return null; // ❌ Invalid credentials
            }
        }

        public List<TraderDTO2> GetAllTraders()
        {
            List<TraderDTO2> list = new List<TraderDTO2>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
        people.personID,
        people.db_Name, 
        people.db_Address,
        people.db_Email,
        people.db_Phone,
        Traders.TraderID,
        wallets.db_liras,
        wallets.db_ounces,
        wallets.db_semiliras
        FROM Traders
        INNER JOIN people ON Traders.personID = people.personID
        INNER JOIN wallets ON wallets.TraderID = Traders.TraderID
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new TraderDTO2
                    {
                        TraderID = reader["TraderID"].ToString(),
                        Id = Convert.ToInt32(reader["personID"]),
                        Name = reader["db_Name"].ToString(),
                        Address = reader["db_Address"].ToString(),
                        Phonenumber = reader["db_Phone"].ToString(),
                        Email = reader["db_Email"].ToString(),
                        Liras = Convert.ToInt32(reader["db_liras"]),
                        Ounces = Convert.ToInt32(reader["db_ounces"]),
                        HalfLiras = Convert.ToInt32(reader["db_semiliras"])
                    });


                }
            }
            return list;
        }

        public string GetPassword(string traderID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT db_password FROM Traders WHERE TraderID = @TraderID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TraderID", traderID);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar(); // Get a single value
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }
                        else
                        {
                            return null; // No password found
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }



        public bool UpdateTraderFull(TraderDTO3 traderDTO)
        {
            bool personUpdated = false;
            bool traderUpdated = false;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Update the People table
                        personUpdated = PeopleData.UpdatePerson(traderDTO.PersonID, new PersonDTO
                        {
                            Id = traderDTO.PersonID,
                            Name = traderDTO.Name,
                            Address = traderDTO.Address,
                            Phonenumber = traderDTO.Phonenumber,
                            Email = traderDTO.Email
                        });

                        if (!personUpdated)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        // ✅ Hash the new password (if provided)
                        string hashedPassword = null;
                        if (!string.IsNullOrWhiteSpace(traderDTO.Password))
                        {
                            var hasher = new PasswordHasher<string>();
                            hashedPassword = hasher.HashPassword(null, traderDTO.Password);
                        }

                        // 2. Update the Traders table
                        string updateTraderQuery = @"
                    UPDATE Traders
                    SET db_password = @password
                    WHERE TraderID = @traderID AND personID = @personID;";

                        using (var traderCommand = new SqlCommand(updateTraderQuery, connection, transaction))
                        {
                            traderCommand.Parameters.AddWithValue("@password", (object?)hashedPassword ?? DBNull.Value);
                            traderCommand.Parameters.AddWithValue("@traderID", traderDTO.TraderID);
                            traderCommand.Parameters.AddWithValue("@personID", traderDTO.PersonID);

                            int rows = traderCommand.ExecuteNonQuery();
                            traderUpdated = rows > 0;
                        }

                        if (traderUpdated)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
