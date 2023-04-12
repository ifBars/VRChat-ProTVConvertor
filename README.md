ProTVConvertor

ProTVConvertor is a program that allows users to convert YouTube video links, YouTube playlist links, and http and https video links into the correct format required for ProTV, a VRChat development addon that allows users to create playlists to watch in VRChat. With this program, you can convert all the links into a playlist, with names, in the correct format. The program also comes with a feature for downloading thumbnails, checking for updates automatically, and it can automatically remove any YouTube videos that have been deleted or privated, or any invalid YouTube video.

Installation

Download the setup.exe file from the ProTVConvertor repository.
Run the setup.exe file to install the program.
Run the ProTVConvertor desktop icon.

Usage

Enter a single, full YouTube URL to add one URL to the list of URLs.
Optionally, add a prefix to put before the YouTube links (e.g., https://nextnex.com/?url=).
Press "Add Link" to add the one link.
Paste a playlist ID and press "Load Playlist" to grab every video in the playlist. The playlist ID is after the "list=" in the YouTube playlist link. For example, in the link "https://www.youtube.com/playlist?list=PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn", you would paste "PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn".
To set a custom name for a single URL, delete the "[Name (Optional)]" and replace it with the name you want for the URL.
To set custom names for a playlist, tick "Use Custom Names" and load the playlist from the ID. It will then ask for a custom name for each song. If you do not want a custom name for that one, you can simply press enter.
To delete all URLs, press "Empty Links." This will not delete the file, but it will clear all current indexes and write over them every time you export to the same file.
If you are using URL Prefixes, make sure it is an HTTPS link like "https://nextnex.com/?url=", and make sure to put in your prefix before loading a playlist or link.
For any questions, comments, concerns, etc, contact P.Penguin#0420.

Quick Note: If you are using URL Prefixes, make sure it is an HTTPS link like https://nextnex.com/?url=. Also, make sure to put in your prefix before loading a playlist or link.

image

SOURCE CODE USAGE:

	1. Compile code and open the application: First make sure you have Visual Studio installed. Next, navigate to the folder containing ProTVConverter.sln, and double click the file. Once the file has opened, press Project, and Build Solution. Once you have compiled the code, double-click on the exe file, in the UrlArray\bin\Debug folder, to open the application.
	
	2. Add URLs to the list: Type or copy and paste the URL of a YouTube video or playlist into the “URL” field. If you wish, add a name for the video or playlist into the “Name (Optional)” field. Once you have entered all desired information, click the “Add Link” button. If there is an error with your URL, the application will display a message telling you to input a valid URL.

	3. Change the prefix (optional): If you wish to add a prefix to all of the URLs, type it into the “Prefix” field. If you are entering a full URL in the “Prefix” field, make sure it starts with “https” or else the application may not work.

	4. Track the number of URLs added: The “Indexes” field will tell you how many links have been added.

	5. Write the URLs and names to a file: Once you have added all the URLs and names you wish to include in the playlist, type a name for the file into the “File Name (No Need for .txt)” field. Once you have entered a name, click the “Write to File” button. The application will open a file dialog that allows you to select a folder and choose where to save the .txt file.

	6. Review the file: Once you have saved the .txt file, open it to review the list of URLs and names.

	7. Please note that this program requires a YouTube API key, which must be entered in the line that reads "YOUR_API_KEY_HERE". If you do not have a YouTube API key, the application will not work.
