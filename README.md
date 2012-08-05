NAccelerate
===========

Very simple WebClient wrapper that is able to do multi-threaded segmented downloads (like in download accelerators)

*License*: [MIT License](http://en.wikipedia.org/wiki/MIT_License)

*Version*: 0.1.0.1

How to use
-----------

```
using (var client = new AcceleratedWebClient()) {
	client.DownloadFileCompleted += file_Completed /* your handler (after file was downloaded) */ ;
	client.DownloadProgressChanged += file_ProgressChanged /* your handler (to show download progress) */ 
	client.DownloadFileAsync(new Uri(uriToDownload), fileToSave, 5 /* number of segments (threads) */);
}
```