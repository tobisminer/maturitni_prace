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
        public ActionResult<List<RoomDTO>> List()
        {
            var rooms = db.Rooms.ToList().Select(room => new RoomDTO
            {
                id = room.id,
                created_at = room.created_at,
                is_full = room.is_full,
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
            if (id < 0)
            {
                return BadRequest("Invalid ID");
            }
            if (db.Rooms.Find(id) == null)
            {
                return NotFound("Not Found");
            }

            db.Rooms.Remove(db.Rooms.Find(id));
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
            if (id < 0)
            {
                return BadRequest("Invalid ID");
            }
            if (db.Rooms.Find(id) == null)
            {
                return NotFound("Not Found");
            }
            if(authorization == "")
            {
                return Unauthorized("Invalid Authentication");
            }

            var room = db.Rooms.Find(id)!;
            if (room.is_full != null && (bool)room.is_full)
            {
                return BadRequest("Room is full");
            }
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            if (identification == null)
            {
                return Unauthorized("Invalid Authentication");
            }

            if (room.key_person_1 == null)
            {
                room.key_person_1 = identification;
            }
            else if (room.key_person_2 == null)
            {
                room.key_person_2 = identification;
                room.is_full = true;
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
            if (id < 0)
            {
                return BadRequest("Invalid ID");
            }
            if (db.Rooms.Find(id) == null)
            {
                return NotFound("Not Found");
            }

            var room = db.Rooms.Find(id)!;
            if (room.key_person_1 == null || room.key_person_2 == null)
            {
                return BadRequest("Room is not full");
            }
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            if (identification == null)
            {
                return Unauthorized("Invalid Authentication");
            }

            if (identification != room.key_person_1 && identification != room.key_person_2)
            {
                return Unauthorized("Invalid Identification");
            }

            var messageObject = new Message
            {
                message = message.message,
                sender = identification,
                send_at = DateTime.Now
            };

            room.Messages.Add(messageObject);

            db.SaveChanges();

            //convert messageObject to string variable

            var messageString = JsonConvert.SerializeObject(messageObject);

            

            newMessageHub.Clients.Client(NewMessageHub.connectionToken[room.key_person_1]).SendAsync("ReceiveMessage", messageString);
            newMessageHub.Clients.Client(NewMessageHub.connectionToken[room.key_person_2]).SendAsync("ReceiveMessage", messageString);
            return Ok("Message sent");
        }

        [HttpGet("messageList/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Message>> GetMessages(int id, [FromHeader] string identification)
        {
            if (id < 0)
            {
                return BadRequest("Invalid ID");
            }
            if (db.Rooms.Find(id) == null)
            {
                return NotFound("Not Found");
            }

            var room = db.Rooms.Include(room => room.Messages).FirstOrDefault(room => room.id == id);
            if (room.key_person_1 == null || room.key_person_2 == null)
            {
                return BadRequest("Room is not full");
            }

            if (identification != room.key_person_1 && identification != room.key_person_2)
            {
                return Unauthorized("Invalid Identification");
            }

            return Ok(room.Messages);
        }


    }


}
