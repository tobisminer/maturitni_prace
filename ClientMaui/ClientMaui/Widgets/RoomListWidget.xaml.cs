using ClientMaui.Entities.Room;

namespace ClientMaui.Widgets;

public partial class RoomListWidget : ContentView
{
    private Room room;
    public RoomListWidget(Room room)
    {
        InitializeComponent();

        this.room = room;
        RoomHeader.Text = room.name;
        var personCount = GetNumberOfPerson();
        RoomHeader.Text += personCount switch
        {
            1 => "👤",
            _ => "\ud83e\udeb9"
        };
        RoomSecurity.Text =
            $"{room.RoomType} ({BlockCypherModeHelper.ConvertToString(room.BlockCypherMode ?? BlockCypherMode.None)})";
        ConnectButton.IsEnabled = room.can_connect;
    }

    private int GetNumberOfPerson()
    {
        return room.key_person_1 != null ? 1 : 0;
    }
}