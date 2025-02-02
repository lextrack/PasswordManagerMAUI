using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace PasswordManager
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

#if ANDROID
            // Avoid screenshots
            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
#endif
        }

        protected override void OnPause()
        {
            base.OnPause();

#if ANDROID
            // Try to occult the app
            if (!IsFinishing)
            {
                MoveTaskToBack(true);
            }
#endif
        }
    }
}