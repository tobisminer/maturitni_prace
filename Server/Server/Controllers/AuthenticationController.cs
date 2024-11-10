using Microsoft.AspNetCore.Mvc;
using Server.Cryptography;
using Server.Data;
using Server.Models;


namespace Server.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController(ApplicationDbContext db) : ControllerBase
    {
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Register(UserRegister register)
        {
            //Zkrontrolování vstupu
            if (register.username == null || register.password == null)
            {
                return BadRequest("Invalid input");
            }
            if (db.UserLogins.Any(user => user.username == register.username))
            {
                return BadRequest("Username already exists");
            }
            //Převod na hash, pro bezpečné uložení
            var hash = LocalHash.Sha256(register.password);
            var identifier = RandomGenerator.GenerateRandomString();
            var user = new UserLogin
            {
                username = register.username,
                passwordHash = hash,
                identifier = identifier
            };

            db.UserLogins.Add(user);
            db.SaveChanges();
            return Ok("User registered");
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Login(UserRegister login)
        {
            //Zkontrolování vstupu
            if (login.username == null || login.password == null)
            {
                return BadRequest("Invalid input");
            }
            //Převod na hash, abychom mohli porovnat s uloženým
            var hash = LocalHash.Sha256(login.password);
            var user = db.UserLogins.FirstOrDefault(user => user.username == login.username && user.passwordHash == hash);
            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }
            //Kontrola jestli uživatel už nemá vystavený platný token
            var token = db.Tokens.FirstOrDefault(token => token.identifier == user.identifier)?.token;
            if (token != null)
            {
                return Ok(token);
            }
            //Generace nového tokenu
            token = RandomGenerator.GenerateRandomString();
            var tokenModel = new Token
            {
                token = token,
                identifier = user.identifier
            };
            db.Tokens.Add(tokenModel);
            db.SaveChanges();
            return Ok(token);
        }

        [HttpGet("renew")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Renew([FromHeader] string authorization)
        {
            authorization = authorization.Replace("Bearer ", "");
            var token = db.Tokens.FirstOrDefault(token => token.token == authorization);
            if (token == null)
            {
                return BadRequest("Invalid token");
            }
            token.token = RandomGenerator.GenerateRandomString();
            token.created_at = DateTime.Now;
            db.SaveChanges();
            return Ok(token.token);

        }
    }
}
