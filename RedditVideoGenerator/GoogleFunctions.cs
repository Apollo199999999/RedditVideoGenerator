using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RedditVideoGenerator
{
    public class GoogleFunctions
    {
        public async Task<UserCredential> OAuthSignIn()
        {
            //login to google account using oauth2
            UserCredential credential;
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("RedditVideoGenerator.Resources.client_secret.json"))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }

            return credential;

        }

        public YouTubeService StartYouTubeService(UserCredential credential)
        {
            //init youtubeservice to upload video
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            return youtubeService;

        }


        public async Task UploadVideo(string filePath, Video YTVideo, YouTubeService youtubeService)
        {
            //upload video
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(YTVideo, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
            }
        }

        public async Task UploadThumbnail(string filePath, string VideoId, YouTubeService youtubeService)
        {
            //set video thumbnail
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videoThumbnailRequest = youtubeService.Thumbnails.Set(AppVariables.YTVideoId, fileStream, "image/png");
                videoThumbnailRequest.ProgressChanged += VideoThumbnailRequest_ProgressChanged;
                videoThumbnailRequest.ResponseReceived += VideoThumbnailRequest_ResponseReceived;

                await videoThumbnailRequest.UploadAsync();
            }
        }

        #region YouTube API Event Handlers

        private void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:

                    AppVariables.mainWindow.Dispatcher.Invoke(() =>
                    {
                        AppVariables.mainWindow.ConsoleOutput.AppendText(String.Format("> {0} bytes sent.", progress.BytesSent) + "\r\n");
                    });

                    break;

                case UploadStatus.Failed:

                    AppVariables.mainWindow.Dispatcher.Invoke(() =>
                    {
                        AppVariables.mainWindow.ConsoleOutput.AppendText(String.Format("> An error prevented the upload from completing.\n{0}", progress.Exception.Message) + "\r\n");
                    });

                    MessageBox.Show(String.Format("An error prevented the video upload from completing. You can try manually uploading the video to YouTube.\n{0}", progress.Exception.Message),
                        "Error uploading video", MessageBoxButton.OK, MessageBoxImage.Error);

                    AppVariables.ErrorUploadingVideo = true;

                    break;
            }
        }

        private async void videosInsertRequest_ResponseReceived(Video video)
        {
            AppVariables.mainWindow.Dispatcher.Invoke(() =>
            {
                AppVariables.mainWindow.ConsoleOutput.AppendText(String.Format("> Video id '{0}' was successfully uploaded.", video.Id) + "\r\n");
            });

            AppVariables.YTVideoId = video.Id;

            await Task.Delay(100);
        }

        private async void VideoThumbnailRequest_ResponseReceived(ThumbnailSetResponse obj)
        {
            AppVariables.mainWindow.Dispatcher.Invoke(() =>
            {
                AppVariables.mainWindow.ConsoleOutput.AppendText("> Successfully set video thumbnail.\r\n");
            });

            await Task.Delay(100);
        }

        private void VideoThumbnailRequest_ProgressChanged(IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:

                    AppVariables.mainWindow.Dispatcher.Invoke(() =>
                    {
                        AppVariables.mainWindow.ConsoleOutput.AppendText(String.Format("> {0} bytes sent.", progress.BytesSent) + "\r\n");
                    });

                    break;

                case UploadStatus.Failed:

                    AppVariables.mainWindow.Dispatcher.Invoke(() =>
                    {
                        AppVariables.mainWindow.ConsoleOutput.AppendText(String.Format("> An error prevented the thumbnail upload from completing.\n{0}", progress.Exception.Message) + "\r\n");
                    });

                    MessageBox.Show(String.Format("An error prevented the thumbnail upload from completing. You can try manually setting the thumbnail for the video on YouTube.\n{0}", progress.Exception.Message),
                        "Error uploading thumbnail", MessageBoxButton.OK, MessageBoxImage.Error);

                    AppVariables.ErrorUploadingThumbnail = true;

                    break;
            }
        }

        #endregion
    }
}
