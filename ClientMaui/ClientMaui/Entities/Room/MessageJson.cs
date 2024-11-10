namespace ClientMaui.Entities.Room
{
    public class Message
    {
        public int id { get; set; }
        public string? sender { get; set; }
        public string message { get; set; }
        public BlockCypherMode? BlockCypherMode { get; set; }
        public DateTime send_at { get; set; }

    }
}
