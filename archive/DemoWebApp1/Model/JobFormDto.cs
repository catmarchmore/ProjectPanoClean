namespace ProjectPano.Model
{
    public class JobFormDto
    {
        public int JobID { get; set; }
        public string? JobNum { get; set; }
        public string? BigTimeJobDisplayName { get; set; }
        public DateTime? JobStartDate { get; set; }
        public DateTime? JobFinishDate { get; set; }
        public int? ClientID { get; set; }
        public string? ClientPM { get; set; }
        public string? AFE { get; set; }
        public int? MgrID { get; set; }
        public string? BillingStatus { get; set; }
        public int? RateSheetID { get; set; }
        public decimal? ProjectValue { get; set; }
        public decimal? NewMexTaxAmt { get; set; }
        public int? CorpID { get; set; }
        public int? IndustrySectorID { get; set; }
        public int? ProjectProgramID { get; set; }
        public int? RegionID { get; set; }
        public int? StreamID { get; set; }
        public string? ResourceStatus { get; set; }
        public int? SubClientID { get; set; }
        public int? ProjectTypeID { get; set; }
        public string? JobComments { get; set; }
        public decimal? OTPctJob { get; set; }
        public bool TASKS { get; set; } = true; // default true

        public decimal? Probability { get; set; }
        public DateTime? BacklogStartDate { get; set; }

    }

}
