using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace ClientMaui.Database.Entities
{
    public class MessageDbEntity
    {

        [PrimaryKey, AutoIncrement]
        private int Id { get; set; }
        public string Message { get; set; }
        public string EncryptedMessage { get; set; }

    }
}
