namespace ProjectPano.Model
{
    public class vwJob
    {
         public int JobID { get; set; }
        public int ClientID { get; set; }
        public string ClientNameShort { get; set; }
        public string BigTimeJobDisplayName { get; set; }
        public string JobName { get; set; }
        public string? JobNum { get; set; }
        public DateTime JobStartDate { get; set; }
        public string MgrName { get; set; }
        public DateTime JobFinishDate { get; set; }

        public string BillingStatus { get; set; }

        public string RateSheetName { get; set; }

        public string? AFE { get; set; }
        public string? ClientPM {  get; set; }
        public int CorpID {  get; set; }    

        public int? ProjectProgramID { get; set; }
        public int? RegionID { get; set; }
        public int? StreamID { get; set; }
        public int? ProjectTypeID { get; set; }

        public string? ResourceStatus { get; set; }
        public string ClientJob { get; set; }
        public decimal CURRCOST { get; set; }
        public decimal Probability { get; set; }

    }
}
