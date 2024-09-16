using ClientMaui.Entities.Room;

namespace ClientMaui.Widgets;

public partial class RoomListWidget : ContentView
{
    private Room room;
    public RoomListWidget(Room room)
    {
        InitializeComponent();

        this.room = room;
        RoomHeader.Text += room.id + " - ";
        int personCount = GetNumberOfPerson();
        RoomHeader.Text += personCount switch
        {
            1 => "👤",
            _ => "\ud83e\udeb9"
        };
        ConnectButton.IsEnabled = room.can_connect;
    }

    private int GetNumberOfPerson()
    {
        return room.key_person_1 != null ? 1 : 0;
    }
}