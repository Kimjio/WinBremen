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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Text;
using WinBremen.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            var state = RandomUtils.GenerateRandomString(50);

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


            myButton.Content = RandomUtils.GenerateRandomString(50);
        }
    }
}
