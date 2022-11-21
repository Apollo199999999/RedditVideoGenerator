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
using System.IO;
using Reddit;
using Reddit.Controllers;
using RedditVideoGenerator.Controls;
using System.CodeDom;
using System.Globalization;
using static RedditVideoGenerator.TypeExtensions;

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
                    false                                   // Whether to change accents automatically
                );
                Wpf.Ui.Appearance.Accent.Apply(Color.FromRgb(255, 69, 0));
            };
        }

        private void ConsoleOutput_Loaded(object sender, RoutedEventArgs e)
        {
            //call main function which is the entry point of the video generation
            Main();
        }

        #region Helper functions

        public void SaveControlAsImage(Control control, string path)
        {
            var viewbox = new Viewbox();
            viewbox.Child = control;
            viewbox.Measure(control.RenderSize);
            viewbox.Arrange(new Rect(new Point(0, 0), control.RenderSize));
            viewbox.UpdateLayout();

            // RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.DesiredSize.Width, (int)chart.DesiredSize.Height, 96, 96, PixelFormats.Pbgra32);
            var rtb = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Default);
            rtb.Render(control);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = File.OpenWrite(path))
            {
                encoder.Save(stream);
            }

            viewbox.Child = null;
        }

        #endregion

        #region Main function

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

            //GET COMMENTS
            //get id of random top monthly post
            string TopPostID = redditFunctions.GetRandomTopMonthlyPostID();

            //get top comments from said post
            List<Comment> comments = redditFunctions.GetPostTopComments(TopPostID);

            //init new comment card
            CommentCard commentCard = new CommentCard();
            commentCard.PostSubredditText.Text = "r/" + AppVariables.SubReddit;
            commentCard.PostTitleText.Text = AppVariables.PostTitle;

            //iterate through comments and generate comment cards
            foreach (Comment comment in comments)
            {
                if (comment.Author != null && comment.Body != null && comment.Body != "[deleted]")
                {
                    commentCard.CommentAuthorText.Text = "u/" + comment.Author;
                    commentCard.CommentBodyText.Html = comment.BodyHTML.Replace('<','[').Replace('>',']');
                    commentCard.CommentUpvoteCountText.Text = comment.UpVotes.ToKMB() + " upvotes";
                    commentCard.CommentDateText.Text = comment.Created.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm");
                    SaveControlAsImage(commentCard, Path.Combine(AppVariables.FramesDirectory, comment.Id + ".png"));
                }
            }

        }

        #endregion
    }
}
