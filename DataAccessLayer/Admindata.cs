using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

namespace DataAccessLayer
{
    public class AdminDTO
    {
        public int AdminID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public AdminDTO() { }

        public AdminDTO(int adminId, string name, string password, string email, string phone)
        {
            AdminID = adminId;
            Name = name;
            Password = password;
            Email = email;
            Phone = phone;
        }
    }

    public class AdminDTO2
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public AdminDTO2() { }

        public AdminDTO2(string name, string password, string email, string phone)
        {
            Name = name;
            Password = password;
            Email = email;
            Phone = phone;
        }
    }

    public class AdminData
    {
        private static readonly PasswordHasher<string> _hasher = new();

        public AdminDTO? Login(string email, string inputPassword)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = @"SELECT adminID, db_adminname, db_password, db_email, db_phone 
                                 FROM Admins 
                                 WHERE db_email = @Email";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string hashedPassword = reader["db_password"].ToString();
                    var result = _hasher.VerifyHashedPassword(null, hashedPassword, inputPassword);

                    if (result == PasswordVerificationResult.Success)
                    {
                        return new AdminDTO(
                            adminId: (int)reader["adminID"],
                            name: reader["db_adminname"].ToString(),
                            password: hashedPassword,
                            email: reader["db_email"].ToString(),
                            phone: reader["db_phone"].ToString()
                        );
                    }
                }

                return null;
            }
        }

        public static AdminDTO? GetAdminByID(int adminId)
        {
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                const string query = @"
                SELECT adminID, db_adminname, db_password, db_email, db_phone
                FROM Admins
                WHERE adminID = @adminId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminId", adminId);

                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new AdminDTO(
                                    adminId: (int)reader["adminID"],
                                    name: reader["db_adminname"].ToString(),
                                    password: reader["db_password"].ToString(),
                                    email: reader["db_email"].ToString(),
                                    phone: reader["db_phone"].ToString()
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error getting admin by ID: " + ex.Message);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return null;
        }

        public static int AddAdmin(AdminDTO2 admin)
        {
            if (admin == null)
                return -1;

            string hashedPassword = _hasher.HashPassword(null, admin.Password ?? "");

            const string query = @"
            INSERT INTO Admins (db_adminname, db_password, db_email, db_phone)
            VALUES (@name, @password, @email, @phone);
            SELECT SCOPE_IDENTITY();";

            try
            {
                using (var connection = new SqlConnection(ConnectionString.connectionstring))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", admin.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@email", admin.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@phone", admin.Phone ?? (object)DBNull.Value);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting admin: " + ex.Message);
                return -1;
            }
        }

        public static bool UpdateAdmin(int adminId, AdminDTO admin)
        {
            string hashedPassword = _hasher.HashPassword(null, admin.Password ?? "");

            const string query = @"
            UPDATE Admins
            SET db_adminname = @Name,
                db_password = @Password,
                db_email = @Email,
                db_phone = @Phone
            WHERE adminID = @AdminID";

            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", admin.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Password", hashedPassword);
                command.Parameters.AddWithValue("@Email", admin.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Phone", admin.Phone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AdminID", adminId);

                try
                {
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating admin: " + ex.Message);
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public static bool EmailExists(string email)
        {
            const string query = "SELECT COUNT(*) FROM Admins WHERE db_email = @Email";
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error checking email existence: " + ex.Message);
                    return false;
                }
            }
        }

        public static bool DeleteAdmin(int adminId)
        {
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                try
                {
                    connection.Open();
                    const string query = "DELETE FROM Admins WHERE adminID = @adminId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@adminId", adminId);
                        int rows = command.ExecuteNonQuery();
                        return rows > 0;
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

        public static List<AdminDTO> GetAllAdmins()
        {
            var admins = new List<AdminDTO>();

            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                const string query = "SELECT adminID, db_adminname, db_password, db_email, db_phone FROM Admins";

                using (var command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                admins.Add(new AdminDTO(
                                    (int)reader["adminID"],
                                    reader["db_adminname"].ToString(),
                                    reader["db_password"].ToString(),
                                    reader["db_email"].ToString(),
                                    reader["db_phone"].ToString()
                                ));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error fetching admins: " + ex.Message);
                        return new List<AdminDTO>();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return admins;
        }
    }
}
