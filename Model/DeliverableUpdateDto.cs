using System.Text.Json.Serialization;

namespace ProjectPano.Model
{
    public class DeliverableUpdateDto
    {
        [JsonPropertyName("DeliverableID")]
        public int DeliverableID { get; set; }

        [JsonPropertyName("OBID")]
        public int OBID { get; set; }

        [JsonPropertyName("JobID")]
        public int JobID { get; set; }

        [JsonPropertyName("DelNum")]
        public string? DelNum { get; set; }

        [JsonPropertyName("DelName")]
        public string? DelName { get; set; }

        [JsonPropertyName("DelHours")]
        public decimal DelHours { get; set; }

        [JsonPropertyName("DelCost")]
        public decimal DelCost { get; set; }

        [JsonPropertyName("DelComment")]
        public string? DelComment { get; set; }

        [JsonPropertyName("PlanStartDate")]
        public DateTime? PlanStartDate { get; set; }

        [JsonPropertyName("PlanFinishDate")]
        public DateTime? PlanFinishDate { get; set; }
        [JsonPropertyName("DelRev")]
        public string? DelRev { get; set; }

        [JsonPropertyName("DelGp1")]
        public string? DelGp1 { get; set; }

        [JsonPropertyName("DelGp2")]
        public string? DelGp2 { get; set; }

        [JsonPropertyName("DelGp3")]
        public string? DelGp3 { get; set; }

        [JsonPropertyName("DelGp4")]
        public string? DelGp4 { get; set; }
    }

}
