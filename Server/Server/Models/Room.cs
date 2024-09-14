using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public ICollection<Message> Messages { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

        public bool? can_connect { get; set; }

        public DateTime created_at { get; set; }


    }

    public class RoomDTO
    {
        public int id { get; set; }

        public ICollection<Message> Messages { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

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

    public class MessageDTO
    {
        public int id { get; set; }
        public string? sender { get; set; }
        public string? message { get; set; }
        public DateTime send_at { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
    }

}
