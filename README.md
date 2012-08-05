NAccelerate
===========

Very simple WebClient wrapper that is able to do multi-threaded segmented downloads (like in download accelerators)

*License*: [MIT License](http://en.wikipedia.org/wiki/MIT_License)
*Version*: 0.1.0.1

How to use
-----------

'
using (var client = new AcceleratedWebClient()) {
	client.DownloadFileCompleted += /* your handler (after file was downloaded) */ ;
	client.DownloadProgressChanged += /* your handler (to show download progress) */ 
	client.DownloadFileAsync(new Uri(/* your URI to download */), fileToSave, 5 /* number of segments (threads) */);
}
'