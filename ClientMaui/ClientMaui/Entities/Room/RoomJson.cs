namespace ClientMaui.Entities.Room
{
    public class Room
    {
        public int id { get; set; }
        public string name { get; set; }
        public Message[]? messages { get; set; }
        public string RoomType { get; set; }
        public BlockCypherMode? BlockCypherMode { get; set; }
        public string? key_person_1 { get; set; }
        public string? key_person_2 { get; set; }
        public bool can_connect { get; set; }
        public DateTime created_at { get; set; }
    }
}
