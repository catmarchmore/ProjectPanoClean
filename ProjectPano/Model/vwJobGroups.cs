namespace ProjectPano.Model
{
    public class vwJobGroups
    {
        public int JobGp1ID { get; set; }

        public string? JobGp1Desc { get; set; }
        public int? JobGp2ID { get; set; }

        public string? JobGp2Desc { get; set; }
        public string? ClientJob { get; set; }
        public int? JobID { get; set; }
        public string? MYTASK { get; set; }
        public int? OBID { get; set; }
        public decimal? OB_HRS { get; set; }
        public decimal? OB_COST { get; set; }
        public decimal? ApprovedCNHRS { get; set; }
        public decimal? ApprovedCNCOST { get; set; }
        public decimal? UnApprovedCNHRS { get; set; }
        public decimal? UnApprovedCNCOST { get; set; }
        public decimal? CURRHRS { get; set; }
        public decimal? CURRCOST { get; set; }
        public decimal? BILLQTY { get; set; }
        public decimal? BILLWITHADMINDISC { get; set; }
        public decimal? PrevWkCumulHrs { get; set; }
        public decimal? PrevWkCumulCost { get; set; }
        public decimal? CurrWkHrs { get; set; }
        public decimal? CurrWkCost { get; set; }
        public decimal? ETC_Hrs { get; set; }
        public decimal? ETC_Cost { get; set; }
        public decimal? EAC_Hrs { get; set; }
        public decimal? EAC_Cost { get; set; }
        public int? CorpID { get; set; }
        public string? CorpDesc { get; set; }
        public int? DiscGroupSort { get; set; }
        public int? DiscSort { get; set; }
        public string? DiscGroup { get; set; }
        public decimal? CorpBudget { get; set; }
        public decimal? CorpCumulSpend { get; set; }
        public decimal? CorpPrevWkCumulSpend { get; set; }
        public decimal? OpsBudget { get; set; }
        public decimal? OpsCumulSpend { get; set; }
        public decimal? OpsPrevWkCumulSpend { get; set; }
    }
}
