# ProTVConverter
 VRChat Youtube to ProTV Playlist Converter
 
 USAGE:
 
	-Run setup.exe to install, optionally create a shortcut for UrlArray.application
	-Enter a single, full youtube URL to add one URL to the list of URLS
	-Optionally, add a prefix to put before the youtube links (IE: https://nextnex.com/?url=)
	-Press add link to add the one link
	-Paste a playlist ID and press load playlist to grab every video in the playlist 
	-IE: The playlist ID is after the list= https://www.youtube.com/playlist?list=PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn
	You would paste PL9n07ws2xINsLzMHXJqpHhmpltpFNQgQn in this case
	-You can press empty links to delete all URL indexs at once, this does not delete the file
	It will clear all current indexes and write over them everytime you export to the same file
	-To set a custom name for a single url, delete the [Name (Optional)] and replace it with the name you want for the url
	-To set custom names for a playlist, tick Use Custom Names, and load the playlist from the ID
	It will then ask for a custom name for each song, if you do not want a custom name for that one, you can simply press enter
 	-For any questions, comments, concerns, etc, P.Penguin#0420
	
	-Quick Note: If you are using URL Prefixes, make sure it is an HTTPS link like https://nextnex.com/?url= as well as,
	Make sure to put in your prefix before loading a playlist or link

![image](https://user-images.githubusercontent.com/114284668/212461669-486d9c18-39f3-43a8-9be9-60d3dde40dda.png)

SOURCE CODE USAGE:

	1. Compile code and open the application: First make sure you have Visual Studio installed. Next, navigate to the folder containing ProTVConverter.sln, and double click the file. Once the file has opened, press Project, and Build Solution. Once you have compiled the code, double-click on the exe file, in the UrlArray\bin\Debug folder, to open the application.
	
	2. Add URLs to the list: Type or copy and paste the URL of a YouTube video or playlist into the “URL” field. If you wish, add a name for the video or playlist into the “Name (Optional)” field. Once you have entered all desired information, click the “Add Link” button. If there is an error with your URL, the application will display a message telling you to input a valid URL.

	3. Change the prefix (optional): If you wish to add a prefix to all of the URLs, type it into the “Prefix” field. If you are entering a full URL in the “Prefix” field, make sure it starts with “https” or else the application may not work.

	4. Track the number of URLs added: The “Indexes” field will tell you how many links have been added.

	5. Write the URLs and names to a file: Once you have added all the URLs and names you wish to include in the playlist, type a name for the file into the “File Name (No Need for .txt)” field. Once you have entered a name, click the “Write to File” button. The application will open a file dialog that allows you to select a folder and choose where to save the .txt file.

	6. Review the file: Once you have saved the .txt file, open it to review the list of URLs and names.

	7. Please note that this program requires a YouTube API key, which must be entered in the line that reads "YOUR_API_KEY_HERE". If you do not have a YouTube API key, the application will not work.
