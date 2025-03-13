using ClientMaui.Entities.Room;

namespace ClientMaui.Widgets;

public partial class RoomListWidget : ContentView
{
    // Vytváří widget pro jednotlivé místnosti
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
            2 => "👥",
            _ => "\ud83e\udeb9"
        };
        RoomSecurity.Text = $"{room.RoomType} ({BlockCypherModeHelper.ConvertToString(room.BlockCypherMode ?? BlockCypherMode.None)})";
        ConnectButton.IsEnabled = room.can_connect;
    }

    private int GetNumberOfPerson()
    {
        var personCount = 0;
        if (room.key_person_1 != null)
        {
            personCount++;
        }
        if (room.key_person_2 != null)
        {
            personCount++;
        }
        return personCount;
    }
}