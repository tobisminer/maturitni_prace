using ClientMaui.API;
using ClientMaui.Entities.Room;
using ClientMaui.Pages;
using ClientMaui.Widgets;
using Newtonsoft.Json;
using System.Net;

namespace ClientMaui;

public partial class RoomSelect
{
    private readonly Endpoint _endpoint;
    public RoomSelect(Endpoint endpoint)
    {
        InitializeComponent();
        _endpoint = endpoint;
    }
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            RoomList.Children.Clear();
            var response = await _endpoint.Request(APIEndpoints.RoomEndpoints.RoomList);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
                return;
            }

            var rooms = JsonConvert.DeserializeObject<List<Room>>(response.Content!);

            if (rooms == null) return;

            foreach (var room in rooms)
            {
                //Pøidání místnosti do listu
                var roomWidget = new RoomListWidget(room);

                roomWidget.ConnectButton.Clicked += async (_, _) =>
                {
                    await Navigation.PushAsync(new ChatRoom(_endpoint,
                                                    room));
                };

                RoomList.Children.Add(roomWidget);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void CreateRoomButton_OnClicked(object? sender, EventArgs e)
    {
        var page = new RoomCreate(_endpoint);
        Navigation.PushAsync(page);
    }
}