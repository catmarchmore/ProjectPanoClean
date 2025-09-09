namespace ProjectPano.Model
{
    public class vwActuals_byProject_byWeek
    {
        public int JobID { get; set; }

        public decimal BillQty { get; set; }
        public decimal BillwithAdminDisc { get; set; }
        public DateTime WeekEnd { get; set; }
        public int obid { get; set; }
        public string myTask { get; set; }

        public string DiscGroup { get; set; }
        public string? EmpResGroupDesc { get; set; }
        public int? EmpGroupID { get; set; }
        public string? StackedAreaClient { get; set; }
        public string? ClientNameShort { get; set; }
        public string? JobName { get; set; }

    }
}
