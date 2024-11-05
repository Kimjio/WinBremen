using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;

namespace WinBremen
{
    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            bool isRedirect = DecideRedirection().GetAwaiter().GetResult();

            if (!isRedirect)
            {
                Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(
                        DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    _ = new App();
                });
            }

            return 0;
        }

        private static async Task<bool> DecideRedirection()
        {
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
            AppInstance mainInstance = AppInstance.FindOrRegisterForKey("main");

            bool isRedirect = false;
            if (!mainInstance.IsCurrent)
            {
                isRedirect = true;
                await mainInstance.RedirectActivationToAsync(args);
            }

            return isRedirect;
        }
    }
}
