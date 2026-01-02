using System;
using DataAccessLayer;

namespace BussinnessLayer
{
    public class Wallets
    {
        public bool AddToWallet(string walletID, int liras, int ounces, int semiLiras)
        {
            try
            {
                return Walletdata.AddWallet(walletID, liras, ounces, semiLiras);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to wallet: {ex.Message}");
                return false;
            }
        }

        public bool ReduceFromWallet(string walletID, int liras, int ounces, int semiLiras)
        {
            try
            {
                return Walletdata.ReduceFromWallet(walletID, liras, ounces, semiLiras);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to wallet: {ex.Message}");
                return false;
            }
        }

        public WalletDTO? GetWalletByTraderID(string traderID)
        {
            try
            {
                return Walletdata.GetWalletByTraderID(traderID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching wallet information: {ex.Message}");
                return null;
            }
        }

        public List<ActionDTO> GetAllActionsOnWallet(string walletID)
        {
            return Walletdata.GetAllActionsOnWallet(walletID);
        }

        public int GetNumberOfIncomes(string walletID)
        {
            return Walletdata.GetNumberOfIncomes(walletID);
        }

        public int GetNumberOfDecomes(string walletID)
        {
            return Walletdata.GetNumberOfDecomes(walletID);
        }

    }
}
