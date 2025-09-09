namespace ProjectPano.Model
{
    public class vwCumulSpendforWeekly_Brian
    {
        public string jobid { get; set; }
        public string MgrName { get; set; }
        public string Client { get; set; }
        public string ClientJob { get; set; }
        public decimal OriginalBudget { get; set; }
        public decimal CurrentBudget { get; set; }
        public decimal CurrentCumulativeSpend { get; set; }
        public decimal PercentSpent { get; set; }
        public decimal PercentComplete { get; set; }

        public decimal EACCost { get; set; }
        public  DateTime FinishDate { get; set; }
        public string comment { get; set; }
        public int ProjectProgID { get; set; }
        public decimal? Invoiced { get; set; }
        public string RegionDesc { get; set; }
        public string StreamDesc { get; set; }
        public string ProjectTypeDesc { get; set; }

    }
}
