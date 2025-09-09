namespace ProjectPano.Model
{
    public class vwDiscETC
    {
        public int DiscEtcID { get; set; }
        public int JobID { get; set; }
        public int OBID { get; set; }
        public string myTask {  get; set; }
        public DateTime RptWeekend { get; set; }
        public decimal ETCHrs { get; set; }
        public decimal ETCCost { get; set; }
        public decimal EACHrs { get; set; }  
        public decimal EACCost { get; set; }
        public string ETCComment { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime PlanStartWE { get; set; }
        public DateTime PlanFinishWE { get; set; }
        public int EmpID {  get; set; } 
        public int EmpGroupID { get; set; }
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
        public string ETCRateTypeDesc { get; set; }
        public int ETCRateTypeID { get; set; }
        public int MapChk { get; set; }
        public decimal Probability { get; set; }

    }
}
