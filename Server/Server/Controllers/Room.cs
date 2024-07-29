using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class Room : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public Room(ApplicationDbContext db)
        {
            _db = db;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<RoomDTO>> List()
        {
            return Ok(_db.Rooms.ToList());
        }
        [Route("delete")]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Delete()
        {
            _db.Rooms.RemoveRange(_db.Rooms);
            _db.SaveChanges();
            return Ok("Deleted");
        }

    }

    
}
