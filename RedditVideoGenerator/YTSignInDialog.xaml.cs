using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RedditVideoGenerator
{
    /// <summary>
    /// Interaction logic for YTSignInDialog.xaml
    /// </summary>
    public partial class YTSignInDialog
    {
        public YTSignInDialog()
        {
            InitializeComponent();

            //create a new dispatcher timer to update theme
            DispatcherTimer ThemeUpdater = new DispatcherTimer();
            ThemeUpdater.Interval = TimeSpan.FromMilliseconds(1000);
            ThemeUpdater.Tick += ThemeUpdater_Tick;
            ThemeUpdater.Start();

            Loaded += (s, e) =>
            {
                ThemeUpdater_Tick(null, null);
            };
        }

        private void ThemeUpdater_Tick(object sender, EventArgs e)
        {
            Wpf.Ui.Appearance.Accent.Apply(Color.FromRgb(255, 69, 0));

            bool is_light_mode = true;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                if (v != null && v.ToString() == "0")
                    is_light_mode = false;
            }
            catch { }

            if (is_light_mode == true)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Light,     // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                    false                                   // Whether to change accents automatically
                );

                YTImage.Source = new BitmapImage(new Uri(Path.Combine(AppVariables.ResourcesDirectory, "YTLight.png")));
            }
            else if (is_light_mode == false)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                  Wpf.Ui.Appearance.ThemeType.Dark,      // Theme type
                  Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                  false                                  // Whether to change accents automatically
                );

                YTImage.Source = new BitmapImage(new Uri(Path.Combine(AppVariables.ResourcesDirectory, "YTDark.png")));
            }
        }

        private void SignInBtn_Click(object sender, RoutedEventArgs e)
        {
            //unsubscribe from closing event handler
            this.Closing -= YTSignInDialog_Closing;

            //set dialog result
            this.DialogResult = true;

            //close window
            this.Close();
        }

        private void YTSignInDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //set dialog result
            this.DialogResult = false;
        }
    }
}
