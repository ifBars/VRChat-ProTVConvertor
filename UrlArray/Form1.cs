using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace UrlArray
{
    public partial class Form1 : Form
    {
        // Url and Name Lists
        List<string> urlList = new List<string>();
        List<string> nameList = new List<string>();
        // Indexs/URLs in lists
        int index = 0;
        // Prefix to add before URLS
        string prefix = "";
        // Register file path
        string filePath;
        // Register name of file input
        string userInput = "urls";
        // Register real video name variable
        public static string realVideoTitle;
        // Register playlist Items
        List<PlaylistItem> playlistItems = new List<PlaylistItem>();

        // Register Playlist Response
        PlaylistItemListResponse playlistItemsListResponse;

        // Initialize Form
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Register YouTube Service
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "YOUR_API_KEY",
                ApplicationName = this.GetType().ToString()
            });

            // Validate input URL
            if (!IsValidYoutubeUrl(textBox1.Text))
            {
                if(!IsValidUrl(textBox1.Text))
                {
                    label1.Text = "Please input valid link url";
                    return;
                }
                    
            }

            // Get video name 
            string videoName;

            if (textBox4.Text != "" && textBox4.Text.Contains("Name (Optional)") == false)
            {
                videoName = textBox4.Text;
            } else
            {
                videoName = "Song" + index.ToString();
            }

            // Get prefix
            prefix = textBox2.Text.Contains("https") ? textBox2.Text : "";

            // Add the URL from the text box to the list
            label1.Text = "Adding Link";
            urlList.Insert(index, "@" + prefix + textBox1.Text);
            nameList.Insert(index, videoName);

            // Clearing up
            textBox1.Text = "";
            textBox4.Text = "";
            label1.Text = "Link added";
            index++;
            label3.Text = index.ToString();
        }

        private bool IsValidYoutubeUrl(string url)
        {
            return Regex.IsMatch(url, @"www.youtube.com/watch?v=");
        }

        private bool IsValidUrl(string url)
        {
            return Regex.IsMatch(url, @"https://");
        }

        private string GetVideoName(YouTubeService youtubeService, string url)
        {
            var videoRequest = youtubeService.Videos.List("snippet");

            // Parse video id from URL
            string videoId = Regex.Match(url, @"v=([^&]+)").Groups[1].Value;
            videoRequest.Id = videoId;

            // Grab video name
            var videoResponse = videoRequest.Execute();

            if (videoResponse.Items.Count > 0)
            {
                var video = videoResponse.Items[0];
                return video.Snippet.Title;
            }
            else
            {
                // Handle error scenario
                MessageBox.Show("Error grabbing video name");
                return "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "Writing to file";

            // Check if user prefix exists
            if (textBox2.Text.Contains("https"))
            {
                prefix = textBox2.Text;
            }

            if(textBox5.Text.Contains("File Name (No Need for .txt)") == false)
            {
                userInput = textBox5.Text;
            }

            // Create a new FolderBrowserDialog
            var folderBrowserDialog1 = new FolderBrowserDialog();

            // Show the dialog
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // Get the selected folder path
                string folderPath = folderBrowserDialog1.SelectedPath;

                // Append the file name to the folder path
                filePath = Path.Combine(folderPath, userInput + ".txt");
            }

                // Make sure the lists are the same length
                if (urlList.Count != nameList.Count)
            {
                MessageBox.Show("Error: lists are not the same length");
                return;
            }

            try
            {
                // Erase the file each time before rewriting to it
                File.WriteAllText(filePath, string.Empty);

                // Write the list of URLs and names to a file
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    for (int i = 0; i < urlList.Count; i++)
                    {
                        sw.WriteLine(urlList[i]);
                        sw.WriteLine(nameList[i]);
                        sw.WriteLine("");
                    }
                    label1.Text = "File written successfully";
                }
            }
            catch (IOException)
            {
                MessageBox.Show("An error occurred while trying to access the file: Is the file open in another program?");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "Attempting to grab playlist";

            // Register YouTube Service
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "YOUR_API_KEY",
                ApplicationName = this.GetType().ToString()
            });

            // Get the playlist ID from the text box
            var playlistId = textBox3.Text;
            textBox3.Text = "";

            // Try playlist ID and throw error if Incorrect
            try
            {
                // Set up the request to retrieve the videos in the playlist
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = playlistId;
                playlistItemsListRequest.MaxResults = 500;
                playlistItemsListRequest.MaxResults = 500;

                // Set the PageToken property to an empty string to retrieve the first page of results
                playlistItemsListRequest.PageToken = "";

                // Initialize the list to hold the results
                playlistItems = new List<PlaylistItem>();

                // Initialize the counter variable
                var totalResults = 0;

                // Retrieve the results
                while (totalResults < 500)
                {
                    // Execute the request
                    playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                    // Add the results to the list
                    playlistItems.AddRange(playlistItemsListResponse.Items);

                    // Increment the counter variable
                    totalResults += playlistItemsListResponse.Items.Count;

                    // Set the page token for the next request
                    playlistItemsListRequest.PageToken = playlistItemsListResponse.NextPageToken;

                    // If there are no more pages, break out of the loop
                    if (string.IsNullOrEmpty(playlistItemsListResponse.NextPageToken))
                    {
                        break;
                    }
                }
            }
            catch (GoogleApiException ev)
            {
                if (ev.Error.Code == 404)
                {
                    MessageBox.Show("Invalid playlist ID. Is the playlist private? All playlists must be public to grab.");
                }
                else
                {
                    MessageBox.Show("An error occurred: " + ev.Message);
                }
            }

            // Print the title and URL of each video in the playlist
            foreach (var playlistItem in playlistItems)
            {

                string videoTitle = "";
                realVideoTitle = playlistItem.Snippet.Title;

                if (textBox4.Text.Contains("Name (Optional)") == false)
                {
                    Console.WriteLine("Custom Name");
                    Form2 f2 = new Form2();
                    videoTitle = f2.openName();
                }
                else
                {
                    videoTitle = realVideoTitle;
                }

                
                var videoId = playlistItem.Snippet.ResourceId.VideoId;
                var videoUrl = "https://www.youtube.com/watch?v=" + videoId;
                urlList.Insert(index, "@" + prefix + videoUrl);
                nameList.Insert(index, videoTitle);
                label1.Text = videoTitle + " has been added";
                index++;
                label3.Text = index.ToString();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Clear all URLS
            urlList.Clear();
            nameList.Clear();
            index = 0;
            label3.Text = index.ToString();
        }
    }
}