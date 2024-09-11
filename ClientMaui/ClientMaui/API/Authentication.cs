using ClientMaui.Entities;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace ClientMaui.API
{
    internal class Authentication(Endpoint endpoint)
    {

        public static string? Token { get; set; } = "";

        public async void Register(string username, string password)
        {
            var user = new User
            {
                username = username,
                password = password
            };
            var response = await endpoint.Request("api/register", method: Method.Post, body: JsonConvert.SerializeObject(user));
            if (response.StatusCode != HttpStatusCode.OK) return;
            Login(username, password);
        }
        public async void Login(string username, string password)
        {
            var user = new User
            {
                username = username,
                password = password
            };
            var response = await endpoint.Request("api/login", method: Method.Post, body: JsonConvert.SerializeObject(user));
            if (response.StatusCode != HttpStatusCode.OK) return;
            Token = response.Content;

        }

        public async Task RenewToken()
        {
            var response = await endpoint.Request("api/renew");
            if (response.StatusCode != HttpStatusCode.OK) return;
            Token = response.Content;
        }



    }
}
