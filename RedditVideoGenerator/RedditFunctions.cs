using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Unidecode.NET;

namespace RedditVideoGenerator
{
    public class RedditFunctions
    {
        public RedditClient redditClient;

        public async Task<string> GenerateAccessToken()
        {
            string accessToken = "";

            try
            {
                //generate unique device id
                AppVariables.RedditDeviceId = Guid.NewGuid().ToString();

                using (HttpClient client = new HttpClient())
                {
                    //make a POST request to reddit api to obtain app specific access token
                    var values = new Dictionary<string, string>
                    {
                        { "grant_type", "https://oauth.reddit.com/grants/installed_client" },
                        { "device_id", AppVariables.RedditDeviceId }
                    };

                    //add http auth header to request (username being clientid)
                    var username = AppVariables.RedditAppId;
                    var password = "";
                    string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                                   .GetBytes(username + ":" + password));

                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", content);

                    var responseString = await response.Content.ReadAsStringAsync();

                    //deserialize the json response
                    JObject jObject = JObject.Parse(responseString);
                    accessToken = jObject["access_token"].Value<string>();
                }
            }
            catch (Exception ex)
            {
                //show error message, then exit app
                MessageBoxResult messageBox = MessageBox.Show("Unable to obtain Reddit API access token. " +
                    "Check your internet connection or try again later.\n" + "Reason: " + ex.Message, "Error contacting Reddit",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (messageBox == MessageBoxResult.OK)
                {
                    AppVariables.mainWindow.Close();
                }
            }

            return accessToken;
        }

        public int TryInitializeRedditClient(string accessToken)
        {
            try
            {
                //init reddit client
                redditClient = new RedditClient(appId: AppVariables.RedditAppId, accessToken: accessToken);

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
                    return 1;
                }

            }


            return 0;
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
            AppVariables.PostTitle = RandomPost.Title.Unidecode().ToUTF8().Trim();
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
