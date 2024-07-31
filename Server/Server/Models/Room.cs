using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public List<Message>? message_log { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

        public bool? is_full { get; set; }

        public DateTime created_at { get; set; }


    }

    public class RoomDTO
    {
        public int id { get; set; }

        public List<MessageDTO>? message_log { get; set; }

        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }

        public bool? is_full { get; set; }

        public DateTime created_at { get; set; }
    }

    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? sender { get; set; }
        public string? message { get; set; }
    }

    public class MessageDTO
    {
        public int id { get; set; }
        public string? sender { get; set; }
        public string? message { get; set; }
    }

}
