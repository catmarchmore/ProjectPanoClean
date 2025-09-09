namespace ProjectPano.Model
{
    public class ETCSpreadChartDto
    {
        public DateTime WeekEnding { get; set; }
        public string EmpResGroupDesc { get; set; }
        public string ResourceStatus { get; set; }
        public string ActualETC { get; set; }
        public decimal SpreadHrs { get; set; }
        public decimal? WklyBillableOH { get; set; }
        public decimal? WklyBillableOHOT { get; set; }
        public decimal? TotalWklyBillableOH { get; set; }
        public decimal? TotalWklyBillableOHOT { get; set; }

    }
}
