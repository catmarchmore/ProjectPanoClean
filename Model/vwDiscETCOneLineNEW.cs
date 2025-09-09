namespace ProjectPano.Model
{
    public class vwDiscETCOneLineNEW
    {
        public int JobID { get; set; }
        public string MgrName { get; set; }
        public string ClientNameShort {  get; set; }

        public int SimpleFlag { get; set; }
        public string ClientJob { get; set; }
        public decimal? ETCHrs_PM { get; set; }
        public DateTime? PlanStartWE_PM { get; set; }
        public DateTime? PlanFinishWE_PM { get; set; }
        public decimal? CurveID_PM { get; set; }
        public string? CurveName_PM { get; set; }
        public int? OBID_PM { get; set; }
        public int? DiscETCID_PM { get; set; }

        public decimal? ETCHrs_Engr { get; set; }
        public DateTime? PlanStartWE_Engr { get; set; }
        public DateTime? PlanFinishWE_Engr { get; set; }
        public decimal? CurveID_Engr { get; set; }
        public string? CurveName_Engr { get; set; }
        public int? OBID_Engr { get; set; }
        public int? DiscETCID_Engr { get; set; }
        public decimal? ETCHrs_EIC { get; set; }
        public DateTime? PlanStartWE_EIC { get; set; }
        public DateTime? PlanFinishWE_EIC { get; set; }
        public decimal? CurveID_EIC { get; set; }
        public string? CurveName_EIC { get; set; }
        public int? OBID_EIC { get; set; }
        public int? DiscETCID_EIC { get; set; }

        public decimal? ETCHrs_Design { get; set; }
        public DateTime? PlanStartWE_Design { get; set; }
        public DateTime? PlanFinishWE_Design { get; set; }
        public decimal? CurveID_Design { get; set; }
        public string? CurveName_Design { get; set; }
        public int? OBID_Design { get; set; }
        public int? DiscETCID_Design { get; set; }
    }
}
