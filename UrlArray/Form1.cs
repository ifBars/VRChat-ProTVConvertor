using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Policy;
using System.Drawing;
using System.Text;

namespace ProTVConverter
{
    public partial class Form1 : Form
    {
        // Url, Name, and Thumbnail Lists
        List<string> urlList = new List<string>();
        List<string> nameList = new List<string>();
        List<string> thumbnailList = new List<string>();
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

        // INPUT YOUR API KEY HERE
        string keyAPI = "AIzaSyAribhLCMFwNSyRWQ08tvDUorRg_36CPqA";

        // Initialize Form
        public Form1()
        {
            InitializeComponent();

        }

        YouTubeService RegisterYT()
        {
            // Register YouTube Service
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = keyAPI,
                ApplicationName = this.GetType().ToString()
            });

            return youtubeService;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Link = " + textBox1.Text);

            // Validate input URL
            if (!IsValidYoutubeUrl(textBox1.Text))
            {
                Console.WriteLine("Is NOT valid yt link");
                if (!IsValidUrl(textBox1.Text))
                {
                    MessageBox.Show("The URL is invalid. Please enter a valid YouTube or HTTP link.");
                    return;
                }

            }

            // Deleted/Private Video Check
            string newLink = textBox1.Text;
            if (IsValidYoutubeUrl(textBox1.Text))
            {
                if (GetVideoName(RegisterYT(), textBox1.Text) == "Deleted")
                {
                    MessageBox.Show("The link you entered has been removed from Youtube or is invalid.");
                    textBox1.Text = "URL";
                    textBox4.Text = "Name (Optional)";
                    return;
                }

                if (GetVideoName(RegisterYT(), textBox1.Text) == "Private video")
                {
                    MessageBox.Show("The link you entered has been privated on Youtube or is invalid.");
                    textBox1.Text = "URL";
                    textBox4.Text = "Name (Optional)";
                    return;
                }

                if (textBox1.Text.Contains("&list="))
                {
                    newLink = textBox1.Text.Substring(0, textBox1.Text.IndexOf("&list="));
                }

            }

            // Get video name 
            string videoName;

            if (textBox4.Text != "" && textBox4.Text.Contains("Name (Optional)") == false)
            {
                videoName = textBox4.Text;
            }
            else
            {
                if (IsValidYoutubeUrl(textBox1.Text))
                {
                    Console.WriteLine("Is valid yt link");
                    videoName = GetVideoName(RegisterYT(), newLink);
                }
                else
                {
                    videoName = "Video " + index.ToString();
                }
            }

            // Get prefix
            prefix = textBox2.Text.Contains("https") ? textBox2.Text : "";

            // Add the URL from the text box to the list
            label1.Text = "Adding Link";
            urlList.Insert(index, "@" + prefix + newLink);
            nameList.Insert(index, videoName);

