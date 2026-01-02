using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace BussinnessLayer
{
    public class request
    {
        public bool AddRequestToBeAUser(PersonDTO personDTO, string traderID, string password)
        {
            // Perform validation and business logic before calling DAL
            if (string.IsNullOrEmpty(traderID) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Trader ID or Password is empty.");
                return false;
            }

            // Call Data Access Layer (DAL) to perform the database action
            return requesttobeauser.Addrequesttobeauser(personDTO, traderID, password);
        }

        // Method to validate if the request to become a user is valid
        public bool IsValidRequest(string traderID)
        {
            if (string.IsNullOrEmpty(traderID))
            {
                Console.WriteLine("Trader ID is invalid.");
                return false;
            }

            // Call Data Access Layer (DAL) to perform validation checks
            return requesttobeauser.IsItGoodRequest(traderID);
        }


        
        // Method to accept or reject a user request
        public bool AcceptRejectRequest(string traderID)
        {
            if (string.IsNullOrEmpty(traderID))
            {
                Console.WriteLine("Trader ID is invalid.");
                return false;
            }

            try
            {
                // Call the DAL to accept or reject the request based on the business rules
                return requesttobeauser.AcceptRejectrequest(traderID);
            }
            catch (Exception ex)
            {
                // Handle any business-specific errors
                Console.WriteLine($"Error while processing the request: {ex.Message}");
                return false;
            }
        }

        public List<(string TraderID, int PersonID)> GetAllRequests()
        {
            try
            {
                return requesttobeauser.GetAllRequests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all requests: {ex.Message}");
                return new List<(string, int)>();
            }
        }
    }
}
