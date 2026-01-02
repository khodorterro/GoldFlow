using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace BussinnessLayer
{
    public class Trader
    {
        private Traderdata dal = new Traderdata();

        public TraderDTO Login(string traderID, string password)
        {
            if (string.IsNullOrWhiteSpace(traderID) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Trader ID and password cannot be empty");

            var trader = dal.Login(traderID, password);
            if (trader == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            return trader;
        }

        public List<TraderDTO2> GetAllTrader()
        {
            return dal.GetAllTraders();
        }

        public bool UpdateTrader(TraderDTO3 trader)
        {
            return dal.UpdateTraderFull(trader);
        }

        public string GetPassword(string traderID)
        {
            return dal.GetPassword(traderID);
        }
    }
}


