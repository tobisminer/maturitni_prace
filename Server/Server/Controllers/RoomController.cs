using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Data;
using Server.Models;
using Server.SignalR;

namespace Server.Controllers
{
    [Route("api/room")]
    [ApiController]

    public class RoomController(ApplicationDbContext db, IHubContext<NewMessageHub> newMessageHub) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Index()
        {
            return Ok();
        }


        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<RoomDTO>> List([FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var rooms = (from room in db.Rooms.ToList()
            let canConnect = room.key_person_1 == identification || room.key_person_2 == identification || room.key_person_1 == null || room.key_person_2 == null
            select new RoomDTO
            {
                id = room.id,
                created_at = room.created_at,
                can_connect = canConnect,
                key_person_1 = room.key_person_1 != null ? "Full" : null,
                key_person_2 = room.key_person_2 != null ? "Full" : null
            }).ToList();

            return Ok(rooms);
        }

        [HttpGet("listDangerous")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<Room>> ListDangerous()
        {
            return Ok(db.Rooms.Include(room => room.Messages).ToList());
        }

        [HttpDelete("delete/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Delete(int id)
        {
            //TODO: Add authentication
            var result = Validate(id);
            if (result != null)
            {
                return result;
            }

            db.Rooms.Remove(db.Rooms.Find(id)!);
            db.SaveChanges();
            return Ok("Deleted");
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<RoomDTO> Create()
        {
            var room = new Room();
            db.Rooms.Add(room);
            db.SaveChanges();

            return Ok(room);
        }

        [HttpGet("connect/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Connect(int id, [FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var room = db.Rooms.Find(id);
            var result = Validate(id, identification, true);
            if (result != null)
            {
                return result;
            }

            if (identification == room?.key_person_1 ||
                identification == room?.key_person_2)
            {
                return Ok();
            }
            
         
            if (room.key_person_1 == null)
            {
                room.key_person_1 = identification;
            }
            else if (room.key_person_2 == null)
            {
                room.key_person_2 = identification;
            }
            db.SaveChanges();
            return Ok();
        }

        [HttpPost("sendMessage/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Message(int id, [FromBody] Message message, [FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var room = db.Rooms.Find(id);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var result = Validate(id, identification, true, true);
            if (result != null)
            {
                return result;
            }
            
            var messageObject = new Message
            {
                message = message.message,
                sender = authorization,
                send_at = DateTime.Now
            };

            room.Messages.Add(messageObject);

            db.SaveChanges();

            //convert messageObject to string variable

            var messageString = JsonConvert.SerializeObject(messageObject);
            newMessageHub.SendToUser(room.key_person_1, messageString);
            newMessageHub.SendToUser(room.key_person_2, messageString);
            return Ok("Message sent");
        }

        [HttpGet("messageList/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Message>> GetMessages(int id, [FromHeader] string identification)
        {
            var room = db.Rooms.Include(room => room.Messages).FirstOrDefault(room => room.id == id);

            var result = Validate(id, identification, true, true);
            return result ?? Ok(room.Messages);
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
