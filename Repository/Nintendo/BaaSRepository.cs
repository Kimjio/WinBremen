using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WinBremen.Models.Nintendo;
using WinBremen.Models.Nintendo.BaaS;
using WinBremen.Utils;
using Windows.Media.Protection.PlayReady;

namespace WinBremen.Repository.Nintendo
{
    class BaaSRepository
    {
        private static readonly string BASE_URL = "https://251737943c34c5e6ae451f19ff36c2bb.baas.nintendo.com";

        public static async Task<LoginResponse> Login(LoginRequest request)
        {
            JsonContent content = JsonContent.Create(request);

            var res = await HttpUtils.client.PostAsync($"{BASE_URL}/core/v1/gateway/sdk/login", content);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<LoginResponse>();
            }
            else
            {
                var error = await res.Content.ReadFromJsonAsync<Error>();
                throw new Exception(error.title);
            }
        }

        public static async Task<LoginResponse> Federation(FederationRequest request)
        {
            JsonContent content = JsonContent.Create(request);

            var res = await HttpUtils.client.PostAsync($"{BASE_URL}/core/v1/gateway/sdk/federation", content);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<LoginResponse>();
            }
            else
            {
                var error = await res.Content.ReadFromJsonAsync<Error>();
                throw new Exception(error.detail);
            }
        }
    }
}
