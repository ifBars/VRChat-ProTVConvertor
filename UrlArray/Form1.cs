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
using System.Drawing;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Linq;

namespace ProTVConverter
{
    public partial class Form1 : Form
    {
        // -----------------------
        // Class Initialization
        // -----------------------

        // Initialize the Version class
        Version v = new Version("ifBars", "ProTVConvertor");

        // Initialize YouTube API service (moved to the bottom for clarity)
        private YouTubeService youTubeAPI = null;

        // -----------------------
        // Lists for URLs, Names, and Thumbnails
        // -----------------------

        // List for storing URLs
        List<string> urlList = new List<string>();

        // List for storing names
        List<string> nameList = new List<string>();

        // List for storing thumbnails
        List<string> thumbnailList = new List<string>();

        // -----------------------
        // Variables related to URLs
        // -----------------------

        // Index for lists
        int index = 0;

        // Prefix added before URLs
        string prefix = "";

        // -----------------------
        // File-related Variables
        // -----------------------

        // Path of the file
        string filePath;

        // Name of file input
        string userInput = "urls";

        // Actual video title
        public static string realVideoTitle;

        // -----------------------
        // Export-related Variables
        // -----------------------

        // Indicates whether the export is a fast one
        public bool fastExport = false;

        // Indicates whether the system is currently exporting
        public bool isExporting = false;

        // Indicates wether the current set of URLS has already been scanned for invalid links
        public bool hasRemoved = false;

        // -----------------------
        // API Configuration
        // -----------------------

        // Load the API key from a secure place (for the sake of time, it remains here)
        // TODO: Consider loading this from a configuration file or environment variable
        string keyAPI = "AIzaSyAcgvFEG2hJSRLhqpa8ocMTmxq4Og7Fcnw";

        string logFilePath = string.Empty;
        bool doLog = false;

        // Initialize Form
        public Form1()
        {
            InitializeComponent();
            youTubeAPI = RegisterYT();
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
            prefix = "";
            if (isExporting == false)
            {
                // Console.WriteLine("Link = " + textBox1.Text);

                bool valid = IsValidYoutubeUrl(textBox1.Text);

                // Validate input URL
                if (!valid)
                {
                    // Console.WriteLine("Is NOT valid yt link");
                    if (!IsValidUrl(textBox1.Text))
                    {
                        MessageBox.Show("The URL is invalid. Please enter a valid YouTube or HTTP link.");
                        return;
                    }

                }

                // Deleted/Private Video Check
                string newLink = textBox1.Text;
                if (valid)
                {
                    string name = GetVideoName(youTubeAPI, textBox1.Text);
                    if (name == "Deleted")
                    {
                        MessageBox.Show("The link you entered has been removed from Youtube or is invalid.");
                        textBox1.Text = "URL";
                        textBox4.Text = "Name (Optional)";
                        return;
                    }

                    if (name == "Private video")
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
                    if (valid)
                    {
                        // Console.WriteLine("Is valid yt link");
                        videoName = GetVideoName(youTubeAPI, newLink);
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

                hasRemoved = false;
            }
            else
            {
                MessageBox.Show("Program is currently exporting, please wait until it is finished!");
            }
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
            try
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
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    MessageBox.Show("Bad video link/request!");
                    return "API Error";
                }
                else if (ex.Message.Contains("API key expired"))
                {
                    MessageBox.Show("API key is expired!");
                    return "API Error";
                }
                else if (ex.Message.Contains("quota"))
                {
                    MessageBox.Show("Daily API Limit Reached. Please try again tomorrow.");
                    return "API Error";
                }
                else
                {
                    MessageBox.Show("An error occurred while trying to access the YouTube API: " + ex.Message);
                    return "API Error";
                }
            }
        }

