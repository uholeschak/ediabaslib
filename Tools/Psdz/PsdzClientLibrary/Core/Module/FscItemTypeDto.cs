using System;

namespace BMW.Rheingold.InfoProvider.SWT.DTOs
{
    public class FscItemTypeDto
    {
        public SwIdTypeDto swID { get; set; }

        public FscTypeDto fsc { get; set; }

        public string diagnoseAddr { get; set; }

        public string buildPurpose { get; set; }

        public string ecuData { get; set; }

        public DateTime genTime { get; set; }

        public string Individualization { get; set; }

        public FscItemTypeDto()
        {
            swID = new SwIdTypeDto();
            fsc = new FscTypeDto();
        }
    }

}
