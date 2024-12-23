using ClientMaui.Entities;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace ClientMaui.API
{
    /*
     * This class is responsible for handling the authentication of the user
     */
    internal class Authentication(Endpoint endpoint)
    {
        public static string? Token { get; set; } = "";

        public async Task<bool> Register(string username, string password)
        {
            var user = new User
            {
                username = username,
                password = password
            };
            var response = await endpoint.Request(APIEndpoints.UserEndpoints.Register, method: Method.Post, body: JsonConvert.SerializeObject(user));
            if (response.StatusCode != HttpStatusCode.OK) return false;
            return await Login(username, password);
        }
        public async Task<bool> Login(string username, string password)
        {
            var user = new User
            {
                username = username,
                password = password
            };
            var response = await endpoint.Request(APIEndpoints.UserEndpoints.Login, method: Method.Post, body: JsonConvert.SerializeObject(user));
            if (response.StatusCode != HttpStatusCode.OK) return false;
            Token = response.Content.Replace("\"", "");

            // save username so we can use it later
            endpoint.username = username;
            return true;
        }




        public async Task RenewToken()
        {
            var response = await endpoint.Request(APIEndpoints.UserEndpoints.Renew);
            if (response.StatusCode != HttpStatusCode.OK) return;
            Token = response.Content;
        }
    }
}
