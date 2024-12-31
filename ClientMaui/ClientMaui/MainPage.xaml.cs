using ClientMaui.API;
using ClientMaui.Pages;
using System.Net;
using static ClientMaui.API.APIEndpoints;

namespace ClientMaui
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadDefault();
        }

        private void LoadDefault()
        {
            IPadressEntry.Text = Preferences.Default.Get("IPAddress", "");
            PortEntry.Text = Preferences.Default.Get("Port", "");
        }

        private async void ConnectButton_OnClicked(object? sender, EventArgs e)
        {
            try
            {
                Preferences.Default.Set("IPAddress", IPadressEntry.Text);
                Preferences.Default.Set("Port", PortEntry.Text);
                var endpoint = new Endpoint($"http://{IPadressEntry.Text}:{PortEntry.Text}");
                var response = await endpoint.Request(RoomEndpoints.Index);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    await DisplayAlert("Error", "Error occured while connecting to server, check IP address and port!", "OK");
                    return;
                }
                await Navigation.PushAsync(new LoginPage(endpoint));
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

}
