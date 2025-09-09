using System.Text.Json.Serialization;

namespace ProjectPano.Model
{
    public class DiscETCNEWDto
    {
        public int JobID { get; set; }
        public int OBID { get; set; }
        public DateTime PlanStartWE { get; set; }
        public DateTime PlanFinishWE { get; set; }
        public decimal ETCHrs { get; set; }
        public int CurveID { get; set; }
        public string? ETCComment { get; set; }
        public int EmpGroupID { get; set; }
        public int? EmpID { get; set; }
        public DateTime RptWeekend { get; set; }
    }

}
