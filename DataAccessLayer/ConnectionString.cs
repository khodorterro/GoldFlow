using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    public static class ConnectionString
    {
        public static string connectionstring { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            connectionstring = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionstring))
                throw new Exception("Connection string is not configured.");
        }
    }
}
