using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RedditVideoGenerator
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            //set text of version text
            VersionText.Text = "Version " + AppVariables.AppVersion;

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
            }
            else if (is_light_mode == false)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                  Wpf.Ui.Appearance.ThemeType.Dark,      // Theme type
                  Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                  false                                  // Whether to change accents automatically
                );

            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //close this window
            this.Close();
        }
    }
}
