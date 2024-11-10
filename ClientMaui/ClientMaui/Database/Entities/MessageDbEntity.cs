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
