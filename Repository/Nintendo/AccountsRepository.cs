using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WinBremen.Models.Nintendo;
using WinBremen.Models.Nintendo.Accounts;
using WinBremen.Utils;

namespace WinBremen.Repository.Nintendo
{
    class AccountsRepository
    {
        private static readonly string BASE_URL = "https://accounts.nintendo.com";

        public static async Task<SessionTokenResponse> SessionToken(string client_id, string session_token_code, string session_token_code_verifier)
        {
            FormUrlEncodedContent content = new([
                new("client_id", client_id),
                new("session_token_code", session_token_code),
                new("session_token_code_verifier", session_token_code_verifier)
            ]);

            var res = await HttpUtils.client.PostAsync($"{BASE_URL}/connect/1.0.0/api/session_token", content);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<SessionTokenResponse>();
            }
            else
            {
                var error = await res.Content.ReadFromJsonAsync<AccountError>();
                throw new Exception(error.error_description);
            }
        }

        public static async Task<TokenResponse> Token(TokenRequest request)
        {
            JsonContent content = JsonContent.Create(request);

            var res = await HttpUtils.client.PostAsync("https://api.accounts.nintendo.com/1.0.0/gateway/sdk/token", content);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<TokenResponse>();
            }
            else
            {
                var error = await res.Content.ReadFromJsonAsync<Error>();
                throw new Exception(error.detail);
            }
        }
    }
}
