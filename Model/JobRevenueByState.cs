namespace ProjectPano.Model
{
    public class JobRevenueByState
    {
        public string StateCode { get; set; }   // e.g., "TX", "CA"
        public int JobCount { get; set; }       // number of active jobs
        public decimal Revenue { get; set; }    // revenue $
        public string Clients {  get; set; }
    }
}
