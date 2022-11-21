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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Reddit;
using Reddit.Controllers;

namespace RedditVideoGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                    this,                                  // Window class
                    Wpf.Ui.Appearance.BackgroundType.Mica, // Background type
                    true                                   // Whether to change accents automatically
                );
            };
        }

        private void ConsoleOutput_Loaded(object sender, RoutedEventArgs e)
        {
            //call main function which is the entry point of the video generation
            Main();
        }

        public async void Main()
        {
            //show some initialization text
            ConsoleOutput.AppendText("> Initializing RedditVideoGenerator...\r\n");

            //store MainWindow as an instance in AppVariables
            AppVariables.mainWindow = this;

            //create neccessary directories
            Directory.CreateDirectory(AppVariables.WorkingDirectory);
            Directory.CreateDirectory(AppVariables.FramesDirectory);

            //allow some time in case some libraries/code hasn't loaded yet
            await Task.Delay(1000);

            ConsoleOutput.AppendText("> Intialization complete \r\n");

            //wait a while
            await Task.Delay(100);

            ConsoleOutput.AppendText("> Getting random top monthly post from r/" + AppVariables.SubReddit + "\r\n");

            //wait a while
            await Task.Delay(500);

            //start redditfunctions
            RedditFunctions redditFunctions = new RedditFunctions();
            //get id of random top monthly post
            string TopPostID = redditFunctions.GetRandomTopMonthlyPostID();
            //get top comments from said post
            List<Comment> comments = redditFunctions.GetPostTopComments(TopPostID);
        }
    }
}
