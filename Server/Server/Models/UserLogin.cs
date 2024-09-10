using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class UserRegister
    {
       public string? username { get; set; }

        public string? password { get; set; }

    }



    public class UserLogin
    {
        public UserLogin()
        {
            created_at = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string? username { get; set; }

        public string? passwordHash { get; set; }

        public string? identifier { get; set; }

        public DateTime created_at { get; set; }
    }
    public class UserLoginDTO
    {
        public int id { get; set; }

        public string? username { get; set; }

        public string? passwordHash { get; set; }

        public DateTime created_at { get; set; }
    }
}
