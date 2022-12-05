using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NAudio.Wave;
using Octokit;
using RedditVideoGenerator.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Comment = Reddit.Controllers.Comment;
using FileMode = System.IO.FileMode;

namespace RedditVideoGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow
    {
        #region Window setup and event handlers (including update checking)

        public MainWindow()
        {
            InitializeComponent();

            //Upgrade settings if needed
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

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

        private async void ConsoleOutput_Loaded(object sender, RoutedEventArgs e)
        {
            //check for updates before calling main function
            ConsoleOutput.AppendText("> Checking for updates...\r\n");

            await Task.Delay(100);

            if (CheckForInternetConnection() == true)
            {
                //compare the latest release tag with the app version. If they are different, update is available
                GitHubClient client = new GitHubClient(new ProductHeaderValue("RedditVideoGenerator"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Apollo199999999", "RedditVideoGenerator");

                if (releases.Count > 0)
                {
                    Version latestGitHubVersion = new Version(releases[0].TagName);
                    Version localVersion = new Version(AppVariables.AppVersion);

                    //Compare versions
                    int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                    if (versionComparison < 0)
                    {
                        //The version on GitHub is more up to date than this local release.

                        ConsoleOutput.AppendText("> An update is available.\r\n");

                        await Task.Delay(100);

                        //show the messagebox
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Content = new Grid()
                        {
                            Height = 45,
                            VerticalAlignment = VerticalAlignment.Top,
                            Children =
                            {
                                new Image()
                                {
                                    Source = new BitmapImage(new Uri(Path.Combine(AppVariables.ResourcesDirectory, "HelpIcon.png"))),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Width = 45,
                                    Height = 45
                                },

                                new TextBlock()
                                {
                                    Text = "An update is available for RedditVideoGenerator. Would you like to go to GitHub to download it?",
                                    TextWrapping = TextWrapping.Wrap,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Margin = new Thickness(55, 0, 0, 0),
                                    FontSize = 14
                                }
                            }
                        };

                        messageBox.Title = "Update available";
                        messageBox.ButtonLeftName = "Yes";
                        messageBox.ButtonLeftAppearance = Wpf.Ui.Common.ControlAppearance.Primary;
                        messageBox.ButtonRightName = "No";
                        messageBox.ButtonRightAppearance = Wpf.Ui.Common.ControlAppearance.Secondary;
                        messageBox.MicaEnabled = true;
                        messageBox.ResizeMode = ResizeMode.NoResize;
                        messageBox.Height = 185;
                        messageBox.ButtonLeftClick += (s, args) =>
                        {
                            //go to the github page
                            Process.Start("https://github.com/Apollo199999999/RedditVideoGenerator/releases");

                            //exit the application
                            Application.Current.Shutdown();

                            return;
                        };

                        messageBox.ButtonRightClick += (s, args) =>
                        {
                            messageBox.Close();
                        };

                        messageBox.ShowDialog();

                    }
                    else
                    {
                        ConsoleOutput.AppendText("> No updates are available.\r\n");

                        await Task.Delay(100);
                    }

                }

            }
            else
            {
                ConsoleOutput.AppendText("> Unable to check for updates. Check your internet connection, " +
                    "and RedditVideoGenerator will check for updates on next launch.\r\n");

                await Task.Delay(100);
            }

            //call main function which is the entry point of the video generation
            Main();
        }

        private void ConsoleOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            //scroll to end
            ConsoleOutput.ScrollToEnd();
        }

        private void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            //show AboutWindow (without blocking calling thread)
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.Show();
            aboutWindow.Activate();
            this.IsEnabled = false;
            aboutWindow.Closed += (s, args) =>
            {
                this.IsEnabled = true;
            };
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            //save video resources in case of app crashes
            try
            {
                SaveVideoResourcesToDesktopWithShutdown();
            }
            catch
            {
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region Helper functions

        //Check Internet Connection Function
        //Creating the extern function...  
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public static bool CheckForInternetConnection()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        //function to generate unique random numbers
        public static List<int> GenerateUniqueRandInt(int LowerBound, int UpperBound, int times)
        {
            Random rand = new Random();
            List<int> listNumbers = new List<int>();
            int number;
            for (int i = 0; i < times; i++)
            {
                do
                {
                    number = rand.Next(LowerBound, UpperBound);
                } while (listNumbers.Contains(number));
                listNumbers.Add(number);
            }

            return listNumbers;
        }

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

        public static TimeSpan GetDuration(string filename)
        {
            using (var shell = ShellObject.FromParsingName(filename))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                return TimeSpan.FromTicks((long)t);
            }
        }

        public void SaveVideoResourcesToDesktop()
        {
            //copy video file and thumbnail to user desktop
            //remove invalid chars from post title
            string CopiedVideoFilename = AppVariables.PostId + " - " + AppVariables.PostTitle;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                CopiedVideoFilename = CopiedVideoFilename.Replace(c.ToString(), "");
            }

            //copy the output and thumbnail to the user's desktop
            File.Copy(Path.Combine(AppVariables.OutputDirectory, "output.mp4"), Path.Combine(AppVariables.UserDesktopDirectory, CopiedVideoFilename + ".mp4"), true);
            File.Copy(Path.Combine(AppVariables.OutputDirectory, "thumbnail.png"), Path.Combine(AppVariables.UserDesktopDirectory, "thumbnail - " + CopiedVideoFilename + ".png"), true);

            //write video title and description to text files on desktop
            File.WriteAllText(Path.Combine(AppVariables.UserDesktopDirectory, "title - " + CopiedVideoFilename + ".txt"), AppVariables.VideoTitle);
            File.WriteAllText(Path.Combine(AppVariables.UserDesktopDirectory, "description - " + CopiedVideoFilename + ".txt"), AppVariables.VideoDescription);

            //create a array for the files to show
            string[] FilesToShow = new string[] { CopiedVideoFilename + ".mp4",
                "thumbnail - " + CopiedVideoFilename + ".png",
                "title - " + CopiedVideoFilename + ".txt",
                "description - " + CopiedVideoFilename + ".txt"
            };

            //show the files in file explorer
            ExplorerMethods.OpenFolderAndSelectFiles(AppVariables.UserDesktopDirectory, FilesToShow);

            //clean up working directory
            Directory.Delete(AppVariables.WorkingDirectory, true);

        }

        public void SaveVideoResourcesToDesktopWithShutdown()
        {
            SaveVideoResourcesToDesktop();

            //exit application
            Application.Current.Shutdown();
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

            //delete working directory
            if (Directory.Exists(AppVariables.WorkingDirectory))
            {
                Directory.Delete(AppVariables.WorkingDirectory, true);
            }

            //create neccessary directories
            Directory.CreateDirectory(AppVariables.WorkingDirectory);
            Directory.CreateDirectory(AppVariables.FramesDirectory);
            Directory.CreateDirectory(AppVariables.AudioDirectory);
            Directory.CreateDirectory(AppVariables.OutputDirectory);

            //kill ffmpeg if running
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //allow some time in case some libraries/code hasn't loaded yet
            await Task.Delay(500);

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

            //get post awards and show them if applicable
            if (AppVariables.PostPlatinumCount >= 1)
            {
                titleCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                titleCard.PlatinumAwardsPanel.Visibility = Visibility.Visible;
                titleCard.PlatinumCount.Text = AppVariables.PostPlatinumCount.ToString();
            }
            if (AppVariables.PostGoldCount >= 1)
            {
                titleCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                titleCard.GoldAwardsPanel.Visibility = Visibility.Visible;
                titleCard.GoldCount.Text = AppVariables.PostGoldCount.ToString();
            }
            if (AppVariables.PostSilverCount >= 1)
            {
                titleCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                titleCard.SilverAwardsPanel.Visibility = Visibility.Visible;
                titleCard.SilverCount.Text = AppVariables.PostSilverCount.ToString();
            }

            //show nsfw tag if applicable
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

            #region Get top comments

            ConsoleOutput.AppendText("> Getting top comments...\r\n");

            await Task.Delay(100);

            //get top comments from said post
            List<Comment> comments = redditFunctions.GetPostTopComments(TopPostID);

            await Task.Delay(100);

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
                    TotalDuration += GetDuration(video);
                }

                if (TotalDuration >= new TimeSpan(0, 0, 15, 0))
                {
                    break;
                }

                if (comment.Author != null && comment.Body != null && comment.Body != "[deleted]"
                    && comment.Body != "[removed]" && comment.Author.ToLower().Contains("moderator") == false)
                {
                    //init new comment card
                    CommentCard commentCard = new CommentCard();
                    commentCard.PostSubredditText.Text = "r/" + AppVariables.SubReddit;

                    if (AppVariables.PostIsNSFW == true)
                    {
                        commentCard.PostNSFWTag.Text = "[nsfw] ";
                    }

                    commentCard.PostTitleText.Text = AppVariables.PostTitle;

                    int FilenameCount = 0;

                    //create directory in output dir for storing video files
                    string CommentSentenceOutputDir = Path.Combine(AppVariables.OutputDirectory, comment.Id);
                    Directory.CreateDirectory(CommentSentenceOutputDir);

                    //set commentcard properties
                    commentCard.CommentAuthorText.Text = "u/" + comment.Author;

                    //show comment awards if applicable
                    if (comment.Awards.Platinum >= 1)
                    {
                        commentCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                        commentCard.PlatinumAwardsPanel.Visibility = Visibility.Visible;
                        commentCard.PlatinumCount.Text = comment.Awards.Platinum.ToString();
                    }
                    if (comment.Awards.Gold >= 1)
                    {
                        commentCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                        commentCard.GoldAwardsPanel.Visibility = Visibility.Visible;
                        commentCard.GoldCount.Text = comment.Awards.Gold.ToString();
                    }
                    if (comment.Awards.Silver >= 1)
                    {
                        commentCard.AwardsPanelSeparator.Visibility = Visibility.Visible;
                        commentCard.SilverAwardsPanel.Visibility = Visibility.Visible;
                        commentCard.SilverCount.Text = comment.Awards.Silver.ToString();
                    }

                    //set commentcard properties
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
                Path.Combine(AppVariables.OutputDirectory, "output_temp.mp4"));

            //run ffmpeg
            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), VideoCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //clean up files in output directory
            var FilesList = Directory.GetFiles(AppVariables.OutputDirectory);

            foreach (string file in FilesList)
            {
                if (Path.GetFileName(file) != "output_temp.mp4")
                {
                    File.Delete(file);
                }
            }

            ConsoleOutput.AppendText("> Done concatenating videos.\r\n");

            await Task.Delay(100);

            #endregion

            #region Background music generation

            ConsoleOutput.AppendText("> Adding background music...\r\n");

            await Task.Delay(100);

            //get all available music
            string[] BGMFiles = Directory.GetFiles(AppVariables.BGMusicDirectory);

            //pick 3 random numbers to use as filename for music
            List<int> RandomBGMFileNames = GenerateUniqueRandInt(0, BGMFiles.Length, 3);

            string Bgm1Path = Path.Combine(AppVariables.BGMusicDirectory, RandomBGMFileNames[0].ToString() + ".wav");
            string Bgm2Path = Path.Combine(AppVariables.BGMusicDirectory, RandomBGMFileNames[1].ToString() + ".wav");
            string Bgm3Path = Path.Combine(AppVariables.BGMusicDirectory, RandomBGMFileNames[2].ToString() + ".wav");

            //concat the 3 music files using ffmpeg
            string BGMConcatCommand = String.Format(" -i {0} -i {1} -i {2} -filter_complex concat=n=3:v=0:a=1 -vn {3}",
                Bgm1Path,
                Bgm2Path,
                Bgm3Path,
                Path.Combine(AppVariables.AudioDirectory, "bgm.wav"));

            //run ffmpeg
            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), BGMConcatCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //combine bgm with video
            string BGMCombineCommand = String.Format(" -i {0} -stream_loop -1 -i {1} -filter_complex \"[0:a][1:a]amerge=inputs=2[a]\" -map 0:v -map \"[a]\" -c:v copy -ac 2 -shortest {2}",
                Path.Combine(AppVariables.OutputDirectory, "output_temp.mp4"),
                Path.Combine(AppVariables.AudioDirectory, "bgm.wav"),
                Path.Combine(AppVariables.OutputDirectory, "output.mp4"));

            //run ffmpeg
            await StartProcess(Path.Combine(AppVariables.FFmpegDirectory, "ffmpeg.exe"), BGMCombineCommand);

            //kill ffmpeg after
            await StartProcess("taskkill.exe", " /f /im ffmpeg.exe");

            //clean up
            File.Delete(Path.Combine(AppVariables.OutputDirectory, "output_temp.mp4"));
            File.Delete(Path.Combine(AppVariables.AudioDirectory, "bgm.wav"));

            ConsoleOutput.AppendText("> Done adding background music.\r\n");

            await Task.Delay(100);

            #endregion

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

            //show awards from post if applicable
            if (AppVariables.PostPlatinumCount >= 1)
            {
                thumbnailImage.AwardsPanelSeparator.Visibility = Visibility.Visible;
                thumbnailImage.PlatinumAwardsPanel.Visibility = Visibility.Visible;
                thumbnailImage.PlatinumCount.Text = AppVariables.PostPlatinumCount.ToString();
            }
            if (AppVariables.PostGoldCount >= 1)
            {
                thumbnailImage.AwardsPanelSeparator.Visibility = Visibility.Visible;
                thumbnailImage.GoldAwardsPanel.Visibility = Visibility.Visible;
                thumbnailImage.GoldCount.Text = AppVariables.PostGoldCount.ToString();
            }
            if (AppVariables.PostSilverCount >= 1)
            {
                thumbnailImage.AwardsPanelSeparator.Visibility = Visibility.Visible;
                thumbnailImage.SilverAwardsPanel.Visibility = Visibility.Visible;
                thumbnailImage.SilverCount.Text = AppVariables.PostSilverCount.ToString();
            }

            SaveControlAsImage(thumbnailImage, Path.Combine(AppVariables.OutputDirectory, "thumbnail.png"));

            ConsoleOutput.AppendText("> Thumbnail generation complete.\r\n");

            await Task.Delay(100);

            #endregion

            #endregion

            #region Youtube API Queries

            #region Generate video title and description

            ConsoleOutput.AppendText("> Generating video title and description...\r\n");

            await Task.Delay(100);

            //init yt video title and description
            AppVariables.VideoTitle = String.Format("[r/{0}] {1}", AppVariables.SubReddit, AppVariables.PostTitle).ToUTF8().Replace("<", "[").Replace(">", "]").TruncateLongString(100);
            AppVariables.VideoDescription = AppVariables.VideoTitle + "\n" + "Thanks for watching! Leave a like if you have enjoyed this video and subscribe to never miss an upload. \n\n" + "Music: \n";

            //get music credits to put in video description
            foreach (int i in RandomBGMFileNames)
            {
                string MusicLicensePath = Path.Combine(AppVariables.MusicLicenseDirectory, i.ToString() + ".txt");
                AppVariables.VideoDescription += File.ReadAllText(MusicLicensePath) + "\n\n";
            }

            AppVariables.VideoDescription = AppVariables.VideoDescription.ToUTF8().Replace("<", "[").Replace(">", "]").TruncateLongString(5000);

            ConsoleOutput.AppendText("> Done generating video title and description.\r\n");

            await Task.Delay(100);

            #endregion

            #region Confirmation on whether to upload video

            //show the messagebox
            Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
            messageBox.Content = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Top,
                Children =
                {
                    new Image()
                    {
                        Source = new BitmapImage(new Uri(Path.Combine(AppVariables.ResourcesDirectory, "HelpIcon.png"))),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = 45,
                        Height = 45
                    },

                    new TextBlock()
                    {
                        Text = "Do you want to upload this video to YouTube? \n" +
                        "(Note that this might not be successful, as multiple users may be using this app at the same time to upload videos to YouTube, " +
                        "which may cause RedditVideoGenerator to exceed it's daily quota usage for the YouTube API).",
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(55, 0, 0, 0),
                        FontSize = 14
                    }
                }
            };
           
            messageBox.Title = "Upload to YouTube?";
            messageBox.ButtonLeftName = "Yes";
            messageBox.ButtonLeftAppearance = Wpf.Ui.Common.ControlAppearance.Primary;
            messageBox.ButtonRightName = "No";
            messageBox.ButtonRightAppearance = Wpf.Ui.Common.ControlAppearance.Secondary;
            messageBox.ResizeMode = ResizeMode.NoResize;
            messageBox.MicaEnabled = true;
            messageBox.Height = 250;
            messageBox.Width = 420;
            messageBox.ButtonLeftClick += (s, args) =>
            {
                messageBox.Close();
            };

            messageBox.ButtonRightClick += (s, args) =>
            {
                messageBox.Close();

                this.Close();

                return;
            };

            messageBox.ShowDialog();

            #endregion

            #region YouTube video uploading

            #region Sign in using OAuth

            //init OAuth variables
            GoogleFunctions googleFunctions = new GoogleFunctions();
            UserCredential credential;

            ConsoleOutput.AppendText("> Sign in to your YouTube account when prompted.\r\n");

            await Task.Delay(100);

            //show yt sign in dialog
            YTSignInDialog yTSignInDialog = new YTSignInDialog();
            yTSignInDialog.Owner = this;
            bool? OAuthConsentResult = yTSignInDialog.ShowDialog();
            yTSignInDialog.Activate();
            this.IsEnabled = false;

            if (OAuthConsentResult == true)
            {
                this.IsEnabled = true;

                ConsoleOutput.AppendText("> Signing in to your YouTube account...\r\n");

                await Task.Delay(100);

                //sign in to google account using oauth
                credential = await googleFunctions.OAuthSignIn();
            }
            else
            {
                this.IsEnabled = true;

                this.Close();

                return;
            }

            #endregion

            #region Start YT Service and init Video

            ConsoleOutput.AppendText("> Starting YouTube service...\r\n");

            await Task.Delay(100);

            //start yt service
            YouTubeService youTubeService = googleFunctions.StartYouTubeService(credential);

            //init video properties
            var YTVideo = new Video();
            YTVideo.Snippet = new VideoSnippet();
            YTVideo.Snippet.Title = AppVariables.VideoTitle;
            YTVideo.Snippet.Description = AppVariables.VideoDescription;
            YTVideo.Snippet.Tags = new string[] { "reddit", "r/askreddit", "funny", "comedy", "entertainment", "stories", "life", "video", "askreddit", "humor", "humour" };
            YTVideo.Snippet.CategoryId = "24"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            YTVideo.Status = new VideoStatus();
            YTVideo.Status.MadeForKids = false;
            YTVideo.Status.SelfDeclaredMadeForKids = false;
            YTVideo.Status.PrivacyStatus = "public"; // or "private" or "unlisted"
            var filePath = Path.Combine(AppVariables.OutputDirectory, "output.mp4");

            ConsoleOutput.AppendText("> Done starting YouTube service.\r\n");

            await Task.Delay(100);

            #endregion

            #region Upload Video

            //upload video to YT
            ConsoleOutput.AppendText("> Uploading video to YouTube...\r\n");

            await Task.Delay(100);

            //upload video to yt
            await googleFunctions.UploadVideo(filePath, YTVideo, youTubeService);

            if (AppVariables.ErrorUploadingVideo == true)
            {
                this.Close();
                return;
            }

            #endregion

            #region Upload thumbnail

            ConsoleOutput.AppendText("> Uploading video thumbnail...\r\n");

            await Task.Delay(100);

            await googleFunctions.UploadThumbnail(Path.Combine(AppVariables.OutputDirectory, "thumbnail.png"), AppVariables.YTVideoId, youTubeService);

            if (AppVariables.ErrorUploadingThumbnail == true)
            {
                this.Close();
                return;
            }

            #endregion

            #endregion

            #endregion

            #region Cleaning up

            ConsoleOutput.AppendText("> YouTube video link: https://youtu.be/" + AppVariables.YTVideoId + "\r\n");

            await Task.Delay(100);

            ConsoleOutput.AppendText("> Copying video resources to your Desktop...\r\n");

            await Task.Delay(100);

            SaveVideoResourcesToDesktop();

            ConsoleOutput.AppendText("> You can now close RedditVideoGenerator.");

            #endregion

            return;
        }

        #endregion
    }
}
