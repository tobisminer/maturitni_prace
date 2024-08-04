using ClientMaui.Entities.Room;

namespace ClientMaui.Widgets;

public partial class RoomWidget : ContentView
{
    private Room room;
    public RoomWidget(Room room)
    {
        InitializeComponent();

        this.room = room;
        RoomHeader.Text += room.id + " - ";
        var personCount = GetNumberOfPerson();
        RoomHeader.Text += personCount switch
        {
            1 => "👤",
            2 => "👥",
            _ => "\ud83e\udeb9"
        };
        ConnectButton.IsEnabled = personCount < 2;
    }
    
    private int GetNumberOfPerson()
    {
        if (room.is_full) return 2;
        return room.key_person_1 != null ? 1 : 0;
    }
}