namespace ProjectPano.Model
{
    public class vwNewJobs
    {
        public string myYearMonth { get; set; }
        public int JobID { get; set; }
        public string ClientName { get; set; }

        public string JobName { get; set; }
        public string JobNum { get; set; }
        public decimal Amount { get; set; }

        public string Status { get; set; }

        public string MgrName { get; set; }
        public DateTime AwardDate { get; set; }
        public string ProjectTypeDesc { get; set; }
    }
}
