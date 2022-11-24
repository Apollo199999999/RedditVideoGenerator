using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs;

namespace RedditVideoGenerator
{
    public class RedditFunctions
    {
        public RedditClient redditClient;

        public void InitializeRedditClient()
        {
            try
            {
                //init reddit client
                redditClient = new RedditClient(appId: APIKeys.RedditAppID, appSecret: APIKeys.RedditAppSecret, 
                    accessToken: APIKeys.RedditAccessToken, refreshToken: APIKeys.RedditRefreshToken);

                //try querying something from reddit
                var subreddit = redditClient.Subreddit(AppVariables.SubReddit);
                List<Post> posts = subreddit.Posts.GetTop(new TimedCatSrListingInput());
            }
            catch
            {
                //show error message, then exit app
                MessageBoxResult messageBox = MessageBox.Show("Unable to initialize Reddit Client to contact Reddit. " +
                    "Check your internet connection and try again later.", "Error contacting reddit",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (messageBox == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }

            }
        }

        public string GetRandomTopMonthlyPostID()
        {
            //get top monthpy posts from subreddit
            var subreddit = redditClient.Subreddit(AppVariables.SubReddit);
            List<Post> posts = subreddit.Posts.GetTop(new TimedCatSrListingInput(t: "month", limit: 100));

            //choose random post from list
            Random rnd = new Random();
            int r = rnd.Next(posts.Count);

            //update app variables
            AppVariables.PostTitle = posts[r].Title;
            AppVariables.PostAuthor = posts[r].Author;
            AppVariables.PostCommentCount = posts[r].Listing.NumComments;
            AppVariables.PostUpvoteCount = posts[r].UpVotes;
            AppVariables.PostCreationDate = posts[r].Created;
            AppVariables.PostIsNSFW = posts[r].NSFW;
            AppVariables.PostId = posts[r].Id;

            return "t3_" + posts[r].Id;
        }

        public List<Comment> GetPostTopComments(string postID)
        {
            //get top comments
            Post post = redditClient.Subreddit(AppVariables.SubReddit).Post(postID).About();
            List<Comment> comments = post.Comments.GetTop(depth: 0, showMore: true, limit: 40);
            return comments;
        }
    }
}
