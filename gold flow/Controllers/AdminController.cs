using BussinnessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gold_flow.Controllers
{
    [Route("api/Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController()
        {
            _adminService = new AdminService();
        }

        [HttpGet("GetAdminById", Name = "GetAdminById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AdminDTO> GetAdminById(int id)
        {
            if (id < 1)
                return BadRequest($"Not accepted ID {id}");

            var admin = _adminService.GetAdminById(id);
            if (admin == null)
                return NotFound($"Admin with ID {id} not found.");

            return Ok(admin);
        }

        [HttpPost("AddAdmin", Name = "AddAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<int> AddAdmin([FromBody] AdminDTO2 admin)
        {
            if (admin == null || string.IsNullOrWhiteSpace(admin.Name) ||
                string.IsNullOrWhiteSpace(admin.Password) ||
                string.IsNullOrWhiteSpace(admin.Email) ||
                string.IsNullOrWhiteSpace(admin.Phone))
            {
                return BadRequest("Invalid admin data.");
            }

            // ✅ Check for email conflict before attempting insert
            if (_adminService.EmailAlreadyUsed(admin.Email))
            {
                return Conflict("An account with this email already exists.");
            }

            try
            {
                int adminId = _adminService.AddAdmin(admin);
                if (adminId <= 0)
                    return StatusCode(500, "Failed to create admin.");

                return CreatedAtRoute("GetAdminById", new { id = adminId }, adminId);
            }
            catch (Exception ex)
            {
                // You can optionally log the exception here
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        [HttpPost("Login", Name = "AdminLogin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and Password must be provided.");

            var admin = _adminService.Login(request.Email, request.Password);

            if (admin != null)
            {
                return Ok(admin); // return AdminDTO
            }
            else
            {
                return Unauthorized("Invalid email or password.");
            }
        }

        [HttpPut("UpdateAdmin", Name = "UpdateAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateAdmin(int id, [FromBody] AdminDTO admin)
        {
            if (admin == null)
                return BadRequest("Admin object must be provided.");

            var updated = _adminService.UpdateAdmin(id, admin);
            if (updated)
                return Ok("Admin updated successfully.");
            else
                return NotFound($"Admin with ID {id} not found.");
        }

        [HttpDelete("DeleteAdmin", Name = "DeleteAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteAdmin(int adminId)
        {
            var result = _adminService.DeleteAdmin(adminId);

            if (result)
            {
                return Ok(new { message = "Admin deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "Admin not found or could not be deleted." });
            }
        }


        [HttpGet("GetAllAdmins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<AdminDTO>> GetAllAdmins()
        {
            try
            {
                var admins = _adminService.GetAllAdmins();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching admins: {ex.Message}");
            }
        }
        public class AdminLoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
