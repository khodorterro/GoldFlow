using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;

namespace BussinnessLayer
{
    public class Guildtraders
    {
        public static bool AddGuildTrader(PersonDTO personDTO)
        {
            return guildtraders.AddGuildTrader(personDTO);
        }
        public static bool TraderIdExists(string traderId)
        {
            return guildtraders.TraderIdExists(traderId);
        }
        public static bool Deleteguildtrader(string traderId)
        {
            return guildtraders.Deleteguildtrader(traderId);
        }

        public static List<GuildTraderDTO2> GetAllGuildTraders()
        {
            return guildtraders.GetAllGuildTraders();
        }


    }
}
