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

            Loaded += (sender, args) =>
            {
                //set text of version text
                VersionText.Text = "Version: " + AppVariables.AppVersion;

                //automatic theme switcher
                Wpf.Ui.Appearance.Watcher.Watch(
                    this,                                  // Window class
                    Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                    false                                   // Whether to change accents automatically
                );
                Wpf.Ui.Appearance.Accent.Apply(Color.FromRgb(255, 69, 0));
            };
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //close this window
            this.Close();
        }
    }
}
