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
using Shell32;
using System.Linq;

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

        private void ConsoleOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //scroll to end
            ConsoleOutput.ScrollToEnd();
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

        public void SpeakText(string text, string path)
        {
            //use speech synthesiser to speak text
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToWaveFile(path);
            synthesizer.Speak(text);
            synthesizer.Dispose();

            if (GetWavFileDuration(path) == TimeSpan.Zero)
            {
                //replace wav file with empty one with duration of 200ms
                PromptBuilder empty = new PromptBuilder();
                empty.AppendText(".");
                empty.AppendBreak(TimeSpan.FromMilliseconds(200));

                SpeechSynthesizer EmptySynthesizer = new SpeechSynthesizer();
                EmptySynthesizer.SetOutputToWaveFile(path);
                EmptySynthesizer.Speak(empty);
                EmptySynthesizer.Dispose();
            }
        }

        public static bool GetDuration(string filename, out TimeSpan duration)
        {
            try
            {
                var shl = new Shell();
                var fldr = shl.NameSpace(Path.GetDirectoryName(filename));
                var itm = fldr.ParseName(Path.GetFileName(filename));

                // Index 27 is the video duration [This may not always be the case]
                var propValue = fldr.GetDetailsOf(itm, 27);

                return TimeSpan.TryParse(propValue, out duration);
            }
            catch (Exception)
            {
                duration = new TimeSpan();
                return false;
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

            ConsoleOutput.AppendText("> Initializing Reddit client...\r\n");

            await Task.Delay(100);

            //start redditfunctions
            RedditFunctions redditFunctions = new RedditFunctions();
            redditFunctions.TryInitializeRedditClient();

            ConsoleOutput.AppendText("> Done initializing Reddit client.\r\n");

            await Task.Delay(100);

            #endregion

            #region Get random top yearly post

            ConsoleOutput.AppendText("> Getting random top yearly post from r/" + AppVariables.SubReddit + "\r\n");

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
            string TitleCommand = System.String.Format(" -nostdin -loop 1 -i {0} -i {1} -shortest -vcodec libx264 -acodec aac -ac 1 -ar 48000 -pix_fmt yuv420p -s 1920x1080 -bsf:v h264_mp4toannexb -r 30 -fps_mode cfr {2}",
                Path.Combine(AppVariables.FramesDirectory, "title.png"),
                Path.Combine(AppVariables.AudioDirectory, "title.wav"),
                Path.Combine(AppVariables.OutputDirectory, "title.mp4"));

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
                //check total duration of videos in output dir to see if it exceeds 15 mins, and if so, break out of foreach loop and dont generate comment video
                TimeSpan TotalDuration = new TimeSpan();

                var OutputVideos = Directory.GetFiles(AppVariables.OutputDirectory);

                foreach (string video in OutputVideos)
                {
                    TimeSpan VideoDuration;

                    if (GetDuration(video, out VideoDuration))
                    {
                        TotalDuration += VideoDuration;
                    }
                }

                if (TotalDuration >= new TimeSpan(0, 0, 15, 0))
                {
                    break;
                }


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
                    commentCard.CommentBodyText.Document.LineHeight = 1.0;

                    //next, we extract the raw text from the richtextbox (which is the comment text) and clear it
                    string[] commentSentences = Regex.Split(StringFromRichTextBox(commentCard.CommentBodyText).Trim().Replace("•\t", "• "), @"(?<=[.!?])|(?=[\n])");
                    commentCard.CommentBodyText.Document.Blocks.Clear();

                    //iterate through sentences in commentsentences
                    foreach (string sentence in commentSentences)
                    {
                        //append text and scroll to end
                        commentCard.CommentBodyText.AppendText(sentence);
                        commentCard.CommentBodyText.ScrollToEnd();

                        //save control
                        SaveControlAsImage(commentCard, Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"));

                        //use microsoft tts api to read sentence
                        SpeakText(sentence, Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"));

                        //run ffmpeg
                        string SentenceCommand = System.String.Format(" -nostdin -loop 1 -i {0} -i {1} -shortest -vcodec libx264 -acodec aac -ac 1 -ar 48000 -pix_fmt yuv420p -s 1920x1080 -bsf:v h264_mp4toannexb -r 30 -fps_mode cfr {2}",
                            Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"),
                            Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"),
                            Path.Combine(CommentSentenceOutputDir, FilenameCount.ToString() + ".mp4"));

                        await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), SentenceCommand);

                        //kill ffmpeg after
                        await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

                        //clean up and delete files
                        File.Delete(Path.Combine(AppVariables.FramesDirectory, FilenameCount.ToString() + ".png"));
                        File.Delete(Path.Combine(AppVariables.AudioDirectory, FilenameCount.ToString() + ".wav"));

                        FilenameCount++;
                    }

                    //combine all sentence videos
                    //create a new list and sort the files
                    var list = Directory.GetFiles(CommentSentenceOutputDir);
                    Array.Sort(list, new AlphanumComparatorFast());

                    StreamWriter sw = File.CreateText(Path.Combine(CommentSentenceOutputDir, "FFmpegFiles.txt"));

                    //write video filepaths to a txt file
                    foreach (string file in list)
                    {
                        sw.WriteLine(String.Format("file '{0}'", Path.GetFileName(file)));
                    }

                    sw.Close();
                    sw.Dispose();

                    //cmd commands
                    string CommentCommand = String.Format(" -nostdin -f concat -safe 0 -i {0} -c copy -r 30 -fps_mode cfr {1}",
                        Path.Combine(CommentSentenceOutputDir, "FFmpegFiles.txt"),
                        Path.Combine(AppVariables.OutputDirectory, comment.Id + ".mp4"));

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

            #region Video Concatenation

            ConsoleOutput.AppendText("> Concatenating videos...\r\n");

            await Task.Delay(100);

            //copy transition video from resources to output dir
            File.Copy(Path.Combine(AppVariables.ResourcesDirectory, "transition.mp4"),
                Path.Combine(AppVariables.OutputDirectory, "transition.mp4"));

            //iterate through videos in output directory, generating a text file of videos.
            DirectoryInfo VideosListDirInfo = new DirectoryInfo(AppVariables.OutputDirectory);
            FileInfo[] VideosList = VideosListDirInfo.GetFiles().OrderBy(p => p.CreationTime).ToArray();

            StreamWriter streamWriter = File.CreateText(Path.Combine(AppVariables.OutputDirectory, "FFmpegFiles.txt"));
            streamWriter.WriteLine("file 'title.mp4'");

            //write video filepaths to a txt file
            foreach (FileInfo fileInfo in VideosList)
            {
                string file = fileInfo.FullName;

                if (Path.GetFileNameWithoutExtension(file) != "transition" && Path.GetFileNameWithoutExtension(file) != "title")
                {
                    streamWriter.WriteLine("file 'transition.mp4'");
                    streamWriter.WriteLine(String.Format("file '{0}'", Path.Combine(AppVariables.OutputDirectory, Path.GetFileNameWithoutExtension(file) + ".mp4")));
                }

            }

            //copy outro video from resources to output dir
            File.Copy(Path.Combine(AppVariables.ResourcesDirectory, "outro.mp4"),
                Path.Combine(AppVariables.OutputDirectory, "outro.mp4"));

            //add outro video to ffmpegfiles.txt
            streamWriter.WriteLine("file 'transition.mp4'");
            streamWriter.WriteLine("file 'outro.mp4'");

            streamWriter.Close();
            streamWriter.Dispose();

            //cmd commands
            string VideoCommand = String.Format(" -nostdin -f concat -safe 0 -i {0} -c copy -r 30 -fps_mode cfr {1}",
                Path.Combine(AppVariables.OutputDirectory, "FFmpegFiles.txt"),
                Path.Combine(AppVariables.OutputDirectory, "output.mp4"));

            //run ffmpeg
            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), VideoCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //clean up files in output directory
            var FilesList = Directory.GetFiles(AppVariables.OutputDirectory);

            foreach (string file in FilesList)
            {
                if (Path.GetFileName(file) != "output.mp4")
                {
                    File.Delete(file);
                }
            }

            ConsoleOutput.AppendText("> Done concatenating videos.\r\n");

            await Task.Delay(100);

            #endregion

            #endregion

            #region Youtube video configuration and uploading

            #region Thumbnail generation

            ConsoleOutput.AppendText("> Generating video thumbnail for YouTube...\r\n");

            await Task.Delay(100);

            //generate thumbnail
            //get random thumbnail accent color from list
            Random random = new Random();
            int AccentIndex = random.Next(0, AppVariables.ThumbnailAccentColors.Count);

            //init new thumbnail image and set properties
            ThumbnailImage thumbnailImage = new ThumbnailImage();
            //add delay to give time for controls to load
            await Task.Delay(200);
            thumbnailImage.SubredditText.Text = "r/" + AppVariables.SubReddit;
            thumbnailImage.TitleText.Text = AppVariables.PostTitle;
            thumbnailImage.SetAccentColor(AppVariables.ThumbnailAccentColors[AccentIndex]);
            //add delay to give time for titletext control to load and for layout to be updated
            await Task.Delay(200);
            thumbnailImage.SetVariableTitleFontSize();
            SaveControlAsImage(thumbnailImage, Path.Combine(AppVariables.OutputDirectory, "thumbnail.png"));

            ConsoleOutput.AppendText("> Thumbnail generation complete.\r\n");

            await Task.Delay(100);

            #endregion

            #endregion
        }

        #endregion
    }
}
