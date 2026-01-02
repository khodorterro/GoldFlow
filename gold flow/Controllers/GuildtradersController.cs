using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BussinnessLayer;
namespace gold_flow.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/Guildtraders")]
    [ApiController]
    public class GuildtradersController : ControllerBase
    {
        [HttpPost("AddGuildTrader")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AddGuildTrader([FromBody] PersonDTO personDTO)
        {
            if (personDTO == null)
                return BadRequest("Invalid trader data.");

            bool success = Guildtraders.AddGuildTrader(personDTO);

            if (success)
                return Ok("Guild trader added successfully.");
            else
                return StatusCode(500, "Failed to add guild trader.");
        }

        // GET: api/GuildTrader/exists/TRD-20250415-001
        [HttpGet("exists/{traderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult CheckTraderIdExists(string traderId)
        {
            bool exists = Guildtraders.TraderIdExists(traderId);
            return Ok(exists);
        }

        // DELETE: api/GuildTrader/TRD-20250415-001
        [HttpDelete("Delete/{traderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteGuildTrader(string traderId)
        {
            bool deleted = Guildtraders.Deleteguildtrader(traderId);

            if (deleted)
                return Ok("Guild trader deleted.");
            else
                return NotFound("Trader not found or deletion failed.");
        }


        [HttpGet("GetAllGuildTraders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<GuildTraderDTO2>> GetAllGuildTraders()
        {
            try
            {
                var traders = Guildtraders.GetAllGuildTraders();
                return Ok(traders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
