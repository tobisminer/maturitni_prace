using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/RSA")]
    [ApiController]
    public class RSARoomController(ApplicationDbContext db) : ControllerBase
    {
        [HttpGet("saveMessage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> List([FromHeader] string authorization, [FromBody] Message message)
        {
            /*authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var validation = Validate(message.roomId, identification, true, true);
            if (validation != null)
            {
                return validation;
            }
            var room = db.Rooms.Find(message.roomId);*/
            return Ok("Message saved");
        }
        private ActionResult? Validate(int? roomId = null, string? identification = null, bool checkIdentification = false, bool checkRoomIdentification = false)
        {
            if (roomId is < 0)
            {
                return BadRequest("Invalid ID");
            }
            var room = db.Rooms.Find(roomId);
            if (roomId != null && room == null)
            {
                return NotFound("Not Found");
            }

            if (checkIdentification && string.IsNullOrEmpty(identification))
            {
                return Unauthorized("Invalid Authentication");
            }

            if (checkRoomIdentification && (identification != room?.key_person_1 && identification != room?.key_person_2))
            {
                return Unauthorized("Invalid access to room");
            }

            return null;
        }
    }
}
