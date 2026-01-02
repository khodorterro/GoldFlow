using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BussinnessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.SignalR;
using hub;

namespace gold_flow.Controllers
{
    //[Route("api/[controller]")]
    [Route("Transaction")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly Transactionsdata _transactionBLL = new Transactionsdata();

        private readonly IHubContext<WalletHub> _hubContext; // 👈 Injected HubContext

        private readonly IHubContext<PendingTransactionsHub> _hubPendingContext;
        public TransactionsController(IHubContext<WalletHub> hubContext, IHubContext<PendingTransactionsHub> hubPendingContext)
        {
            _hubContext = hubContext;
            _hubPendingContext = hubPendingContext;
        }

        // GET: api/transactions
        [HttpGet("GetAllTransactions")]
        public ActionResult<List<Transaction>> GetAll()
        {
            var transactions = _transactionBLL.GetAllTransactions();
            return Ok(transactions);
        }

        // GET: api/transactions/trader/{traderID}
        [HttpGet("trader/{traderID}")]
        public ActionResult<List<Transaction>> GetByTraderID(string traderID)
        {
            try
            {
                var transactions = _transactionBLL.GetTransactionByTraderID(traderID);
                if (transactions == null || transactions.Count == 0)
                    return NotFound($"No transactions found for trader ID: {traderID}");

                return Ok(transactions);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/transaction/{transactionID}
        [HttpGet("GetTransacionById/{transactionID}")]
        public ActionResult<Transaction> GetTransactionById(int transactionID)
        {
            try
            {
                var transaction = _transactionBLL.GetTransactionById(transactionID);
                if (transaction == null)
                    return NotFound();

                return Ok(transaction);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // POST: api/transactions
        [HttpPost("AddTransaction")]
        public async Task<ActionResult> Create([FromBody] Transaction transaction) // 👈 async
        {
            try
            {
                bool success = _transactionBLL.AddTransaction(transaction);
                if (success)
                {
                    await _hubContext.Clients.All.SendAsync("RefreshTraders");
                    await _hubPendingContext.Clients.All.SendAsync("RefreshTransactions");
                    await _hubPendingContext.Clients.All.SendAsync("RefreshWallet");
                    return Ok("Transaction created successfully");
                }
                else
                {
                    return BadRequest("Failed to create transaction");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Confirm/{transactionID}")]
        public async Task<ActionResult> ConfirmTransaction(int transactionID)
        {
            try
            {
                bool success = _transactionBLL.ConfirmTransaction(transactionID);
                if (success)
                {
                    await _hubPendingContext.Clients.All.SendAsync("TransactionConfirmed", transactionID);
                    return Ok("Transaction confirmed.");
                }
                else
                {
                    return BadRequest("Failed to confirm transaction.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Canceled/{transactionID}")]
        public async Task<ActionResult> CanceledTransaction(int transactionID)
        {
            try
            {
                bool success = _transactionBLL.CancelTransaction(transactionID);
                if (success)
                {
                    await _hubPendingContext.Clients.All.SendAsync("TransactionCanceled", transactionID);
                    return Ok("Transaction canceled.");
                }
                else
                {
                    return BadRequest("Failed to cancel transaction.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
