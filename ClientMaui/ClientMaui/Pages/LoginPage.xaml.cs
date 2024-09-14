using ClientMaui.API;

namespace ClientMaui.Pages;

public partial class LoginPage : ContentPage
{
	private readonly Endpoint _endpoint;
    private readonly Authentication _authentication;
    public LoginPage(Endpoint endpoint)
	{
		InitializeComponent();
        _endpoint = endpoint;
        _authentication = new Authentication(endpoint);
    }


    private async void RegisterBtn_OnClicked(object? sender, EventArgs e)
    { 
        var result = await _authentication.Register(UsernameEntry.Text, PasswordEntry.Text);
        if (result)
        {
            await DisplayAlert("Success", "User registered successfully!", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Error occured while registering user!", "OK");
        }
    }

    private async void LoginBtn_OnClicked(object? sender, EventArgs e)
    {
        var result = await _authentication.Login(UsernameEntry.Text, PasswordEntry.Text);
        if (result)
        {
            await Navigation.PushAsync(new RoomSelect(_endpoint));
        }
        else
        {
            await DisplayAlert("Error", "Error occured while logging in user!", "OK");
        }
       
    }
}