using System.Net;

namespace NAccelerate
{
    internal class RangeWebClientInternal : WebClient
    {
        protected override WebRequest GetWebRequest(System.Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            if (From.HasValue && To.HasValue)
                request.AddRange(From.Value, To.Value);
            else if (From.HasValue)
                request.AddRange(From.Value);
            return request;
        }

        public long? From { get; set; }
        public long? To { get; set; }
    }
}