            // Clearing up
            textBox1.Text = "URL";
            textBox4.Text = "Name (Optional)";
            label1.Text = "Link added";
            index++;
            label3.Text = index.ToString();
        }

        private bool IsValidYoutubeUrl(string url)
        {
            bool returnVal = false;

            if (url.Contains("www.youtube.com/watch?v="))
            {
                returnVal = true;
            }

            return returnVal;
        }

        private bool IsValidUrl(string url)
        {
            bool isValidUrl = false;

            if (url.Contains("http") || url.Contains("https"))
            {
                isValidUrl = true;
            }

            return isValidUrl;
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
                return "Deleted video";
            }
        }

        private void GetVideoThumbnail(YouTubeService youtubeService, string url, string filePath)
        {
            // Parse video id from URL
            string videoId = Regex.Match(url, @"v=([^&]+)").Groups[1].Value;

            var request = youtubeService.Videos.List("snippet");
            request.Id = videoId;
            var response = request.Execute();

            if (response.Items.Count == 0)
            {
                // MessageBox.Show("The thumbnail for " + url + " could not be downloaded. Is the video deleted?");
                return;
            }

            var thumbnailUrl = response.Items[0].Snippet.Thumbnails.Default__.Url;
            var thumbnailName = $"{videoId}.jpg";
            var filePath2 = Path.Combine(filePath, "thumbnails");
            Directory.CreateDirectory(filePath2);
            var thumbnailPath = Path.Combine(filePath2, thumbnailName);

            thumbnailList.Add(thumbnailName);

            using (var client = new WebClient())
            {
                client.DownloadFile(thumbnailUrl, thumbnailPath);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "Writing to file, please wait";

            // Check if user prefix exists
            if (textBox2.Text.Contains("https"))
            {
                prefix = textBox2.Text;
            }

            if (textBox5.Text.Contains("File Name (No Need for .txt)") == false)
            {
                userInput = textBox5.Text;
            }

            // Create a new FolderBrowserDialog
            var folderBrowserDialog1 = new FolderBrowserDialog();

            string folderPath = "";

            // Show the dialog
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // Get the selected folder path
                folderPath = folderBrowserDialog1.SelectedPath;

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

                if (checkBox2.Checked == true)
                {
                    foreach (string url in urlList)
                    {

                        Console.WriteLine(url + " - Checkbox is checked | Valid Link Check - " + IsValidYoutubeUrl(url));
                        if (IsValidYoutubeUrl(url))
                        {
                            if (GetVideoName(RegisterYT(), url) != "Deleted video" || GetVideoName(RegisterYT(), url) != "Private video")
                            {
                                Console.WriteLine("Grabbing thumbnail");
                                GetVideoThumbnail(RegisterYT(), url, folderPath);
                            }
                        }
                    }
                }

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < urlList.Count; i++)
                    {
                        if (IsValidYoutubeUrl(urlList[i]))
                        {
                            if (GetVideoName(RegisterYT(), urlList[i]) == "Deleted video" || GetVideoName(RegisterYT(), urlList[i]) == "Private video" || GetVideoName(RegisterYT(), urlList[i]) == "")
                            {
                                if (checkBox3.Checked == true)
                                {
                                    urlList.Remove(urlList[i]);
                                }
                                else
                                {
                                    MessageBox.Show("The video " + urlList[i] + " is either deleted or private.");
                                }
                            }
                        }

                        sb.AppendLine(urlList[i]);
                        sb.AppendLine(nameList[i]);

                        if (IsValidYoutubeUrl(urlList[i]))
                        {
                            if (checkBox2.Checked == true)
                            {
                                if (thumbnailList.Count > i)
                                {
                                    sb.AppendLine(thumbnailList[i]);
                                }
                            }
                        }

                        sb.AppendLine("");
                        int num = int.Parse(label8.Text);
                        num++;
                        label8.Text = num.ToString();
                    }

                    sw.Write(sb.ToString());
                }

                textBox5.Text = "File Name (No Need for .txt)";
                MessageBox.Show("File has been successfully exported!");
                label1.Text = "File written successfully";
                label8.Text = "0";
            }
            catch (IOException)
            {
                MessageBox.Show("An error occurred while trying to access the file: Is the file open in another program?");
            }
        }

        public static string CreateLogFile()
        {
            // Get the application folder path
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            // Create the logs folder if it doesn't exist
            Directory.CreateDirectory(folderPath);

            // Get the current date and time
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Create the log file path with the timestamp as the filename
            string logFilePath = Path.Combine(folderPath, $"log_{timestamp}.txt");

            // Create the log file if it doesn't exist
            if (!File.Exists(logFilePath))
            {
                using (StreamWriter sw = File.CreateText(logFilePath))
                {
                    sw.WriteLine($"Log file created on {DateTime.Now.ToString()}.");
                }
            }
            return logFilePath;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "Attempting to grab playlist";

            // Get the playlist ID from the text box
            var playlistId = textBox3.Text;
            textBox3.Text = "Playlist ID";

            // Initialize the blocking collection to hold the results
            var playlistItemsBuffer = new BlockingCollection<PlaylistItem>();

            // Try playlist ID and throw error if Incorrect
            try
            {
                // Set up the request to retrieve the videos in the playlist
                var playlistItemsListRequest = RegisterYT().PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = playlistId;
                playlistItemsListRequest.MaxResults = 500;

                // Execute the request in parallel
                await Task.Run(async () =>
                {
                    var tasks = new List<Task>();

                    while (true)
                    {
                        // Execute the request
                        var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                        // Add the results to the blocking collection
                        foreach (var playlistItem in playlistItemsListResponse.Items)
                        {
                            playlistItemsBuffer.Add(playlistItem);
                        }

                        // Set the page token for the next request
                        playlistItemsListRequest.PageToken = playlistItemsListResponse.NextPageToken;

                        // If there are no more pages, break out of the loop
                        if (string.IsNullOrEmpty(playlistItemsListResponse.NextPageToken))
                        {
                            break;
                        }

                        // Execute the next request in parallel
                        var task = Task.Run(async () =>
                        {
                            await playlistItemsListRequest.ExecuteAsync();
                        });

                        tasks.Add(task);

                        // Limit the number of parallel requests
                        if (tasks.Count >= 4)
                        {
                            await Task.WhenAny(tasks);
                            tasks.RemoveAll(t => t.IsCompleted);
                        }
                    }

                    // Wait for any remaining requests to complete
                    await Task.WhenAll(tasks);
                });

                // Complete the blocking collection to signal that no more items will be added
                playlistItemsBuffer.CompleteAdding();
                label1.Text = "Playlist Grabbed";

                // Update the UI with the results (if needed)
            }
            catch (GoogleApiException ev)
            {
                if (ev.Error.Code == 404)
                {
                    MessageBox.Show("Invalid playlist ID. Is the playlist private? All playlists must be public to grab.");
                }
                else
                {
                    MessageBox.Show("An error occurred: " + ev.Message + "\n\nPlease check your internet connection or API key.");

                    // Log the exception to a file
                    string logFilePath = CreateLogFile();
                    string logMessage = string.Format("Error message: {0}\nStack trace: {1}\n\n", ev.Message, ev.StackTrace);
                    File.AppendAllText(logFilePath, logMessage);
                }
            }

            // Print the title and URL of each video in the playlist
            foreach (var playlistItem in playlistItemsBuffer.GetConsumingEnumerable())
            {

                string videoTitle = "";
                realVideoTitle = playlistItem.Snippet.Title;

                // Deleted/Private Video Check
                if (realVideoTitle == "Deleted")
                {
                    return;
                }

                if (realVideoTitle == "Private video")
                {
                    return;
                }

                if (checkBox1.Checked == true)
                {
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
            GC.Collect();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Text == "DARK MODE")
            {
                button5.Text = "LIGHT MODE";
                this.BackColor = SystemColors.ControlDarkDark;
                panel1.BackColor = SystemColors.ControlDarkDark;
                panel2.BackColor = SystemColors.ControlDarkDark;
                panel3.BackColor = SystemColors.ControlDarkDark;
            }
            else
            {
                button5.Text = "DARK MODE";
                this.BackColor = SystemColors.Control;
                panel1.BackColor = SystemColors.Control;
                panel2.BackColor = SystemColors.Control;
                panel3.BackColor = SystemColors.Control;
            }
        }
    }
}