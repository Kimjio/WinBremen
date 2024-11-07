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
using Microsoft.UI.Xaml.Media.Imaging;
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
using WinBremen.Models.Nintendo.Accounts;
using WinBremen.Models.Nintendo.BaaS;
using WinBremen.Models.Nintendo.Music;
using WinBremen.Repository.Nintendo;
using WinBremen.Utils;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Web.Http;
using WinRT;
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
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
        }

        private HttpClient client = new();

        private void OnActivated(object sender, AppActivationArguments args)
        {
            ExtendedActivationKind kind = args.Kind;
            var data = (ProtocolActivatedEventArgs)args.Data;
            var query = HttpUtility.ParseQueryString(data.Uri.Fragment[1..]);
            Debug.WriteLine(query);
            Login(query["session_token_code"], query["state"]);
        }

        private void NavViewTitleBar_BackRequested(TitleBar sender, object args)
        {
            /*if (NavFrame.CanGoBack)
            {
                NavFrame.GoBack();
            }*/
        }

        private void NavViewTitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            NALogin();

            myButton.Content = RandomUtils.GenerateRandomString(50);
        }

        private NintendoAccount account;

        private NXBaasUserInfo user;

        private async void Login(string session_token_code, string session_token_code_verifier)
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
            req.locale = CultureInfo.CurrentCulture.Name;

            var res = await BaaSRepository.Login(req);

            var sessionToken = await AccountsRepository.SessionToken("a9b03ca3519e14f4", session_token_code, session_token_code_verifier);

            var token = await AccountsRepository.Token(new Models.Nintendo.Accounts.TokenRequest() { client_id = "a9b03ca3519e14f4", session_token = sessionToken.session_token });

            account = token.user;

            FederationRequest req1 = new()
            {
                assertion = jwt,
                osType = FederationRequest.OSType.Android,
                idpAccount = new() { idp = "nintendoAccount", idToken = token.idToken },
                deviceAccount = res.createdDeviceAccount,
                previousUserId = res.user.id,
                sessionId = res.sessionId
            };

            var federation = await BaaSRepository.Federation(req1);

            var mToken = await MusicRepository.Token(federation.idToken, new() { baasUserID = federation.user.id, lang = CultureInfo.CurrentCulture.Name });

            user = mToken.user;

            DispatcherQueue.TryEnqueue(() =>
            {
                Nickname.Text = user.nickname;
                ScreenName.Text = account.screenName;
                AccountPicture.ProfilePicture = new BitmapImage(new(mToken.user.thumbnailURL));
                AccountPictureLarge.ProfilePicture = new BitmapImage(new(mToken.user.thumbnailURL));
            });

            Debug.WriteLine(mToken.token);

            Windows.Web.Http.HttpClient httpClient = new();

            httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Cookie", $"X-Nintendo-Media-EdgeToken={mToken.edgeToken}");

            var res1 = await AdaptiveMediaSource.CreateFromUriAsync(new($"https://media-assets.m.nintendo.com/basic/tracks/73f132bb-c870-4300-b572-1703ac4722de/packaged/c926f6da-229e-446b-9a0f-ead4a06e27a8/1e420927fef0113d02d6e4e12bde392600c7961115d066b9d976335a09ccbbb5/c/master.mpd?country={RegionInfo.CurrentRegion.Name}"),
                httpClient);
            //var res2 = await AdaptiveMediaSource.CreateFromUriAsync(new($"https://media-assets.m.nintendo.com/basic/tracks/73f132bb-c870-4300-b572-1703ac4722de/packaged/c926f6da-229e-446b-9a0f-ead4a06e27a8/1e420927fef0113d02d6e4e12bde392600c7961115d066b9d976335a09ccbbb5/i1/master.mpd?country={RegionInfo.CurrentRegion.Name}"),
            //    httpClient);
            //var res3 = await AdaptiveMediaSource.CreateFromUriAsync(new($"https://media-assets.m.nintendo.com/basic/tracks/73f132bb-c870-4300-b572-1703ac4722de/packaged/c926f6da-229e-446b-9a0f-ead4a06e27a8/1e420927fef0113d02d6e4e12bde392600c7961115d066b9d976335a09ccbbb5/i3/master.mpd?country={RegionInfo.CurrentRegion.Name}"),
            //    httpClient);

            if (res1.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    var ams = res1.MediaSource;
                    ams.AdvancedSettings.AllSegmentsIndependent = true;
                    //res2.MediaSource.AdvancedSettings.AllSegmentsIndependent = true;
                    //res3.MediaSource.AdvancedSettings.AllSegmentsIndependent = true;

                    mediaPlayer.SetMediaPlayer(new MediaPlayer());

                    var PlaybackList = new MediaPlaybackList();
                    //PlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromAdaptiveMediaSource(res2.MediaSource)));
                    PlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromAdaptiveMediaSource(ams)));
                    //PlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromAdaptiveMediaSource(res3.MediaSource)));
                    PlaybackList.AutoRepeatEnabled = true;
                    PlaybackList.MaxPlayedItemsToKeepOpen = 3;
                    
                    mediaPlayer.MediaPlayer.Source = PlaybackList;

                    // mediaPlayer.MediaPlayer.IsLoopingEnabled = true;
                    // mediaPlayer.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
                    mediaPlayer.MediaPlayer.Play();

                    ams.InitialBitrate = ams.AvailableBitrates.Max();
                    //res2.MediaSource.InitialBitrate = res2.MediaSource.AvailableBitrates.Max();
                    //res3.MediaSource.InitialBitrate = res3.MediaSource.AvailableBitrates.Max();

                    ams.DownloadRequested += DownloadRequested;
                    //res2.MediaSource.DownloadRequested += DownloadRequested;
                    //res3.MediaSource.DownloadRequested += DownloadRequested;
                });
            }
        }

        private void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (sender.Position >= TimeSpan.MaxValue && (long)sender.Position.TotalMicroseconds >= 141988395)
            {
                sender.Position = TimeSpan.FromMicroseconds(8640812);
            }
        }

        private void DownloadRequested(AdaptiveMediaSource sender, AdaptiveMediaSourceDownloadRequestedEventArgs args)
        {
            if (args.ResourceType == AdaptiveMediaSourceResourceType.InitializationSegment)
            {
                string originalUri = args.ResourceUri.ToString();

                // override the URI by setting a property on the result sub object
                args.Result.ResourceUri = new Uri($"{originalUri}?country={RegionInfo.CurrentRegion.Name}");
            }

            if (args.ResourceType == AdaptiveMediaSourceResourceType.MediaSegment)
            {
                string originalUri = args.ResourceUri.ToString();

                // override the URI by setting a property on the result sub object
                args.Result.ResourceUri = new Uri($"{originalUri}?country={RegionInfo.CurrentRegion.Name}");
            }
        }

        private void NALogin()
        {
            var state = RandomUtils.GenerateRandomString(50);
            var codeChallenge = new JwtBase64UrlEncoder().Encode(SHA256.HashData(Encoding.UTF8.GetBytes(state)));

            Debug.WriteLine(state);
            Debug.WriteLine(codeChallenge);

            Process.Start(new ProcessStartInfo($"https://accounts.nintendo.com/connect/1.0.0/authorize?state={state}&redirect_uri=npfa9b03ca3519e14f4%3A%2F%2Fauth&client_id=a9b03ca3519e14f4&lang={CultureInfo.CurrentCulture.Name}&scope=openid+user+user.birthday+user.mii+user.screenName&response_type=session_token_code&session_token_code_challenge={codeChallenge}&session_token_code_challenge_method=S256") { UseShellExecute = true });
        }
    }
}
