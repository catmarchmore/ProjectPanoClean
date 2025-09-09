namespace ProjectPano.Model
{
    public class tbForecast
    {
        public int ForecastID { get; set; }
        public int OBID { get; set; }
        public DateTime ForecastDateWE { get; set; }
        public decimal PctComplete { get; set; }
        public decimal EAC_Hrs { get; set; }
        public decimal EAC_Cost { get; set; }
        public string DiscForecastComment { get; set; }
        public int JobID { get; set; }

        public int EmpGroupID { get; set; }

    }
}
