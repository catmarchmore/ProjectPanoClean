namespace ProjectPano.Model
{
    public class ETCSpreadWeek
    {
        public int JobID { get; set; }
        public int EmpGroupID { get; set; }
        public DateTime WeekEnding { get; set; }
        public decimal SpreadHrs { get; set; }

        public string EmpResGroupDesc { get; set; }

        public string EmpResGroupLead { get; set; }
        public string ResourceStatus { get; set; }

        public string ClientNameShort { get; set; }
        public string JobName { get; set; }
        public string StackedAreaClient { get; set; }
        public decimal WklyBillableOH { get; set; }
        public decimal WklyBillableOHOT { get; set; }

        public decimal TotalWklyBillableOH { get; set; }
        public decimal TotalWklyBillableOHOT { get; set; }

        public string ActualETC { get; set; }  // "Actual" or "ETC"

    }
}
