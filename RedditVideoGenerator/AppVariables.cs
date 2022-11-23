using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Threading;

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

    }
}
