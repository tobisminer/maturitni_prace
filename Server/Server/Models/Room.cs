using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string name { get; set; }
        public string log { get; set; }
        public DateTime created_at { get; set; }


    }
    public class RoomDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string log { get; set; }
        public DateTime created_at { get; set; }
    }
}
