using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Threading;
using System.Windows.Media;

namespace RedditVideoGenerator
{
    public static class AppVariables
    {
        //instance of mainwindow that will be assigned to after mainwindow initializes
        public static MainWindow mainWindow;

        //directories used to store files
        public static string WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RedditVideoGenerator");
        public static string FramesDirectory = Path.Combine(WorkingDirectory, "frames");
        public static string AudioDirectory = Path.Combine(WorkingDirectory, "audio");
        public static string OutputDirectory = Path.Combine(WorkingDirectory, "output");
        public static string FFmpegDirectory = Path.Combine(Environment.CurrentDirectory, "Resources\\ffmpeg");
        public static string ResourcesDirectory = Path.Combine(Environment.CurrentDirectory, "Resources");

        //reddit variables
        public static string SubReddit = "AskReddit";
        //post variables
        public static string PostTitle;
        public static string PostAuthor;
        public static int PostCommentCount;
        public static int PostUpvoteCount;
        public static DateTime PostCreationDate;
        public static bool PostIsNSFW;
        public static string PostId;

        //accent color of thumbnail
        public static List<Color> ThumbnailAccentColors = 
            new List<Color>() { Colors.Orange, Colors.Yellow, Colors.Green, Colors.CornflowerBlue, Colors.DarkSlateBlue, Colors.DarkOrange, Colors.OrangeRed, Color.FromRgb(255, 69, 0)};

    }
}
