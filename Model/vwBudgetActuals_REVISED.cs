namespace ProjectPano.Model
{
    public class vwBudgetActuals_REVISED
    {
        public int JobID { get; set; }
        public string BigTimeJobDisplayName { get; set; }
        public int OBID { get; set; }
        public string MYTASK { get; set; }
        public DateTime MAXBILLDATE { get; set; }
        public decimal OB_HRS { get; set; }
        public decimal OB_COST { get; set; }
        public decimal ApprovedCNHRS { get; set; }
        public decimal ApprovedCNCOST { get; set; }
        public decimal UnApprovedCNHRS { get; set; }
        public decimal UnApprovedCNCOST { get; set; }
        public decimal CURRHRS { get; set; }
        public decimal CURRCOST { get; set; }
        public decimal BILLCOST { get; set; }
        public decimal BILLQTY { get; set; }
        public decimal BILLWITHADMIN { get; set; }
        public decimal ADMIN_FEE { get; set; }
        public decimal BILLWITHADMINDISC { get; set; }
        public decimal DISCOUNT_AMT { get; set; }
        public decimal PERCENT_SPENT { get; set; }

        public string DiscCode { get; set; }
        public string DiscGroup { get; set; }
        public int DiscGroupSort { get; set; }
        public int DiscSort { get; set; }
        public string MgrName { get; set; }
        public DateTime MyDateStampCHK { get; set; }
        public DateTime ProgressDate { get; set; }

        public decimal PctCompl { get; set; }
        public decimal EAC_Hrs { get; set; }
        public decimal EAC_Cost { get; set; }

        public string BillingStatus { get; set; }

        public string JobLevel { get; set; }

        public string DiscDesc { get; set; }
        public string? ResourceStatus { get; set; }
        public int? DefaultEmpGroupID { get; set; }
        public DateTime CurrWkEnding { get; set; }
        public DateTime PrevWkEnding { get; set; }
        public decimal PrevWkCumulHrs { get; set; }

        public decimal PrevWkCumulCost { get; set; }

        public decimal CurrRate { get; set; }

        public decimal ActRate { get; set; }
        public decimal ETC_Hrs { get; set; }

        public decimal ETC_Cost { get; set; }
        public decimal CurrWkHrs { get; set; }

        public decimal CurrWkCost { get; set; }
        public decimal OBIDEarnedCost { get; set; }


    }
}
