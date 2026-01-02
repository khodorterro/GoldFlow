using BussinnessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gold_flow.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/request")]
    [ApiController]
    public class requestController : ControllerBase
    {
        private readonly request _bll;

        public requestController()
        {
            _bll = new request();
        }

        // POST api/requesttobeauser/add
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AddRequestToBeAUser([FromBody] PersonDTO personDTO, [FromQuery] string traderID, [FromQuery] string password)
        {
            try
            {
                if (personDTO == null || string.IsNullOrEmpty(traderID) || string.IsNullOrEmpty(password))
                {
                    return BadRequest("Invalid input data.");
                }

                bool isAdded = _bll.AddRequestToBeAUser(personDTO, traderID, password);
                if (isAdded)
                {
                    return Ok("Request added successfully.");
                }

                return StatusCode(400, "you already have a  request or you are a user .");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // GET api/requesttobeauser/validate
        [HttpGet("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ValidateRequest([FromQuery] string traderID)
        {
            try
            {
                if (string.IsNullOrEmpty(traderID))
                {
                    return BadRequest("Trader ID is required.");
                }

                bool isValid = _bll.IsValidRequest(traderID);
                if (isValid)
                {
                    return Ok("Request is valid.");
                }

                return NotFound("Request not valid.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // POST api/requesttobeauser/acceptreject
        [HttpPost("acceptreject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AcceptRejectRequest([FromQuery] string traderID)
        {
            try
            {
                if (string.IsNullOrEmpty(traderID))
                {
                    return BadRequest("Trader ID is required.");
                }

                bool isAccepted = _bll.AcceptRejectRequest(traderID);
                if (isAccepted)
                {
                    return Ok("Request processed successfully.");
                }

                return StatusCode(500, "Failed to process the request.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetAllRequests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<object>> GetAllRequests()
        {
            try
            {
                var requests = _bll.GetAllRequests();

                // Map result to a clean JSON object list
                var result = requests.Select(r => new
                {
                    TraderID = r.TraderID,
                    PersonID = r.PersonID
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching requests: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching requests.");
            }
        }
    }
}
