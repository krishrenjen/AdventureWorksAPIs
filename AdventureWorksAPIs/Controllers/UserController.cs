using AdventureWorksAPIs.DTO;
using AdventureWorksAPIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksAPIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {

        private readonly AdventureWorksContext _context;
        public UserController(AdventureWorksContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("myinfo")]
        public async Task<ActionResult<UserInfoDTO>> GetPersonalInfo()
        {
            var businessEntityIdClaim = User.Claims.FirstOrDefault(c => c.Type == "businessEntityId");

            if (businessEntityIdClaim == null || !int.TryParse(businessEntityIdClaim.Value, out var businessEntityId))
            {
                return Unauthorized("Missing or invalid businessEntityId claim.");
            }

            var param = new SqlParameter("@BusinessEntityId", businessEntityId);

            var result = await _context.UserInfoDTOs
                .FromSqlRaw("EXEC GetUserInfo @BusinessEntityId", param)
                .ToListAsync();

            if (result.Count == 0)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(result[0]);
        }
    }
}
