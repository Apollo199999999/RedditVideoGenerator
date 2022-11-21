using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs;

namespace RedditVideoGenerator
{
    public class RedditFunctions
    {
        public RedditClient redditClient = new RedditClient(appId: APIKeys.RedditAppID, appSecret: APIKeys.RedditAppSecret, 
            accessToken: APIKeys.RedditAccessToken, refreshToken: APIKeys.RedditRefreshToken);

        public TextBox ConsoleOut = AppVariables.mainWindow.ConsoleOutput;

        public string GetRandomTopMonthlyPostID()
        {
            //get top monthpy posts from subreddit
            var subreddit = redditClient.Subreddit(AppVariables.SubReddit);
            List<Post> posts = subreddit.Posts.GetTop(new TimedCatSrListingInput(t: "month", limit: 100));

            //choose random post from list
            Random rnd = new Random();
            int r = rnd.Next(posts.Count);
            ConsoleOut.AppendText("> Got post: " + posts[r].Title + "\r\n");

            //update app variables
            AppVariables.PostTitle = posts[r].Title;
            AppVariables.PostAuthor = posts[r].Author;
            AppVariables.PostCommentCount = posts[r].Listing.NumComments;
            AppVariables.PostUpvoteCount = posts[r].UpVotes;
            AppVariables.PostCreationDate = posts[r].Created;

            return "t3_" + posts[r].Id;
        }

        public List<Comment> GetPostTopComments(string postID)
        {
            ConsoleOut.AppendText("> Getting top comments...\r\n");

            //get top comments
            Post post = redditClient.Subreddit(AppVariables.SubReddit).Post(postID).About();
            List<Comment> comments =  post.Comments.GetTop(depth: 0, showMore: true);
            
            ConsoleOut.AppendText("> Number of comments: " + comments.Count + "\r\n");

            return comments;
        }
    }
}
