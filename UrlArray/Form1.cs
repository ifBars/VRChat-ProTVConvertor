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

        // Initialize Version Class
        Version v = new Version("ifBars", "ProTVConvertor");
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
        // Register fast export variable
        public bool FastE = false;
        // Register isExporting variable
        public bool isExporting = false;

        // INPUT YOUR API KEY HERE
        string keyAPI = "YOUR_API_KEY_HERE";

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
            if (isExporting == false)
            {
                // Console.WriteLine("Link = " + textBox1.Text);

                // Validate input URL
                if (!IsValidYoutubeUrl(textBox1.Text))
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
                        // Console.WriteLine("Is valid yt link");
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
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest && ex.Message.Contains("API key expired"))
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
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest && ex.Message.Contains("API key expired"))
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

                    // Log the exception to a file
                    string logFilePath = CreateLogFile();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(string.Format("Error message: {0}\nStack trace: {1}\n\n", ex.Message, ex.StackTrace));
                    sb.AppendLine("API KEY VALID " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                    File.AppendAllText(logFilePath, sb.ToString());
                    return "API Error";
                }
            }
        }

        public bool checkApi()
        {
            var apiKey = keyAPI;
            string videoName = GetVideoName(RegisterYT(), "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
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
                    string logFilePath = CreateLogFile();
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Error message: Error: lists are not the same length");
                    sb.AppendLine("URL " + urlList.Count + " NAME " + nameList.Count + " THUMBNAIL " + thumbnailList.Count);
                    File.AppendAllText(logFilePath, sb.ToString());
                    isExporting = false;
                    checkBox2.Enabled = true;
                    checkBox3.Enabled = true;
                    return;
                }

                if (checkBox3.Checked == true)
                {
                    int scannedIndex = urlList.Count;
                    Invoke(new Action(() => label1.Text = "Removing invalid links " + scannedIndex));

                    if (FastE == false)
                    {
                        for (int i = 0; i < urlList.Count; i++)
                        {
                            if (IsValidYoutubeUrl(urlList[i]))
                            {
                                string videoName = GetVideoName(RegisterYT(), urlList[i]);

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
                                    string videoName = GetVideoName(RegisterYT(), urlList[i]);

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
                                Parallel.ForEach(itemsToRemove.OrderByDescending(x => x), i =>
                                {
                                    urlList.RemoveAt(i);
                                    nameList.RemoveAt(i);
                                    Interlocked.Decrement(ref index);
                                });
                            });

                        label3.Text = index.ToString();
                    }

                    // Update the label with the final scannedIndex value
                    Invoke(new Action(() => label1.Text = "Scan complete " + urlList.Count + " links exported."));
                }

                if (FastE == false)
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
                                        if (GetVideoName(RegisterYT(), urlList[i]) != "Deleted video" || GetVideoName(RegisterYT(), urlList[i]) != "Private video" || GetVideoName(RegisterYT(), urlList[i]) != "API Error")
                                        {
                                            GetVideoThumbnail(RegisterYT(), urlList[i], folderPath);
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
                                Invoke(new Action(() => label8.Text = count.ToString()));
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
                            var semaphore = new SemaphoreSlim(4); // limit to 4 concurrent downloads
                            foreach (var url in urlList)
                            {
                                if (IsValidYoutubeUrl(url))
                                {
                                    if (GetVideoName(RegisterYT(), url) != "Deleted video" || GetVideoName(RegisterYT(), url) != "Private video" || GetVideoName(RegisterYT(), url) != "APPI Error")
                                    {
                                        await semaphore.WaitAsync(); // wait until there's an available slot
                                        try
                                        {
                                            var thumbnailName = GetVideoThumbnail(RegisterYT(), url, folderPath);
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
                                                    Invoke(new Action(() => label8.Text = count.ToString()));

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
                                        Invoke(new Action(() => label8.Text = count.ToString()));

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
                        return;
                    }
                    else if (ev.HttpStatusCode == HttpStatusCode.BadRequest || ev.Message.Contains("API key expired"))
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

                        // Log the exception to a file
                        string logFilePath = CreateLogFile();
                        StringBuilder sb = new StringBuilder();
                        string logMessage = string.Format("Error message: {0}\nStack trace: {1}\n\n", ev.Message, ev.StackTrace);
                        sb.AppendLine(logMessage);
                        sb.AppendLine("API KEY VALID " + checkApi().ToString() + " INTERNET " + IsInternetAvailable().ToString());
                        File.AppendAllText(logFilePath, sb.ToString());
                        return;
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
                if (FastE == false)
                {
                    DialogResult result = MessageBox.Show("WARNING: This can cause the program to freeze during export when exporting large amounts of links, do you want to continue?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        FastE = true;
                        button7.Text = "Disable Fast Export";
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    FastE = false;
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
    }
}