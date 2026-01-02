using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;

namespace DataAccessLayer
{
    public class GuildTraderDTO : PersonDTO
    {
        public string TraderID { get; set; }

        public GuildTraderDTO(int personID, string name, string phone, string address, string email,string traderID)
            : base(personID, name, phone, address, email)
        {
            TraderID = traderID;
        }
    }
    public class GuildTraderDTO2
    {
        public string TraderID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        public GuildTraderDTO2(string TraderID,string Name, string Phone,string Address,string Email) { 
            this.TraderID = TraderID;
            this.Name = Name;
            this.Phone = Phone;
            this.Address = Address;
            this.Email = Email;
        }

    }
    public class guildtraders
    {
        public static bool AddGuildTrader(PersonDTO PDTO)
        {
            using var connection = new SqlConnection(ConnectionString.connectionstring);
            connection.Open();

            // 🔥 First: Check if Email already exists in GuildTraders
            string checkQuery = @"
        SELECT COUNT(*) 
        FROM guildtraders gt
        INNER JOIN people p ON gt.personID = p.personID
        WHERE p.db_Email = @Email";

            using (var checkCmd = new SqlCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@Email", PDTO.Email);

                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Email already exists
                    return false;
                }
            }

            // 🔥 Second: Insert New Person
            int personID = PeopleData.AddPerson(PDTO);
            if (personID <= 0)
            {
                // Failed to add person
                return false;
            }

            // 🔥 Third: Insert into GuildTraders
            string traderid = Traderidgenerator.GenerateTraderId();
            string insertQuery = @"
        INSERT INTO guildtraders (personID, TraderID)
        VALUES (@personID, @traderID)";

            using (var insertCmd = new SqlCommand(insertQuery, connection))
            {
                insertCmd.Parameters.AddWithValue("@personID", personID);
                insertCmd.Parameters.AddWithValue("@traderID", traderid);

                int result = insertCmd.ExecuteNonQuery();
                return result > 0;
            }
        }


        public static bool TraderIdExists(string traderId)
        {
            const string query = "SELECT COUNT(1) FROM guildtraders WHERE TraderID = @TraderID";

            using var connection = new SqlConnection(ConnectionString.connectionstring);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TraderID", traderId);

            try
            {
                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
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

        public static bool Deleteguildtrader(string traderid)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                try
                {
                    connection.Open();

                    // First: Get personID using traderID
                    const string getPersonQuery = "SELECT personID FROM guildtraders WHERE TraderID = @TraderID";
                    int personId;

                    using (SqlCommand getPersonCmd = new SqlCommand(getPersonQuery, connection))
                    {
                        getPersonCmd.Parameters.AddWithValue("@TraderID", traderid);
                        object result = getPersonCmd.ExecuteScalar();
                        if (result == null)
                            return false;

                        personId = Convert.ToInt32(result);
                    }

                    // Second: Delete from guildtraders
                    const string deleteTraderQuery = "DELETE FROM guildtraders WHERE TraderID = @TraderID";
                    using (SqlCommand deleteTraderCmd = new SqlCommand(deleteTraderQuery, connection))
                    {
                        deleteTraderCmd.Parameters.AddWithValue("@TraderID", traderid);
                        deleteTraderCmd.ExecuteNonQuery();
                    }

                    // Third: Delete from people
                    const string deletePersonQuery = "DELETE FROM people WHERE personID = @PersonID";
                    using (SqlCommand deletePersonCmd = new SqlCommand(deletePersonQuery, connection))
                    {
                        deletePersonCmd.Parameters.AddWithValue("@PersonID", personId);
                        int rowsAffected = deletePersonCmd.ExecuteNonQuery();
                        return rowsAffected > 0;
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

        public static List<GuildTraderDTO2> GetAllGuildTraders()
        {
            List<GuildTraderDTO2> traders = new List<GuildTraderDTO2>();

            using (SqlConnection connection = new SqlConnection(ConnectionString.connectionstring))
            {
                const string query = @"
            SELECT  
                p.db_Name, 
                p.db_Phone, 
                p.db_Address, 
                p.db_Email,
                gt.TraderID
            FROM 
                guildtraders gt
            INNER JOIN 
                people p ON gt.personID = p.personID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                traders.Add(new GuildTraderDTO2(
                                    TraderID: reader["TraderID"].ToString(),
                                    Name: reader["db_Name"].ToString(),
                                    Phone: reader["db_Phone"].ToString(),
                                    Address: reader["db_Address"].ToString(),
                                    Email: reader["db_Email"].ToString()
                                ));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error fetching guild traders: " + ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return traders;
        }


    }
}
