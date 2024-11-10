using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Enums;

namespace Server.Models
{
    public class Room
    {
        public Room()
        {
            Messages = new List<Message>();
            created_at = DateTime.Now;
            can_connect = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string name { get; set; }

        public ICollection<Message> Messages { get; set; }
        public RoomType RoomType { get; set; }

        public string? cryptography_key { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

        public string? public_key_person_1 { get; set; }
        public string? public_key_person_2 { get; set; }

        public bool? can_connect { get; set; }

        public DateTime created_at { get; set; }

    }

    public class RoomDTO
    {
        public int id { get; set; }
        public string name { get; set; }

        public ICollection<Message> Messages { get; set; }

        public string RoomType { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

        public string? public_key_person_1 { get; set; }
        public string? public_key_person_2 { get; set; }

        public bool? can_connect { get; set; }

        public DateTime created_at { get; set; }
    }

    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? sender { get; set; }
        public string? message { get; set; }
        public DateTime send_at { get; set; }
    }

    public class RoomCreation
    {
        public string name { get; set; }
        public string room_type { get; set; }
    }
}