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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Speech.Synthesis;

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
        public string StringFromRichTextBox(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }

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

            #region Intialization code

            //store MainWindow as an instance in AppVariables
            AppVariables.mainWindow = this;

            //create neccessary directories
            Directory.CreateDirectory(AppVariables.WorkingDirectory);
            Directory.CreateDirectory(AppVariables.FramesDirectory);
            Directory.CreateDirectory(AppVariables.AudioDirectory);
            Directory.CreateDirectory(AppVariables.OutputDirectory);

            //allow some time in case some libraries/code hasn't loaded yet
            await Task.Delay(1000);

            ConsoleOutput.AppendText("> Intialization complete \r\n");

            #endregion

            //wait a while
            await Task.Delay(100);

            ConsoleOutput.AppendText("> Getting random top monthly post from r/" + AppVariables.SubReddit + "\r\n");

            //wait a while
            await Task.Delay(500);

            #region Reddit API Queries and Video Generation

            //start redditfunctions
            RedditFunctions redditFunctions = new RedditFunctions();

            #region Get random top monthly post
            //GET COMMENTS
            //get id of random top monthly post
            string TopPostID = redditFunctions.GetRandomTopMonthlyPostID();

            ConsoleOutput.AppendText(String.Format("> Got post: '{0}' with id: {1}\r\n", AppVariables.PostTitle, AppVariables.PostId));

            await Task.Delay(100);

            #endregion

            ConsoleOutput.AppendText("> Creating title video...\r\n");

            await Task.Delay(100);

            #region Title Video Generation

            //create title image
            TitleCard titleCard = new TitleCard();
            titleCard.PostAuthorText.Text = "u/" + AppVariables.PostAuthor;
            titleCard.PostSubredditText.Text = "r/" + AppVariables.SubReddit;

            if (AppVariables.PostIsNSFW == true)
            {
                titleCard.PostNSFWTag.Text = "[nsfw] ";
            }

            titleCard.PostTitleText.Text = AppVariables.PostTitle;
            titleCard.PostUpvoteCountText.Text = AppVariables.PostUpvoteCount.ToKMB() + " upvotes";
            titleCard.PostCommentCountText.Text = AppVariables.PostCommentCount.ToKMB() + " comments";
            titleCard.PostDateText.Text = AppVariables.PostCreationDate.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm") + " UTC";

            //save control
            SaveControlAsImage(titleCard, Path.Combine(AppVariables.FramesDirectory, "title.png"));

            //use microsoft tts api to read subreddit and post title
            var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToWaveFile(Path.Combine(AppVariables.AudioDirectory, "title.wav"));
            synthesizer.Speak("r/" + AppVariables.SubReddit + ", " + AppVariables.PostTitle);
            synthesizer.Dispose();

            //run ffmpeg commands to generate title video

            //cmd commands to run
            string command1 = "cd " + AppVariables.ffmpegDirectory;
            string command2 = System.String.Format("ffmpeg -loop 1 -i {0} -i {1} -c:v libx264 -tune stillimage -c:a aac -b:a 192k -pix_fmt yuv420p -shortest {2}",
                Path.Combine(AppVariables.FramesDirectory, "title.png"),
                Path.Combine(AppVariables.AudioDirectory, "title.wav"),
                Path.Combine(AppVariables.OutputDirectory, "title.mp4"));

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + command1 + " & " + command2;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            //clean up and delete files
            File.Delete(Path.Combine(AppVariables.FramesDirectory, "title.png"));
            File.Delete(Path.Combine(AppVariables.AudioDirectory, "title.wav"));

            await Task.Delay(100);

            #endregion

            ConsoleOutput.AppendText("> Getting top comments...\r\n");

            await Task.Delay(100);

            #region Get comments and set up comment card basic boilerplate
            //get top comments from said post
            List<Comment> comments = redditFunctions.GetPostTopComments(TopPostID);

            await Task.Delay(100);

            ConsoleOutput.AppendText("> Number of comments: " + comments.Count + "\r\n");

            await Task.Delay(100);

            //init new comment card
            CommentCard commentCard = new CommentCard();
            commentCard.PostSubredditText.Text = "r/" + AppVariables.SubReddit;

            if (AppVariables.PostIsNSFW == true)
            {
                commentCard.PostNSFWTag.Text = "[nsfw] ";
            }

            commentCard.PostTitleText.Text = AppVariables.PostTitle;

            #endregion

            //iterate through comments and generate comment cards
            foreach (Comment comment in comments)
            {
                if (comment.Author != null && comment.Body != null && comment.Body != "[deleted]" && comment.Body != "[removed]")
                {
                    int FilenameCount = 0;

                    //set commentcard properties
                    commentCard.CommentAuthorText.Text = "u/" + comment.Author;
                    commentCard.CommentUpvoteCountText.Text = comment.UpVotes.ToKMB() + " upvotes";
                    commentCard.CommentDateText.Text = comment.Created.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm") + " UTC";
                    HtmlRichTextBoxBehavior.SetText(commentCard.CommentBodyText, comment.BodyHTML);
                    commentCard.CommentBodyText.Document.PagePadding = new Thickness(0);
                    commentCard.CommentBodyText.Document.FontSize = 32;
                    commentCard.CommentBodyText.Document.FontFamily = new FontFamily(@"/Resources/NotoSans/#Noto Sans");
                    commentCard.CommentBodyText.Document.TextAlignment = TextAlignment.Left;

                    //next, we extract the raw text from the richtextbox (which is the comment text) and clear it
                    string[] commentSentences = Regex.Split(StringFromRichTextBox(commentCard.CommentBodyText).Trim(), @"(?<=[.!?])");
                    commentCard.CommentBodyText.Document.Blocks.Clear();

                    //iterate through sentences in commentsentences
                    foreach (string sentence in commentSentences)
                    {
                        if (sentence != "" && sentence != " ")
                        {
                            commentCard.CommentBodyText.AppendText(sentence);

                            //only check the commentbodytext height after appending the sentence to see if the text will overflow
                            if (commentCard.CommentBodyText.Height > 725)
                            {
                                //clear the rtb and append text again 
                                commentCard.CommentBodyText.Document.Blocks.Clear();
                                commentCard.CommentBodyText.AppendText(sentence);
                            }

                            //save control
                            SaveControlAsImage(commentCard, Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"));
                            FilenameCount++;
                        }
                    }

                    break;
                }

            }

            #endregion

        }

        #endregion
    }
}
