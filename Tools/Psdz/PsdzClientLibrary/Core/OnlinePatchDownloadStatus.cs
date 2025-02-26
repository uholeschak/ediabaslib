using System.Net;

namespace PsdzClientLibrary.Core
{
    public class OnlinePatchDownloadStatus
    {
        public enum PatchFileMode
        {
            Online,
            Offline,
            Cached
        }

        public string Category { get; set; }

        public string DownloadStatusDescription
        {
            get
            {
                string result = "UnexpectedApplicationException using cached files when available";
                if (FileMode == PatchFileMode.Offline)
                {
                    result = "Offline";
                }
                else if (FileMode == PatchFileMode.Cached)
                {
                    result = "cached";
                }
                else if (HttpStatus.HasValue)
                {
                    result = HttpStatus.ToString();
                }
                return result;
            }
        }

        public string Error { get; set; }

        public PatchFileMode FileMode { get; set; }

        public string FileName { get; set; }

        public HttpStatusCode? HttpStatus { get; set; }

        public bool ShouldAddWarningToProtocol
        {
            get
            {
                if (FileMode == PatchFileMode.Online)
                {
                    if (HttpStatus.HasValue && (HttpStatus == HttpStatusCode.OK || HttpStatus == HttpStatusCode.NotFound))
                    {
                        return Error != null;
                    }
                    return true;
                }
                return false;
            }
        }

        public string WarningDescription
        {
            get
            {
                if (HttpStatus.HasValue)
                {
                    return HttpStatus.ToString();
                }
                return Error;
            }
        }
    }
}