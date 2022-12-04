using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void TryInitializeRedditClient()
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
            catch (Exception ex)
            {
                //show error message, then exit app
                MessageBoxResult messageBox = MessageBox.Show("Unable to initialize Reddit client to contact Reddit. " +
                    "Check your internet connection or try again later.\n" + "Reason: " + ex.Message, "Error contacting Reddit",
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
            List<Post> posts = subreddit.Posts.GetTop(new TimedCatSrListingInput(t: "year", limit: 100));

            //choose random post from list
            Random rnd = new Random();
            Post RandomPost;

            //keep generating a random post while the generatedpostids contains the randompost.id
            do
            {
                int r = rnd.Next(posts.Count);
                RandomPost = posts[r];
            } while (Properties.Settings.Default.GeneratedPostIds.Contains(RandomPost.Id));

            //add random post id to settings
            Properties.Settings.Default.GeneratedPostIds.Add(RandomPost.Id);
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();

            //update app variables
            AppVariables.PostTitle = RandomPost.Title;
            AppVariables.PostAuthor = RandomPost.Author;
            AppVariables.PostCommentCount = RandomPost.Listing.NumComments;
            AppVariables.PostUpvoteCount = RandomPost.UpVotes;
            AppVariables.PostCreationDate = RandomPost.Created;
            AppVariables.PostIsNSFW = RandomPost.NSFW;
            AppVariables.PostId = RandomPost.Id;
            AppVariables.PostPlatinumCount = RandomPost.Awards.Platinum;
            AppVariables.PostGoldCount = RandomPost.Awards.Gold;
            AppVariables.PostSilverCount = RandomPost.Awards.Silver;

            return "t3_" + RandomPost.Id;
        }

        public List<Comment> GetPostTopComments(string postID)
        {
            //get top comments
            Post post = redditClient.Subreddit(AppVariables.SubReddit).Post(postID).About();
            List<Comment> comments = post.Comments.GetTop(depth: 0, showMore: true, limit: 500);
            return comments;
        }
    }
}
