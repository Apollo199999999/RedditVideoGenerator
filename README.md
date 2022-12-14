# RedditVideoGenerator

**NOTE: Since some files in the repository utilises Git LFS to be pushed to GitHub, please refrain from downloading, cloning, or forking this repository in order to save bandwidth.**

Project site: https://clickphase.vercel.app/redditvideogenerator

## Overview

RedditVideoGenerator - Generate r/AskReddit Reddit videos and upload them to YouTube automatically

![RedditVideoGenerator screenshot](https://user-images.githubusercontent.com/60572589/208618854-bfa87369-ed00-42fe-b545-7fab2c2db273.png)

RedditVideoGenerator is able to fully generate a 15 minute Reddit video, with background music courtesy of Kevin Macleod, a thumbnail image for YouTube, as well as a title and description for YouTube, all fully autonomously.

## Rationale

This recent trend in Reddit Text-To-Speech Videos consists (usually) of a very simple formula:

1) A Reddit post is selected
2) A sentence of text from one of the comments is revealed.
3) Text To Speech reads out said sentence.
4) Once the Text To Speech is complete, the next sentence of text is revealed.
5) Repeat till the whole comment has been read out
6) A transition video is played
7) Move on to the next comment  
8) Repeat steps 2-7 till you have a long enough video

As the format of this process is mostly unchanged, it can therefore be automated.

## Samples

### Sample video generated by RedditVideoGenerator (click on the image to play the video):

[![Sample video](https://user-images.githubusercontent.com/60572589/208622845-19133ee2-92c6-4578-b752-bba466a137a9.png)](https://www.youtube.com/watch?v=0_h4B9_SXWU)

### Sample thumbnail generated by RedditVideoGenerator:

![Sample Thumbnail](https://user-images.githubusercontent.com/60572589/208621038-0706de5c-6945-4f6d-925e-a48c491b9388.png)

## How video generation works

1) RedditVideoGenerator queries the Reddit API to get a random top yearly post
2) Details about the post are fed into the TitleCard control, and an image is generated from the control
3) The post title is then fed into Microsoft's C# Text-To-Speech API (System.Speech), which generate a wave audio file.
4) The image and audio are then combined to form a video using FFmpeg
5) RedditVideoGenerator then queries Reddit API to get the top comments from the post
6) Details about each comment are fed into the CommentCard control
7) The comment body is then split into sentences, and for each sentence, an image file is generated using the CommentCard control, while a wave audio file is generated from the sentence being fed into Microsoft's C# Text-To-Speech API
8) The image and audio are then combined to form a video using FFmpeg
9) All of the videos generated per sentence are then concatenated using FFmpeg to form a comment video
10) This process is then repeated for each comment, until the video duration is 15 minutes
11) The title video and comment videos are concatenated using FFmpeg, with transitions and an outro, from the Resources folder, being included in the video as well.
12) Background music is then generated using music made by Kevin Macleod, and is then added to the video
13) The video thumbnail is then generated using the ThumbnailImage control
14) If the user has indicated that they want to upload the video to YouTube, RedditVideoGenerator queries the YouTube API and signs in to the user's Google account using OAuth 2.0. **Note that video uploading may not always be successful, as there is a daily quota limit for the number of videos that RedditVideoGenerator can upload to YouTube.** For details about how RedditVideoGenerator utilises data associated with your YouTube channel, view our [privacy policy](https://github.com/Apollo199999999/RedditVideoGenerator/blob/main/PRIVACYPOLICY.md).

## How to use

To use RedditVideoGenerator, simply download and install the latest version of RedditVideoGenerator from [releases](https://github.com/Apollo199999999/RedditVideoGenerator/releases) and run it. RedditVideoGenerator will automatically start generating the Reddit video after being run. 

## Customisation

RedditVideoGenerator currently does not offer any built-in customisation options for the generated Reddit videos. However, if you wish to edit the resources used for generating the video (such as the images used in the video), you can modify them in-place in RedditVideoGenerator's resources folder, located in the same directory where you installed RedditVideoGenerator. If you do decide to modify those images, ensure that the file name(s) of the new image(s) are the same as the file name(s) of the old images. However, **I do not recommend modifying the videos in the resources directory (such as the outro and transition videos),** as those videos are encoded using a specific video and audio encoder in order to work with RedditVideoGenerator. Modifying those videos may result in unwanted outcomes. You can always modify the generated Reddit video in a video editor.

## Minimum system requirements

* At least 1GB of free space

* Windows 10 or Windows 11 (I have not tested RedditVideoGenerator with Windows 7 or Windows 8.1, so use RedditVideoGenerator at your own risk if you are using those operating systems) 

## Privacy Policy

You can view RedditVideoGenerator's privacy policy here: https://github.com/Apollo199999999/RedditVideoGenerator/blob/main/PRIVACYPOLICY.md

## License

RedditVideoGenerator is licensed under the MIT License. You can view it here: https://github.com/Apollo199999999/RedditVideoGenerator/blob/main/LICENSE
