using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Token
    {
        [Key]
        public int id { get; set; }
        public string? token { get; set; }
        public string? identifier { get; set; }

        public bool active { get; set; } = true;
        public DateTime created_at { get; set; } = DateTime.Now;
    }
    public class TokenDTO
    {
        public int id { get; set; }
        public string? token { get; set; }
        public string? identifier { get; set; }
        public bool active { get; set; }
        public DateTime created_at { get; set; }
    }
}
