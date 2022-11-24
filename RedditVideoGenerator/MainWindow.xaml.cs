using NAudio.Wave;
using Reddit.Controllers;
using RedditVideoGenerator.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public async Task StartProcess(string filepath, string args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = filepath;
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }

        public static TimeSpan GetWavFileDuration(string fileName)
        {
            using (WaveFileReader wf = new WaveFileReader(fileName))
            {
                TimeSpan duration = wf.TotalTime;
                wf.Dispose();
                return duration;
            }
        }

        public static void WriteSilence(WaveFormat waveFormat, int silenceMilliSecondLength, WaveFileWriter waveFileWriter)
        {
            int bytesPerMillisecond = waveFormat.AverageBytesPerSecond / 1000;
            //an new all zero byte array will play silence
            var silentBytes = new byte[silenceMilliSecondLength * bytesPerMillisecond];
            waveFileWriter.Write(silentBytes, 0, silentBytes.Length);
            waveFileWriter.Dispose();
        }

        public void SpeakText(string text, string path)
        {
            //use speech synthesiser to speak text
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToWaveFile(path);
            synthesizer.Speak(text);
            synthesizer.Dispose();

            if (GetWavFileDuration(path) == TimeSpan.Zero)
            {
                //replace wav file with empty one with duration of half a second
                WaveFormat waveFormat = new WaveFormat(8000, 8, 1);
                WaveFileWriter waveFileWriter = new WaveFileWriter(path, waveFormat);
                WriteSilence(waveFormat, 500, waveFileWriter);
            }
        }

        #endregion

        #region Main function

        public async void Main()
        {
            #region Initialization code

            //show some initialization text
            ConsoleOutput.AppendText("> Initializing RedditVideoGenerator...\r\n");

            //store MainWindow as an instance in AppVariables
            AppVariables.mainWindow = this;

            //create neccessary directories
            Directory.CreateDirectory(AppVariables.WorkingDirectory);
            Directory.CreateDirectory(AppVariables.FramesDirectory);
            Directory.CreateDirectory(AppVariables.AudioDirectory);
            Directory.CreateDirectory(AppVariables.OutputDirectory);

            //kill ffmpeg if running
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //allow some time in case some libraries/code hasn't loaded yet
            await Task.Delay(1000);

            ConsoleOutput.AppendText("> Initialization complete \r\n");

            //wait a while
            await Task.Delay(100);

            #endregion

            #region Reddit API Queries and Video Generation

            #region Initialize Reddit Client

            ConsoleOutput.AppendText("> Initializing Reddit Client...\r\n");

            await Task.Delay(100);

            //start redditfunctions
            RedditFunctions redditFunctions = new RedditFunctions();
            redditFunctions.InitializeRedditClient();

            ConsoleOutput.AppendText("> Done initializing Reddit Client.\r\n");

            await Task.Delay(100);

            #endregion

            #region Get random top monthly post

            ConsoleOutput.AppendText("> Getting random top monthly post from r/" + AppVariables.SubReddit + "\r\n");

            //wait a while
            await Task.Delay(500);

            //get id of random top monthly post
            string TopPostID = redditFunctions.GetRandomTopMonthlyPostID();

            ConsoleOutput.AppendText(String.Format("> Got post: '{0}' with id: {1}\r\n", AppVariables.PostTitle, AppVariables.PostId));

            await Task.Delay(100);

            #endregion

            #region Title Video Generation

            ConsoleOutput.AppendText("> Generating title video...\r\n");

            await Task.Delay(100);

            //create title image
            TitleCard titleCard = new TitleCard();
            titleCard.PostAuthorText.Text = "u/" + AppVariables.PostAuthor;
            titleCard.PostSubredditText.Text = "r/" + AppVariables.SubReddit;

            if (AppVariables.PostIsNSFW == true)
            {
                //show nsfw tag
                titleCard.PostNSFWTag.Text = "[nsfw] ";
            }

            titleCard.PostTitleText.Text = AppVariables.PostTitle;
            titleCard.PostUpvoteCountText.Text = AppVariables.PostUpvoteCount.ToKMB() + " upvotes";
            titleCard.PostCommentCountText.Text = AppVariables.PostCommentCount.ToKMB() + " comments";
            titleCard.PostDateText.Text = AppVariables.PostCreationDate.ToUniversalTime().ToString("dd MMMM yyyy, HH:mm") + " UTC";

            //save control
            SaveControlAsImage(titleCard, Path.Combine(AppVariables.FramesDirectory, "title.png"));

            //use microsoft tts api to read subreddit and post title
            SpeakText("r/" + AppVariables.SubReddit + ", " + titleCard.PostNSFWTag.Text + ", " + titleCard.PostTitleText.Text,
                Path.Combine(AppVariables.AudioDirectory, "title.wav"));

            //run ffmpeg commands to generate title video
            string TitleCommand = System.String.Format(" -nostdin -loop 1 -i {0} -i {1} -shortest -acodec copy -pix_fmt yuv420p {2}",
                Path.Combine(AppVariables.FramesDirectory, "title.png"),
                Path.Combine(AppVariables.AudioDirectory, "title.wav"),
                Path.Combine(AppVariables.OutputDirectory, "title.mkv"));

            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), TitleCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //clean up and delete files
            File.Delete(Path.Combine(AppVariables.FramesDirectory, "title.png"));
            File.Delete(Path.Combine(AppVariables.AudioDirectory, "title.wav"));

            await Task.Delay(500);

            ConsoleOutput.AppendText("> Done generating title video.\r\n");

            await Task.Delay(100);

            #endregion

            #region Get comments and set up comment card basic boilerplate

            ConsoleOutput.AppendText("> Getting top comments...\r\n");

            await Task.Delay(100);

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

            #region Comment video generation

            ConsoleOutput.AppendText("> Generating comment videos...\r\n");

            await Task.Delay(100);

            //iterate through comments and generate comment cards
            foreach (Comment comment in comments)
            {
                if (comment.Author != null && comment.Body != null && comment.Body != "[deleted]"
                    && comment.Body != "[removed]" && comment.Author.ToLower().Contains("moderator") == false)
                {
                    int FilenameCount = 0;

                    //create directory in output dir for storing video files
                    string CommentSentenceOutputDir = Path.Combine(AppVariables.OutputDirectory, comment.Id);
                    Directory.CreateDirectory(CommentSentenceOutputDir);

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
                    string[] commentSentences = Regex.Split(StringFromRichTextBox(commentCard.CommentBodyText).Trim(), @"(?<=[.!?])|(?=[\n])");
                    commentCard.CommentBodyText.Document.Blocks.Clear();

                    //iterate through sentences in commentsentences
                    foreach (string sentence in commentSentences)
                    {
                        //there's a problem with this, this means that the rtb wont keep formatting (screw it idc anymore)
                        if (sentence != "" && sentence != " ")
                        {
                            //append text and scroll to end
                            commentCard.CommentBodyText.AppendText(sentence);
                            commentCard.CommentBodyText.ScrollToEnd();

                            //save control
                            SaveControlAsImage(commentCard, Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"));

                            //use microsoft tts api to read sentence
                            SpeakText(sentence, Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"));

                            //run ffmpeg
                            string SentenceCommand = System.String.Format(" -nostdin -loop 1 -i {0} -i {1} -shortest -acodec copy -pix_fmt yuv420p {2}",
                                Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"),
                                Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"),
                                Path.Combine(CommentSentenceOutputDir, FilenameCount.ToString() + ".mkv"));

                            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), SentenceCommand);

                            //kill ffmpeg after
                            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

                            //clean up and delete files
                            File.Delete(Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"));
                            File.Delete(Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"));

                            FilenameCount++;
                        }

                    }

                    //combine all sentence videos
                    //create a new list and sort the files
                    var list = Directory.GetFiles(CommentSentenceOutputDir);
                    Array.Sort(list, new AlphanumComparatorFast());

                    StreamWriter sw = File.CreateText(Path.Combine(CommentSentenceOutputDir, "FFmpegCommand.txt"));
                    //write video filepaths to a txt file
                    foreach (string file in list)
                    {
                        sw.WriteLine(String.Format("file '{0}'", Path.GetFileName(file)));
                    }
                    sw.Close();
                    sw.Dispose();

                    //cmd commands
                    string CommentCommand = String.Format(" -nostdin -f concat -safe 0 -i {0} -c copy {1}",
                        Path.Combine(CommentSentenceOutputDir, "FFmpegCommand.txt"),
                        Path.Combine(AppVariables.OutputDirectory, comment.Id + ".mkv"));

                    //run ffmpeg
                    await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), CommentCommand);

                    //kill ffmpeg after
                    await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

                    //clean up files
                    Directory.Delete(CommentSentenceOutputDir, true);

                    ConsoleOutput.AppendText("> Finished generating comment video for comment with id: " + comment.Id + "\r\n");

                }

            }

            ConsoleOutput.AppendText("> Finished generating all comment videos.\r\n");

            await Task.Delay(100);

            #endregion

            #region Video concatenation

            ConsoleOutput.AppendText("> Concatenating videos...\r\n");

            await Task.Delay(100);

            //copy transition video from resources to output dir
            File.Copy(Path.Combine(AppVariables.ResourcesDirectory, "transition.mkv"), 
                Path.Combine(AppVariables.OutputDirectory, "transition.mkv"));

            //iterate through videos in output directory, generating a text file of videos.
            var VideosList = Directory.GetFiles(AppVariables.OutputDirectory);

            StreamWriter streamWriter = File.CreateText(Path.Combine(AppVariables.OutputDirectory, "FFmpegCommand.txt"));
            streamWriter.WriteLine("file 'title.mkv'");

            //write video filepaths to a txt file
            foreach (string file in VideosList)
            {
                //normalise videos
                await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), 
                    String.Format(" -i {0} -acodec libvo_aacenc -vcodec libx264 -s 1920x1080 -r 25 -strict experimental {1}", file, file));

                if (Path.GetFileName(file) != "transition.mkv" && Path.GetFileName(file) != "title.mkv")
                {
                    streamWriter.WriteLine("file 'transition.mkv'");
                    streamWriter.WriteLine(String.Format("file '{0}'", Path.GetFileName(file)));
                }
            }
            streamWriter.Close();
            streamWriter.Dispose();

            //cmd commands
            string VideoCommand = String.Format(" -nostdin -f concat -safe 0 -i {0} -c copy {1}",
                Path.Combine(AppVariables.OutputDirectory, "FFmpegCommand.txt"),
                Path.Combine(AppVariables.OutputDirectory, AppVariables.PostTitle + ".mkv"));

            //run ffmpeg
            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), VideoCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            ConsoleOutput.AppendText("> Done concatenating videos.\r\n");

            #endregion

            #endregion

        }

        #endregion

    }
}
