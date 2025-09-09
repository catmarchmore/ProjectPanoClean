namespace ProjectPano.Model
{
    public class ETCSpreadResult
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

    }
}
