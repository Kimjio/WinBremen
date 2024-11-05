using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WinBremen.Models.Nintendo.BaaS;
using WinBremen.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web.Http;
using HttpClient = System.Net.Http.HttpClient;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinBremen
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppInstance.GetCurrent().Activated += OnActivated;
        }

        private HttpClient client = new();

        private void OnActivated(object sender, AppActivationArguments args)
        {
            ExtendedActivationKind kind = args.Kind;
            var data = (ProtocolActivatedEventArgs)args.Data;
            var query = HttpUtility.ParseQueryString(data.Uri.Fragment[1..]);
            Debug.WriteLine(data.ToString());
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            var packageName = "com.nintendo.znba";
            var sha1String = "68d037d7a44e81290873956096bd7dcaa68ca29e";

            var isu = $"{packageName}:{sha1String}";

            var hash = HashUtils.GetHash(isu, 8);

            var jwt = JwtBuilder.Create()
                .Issuer(isu)
                .IssuedAt(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000)
                .Audience("https://251737943c34c5e6ae451f19ff36c2bb.baas.nintendo.com")
                .WithSecret(Encoding.UTF8.GetBytes(hash))
                .WithAlgorithm(new HMACSHA256Algorithm())
                .Encode();

            LoginRequest req = new()
            {
                assertion = jwt,
                osType = LoginRequest.OSType.Android
            };

            var localTimeZone = TimeZoneInfo.Local;
            req.timeZone = localTimeZone.Id;
            req.timeZoneOffset = (long)localTimeZone.BaseUtcOffset.TotalMilliseconds;

            BassLogin(req);
            NALogin();

            myButton.Content = RandomUtils.GenerateRandomString(50);
        }

        private async void BassLogin(LoginRequest req)
        {
            JsonContent content = JsonContent.Create(req);

            try
            {
                var res = await client.PostAsync("https://251737943c34c5e6ae451f19ff36c2bb.baas.nintendo.com/core/v1/gateway/sdk/login", content);
                if (res.IsSuccessStatusCode)
                {
                    var cont = (LoginResponse)await res.Content.ReadFromJsonAsync(typeof(LoginResponse));
                    Debug.WriteLine(cont.idToken);
                }
                else
                {
                    var cont = (Error)await res.Content.ReadFromJsonAsync(typeof(Error));
                    Debug.WriteLine(cont.type);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }

        private void NALogin()
        {
            var state = RandomUtils.GenerateRandomString(50);
            var codeChallenge = new JwtBase64UrlEncoder().Encode(SHA256.HashData(Encoding.UTF8.GetBytes(state)));

            Debug.WriteLine(state);
            Debug.WriteLine(codeChallenge);

            // LoginWindow loginWindow = new();

            // loginWindow.LoginUrl = "";

            // loginWindow.Activate();
            // loginWindow.AppWindow.Resize(new(900, 900));

            Process.Start(new ProcessStartInfo($"https://accounts.nintendo.com/connect/1.0.0/authorize?state={state}&redirect_uri=npfa9b03ca3519e14f4%3A%2F%2Fauth&client_id=a9b03ca3519e14f4&lang={CultureInfo.CurrentCulture.Name}&scope=openid+user+user.birthday+user.mii+user.screenName&response_type=session_token_code&session_token_code_challenge={codeChallenge}&session_token_code_challenge_method=S256") { UseShellExecute = true });
        }
    }
}
