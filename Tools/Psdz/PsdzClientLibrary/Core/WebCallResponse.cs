using System.Net;

namespace PsdzClientLibrary.Core
{
    public class WebCallResponse
    {
        public string Error { get; set; }

        public HttpStatusCode? HttpStatus { get; set; }

        public virtual bool IsSuccessful
        {
            get
            {
                if (HttpStatus.HasValue && HttpStatus.Value >= HttpStatusCode.OK)
                {
                    return HttpStatus.Value <= (HttpStatusCode)299;
                }
                return false;
            }
        }
    }

    public class WebCallResponse<T> : WebCallResponse
    {
        public T Response { get; set; }

        public override bool IsSuccessful
        {
            get
            {
                if (!base.IsSuccessful)
                {
                    if (!base.HttpStatus.HasValue)
                    {
                        return Response != null;
                    }
                    return false;
                }
                return true;
            }
        }
    }
}