using AdventureWorksAPIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdventureWorksAPIs.Controllers
{
    public class IdentityController : Controller
    {
        private const string TokenSecret = "MoveThisSecretKeyToAnotherFileOrService!123";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);
        private const bool HashOverride = true; // change this later


        private readonly AdventureWorksContext _context;
        public IdentityController(AdventureWorksContext context)
        {
            _context = context;
        }

        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody]TokenGenerationRequest request)
        {
            var emailEntry = _context.EmailAddresses
                .FromSqlRaw("EXEC GetEmailEntryByEmail @Email = {0}", request.Email)
                .AsEnumerable()
                .FirstOrDefault();

            if (emailEntry == null)
            {
                return Unauthorized("Email not found.");
            }

            var personEntry =  _context.PersonWithPasswordDTOs
                .FromSqlRaw("EXEC GetPersonWithPasswordByBusinessEntityId @BusinessEntityId = {0}", emailEntry.BusinessEntityId)
                .AsEnumerable()
                .FirstOrDefault();

            //var emailEntry = await _context.EmailAddresses
            //    .Where(e => e.EmailAddress1 == request.Email)
            //    .FirstOrDefaultAsync();

            //if (emailEntry == null)
            //{
            //    return Unauthorized("Email not found.");
            //}

            //var personEntry = await _context.People
            //    .Include(p => p.Password)
            //    .Where(p => p.BusinessEntityId == emailEntry.BusinessEntityId)
            //    .FirstOrDefaultAsync();

            if (personEntry == null || personEntry.PasswordHash == null)
            {
                return Unauthorized("User not found or password not set.");
            }

            if (!VerifyPassword(request.Password, personEntry.PasswordHash, personEntry.PasswordSalt, HashOverride))
            {
                return Unauthorized("Invalid password.");
            }

            var isEmployee = personEntry.PersonType == "EM";

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, request.Email),
                new(JwtRegisteredClaimNames.Email, request.Email),
                new("businessEntityId", personEntry.BusinessEntityId.ToString()),
                new("employee", isEmployee.ToString().ToLower())
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = "localhost:5044",
                Audience = "localhost:5044",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return Ok(jwt);

        }

        private bool VerifyPassword(string password, string passwordHash, string passwordSalt, bool hashOverride)
        {
            if(hashOverride)
            {
                return password == passwordHash;
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash) || string.IsNullOrEmpty(passwordSalt))
            {
                return false;
            }
            var saltBytes = Convert.FromBase64String(passwordSalt);
            using var hmac = new System.Security.Cryptography.HMACSHA256(saltBytes);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHashBase64 = Convert.ToBase64String(computedHash);
            return computedHashBase64 == passwordHash;
        }
    }
}
