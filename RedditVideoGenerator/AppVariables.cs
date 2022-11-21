using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace RedditVideoGenerator
{
    public static class AppVariables
    {
        //instance of mainwindow that will be assigned to after mainwindow initializes
        public static MainWindow mainWindow;

        //directories used to store files
        public static string WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RedditVideoGenerator");
        public static string FramesDirectory = Path.Combine(WorkingDirectory, "frames");

        //reddit variables
        public static string SubReddit = "AskReddit";
        public static string PostTitle;
        public static string PostAuthor;
    }
}
