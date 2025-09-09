namespace ProjectPano.Model
{
    public class vwActuals_byProject_byWeek_nogroup
    {
        public int JobID { get; set; }
        public string BigTimeJobDisplayName { get; set; }
        public decimal BillQty { get; set; }
        public decimal BillwithAdminDisc { get; set; }
        public DateTime WeekEnd { get; set; }

        public decimal CURRENTCOST { get; set; }
        public decimal PeriodPctSpent { get; set; }
    }
}
