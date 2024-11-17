using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using Windows.Storage;
using Windows.Storage.Streams;
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

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["deviceAccount"] != null)
            {
                Login(null, null);
            }
        }
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

        private Models.Nintendo.Music.TokenResponse mToken;

        private NintendoAccount account;

        private NXBaasUserInfo user;

        private int MaxLoopCount = 3;
        private int LoopCount = 0;

        private readonly Windows.Web.Http.HttpClient wHttpClient = new();

        private async void Login(string session_token_code, string session_token_code_verifier)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            ApplicationDataCompositeValue deviceAccount = localSettings.Values["deviceAccount"] as ApplicationDataCompositeValue;

            LoginRequest req;

            if (deviceAccount != null)
            {
                req = new LoginRequestWithDeviceAccount()
                {
                    assertion = jwt,
                    osType = LoginRequest.OSType.Android,
                    deviceAccount = new()
                    {
                        id = deviceAccount["id"] as string,
                        password = deviceAccount["password"] as string,
                    }
                };
            }
            else
            {
                req = new()
                {
                    assertion = jwt,
                    osType = LoginRequest.OSType.Android,
                };
            }

            var localTimeZone = TimeZoneInfo.Local;
            req.timeZone = localTimeZone.Id;
            req.timeZoneOffset = (long)localTimeZone.BaseUtcOffset.TotalMilliseconds;
            req.locale = CultureInfo.CurrentCulture.Name;

            var res = await BaaSRepository.Login(req);

            var userId = res.user.id;

            if (deviceAccount == null)
            {
                deviceAccount = [];

                deviceAccount["id"] = res.createdDeviceAccount.id;
                deviceAccount["password"] = res.createdDeviceAccount.password;

                var sessionToken = await AccountsRepository.SessionToken("a9b03ca3519e14f4", session_token_code, session_token_code_verifier);

                deviceAccount["sessionToken"] = sessionToken.session_token;
                localSettings.Values["deviceAccount"] = deviceAccount;
            }

            var token = await AccountsRepository.Token(
                new Models.Nintendo.Accounts.TokenRequest()
                {
                    client_id = "a9b03ca3519e14f4",
                    session_token = deviceAccount["sessionToken"] as string
                });

            account = token.user;

            if (deviceAccount["idToken"] == null)
            {
                FederationRequest req1 = new()
                {
                    assertion = jwt,
                    osType = FederationRequest.OSType.Android,
                    idpAccount = new()
                    {
                        idp = "nintendoAccount",
                        idToken = token.idToken
                    },
                    deviceAccount = res.createdDeviceAccount,
                    previousUserId = res.user.id,
                    sessionId = res.sessionId
                };

                var federation = await BaaSRepository.Federation(req1);

                deviceAccount["idToken"] = federation.idToken;
                userId = federation.user.id;
            }

            mToken = await MusicRepository.Token(deviceAccount["idToken"] as string,
                new()
                {
                    baasUserID = userId,
                    lang = CultureInfo.CurrentCulture.Name
                });

            user = mToken.user;

            DispatcherQueue.TryEnqueue(() =>
            {
                Nickname.Text = user.nickname;
                ScreenName.Text = account.screenName;
                AccountPicture.ProfilePicture = new BitmapImage(new(mToken.user.thumbnailURL));
                AccountPictureLarge.ProfilePicture = new BitmapImage(new(mToken.user.thumbnailURL));
            });

            wHttpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Cookie", $"X-Nintendo-Media-EdgeToken={mToken.edgeToken}");

            AdaptiveMediaSourceCreationResult res1 = await AdaptiveMediaSource.CreateFromUriAsync(new($"https://media-assets.m.nintendo.com/basic/tracks/73f132bb-c870-4300-b572-1703ac4722de/packaged/c926f6da-229e-446b-9a0f-ead4a06e27a8/1e420927fef0113d02d6e4e12bde392600c7961115d066b9d976335a09ccbbb5/c/master.mpd?country={RegionInfo.CurrentRegion.Name}"), wHttpClient);

            if (res1.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                var ams = res1.MediaSource;
                ams.DownloadRequested += DownloadRequested;

                DispatcherQueue.TryEnqueue(() =>
                {
                    var mediaPlayer = new MediaPlayer()
                    {
                        Source = MediaSource.CreateFromAdaptiveMediaSource(ams)
                    };

                    var LoopStart = 8640812;
                    var LoopSize = 141988395;
                    var End = 158550666 - LoopSize;

                    PlayerSlider.Maximum = mediaPlayer.NaturalDuration.TotalMicroseconds + ((LoopSize - LoopStart) * MaxLoopCount) + End;
                    mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                    PlayerElement.SetMediaPlayer(mediaPlayer);
                });
            }
        }

        private bool MediaPlaying = false;

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                MediaPlaying = true;
                StartPlayerLoop();
            }

            if (sender.CurrentState == MediaPlayerState.Paused)
            {
                MediaPlaying = false;
            }
        }
        private void PlayerSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var LoopStart = 8640812;
            var LoopSize = 141988395;
            var End = 158550666 - LoopSize;


            LoopCount = Math.Min((int)Math.Floor((e.NewValue + (LoopCount * LoopStart)) / LoopSize), MaxLoopCount);

            var LoopedSize = LoopSize * LoopCount;// + Math.Max(LoopStart * (LoopCount - 1), 0);

            DebugText.Text = $"LoopCount: {e.NewValue + (LoopCount * LoopStart)} / {LoopSize} = {LoopCount} /// ({e.NewValue - LoopedSize}) + ({(LoopCount * LoopStart)})";
            if (LoopCount >= MaxLoopCount)
            {
                LoopedSize += End;
            }

            // Debug.WriteLine(e.NewValue - LoopedSize + (LoopCount > 0 ? 8640812 : 0));
            PlayerElement.MediaPlayer.Position = TimeSpan.FromMicroseconds(e.NewValue - LoopedSize + (LoopCount * LoopStart));
        }

        /// <summary>
        ///  TODO: Loop
        /// </summary>
        private void StartPlayerLoop()
        {
            new Thread(() =>
            {
                while (MediaPlaying)
                {
                    Thread.Sleep(1);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var Player = PlayerElement.MediaPlayer;

                        if (Player.Position < TimeSpan.MaxValue && (long)Player.Position.TotalMicroseconds >= 141988395)
                        {
                            Player.Position = TimeSpan.FromMicroseconds(8640812);
                            LoopCount++;
                        }

                        var LoopSize = 141988395 - 8640812;
                        var LoopedSize = LoopSize * LoopCount;
                        // PlayerSlider.Value = LoopedSize + Player.Position.TotalMicroseconds + (LoopCount > 0 ? 8640812 : 0);

                    });
                }
            }).Start();
        }

        private void DownloadFailed(AdaptiveMediaSource sender, AdaptiveMediaSourceDownloadFailedEventArgs args)
        {
            Debug.WriteLine("DownloadFailed");
        }

        private void DownloadRequested(AdaptiveMediaSource sender, AdaptiveMediaSourceDownloadRequestedEventArgs args)
        {
            if (args.ResourceType == AdaptiveMediaSourceResourceType.InitializationSegment)
            {
                string originalUri = args.ResourceUri.ToString();


                var newUri = new Uri($"{originalUri}?country={RegionInfo.CurrentRegion.Name}");

                // override the URI by setting a property on the result sub object
                args.Result.ResourceUri = newUri;
            }

            if (args.ResourceType == AdaptiveMediaSourceResourceType.MediaSegment)
            {
                string originalUri = args.ResourceUri.ToString();
                var newUri = new Uri($"{originalUri}?country={RegionInfo.CurrentRegion.Name}");

                // override the URI by setting a property on the result sub object
                args.Result.ResourceUri = newUri;
            }
        }

        private void NALogin()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values["deviceAccount"] == null)
            {
                var state = RandomUtils.GenerateRandomString(50);
                var codeChallenge = new JwtBase64UrlEncoder().Encode(SHA256.HashData(Encoding.UTF8.GetBytes(state)));

                Process.Start(new ProcessStartInfo($"https://accounts.nintendo.com/connect/1.0.0/authorize?state={state}&redirect_uri=npfa9b03ca3519e14f4%3A%2F%2Fauth&client_id=a9b03ca3519e14f4&lang={CultureInfo.CurrentCulture.Name}&scope=openid+user+user.birthday+user.mii+user.screenName&response_type=session_token_code&session_token_code_challenge={codeChallenge}&session_token_code_challenge_method=S256") { UseShellExecute = true });
            }
            else
            {
                Login(null, null);
            }
        }

        SpringVector3NaturalMotionAnimation _springAnimation;

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = Compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void AccountPicture_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(0.9f);

            AccountButton.StartAnimation(_springAnimation);
        }

        private void AccountButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(0.95f);

            AccountButton.StartAnimation(_springAnimation);
        }

        private void AccountButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CreateOrUpdateSpringAnimation(1f);

            AccountButton.StartAnimation(_springAnimation);
        }
    }
}
