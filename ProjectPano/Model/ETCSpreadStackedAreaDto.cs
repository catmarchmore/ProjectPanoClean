namespace ProjectPano.Model
{
    public class ETCSpreadStackedAreaDto
    {
        public DateTime WeekEnding { get; set; }
        public string StackedAreaClient { get; set; }
        public string ResourceStatus { get; set; }
        public string ActualETC { get; set; }
        public decimal SpreadHrs { get; set; }
        public decimal? WklyBillableOH { get; set; }
        public decimal? WklyBillableOHOT { get; set; }
        public decimal? TotalWklyBillableOH { get; set; }
        public decimal? TotalWklyBillableOHOT { get; set; }
        public decimal? SpreadHrsProb { get; set; }
        public decimal? Probability { get; set; }

    }
}
