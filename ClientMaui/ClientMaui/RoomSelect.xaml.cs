using ClientMaui.API;

namespace ClientMaui;

public partial class RoomSelect : ContentPage
{
    private Endpoint endpoint;
    public RoomSelect(Endpoint endpoint)
    {
        InitializeComponent();
        this.endpoint = endpoint;
    }
}