using ClientMaui.API;
using ClientMaui.Pages;

namespace ClientMaui.Widgets;

public partial class RoomCreateWidget : ContentView
{

	private readonly Endpoint _endpoint;
    public RoomCreateWidget(Endpoint endpoint)
	{
		InitializeComponent();
        _endpoint = endpoint;
    }

    private void CreateBtn_OnClicked(object? sender, EventArgs e)
    {
         var page = new RoomCreate(_endpoint);
        Navigation.PushAsync(page);
    }
}