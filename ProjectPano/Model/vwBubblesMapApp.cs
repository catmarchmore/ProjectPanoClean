namespace ProjectPano.Model
{
    public class vwBubblesMapApp
    {
        public int JobID { get; set; }
        public string? ClientNameShort {  get; set; }
        public string? JobName { get; set; }
        public string? MgrName { get; set; }
        public string? StreamDesc { get; set; }
        public string? ProjectTypeDesc { get; set; }
        public string? RegionDesc { get; set; }

        public decimal? JobTotalValue { get; set; }
        public string? MapState { get; set; }


    }
}
