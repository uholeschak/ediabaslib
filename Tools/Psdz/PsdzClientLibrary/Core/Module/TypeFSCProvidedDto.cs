namespace BMW.Rheingold.InfoProvider.SWT.DTOs
{
    public class TypeFSCProvidedDto
    {
        public string vinShort { get; set; }

        public FscItemTypeDto fscItem { get; set; }

        public CertTypeDto rootCertificate { get; set; }

        public CertTypeDto certificate { get; set; }

        public string orderID { get; set; }

        public string custOrderID { get; set; }

        public string partNo { get; set; }

        public string dealerNo { get; set; }

        public string requestID { get; set; }

        public TypeFSCProvidedDto()
        {
            fscItem = new FscItemTypeDto();
            rootCertificate = new CertTypeDto();
            certificate = new CertTypeDto();
        }
    }
}
