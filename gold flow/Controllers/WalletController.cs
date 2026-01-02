using BussinnessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gold_flow.Controllers
{

    [Route("api/Wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly Wallets _walletService;

        public WalletController()
        {
            _walletService = new Wallets();
        }

        [HttpGet("{traderID}")]
        public IActionResult GetWalletByTraderID(string traderID)
        {
            var wallet = _walletService.GetWalletByTraderID(traderID);
            if (wallet == null)
            {
                return NotFound("Wallet not found.");
            }
            return Ok(wallet);
        }

        [HttpPut("add/{walletID}")]
        public IActionResult AddToWallet(string walletID, [FromBody] WalletUpdateModel updateModel)
        {
            bool result = _walletService.AddToWallet(walletID, updateModel.Liras, updateModel.Ounces, updateModel.HalfLiras);
            if (result)
            {
                return Ok("Money added to wallet successfully.");
            }
            return BadRequest("Failed to add money to wallet.");
        }

        [HttpPut("reduce/{walletID}")]
        public IActionResult ReduceFromWallet(string walletID, [FromBody] WalletUpdateModel updateModel)
        {
            bool result = _walletService.ReduceFromWallet(walletID, updateModel.Liras, updateModel.Ounces, updateModel.HalfLiras);
            if (result)
            {
                return Ok("Money reduced from wallet successfully.");
            }
            return BadRequest("Failed to reduce money from wallet.");
        }

        [HttpGet("Getallactions/{walletID}")]
        public IActionResult GetAllActionsOnWallet(string walletID)
        {
            try
            {
                var actions = _walletService.GetAllActionsOnWallet(walletID);

                if (actions == null || actions.Count == 0)
                {
                    return NotFound(new { message = "No actions found for this wallet." });
                }

                return Ok(actions);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllActionsOnWallet: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving wallet actions." });
            }
        }

        [HttpGet("GetIncomes/{walletID}")]
        public IActionResult GetIncomes(string walletID)
        {
            try
            {
                int incomeCount = _walletService.GetNumberOfIncomes(walletID); // or your BLL layer
                return Ok(new { incomeCount });
            }
            catch (Exception ex)
            {
                return BadRequest("Error getting incomes: " + ex.Message);
            }
        }

        [HttpGet("GetDecomes/{walletID}")]
        public IActionResult GetDecomes(string walletID)
        {
            try
            {
                int decomeCount = _walletService.GetNumberOfDecomes(walletID); // or your BLL layer
                return Ok(new { decomeCount });
            }
            catch (Exception ex)
            {
                return BadRequest("Error getting decomess: " + ex.Message);
            }
        }

    }

    public class WalletUpdateModel
    {
        public int Liras { get; set; }
        public int Ounces { get; set; }
        public int HalfLiras { get; set; }
    }
}

