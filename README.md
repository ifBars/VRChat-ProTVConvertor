# ProTVConvertor

![Capture](https://user-images.githubusercontent.com/114284668/231539397-33625ca8-50d0-43b6-90b6-d9dacbd23e32.PNG)

![image](https://github.com/user-attachments/assets/e04e13fd-2b08-46f9-ad9b-520290c0a320)

ProTVConvertor is a program that allows users to convert YouTube video links, YouTube playlist links, and HTTP and HTTPS video links into the correct format required for ProTV, a VRChat development add-on that allows users to create playlists to watch in VRChat. The program uses threading to complete these tasks as fast as possible, especially on modern CPUs. With this program, you can convert all the links into a playlist, with names, in the correct format. The program also comes with a feature for downloading thumbnails, checking for updates automatically, and it can automatically remove any YouTube videos that have been deleted or private, or any invalid YouTube video.

## THERE IS NOW A WEB VERSION

[https://protv-convertor.onrender.com/](https://protv-convertor.onrender.com/)

## FAQS
 Q: Why did it only export x amount of videos out of the whole playlist?
 A: If you enable "Remove Invalid YT Links", the program automatically removes all invalid video links. Additionally, the program automatically checks for duplicate videos and 
 removes the duplicate.

 Q: What are the risks of using Fast Export?
 A: You can run the risk of only receiving part of the entire playlist's thumbnails. As well as you run the risk of the program crashing, however it is very rare. See [Issue #5](https://github.com/ifBars/VRChat-ProTVConvertor/releases/tag/v2.4.0) for comparison.

 Q: What does it mean if the "API Key Status" says "Invalid" or "0"?
 A: This means the YouTube API key, that the program is using, is invalid, or the app cannot connect to the YT API. If you are using your own API key, refer to the YouTube API portal. If you are using the latest release 
 build, please create an issue on the repository.

## Requirements
 1. Windows 7/8/10/11
 2. [Microsoft .NET Desktop Runtime 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Installation
1. Download the latest release.zip file from the ProTVConvertor repository.
2. Run the ProTVConvetor.exe file to open the program.

## Updating
To update ProTVConvertor, follow these steps.
1. Uninstall your current installion of ProTVConvertor if necessary, some older releases include an installer, however newever versions come as a folder with the exe to run the program inside.
2. Download the latest/desired version of ProTVConvertor.
3. Unzip the files to a folder.
4. Run the ProTVConvetor.exe file, or setup.exe if you are installing an older version, to open the program.
5. If you are on an older version, after installion, you may delete the folder, and run the program by searching "ProTvConvert" in your search bar.

## Usage

1. Enter a single, full YouTube URL to add one URL to the list of URLs.
2. Optionally, add a prefix to put before the YouTube links (e.g., https://nextnex.com/?url=).
3. Press "Add Link" to add the one link.
4. Paste a playlist ID and press "Load Playlist" to grab every video in the playlist. The playlist ID is after the "list=" in the YouTube playlist link. For example, in the link "https://www.youtube.com/playlist?list=PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn", you would paste "PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn".
5. To set a custom name for a single URL, delete the "[Name (Optional)]" and replace it with the name you want for the URL.
6. To set custom names for a playlist, tick "Use Custom Names" and load the playlist from the ID. It will then ask for a custom name for each song. If you do not want a custom name for that one, you can simply press enter.
7. To delete all URLs, press "Empty Links." This will not delete the file, but it will clear all current indexes and write over them every time you export to the same file.
8. If you are using URL Prefixes, make sure it is an HTTPS link like "https://nextnex.com/?url=", and make sure to put in your prefix before loading a playlist or link.
9. For any questions, comments, concerns, etc., contact P.Penguin#7468

Quick Note: If you are using URL Prefixes, make sure it is an HTTPS link, like https://nextnex.com/?url=. Also, make sure to put in your prefix before loading a playlist or link.

## SOURCE CODE USAGE:

	1. Compile code and open the application: First make sure you have Visual Studio installed. Next, navigate to the folder containing ProTVConvertor.sln, and double-click the file. Once the file has opened, press Project, and Build Solution. Once you have compiled the code, double-click on the EXE file, in the UrlArray\bin\Debug folder, to open the application.
	
	2. Add URLs to the list: Type or copy and paste the URL of a YouTube video or playlist into the “URL” field. If you wish, add a name for the video or playlist into the “Name (Optional)” field. Once you have entered all the desired information, click the “Add Link” button. If there is an error with your URL, the application will display a message telling you to input a valid URL.

	3. Change the prefix (optional): If you wish to add a prefix to all the URLs, type it into the “Prefix” field. If you are entering a full URL in the “Prefix” field, make sure it starts with “HTTP” or else the application may not work.

	4. Track the number of URLs added: The “Indexes” field will tell you how many links have been added.

	5. Write the URLs and names to a file: Once you have added all the URLs and names you wish to include in the playlist, type a name for the file into the “File Name (No Need for .txt)” field. Once you have entered a name, click the “Write to File” button. The application will open a file dialog that allows you to select a folder and choose where to save the .txt file.

	6. Review the file: Once you have saved the .txt file, open it to review the list of URLs and names.

	7. Please note that this program requires a YouTube API key, which must be entered in the line that reads "YOUR_API_KEY_HERE". If you do not have a YouTube API key, the application will not work.
