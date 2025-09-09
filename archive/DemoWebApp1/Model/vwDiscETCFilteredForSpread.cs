namespace ProjectPano.Model
{
    public class vwDiscETCFilteredForSpread
    {
        public int DiscEtcID { get; set; }
        public int JobID { get; set; }
        public decimal ETCHrs { get; set; }
        public decimal ETCCost { get; set; }
        public decimal EACHrs { get; set; }  
        public decimal EACCost { get; set; }
        public DateTime PlanStartWE { get; set; }
        public DateTime PlanFinishWE { get; set; }
        public int CurveID { get; set; }
        public string CurveName { get; set; }
        public string ResourceStatus { get; set; }
        public string EmpResGroupDesc { get; set; }
        public string EmpResGroupLead { get; set; }
        public string ClientNameShort { get; set; }
        public string JobName { get; set; }
        public string StackedAreaClient { get; set; }
        public decimal WklyBillable { get; set; }
        public decimal WklyBillableOH { get; set; }
        public decimal WklyBillableOHOT { get; set; }
        public decimal TotalWklyBillable { get; set; }
        public decimal TotalWklyBillableOH { get; set; }
        public decimal TotalWklyBillableOHOT { get; set; }
        public decimal ETCRate { get; set; }
        public decimal Probability { get; set; }

    }
}