        private string GetVideoThumbnail(YouTubeService youtubeService, string url, string filePath)
        {
            try
            {
                // Parse video id from URL
                string videoId = Regex.Match(url, @"v=([^&]+)").Groups[1].Value;

                var request = youtubeService.Videos.List("snippet");
                request.Id = videoId;
                var response = request.Execute();

                if (response.Items.Count == 0)
                {
                    // MessageBox.Show("The thumbnail for " + url + " could not be downloaded. Is the video deleted?");
                    return null;
                }

                var thumbnailUrl = response.Items[0].Snippet.Thumbnails.Default__.Url;
                var thumbnailName = $"{videoId}.jpg";
                var filePath2 = Path.Combine(filePath, "thumbnails");
                Directory.CreateDirectory(filePath2);
                var thumbnailPath = Path.Combine(filePath2, thumbnailName);

                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(thumbnailUrl, thumbnailPath);
                    }

                    int num = Directory.GetFiles(filePath2).Length;
                    // Invoke the method that updates the label9 control from the UI thread
                    label9.Invoke(new Action(() =>
                    {
                        label9.Text = num.ToString();
                        this.Update();
                    }));
                    return thumbnailName;
                }
                catch (Exception ex)
                {
                    // log the error or handle it as appropriate
                    Console.WriteLine($"Error downloading thumbnail for {url}: {ex.Message}");
                    return null;
                }
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    MessageBox.Show("Bad video link/request!");
                    return "API Error";
                }
                else if (ex.Message.Contains("API key expired"))
                {
                    MessageBox.Show("API key is expired!");
                    return "API Error";
                }
                else if (ex.Message.Contains("quota"))
                {
                    MessageBox.Show("Daily API Limit Reached. Please try again tomorrow.");
                    return "API Error";
                }
                else
                {
                    MessageBox.Show("An error occurred: " + ex.Message + "\n\nPlease check your internet connection or API key.");

                    if (doLog)
                    {
                        // Log the exception to a file
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(string.Format("Error message: {0}\nStack trace: {1}\n\n", ex.Message, ex.StackTrace));
                        sb.AppendLine("API KEY VALID " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                        File.AppendAllText(logFilePath, sb.ToString());
                    }

                    return "API Error";
                }
            }
        }

        public bool checkApi()
        {
            var apiKey = keyAPI;
            string videoName = GetVideoName(youTubeAPI, "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            if (videoName == "Rick Astley - Never Gonna Give You Up (Official Music Video)")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            int count = 0;

            if (isExporting == false)
            {
                isExporting = true;
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;

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
                else
                {
                    // MessageBox.Show("Error: No folder path selected, please select a path for the file!");
                    isExporting = false;
                    checkBox2.Enabled = true;
                    checkBox3.Enabled = true;
                    return;
                }

                // Make sure the lists are the same length
                if (urlList.Count != nameList.Count)
                {
                    MessageBox.Show("Error: lists are not the same length");

                    if (doLog)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Error message: Error: lists are not the same length");
                        sb.AppendLine("URL " + urlList.Count + " NAME " + nameList.Count + " THUMBNAIL " + thumbnailList.Count);
                        File.AppendAllText(logFilePath, sb.ToString());
                    }

                    isExporting = false;
                    checkBox2.Enabled = true;
                    checkBox3.Enabled = true;
                    return;
                }

                if (checkBox3.Checked == true && !hasRemoved)
                {
                    hasRemoved = true;
                    int scannedIndex = urlList.Count;
                    Invoke(new Action(() => label1.Text = "Removing invalid links " + scannedIndex));

                    if (fastExport == false)
                    {
                        for (int i = 0; i < urlList.Count; i++)
                        {
                            if (IsValidYoutubeUrl(urlList[i]))
                            {
                                string videoName = nameList[i];

                                if (videoName == "Deleted video" || videoName == "Private video" || videoName == "" || videoName == "API Error")
                                {
                                    urlList.RemoveAt(i);
                                    nameList.RemoveAt(i);
                                    index = index - 1;
                                    label3.Text = index.ToString();
                                    Interlocked.Increment(ref count);
                                    Invoke(new Action(() => label20.Text = count.ToString()));
                                }
                            }

                            // Update the scannedIndex counter
                            scannedIndex -= 1;
                            Invoke(new Action(() => label1.Text = "Removing invalid links " + scannedIndex));
                        }
                    }
                    else
                    {
                        var itemsToRemove = new ConcurrentBag<int>();
                        await Task.Run(() =>
                        {


                            Parallel.For(0, urlList.Count, i =>
                            {
                                if (IsValidYoutubeUrl(urlList[i]))
                                {
                                    string videoName = nameList[i];

                                    if (videoName == "Deleted video" || videoName == "Private video" || videoName == "" || videoName == "API Error")
                                    {
                                        itemsToRemove.Add(i);
                                        Interlocked.Increment(ref count);
                                        Invoke(new Action(() => label20.Text = count.ToString()));
                                    }
                                }

                                // Update the scannedIndex counter
                                Interlocked.Decrement(ref scannedIndex);
                                Invoke(new Action(() => label1.Text = "Removing invalid links " + scannedIndex));
                            });

                        });

                        await Task.Run(() =>
                            {
                                // Remove the items that need to be removed
                                foreach (var i in itemsToRemove.OrderByDescending(x => x))
                                {
                                    urlList.RemoveAt(i);
                                    nameList.RemoveAt(i);
                                    index--;
                                }
                            });

                        label3.Text = index.ToString();
                    }

                    // Update the label with the final scannedIndex value
                    Invoke(new Action(() => label1.Text = "Scan complete " + urlList.Count + " links exported."));
                }

                if (fastExport == false)
                {
                    try
                    {

                        if (checkBox2.Checked == true)
                        {
                            label1.Text = "Downloading thumbnails";
                            await Task.Run(() =>
                            {
                                for (int i = 0; i < urlList.Count; i++)
                                {
                                    if (IsValidYoutubeUrl(urlList[i]))
                                    {
                                        string name = GetVideoName(youTubeAPI, urlList[i]);
                                        if (name != "Deleted video" || name != "Private video" || name != "API Error")
                                        {
                                            GetVideoThumbnail(youTubeAPI, urlList[i], folderPath);
                                        }
                                    }
                                }
                            });
                        }

                        List<string> invalidUrls = new List<string>();

                        label1.Text = "Packing videos to file";
                        await Task.Run(() =>
                        {
                            StringBuilder sb = new StringBuilder();

                            for (int i = 0; i < urlList.Count; i++)
                            {

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
                                Interlocked.Increment(ref count);
                                int num = Int32.Parse(count.ToString()) - Int32.Parse(label20.Text);
                                Invoke(new Action(() => label8.Text = num.ToString()));
                            }

                            using (StreamWriter sw = new StreamWriter(filePath))
                            {
                                sw.Write(sb.ToString());
                            }
                        });

                        textBox5.Text = "File Name (No Need for .txt)";
                        Invoke(new Action(() => MessageBox.Show("File has been successfully exported to " + userInput + ".txt")));
                        label1.Text = "File written successfully";
                        isExporting = false;
                        checkBox2.Enabled = true;
                        checkBox3.Enabled = true;
                        label8.Text = "0";
                        label9.Text = "0";
                        label20.Text = "0";
                    }
                    catch (IOException)
                    {
                        Invoke(new Action(() => MessageBox.Show("An error occurred while trying to access the file: Is the file open in another program?")));
                    }
                }
                else
                {
                    try
                    {
                        if (checkBox2.Checked == true)
                        {
                            label1.Text = "Downloading thumbnails";
                            int numThumbnails = 0;
                            var thumbnailNames = new List<string>();
                            var semaphore = new SemaphoreSlim(5); // limit to 4 concurrent downloads
                            foreach (var url in urlList)
                            {
                                if (IsValidYoutubeUrl(url))
                                {
                                    string name = GetVideoName(youTubeAPI, url);
                                    if (name != "Deleted video" || name != "Private video" || name != "API Error")
                                    {
                                        await semaphore.WaitAsync(); // wait until there's an available slot
                                        try
                                        {
                                            var thumbnailName = GetVideoThumbnail(youTubeAPI, url, folderPath);
                                            if (thumbnailName != null)
                                            {
                                                lock (thumbnailNames) thumbnailNames.Add(thumbnailName);
                                                Interlocked.Increment(ref numThumbnails);

                                                using (var sw = new StreamWriter(filePath, true))
                                                {
                                                    StringBuilder sb = new StringBuilder();

                                                    sb.AppendLine(url);
                                                    sb.AppendLine(nameList[urlList.IndexOf(url)]);
                                                    sb.AppendLine(thumbnailName);
                                                    sb.AppendLine("");

                                                    string entry = sb.ToString();
                                                    Interlocked.Increment(ref count);
                                                    int num = Int32.Parse(count.ToString()) - Int32.Parse(label20.Text);
                                                    Invoke(new Action(() => label8.Text = num.ToString()));

                                                    await sw.WriteAsync(entry);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            semaphore.Release(); // release the slot
                                        }
                                    }
                                }
                            }
                            label9.Text = numThumbnails.ToString();
                            thumbnailList.AddRange(thumbnailNames);
                        }
                        else
                        {
                            // Code for writing to the txt file when checkbox2 is not checked
                            using (var sw = new StreamWriter(filePath))
                            {
                                SemaphoreSlim semaphore = new SemaphoreSlim(1); // Limit to 1 concurrent write operation
                                var writeTasks = new List<Task>();
                                for (int i = 0; i < urlList.Count; i++)
                                {
                                    int index = i; // Create a local copy of i for the lambda expression
                                    writeTasks.Add(Task.Run(async () =>
                                    {
                                        StringBuilder sb = new StringBuilder(); // Create a new StringBuilder for each iteration

                                        sb.AppendLine(urlList[index]);
                                        sb.AppendLine(nameList[index]);
                                        sb.AppendLine("");
                                        string entry = sb.ToString();
                                        Interlocked.Increment(ref count);
                                        int num = Int32.Parse(count.ToString()) - Int32.Parse(label20.Text);
                                        Invoke(new Action(() => label8.Text = num.ToString()));

                                        // Acquire the semaphore to write to the file
                                        await semaphore.WaitAsync();
                                        try
                                        {
                                            await sw.WriteAsync(entry);
                                        }
                                        finally
                                        {
                                            // Release the semaphore
                                            semaphore.Release();
                                        }
                                    }));
                                }
                                await Task.WhenAll(writeTasks.ToArray()); // wait for all writes to complete
                            }
                        }

                        textBox5.Text = "File Name (No Need for .txt)";
                        MessageBox.Show("File has been successfully exported!");
                        label1.Text = "File written successfully";
                        label8.Text = "0";
                        label9.Text = "0";
                        label20.Text = "0";
                        isExporting = false;
                        checkBox2.Enabled = true;
                        checkBox3.Enabled = true;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("An error occurred while trying to access the file: Is the file open in another program?");
                    }
                }
            }
            else
            {
                MessageBox.Show("Program is currently exporting, please wait until it is finished!");
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

        public string GetPlaylistID(string entry)
        {
            string playlistID = "";

            if (entry.Contains("&si="))
            {
                int index = entry.IndexOf("&si=");
                entry.Substring(0, index);
            }

            // Check if the entry is a playlist ID
            if (entry.Length == 34)
            {
                playlistID = entry;
            }

            // Check if the entry is a full playlist URL
            else if (entry.Contains("youtube.com/playlist?list="))
            {
                var uri = new Uri(entry);
                var query = uri.Query;
                if (query.StartsWith("?"))
                {
                    query = query.Substring(1);
                }
                var parameters = query.Split('&');
                foreach (var parameter in parameters)
                {
                    var pair = parameter.Split('=');
                    if (pair.Length == 2 && pair[0] == "list")
                    {
                        playlistID = pair[1];
                        break;
                    }
                }
            }

            // Check if the entry is a playlist URL parameter
            else if (entry.Contains("st="))
            {
                int index = entry.IndexOf("st=") + 3;
                var playlistIDParam = entry.Substring(index);
                playlistID = playlistIDParam;
            }

            // Input format not recognized
            return playlistID;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            prefix = "";
            if (isExporting == false)
            {
                label1.Text = "Attempting to grab playlist";

                // Get the playlist ID from the text box
                var playlistId = GetPlaylistID(textBox3.Text);
                textBox3.Text = "Playlist ID Or Link";
                Console.WriteLine("Playlist ID - " + playlistId);

                // Initialize the blocking collection to hold the results
                var playlistItemsBuffer = new BlockingCollection<PlaylistItem>();

                // Try playlist ID and throw error if Incorrect
                try
                {
                    // Set up the request to retrieve the videos in the playlist
                    var playlistItemsListRequest = youTubeAPI.PlaylistItems.List("snippet");
                    playlistItemsListRequest.PlaylistId = playlistId;
                    playlistItemsListRequest.MaxResults = 1000;

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
                        return;
                    }
                    else if (ev.HttpStatusCode == HttpStatusCode.BadRequest)
                    {
                        MessageBox.Show("Invalid playlist ID/Request. Is the playlist link correct? All playlists must be public to grab.");
                        return;
                    }
                    else if (ev.Message.Contains("API key expired"))
                    {
                        MessageBox.Show("API key is expired!");
                        return;
                    }
                    else if (ev.Message.Contains("quota"))
                    {
                        MessageBox.Show("Daily API Limit Reached. Please try again tomorrow.");
                        return;
                    }
                    else
                    {
                        MessageBox.Show("An error occurred: " + ev.Message + "\n\nPlease check your internet connection or API key.");

                        if (doLog)
                        {
                            // Log the exception to a file
                            StringBuilder sb = new StringBuilder();
                            string logMessage = string.Format("Error message: {0}\nStack trace: {1}\n\n", ev.Message, ev.StackTrace);
                            sb.AppendLine(logMessage);
                            sb.AppendLine("API KEY " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                            File.AppendAllText(logFilePath, sb.ToString());
                        }

                        return;
                    }
                }

                // Print the title and URL of each video in the playlist
                foreach (PlaylistItem playlistItem in playlistItemsBuffer.GetConsumingEnumerable())
                {

                    string videoTitle = "";
                    realVideoTitle = playlistItem.Snippet.Title;

                    if (checkBox3.Checked == true)
                    {
                        // Deleted/Private Video Check
                        if (realVideoTitle == "Deleted")
                        {
                            if (doLog)
                            {
                                StringBuilder sb = new StringBuilder();
                                string logMessage = string.Format("Video deleted so was not added, id: " + playlistItem.Snippet.ResourceId.VideoId);
                                sb.AppendLine(logMessage);
                                sb.AppendLine("API KEY " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                                File.AppendAllText(logFilePath, sb.ToString());
                            }
                            continue;
                        }

                        if (realVideoTitle == "Private video")
                        {
                            if (doLog)
                            {
                                StringBuilder sb = new StringBuilder();
                                string logMessage = string.Format("Video privated so was not added, id: " + playlistItem.Snippet.ResourceId.VideoId);
                                sb.AppendLine(logMessage);
                                sb.AppendLine("API KEY " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                                File.AppendAllText(logFilePath, sb.ToString());
                            }
                            continue;
                        }
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

                    // Get prefix
                    prefix = textBox6.Text.Contains("https") ? textBox6.Text : "";

                    var videoId = playlistItem.Snippet.ResourceId.VideoId;
                    var videoUrl = "https://www.youtube.com/watch?v=" + videoId;
                    urlList.Insert(index, "@" + prefix + videoUrl);
                    nameList.Insert(index, videoTitle);
                    index++;
                    label3.Text = urlList.Count.ToString();
                    hasRemoved = false;
                    this.Update();
                }
            }
            else
            {
                MessageBox.Show("Program is currently exporting, please wait until it is finished!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("WARNING: This will remove all previously entered links, do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                // Clear all URLS
                urlList.Clear();
                nameList.Clear();
                index = 0;
                label3.Text = index.ToString();
            }
            else
            {
                return;
            }
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
                panel4.BackColor = SystemColors.ControlDarkDark;
                panel5.BackColor = SystemColors.ControlDarkDark;
            }
            else
            {
                button5.Text = "DARK MODE";
                this.BackColor = SystemColors.Control;
                panel1.BackColor = SystemColors.Control;
                panel2.BackColor = SystemColors.Control;
                panel3.BackColor = SystemColors.Control;
                panel4.BackColor = SystemColors.Control;
                panel5.BackColor = SystemColors.Control;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (IsInternetAvailable() == true)
            {
                label14.Text = "Connected";
                label14.ForeColor = Color.DarkGreen;
            }
            else
            {
                label14.Text = "Disconnected";
                label14.ForeColor = Color.Red;
            }

            if (checkApi() == true)
            {
                label13.Text = "Working";
                label13.ForeColor = Color.DarkGreen;
            }
            else
            {
                label13.Text = "Invalid";
                label13.ForeColor = Color.Red;
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (isExporting == false)
            {
                if (fastExport == false)
                {
                    DialogResult result = MessageBox.Show("WARNING: This can cause the program to freeze during export when exporting large amounts of links, do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        fastExport = true;
                        button7.Text = "Disable Fast Export";
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    fastExport = false;
                    button7.Text = "Enable Fast Export";
                }
            }
            else
            {
                MessageBox.Show("Program is currently exporting to file, please enable/disable fast mode after.");
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            var updateTask = v.checkUpdate();
            string update = await updateTask;
            MessageBox.Show(update);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (isExporting == true)
            {
                MessageBox.Show("Program is currently exporting, please wait until it is finished!");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (isExporting == true)
            {
                MessageBox.Show("Program is currently exporting, please wait until it is finished!");
            }
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            // Create an instance of the OpenFileDialog class
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the properties of the file dialog
            openFileDialog.Title = "Select a previously exported playlist txt file";
            openFileDialog.Filter = "Text Files|*.txt"; // Filter for text files only

            // Show the file dialog and check if the user clicked the OK button
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;

                // Use async/await to perform file reading and processing asynchronously
                await Task.Run(() =>
                {
                    using (StreamReader reader = new StreamReader(selectedFilePath))
                    {
                        string url = null;
                        string thumbnail = null;
                        string name = null;

                        // Read and process each line until the end of the file
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            // If the line is empty, it's time to add collected data to lists
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                if (url != null && name != null)
                                {
                                    urlList.Add(url);
                                    nameList.Add(name);
                                    thumbnailList.Add(thumbnail); // Add thumbnail regardless, even if it's null

                                    // Reset temporary variables
                                    url = null;
                                    thumbnail = null;
                                    name = null;
                                }
                            }
                            else if (line.Contains("https")) // Assuming this denotes the URL
                            {
                                url = line;
                            }
                            else if (line.Contains(".jpg") || line.Contains(".png")) // Assuming this denotes the thumbnail
                            {
                                thumbnail = line;
                            }
                            else // Assuming it's the name
                            {
                                name = line;
                            }
                        }

                        // Add the last entry if it exists
                        if (url != null && name != null)
                        {
                            urlList.Add(url);
                            nameList.Add(name);
                            thumbnailList.Add(thumbnail); // Add thumbnail regardless, even if it's null
                        }
                    }
                });

                // Update the UI with the result after processing is complete
                label3.Text = urlList.Count.ToString();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (logFilePath == string.Empty)
            {
                logFilePath = CreateLogFile();
            }

            doLog = checkBox4.Checked;
        }
    }
}