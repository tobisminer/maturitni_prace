using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Data;
using Server.Enums;
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
            let canConnect = room.key_person_1 == identification ||
                             room.key_person_2 == identification ||
                             room.key_person_1 == null ||
                             room.key_person_2 == null
            select new RoomDTO
            {
                id = room.id,
                name = room.name,
                created_at = room.created_at,
                RoomType = room.RoomType.ToFriendlyString(),
                can_connect = canConnect,
                key_person_1 = room.key_person_1 != null ? "Full" : null,
                key_person_2 = room.key_person_2 != null ? "Full" : null
            }).ToList();

            return Ok(rooms);
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<RoomDTO> Create([FromBody] RoomCreation roomSettings)
        {
            var type = RoomTypesExtensions.FromFriendlyString(roomSettings.room_type);
            var room = new Room
            {
                name = roomSettings.name,
                RoomType = type,
                created_at = DateTime.Now
            };
            db.Rooms.Add(room);
            db.SaveChanges();

            return Ok(room);
        }

        [HttpGet("roomTypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<RoomTypeJson>> RoomTypes()
        {
            return Ok(RoomTypesExtensions.GetAll());
        }

        [HttpGet("connect/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<int> Connect(int id, [FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var room = db.Rooms.Include(room => room.Messages).ToList().FirstOrDefault(room => room.id == id);
            var result = Validate(id, identification, true);
            if (result != null)
            {
                return result;
            }

            if (identification == room?.key_person_1 ||
                identification == room?.key_person_2)
            {
                return Ok(room!.Messages.Count);
            }
         
            if (room!.key_person_1 == null)
            {
                room.key_person_1 = identification;
            }
            else if (room.key_person_2 == null)
            {
                room.key_person_2 = identification;
            }
            db.SaveChanges();
            return Ok(room.Messages.Count);
        }


        [HttpGet("getSecretKey/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string?> GetSecretKey(int id, [FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var room = db.Rooms.Find(id);
            var result = Validate(id, identification, true, true);
            if (result != null)
            {
                return result;
            }

            if (room.RoomType.isAsymmetric())
            {
                // If the room is RSA, return the public key of the other person
                if (identification == room.key_person_1)
                {
                    return Ok(room.public_key_person_2); 
                }
                else if (identification == room.key_person_2)
                {
                    return Ok(room.public_key_person_1);
                }
                
            }

            return Ok(room.cryptography_key);
        }   

        [HttpPost("setSecretKey/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> SetSecretKey(int id, [FromBody] KeyJson key, [FromHeader] string authorization)
        {
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification = Authentication.GetIdentifierFromToken(db, authorization);
            var room = db.Rooms.Find(id);
            var result = Validate(id, identification, true, true);
            if (result != null)
            {
                return result;
            }

            if (room.RoomType.isAsymmetric())
            {
                if (identification == room.key_person_1)
                {
                    room.public_key_person_1 = key.key;
                } 
                else if (identification == room.key_person_2)
                {
                    room.public_key_person_2 = key.key;
                }
            }
            room!.cryptography_key = key.key;
            db.SaveChanges();
            return Ok("Key set");
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
            
            var messegeForSender = new Message
            {
                message = message.message,
                sender = identification,
                send_at = DateTime.Now

            };
            var messageForReceiver = new Message
            {
                message = message.message,
                sender = null,
                send_at = DateTime.Now
            };

            room!.Messages.Add(messegeForSender);

            db.SaveChanges();

            //convert messageObject to string variable

            var stringForSender = JsonConvert.SerializeObject(messegeForSender);
            var stringForReceiver = JsonConvert.SerializeObject(messageForReceiver);
            newMessageHub.SendToUser(room.key_person_1, room.key_person_1 == identification ? stringForSender : stringForReceiver);
            newMessageHub.SendToUser(room.key_person_2, room.key_person_2 == identification ? stringForSender : stringForReceiver);
            return Ok("Message sent");
        }


        [HttpGet("messageList/{id:int}/{from:int}/{to:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Message>> GetMessages(int id, int from, int to, [FromHeader] string authorization)
        {
            var room = db.Rooms.Include(room => room.Messages).FirstOrDefault(room => room.id == id);
            authorization = Authentication.GetTokenFromHeader(authorization);
            var identification =
                Authentication.GetIdentifierFromToken(db, authorization);

            var result = Validate(id, identification, true, true);
            if (result != null)
            {
                return result;
            }
            if (from < 0 || to < 0 || from > to)
            {
                return BadRequest("Invalid range");
            }
            var messages = room!.Messages.Skip(from).Take(to - from).ToList();
            //remove identification from messages that are not from the user
            foreach (var message in messages.Where(message => message.sender != identification))
            {
                message.sender = null;
            }

            return Ok(messages);
        }
        private ActionResult? Validate(int? roomId = null, string? identification = null, bool checkIdentification = false, bool checkRoomIdentification = false)
        {
            //Tato metoda kontroluje zda je vše v pořádku a v případě že není, vrátí chybovou hlášku, která
            //je pak vrácen klientovi

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
