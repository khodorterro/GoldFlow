using System.Data;
using Microsoft.Data.SqlClient;

namespace DataAccessLayer
{
    public class PersonDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phonenumber { get; set; }
        public string Email { get; set; }
        public PersonDTO() { }
        public PersonDTO(int id, string name, string address, string phonenumber, string email)
        {
            this.Id = id;
            this.Name = name;
            this.Address = address;
            this.Phonenumber = phonenumber;
            this.Email = email;
        }
    }

    public class PeopleData
    {
        public static PersonDTO GetPersonByID(int id)
        {
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = "SELECT * FROM people WHERE personID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int personID =(int)reader["personID"];

                                string name =  reader.GetString(reader.GetOrdinal("db_name"));
                                string address =  reader.GetString(reader.GetOrdinal("db_address"));
                                string phone = reader.GetString(reader.GetOrdinal("db_phone"));
                                string email =  reader.GetString(reader.GetOrdinal("db_email"));

                                return new PersonDTO(personID, name, address, phone, email);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            return null;
        }


        public static int AddPerson(PersonDTO personDTO)
        {
            const string query = @"
        INSERT INTO people(db_name, db_address, db_phone, db_email)
        VALUES(@name, @address, @phone, @email);
        SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", personDTO.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@address", personDTO.Address ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@phone", personDTO.Phonenumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@email", personDTO.Email ?? (object)DBNull.Value);

                connection.Open();
                object result = command.ExecuteScalar();
                connection.Close();
                return Convert.ToInt32(result);
            }
        }

        public static bool DeletePerson(int personId)
        {
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = "DELETE FROM people WHERE personID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", personId);
                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
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

        public static bool UpdatePerson(int id,PersonDTO personDTO)
        {
            using (var connection = new SqlConnection(ConnectionString.connectionstring))
            {
                string query = @"
                               UPDATE people 
                               SET db_name = @name,
                               db_address = @address,
                               db_phone = @phone,
                               db_email = @email
                               WHERE personID = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id",id);
                    command.Parameters.AddWithValue("@name", personDTO.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@address", personDTO.Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@phone", personDTO.Phonenumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@email", personDTO.Email ?? (object)DBNull.Value);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
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



    }
}
