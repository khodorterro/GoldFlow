using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using Microsoft.AspNetCore.Identity;

namespace BussinnessLayer
{
    public class AdminService
    {
        public AdminDTO GetAdminById(int adminId)
        {
            return AdminData.GetAdminByID(adminId);
        }

        public bool EmailAlreadyUsed(string email)
        {
            return AdminData.EmailExists(email);
        }

        public List<AdminDTO> GetAllAdmins()
        {
            return AdminData.GetAllAdmins();
        }

        public int AddAdmin(AdminDTO2 admin)
        {
            if (admin == null)
                throw new ArgumentNullException(nameof(admin));

            return AdminData.AddAdmin(admin);
        }

        public bool UpdateAdmin(int id, AdminDTO admin)
        {
            if (admin == null)
                throw new ArgumentNullException(nameof(admin));

            return AdminData.UpdateAdmin(id, admin);
        }

        public bool DeleteAdmin(int adminId)
        {
            return AdminData.DeleteAdmin(adminId);
        }


        public AdminDTO? Login(string email, string password)
        {
            var allAdmins = AdminData.GetAllAdmins();
            var hasher = new PasswordHasher<string>();

            foreach (var admin in allAdmins)
            {
                if (admin.Email == email)
                {
                    var result = hasher.VerifyHashedPassword(null, admin.Password, password);
                    if (result == PasswordVerificationResult.Success)
                    {
                        return admin;
                    }
                }
            }

            return null;
        }
    }
}
