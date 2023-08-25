using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Threading;
using System.Windows.Media;
using NAudio.Codecs;
using Google.Apis.Auth.OAuth2;

namespace RedditVideoGenerator
{
    public static class AppVariables
    {
        //redditvideogenerator app version
        public static string AppVersion = "1.0.2";

        //instance of mainwindow that will be assigned to after mainwindow initializes
        public static MainWindow mainWindow;

        //directories used to store files
        public static string WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RedditVideoGenerator");
        public static string FramesDirectory = Path.Combine(WorkingDirectory, "frames");
        public static string AudioDirectory = Path.Combine(WorkingDirectory, "audio");
        public static string OutputDirectory = Path.Combine(WorkingDirectory, "output");
        public static string FFmpegDirectory = Path.Combine(Environment.CurrentDirectory, "Resources\\FFmpeg");
        public static string ResourcesDirectory = Path.Combine(Environment.CurrentDirectory, "Resources");
        public static string BGMusicDirectory = Path.Combine(ResourcesDirectory, "bgm\\music");
        public static string MusicLicenseDirectory = Path.Combine(ResourcesDirectory, "bgm\\licenses");
        public static string UserDesktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        //reddit variables
        public static string RedditDeviceId;
        public static string RedditAppId = "EPe81-ibf25Ivll9AYmEgA";
        public static string SubReddit = "AskReddit";
        //reddit post variables
        public static string PostTitle;
        public static string PostAuthor;
        public static int PostCommentCount;
        public static int PostUpvoteCount;
        public static DateTime PostCreationDate;
        public static bool PostIsNSFW;
        public static string PostId;
        public static int PostPlatinumCount;
        public static int PostGoldCount;
        public static int PostSilverCount;

        //accent color of thumbnail
        public static List<Color> ThumbnailAccentColors =
            new List<Color>() { Colors.Orange, Colors.Yellow, Colors.LightGreen, Colors.CornflowerBlue, Colors.DarkSalmon, 
                Colors.DarkOrange, Color.FromRgb(255, 69, 0), Color.FromRgb(255, 168, 0), Color.FromRgb(251, 19, 58) };

        //YouTube variables
        //UserCredential object for Google OAuth
        public static UserCredential userCredential = null;
        //YouTube video variables
        public static string VideoTitle;
        public static string VideoDescription;
        public static bool ErrorUploadingVideo = false;
        public static bool ErrorUploadingThumbnail = false;
        public static string YTVideoId;
    }
}
