using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace BussinnessLayer
{
    public  class commission
    {
        public int AddCommission(decimal amount)
        {
            var commission = new CommissionDTO
            {
                Amount = amount,
                Date = DateTime.Now,
                Active = true
            };

            return CommissionData.AddCommission(commission);
        }

        // Manually update a specific commission to inactive
        public bool DeactivateCommission(int id)
        {
            return CommissionData.UpdateCommissionStatusToFalse(id);
        }

        public bool activateCommission(int id)
        {
            return CommissionData.UpdateCommissionStatusToTrue(id);
        }

        // Delete a commission
        public bool DeleteCommission(int id)
        {
            return CommissionData.DeleteCommission(id);
        }

        // Get all commissions
        public List<CommissionDTO> GetAllCommissions()
        {
            return CommissionData.GetAllCommissions();
        }

        // Get the currently active commission
        public CommissionDTO GetActiveCommission()
        {
            return CommissionData.GetAllCommissions().Find(c => c.Active);
        }
    }
}
