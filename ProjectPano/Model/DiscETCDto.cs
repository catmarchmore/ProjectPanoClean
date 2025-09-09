using System.Text.Json.Serialization;

namespace ProjectPano.Model
{
    public class DiscETCDto
    {
        [JsonPropertyName("DiscEtcID")]
        public int DiscEtcID { get; set; }

        [JsonPropertyName("OBID")]
        public int OBID { get; set; }
        [JsonPropertyName("RptWeekend")]
        public DateTime? RptWeekend { get; set; }

        [JsonPropertyName("ETCHrs")]
        public decimal ETCHrs { get; set; }
        [JsonPropertyName("ETCCost")]
        public decimal? ETCCost { get; set; }
        [JsonPropertyName("EACHrs")]
        public decimal? EACHrs { get; set; }

        [JsonPropertyName("EACCost")]
        public decimal? EACCost { get; set; }

        [JsonPropertyName("ETCComment")]
        public string? ETCComment { get; set; }

        [JsonPropertyName("PlanStartWE")]
        public DateTime? PlanStartWE { get; set; }

        [JsonPropertyName("PlanFinishWE")]
        public DateTime? PlanFinishWE { get; set; }
        [JsonPropertyName("EmpID")]
        public int? EmpID { get; set; }
        [JsonPropertyName("EmpGroupID")]
        public int? EmpGroupID { get; set; }

        [JsonPropertyName("JobID")]
        public int JobID { get; set; }
        [JsonPropertyName("CurveID")]
        public int? CurveID { get; set; }
        [JsonPropertyName("ETCRate")]
        public decimal? ETCRate { get; set; }
        public int ETCRateTypeID { get; set; }
    }

}
