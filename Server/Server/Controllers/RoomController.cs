using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/room")]
    [ApiController]

    public class RoomController(ApplicationDbContext db) : ControllerBase
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
            var rooms = db.Rooms.ToList().Select(room => new RoomDTO { id = room.id, created_at = room.created_at, is_full = room.is_full }).ToList();

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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Connect(int id)
        {
            if (id < 0)
            {
                return BadRequest("Invalid ID");
            }
            if (db.Rooms.Find(id) == null)
            {
                return NotFound("Not Found");
            }

            var room = db.Rooms.Find(id)!;
            if (room.is_full != null && (bool)room.is_full)
            {
                return BadRequest("Room is full");
            }

            var stringIdentification = RandomGenerator.generateRandomString();

            if (room.key_person_1 == null)
            {
                room.key_person_1 = stringIdentification;
            }
            else if (room.key_person_2 == null)
            {
                room.key_person_2 = stringIdentification;
                room.is_full = true;
            }
            db.SaveChanges();
            return Ok(stringIdentification);
        }

        [HttpPost("sendMessage/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> Message(int id, [FromBody] string message, [FromHeader] string identification)
        {
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

            if (identification != room.key_person_1 && identification != room.key_person_2)
            {
                return Unauthorized("Invalid Identification");
            }

            room.Messages.Add(new Message
            {
                message = message,
                sender = identification,
                send_at = DateTime.Now
            });

            db.SaveChanges();
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
