using BussinnessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;

namespace gold_flow.Controllers
{
   // [Route("api/[controller]")]
    [Route("api/commission")]
    [ApiController]
    public class commissionController : ControllerBase
    {
        private readonly commission _manager;

        public commissionController()
        {
            _manager = new commission();
        }

        // GET: api/commission
        [HttpGet("GeALL",Name = "GetAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<CommissionDTO>> GetAll()
        {
            var commissions = _manager.GetAllCommissions();
            return Ok(commissions);
        }

        // GET: api/commission/active
        [HttpGet("Getactive",Name = "GetActive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CommissionDTO> GetActive()
        {
            var commission = _manager.GetActiveCommission();
            if (commission == null)
                return NotFound("No active commission found.");
            return Ok(commission);
        }

        // POST: api/commission
        [HttpPost("AddCommission",Name = "Add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<int> Add([FromBody] decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            var id = _manager.AddCommission(amount);
            return Ok(id);
        }

        // PUT: api/commission/deactivate/5
        [HttpPut("deactivate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Deactivate(int id)
        {
            if(id<=0)
            {
                return BadRequest("input not valid");
            }
            var result = _manager.DeactivateCommission(id);
            if (!result)
                return NotFound("Commission not found or already inactive.");
            return Ok("Commission deactivated.");
        }
        [HttpPut("activate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult activate(int id)
        {
            if (id <= 0)
            {
                return BadRequest("input not valid");
            }
            var result = _manager.activateCommission(id);
            if (!result)
                return NotFound("Commission not found or already active.");
            return Ok("Commission activated.");
        }

        // DELETE: api/commission/5
        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("input not valid");
            }
            var result = _manager.DeleteCommission(id);
            if (!result)
                return NotFound("Commission not found.");
            return Ok("Commission deleted.");
        }

    }
}
