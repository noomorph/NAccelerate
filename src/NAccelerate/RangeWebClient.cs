using System;
using System.Net;
using System.ComponentModel;

namespace NAccelerate
{
    internal class RangeWebClient : IDisposable
    {
        public RangeWebClient()
        {
            this.Wrapped = new RangeWebClientInternal();
            this.Status = RangeWebClientStatus.Initialized;
        }

        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs args)
        {
            this.Status = RangeWebClientStatus.Completed;
            ProgressPercentage = 100;
            if (DownloadFileCompleted != null)
                DownloadFileCompleted(this, args);
        }

        protected virtual void OnDownloadProgressChanged(System.Net.DownloadProgressChangedEventArgs args)
        {
            ProgressPercentage = args.ProgressPercentage;
            if (DownloadProgressChanged != null)
                DownloadProgressChanged(this, args);
        }

        protected RangeWebClientInternal Wrapped { get; private set; }

        public string Filename { get; set; }

        public int ProgressPercentage { get; set; }

        public RangeWebClientStatus Status { get; set; }

        public event AsyncCompletedEventHandler DownloadFileCompleted;

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public void DownloadFileAsync(Uri uri, string customSavePath = null)
        {
            this.Filename = customSavePath ?? Filename;
            this.ProgressPercentage = 0;

            Wrapped.DownloadProgressChanged += (s, e) => OnDownloadProgressChanged(e);
            Wrapped.DownloadFileCompleted += (s, e) => OnDownloadFileCompleted(e);
            Wrapped.DownloadFileAsync(uri, this.Filename);
            Status = RangeWebClientStatus.Running;
        }

        public void Dispose()
        {
            if (this.Wrapped != null)
                this.Wrapped.Dispose();
            this.Filename = null;
            this.From = null;
            this.To = null;
            this.ProgressPercentage = 0;
            this.Status = RangeWebClientStatus.Disposed;
        }

        public long? From
        {
            get
            {
                return Wrapped.From;
            }
            set
            {
                if (this.Status == RangeWebClientStatus.Running)
                    throw new InvalidOperationException("Cannot change range while downloading...");
                Wrapped.From = value;
            }
        }

        public long? To
        {
            get
            {
                return Wrapped.To;
            }
            set
            {
                if (this.Status == RangeWebClientStatus.Running)
                    throw new InvalidOperationException("Cannot change range while downloading...");
                Wrapped.To = value;
            }
        }
    }
}
