using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Models
{
    public class BackendData<T>
    {
        public T Data { get; set; }

        public BackendStatus Status { get; set; }
    }
}
