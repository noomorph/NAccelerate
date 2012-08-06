using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace NAccelerate
{
    public class AcceleratedWebClient : IDisposable
    {
        private List<RangeWebClient> _webClients;
        private string _savePath;
        
        const int BUFFER_SIZE = 500000;
        const int MIN_FILE_SIZE = 500000;

        private int? GetFileSize(Uri uri)
        {
            WebRequest req = System.Net.HttpWebRequest.Create(uri);
            req.Method = "HEAD";
            using (WebResponse resp = req.GetResponse())
            {
                int contentLength;
                if (resp.Headers["Accept-Ranges"] != "bytes")
                    return null;
                if (int.TryParse(resp.Headers.Get("Content-Length"), out contentLength))
                    return contentLength;
            }
            return null;
        }

        private Tuple<long?, long?> GetRange(int index, int count, long size)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");
            if (count == 1)
                return new Tuple<long?, long?>(null, null);

            long minSize = size / (long)count;

            if (index == (count - 1))
                return new Tuple<long?, long?>(index * minSize, null);
            else
                return new Tuple<long?, long?>(index * minSize, (index + 1) * minSize - 1);
        }

        private void MergeParts()
        {
            using (var outputFileStream = File.OpenWrite(_savePath))
                foreach (var client in _webClients)
                {
                    using (var fileStream = File.OpenRead(client.Filename))
                        CopyStream(outputFileStream, fileStream);
                    File.Delete(client.Filename);
                }

        }

        private void CopyStream(Stream destination, Stream source)
        {
            int count;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, count);
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (_webClients.All(c => c.Status == RangeWebClientStatus.Completed))
                OnDownloadFileCompleted(e);
        }

        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs args)
        {
            MergeParts();
            if (DownloadFileCompleted != null)
                DownloadFileCompleted(this, args);
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs args)
        {
            var totalProgress = (double)_webClients.Sum(client => client.ProgressPercentage) / (double)_webClients.Count;
            var callArgs = new SegmentAwareProgressChangedEventArgs((int)Math.Round(totalProgress));
            if (DownloadProgressChanged != null)
                DownloadProgressChanged(this, callArgs);
        }

        public event AsyncCompletedEventHandler DownloadFileCompleted;

        public event SegmentAwareProgressChangedEventHandler DownloadProgressChanged;

        public void DownloadFileAsync(Uri uri, string savePath, int segmentsCount = 1)
        {
            if (segmentsCount < 1)
                throw new ArgumentOutOfRangeException("segmentsCount");

            int? size = GetFileSize(uri);

            if (!size.HasValue)
                segmentsCount = 1;
            else if (size.Value < MIN_FILE_SIZE)
                segmentsCount = 1;

            _savePath = savePath;
            using (File.Create(_savePath))
            {
                // Create empty file to make sure that no write restrictions
            }

            _webClients = new List<RangeWebClient>();

            for (int i = 0; i < segmentsCount; i++)
            {
                var client = new RangeWebClient();

                if (segmentsCount > 1)
                {
                    var range = GetRange(i, segmentsCount, size.Value);
                    client.From = range.Item1;
                    client.To = range.Item2;
                }

                client.DownloadProgressChanged += (s, e) => OnDownloadProgressChanged(e);
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.Filename = Path.ChangeExtension(_savePath, Path.GetExtension(_savePath) + "." + i.ToString("D3"));
                _webClients.Add(client);
            }

            _webClients.ForEach(client => client.DownloadFileAsync(uri));
        }

        public void Dispose()
        {
            if (_webClients != null)
                _webClients.ForEach(w => { if (w != null) w.Dispose(); });
            _webClients.Clear();
            _webClients = null;
        }
    }
}
