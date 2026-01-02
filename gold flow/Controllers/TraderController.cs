using System;
using BussinnessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gold_flow.Controllers
{

    [Route("api")]
    [ApiController]
    public class TraderController : ControllerBase
    {
        private readonly Trader _traderBLL = new Trader();

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var trader = _traderBLL.Login(loginRequest.TraderID, loginRequest.Password);

                if (trader != null)
                {
                    // You can return limited info if you want, not the full password
                    return Ok(new
                    {
                        TraderID = trader.TraderID,
                        PersonID = trader.PersonID
                    });
                }
                else
                {
                    return Unauthorized("Invalid TraderID or Password");
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid TraderID or Password");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("GetAllTraders")]
        public ActionResult<List<TraderDTO2>> GetAllTraders()
        {
            try
            {
                var traders = _traderBLL.GetAllTrader();
                return Ok(traders);
            }
            catch (Exception ex)
            {
                return BadRequest("Error fetching traders: " + ex.Message);
            }
        }

        [HttpPut("UpdateTraderFull")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult UpdateTraderFull([FromBody] TraderDTO3 traderDTO)
        {
            if (traderDTO == null ||
                string.IsNullOrEmpty(traderDTO.TraderID) ||
                traderDTO.PersonID <= 0 ||
                string.IsNullOrEmpty(traderDTO.Password) ||
                string.IsNullOrEmpty(traderDTO.Name) ||
                string.IsNullOrEmpty(traderDTO.Email) ||
                string.IsNullOrEmpty(traderDTO.Phonenumber))
            {
                return BadRequest("Invalid trader data provided.");
            }

            bool updated = _traderBLL.UpdateTrader(traderDTO);

            if (updated)
            {
                return Ok("Trader and Person updated successfully.");
            }
            else
            {
                return BadRequest("Failed to update Trader and Person.");
            }
        }

        [HttpGet("GetPassword")]
        public ActionResult<string> GetPassword(string traderID)
        {
            if (string.IsNullOrEmpty(traderID))
            {
                return BadRequest("Trader ID is required.");
            }

            var password = _traderBLL.GetPassword(traderID);

            if (password == null)
            {
                return NotFound($"No password found for trader ID: {traderID}");
            }

            return Ok(password);
        }
        public class LoginRequest
        {
            public string TraderID { get; set; }
            public string Password { get; set; }
        }
    }
}



