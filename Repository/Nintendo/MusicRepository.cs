using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WinBremen.Models.Nintendo.Music;
using WinBremen.Utils;

namespace WinBremen.Repository.Nintendo
{
    class MusicRepository
    {
        private static readonly string BASE_URL = "https://api.m.nintendo.com";


        public static async Task<TokenResponse> Token(string idToken, TokenRequest request)
        {
            JsonContent content = JsonContent.Create(request);
            HttpRequestMessage requestMessage = new()
            {
                Method = HttpMethod.Post,
                Content = content,
                RequestUri = new Uri($"{BASE_URL}/rights/token"),
            };
            requestMessage.Headers.Authorization = new("Bearer", idToken);

            // content.Headers.Add("AppCheck", "");
            // content.Headers.Add("Authorization", $"Bearer {idToken}");

            var res = await HttpUtils.client.SendAsync(requestMessage);
            return await res.Content.ReadFromJsonAsync<TokenResponse>();
        }
    }
}
