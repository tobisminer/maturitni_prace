namespace ClientMaui.Entities.Room
{
    public class RoomCreateJson
    {
        public string name { get; set; }
        public string room_type { get; set; }
        public BlockCypherMode? block_cypher_mode { get; set; }
    }
}
