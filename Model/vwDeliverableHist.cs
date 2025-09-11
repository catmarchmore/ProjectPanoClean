namespace ProjectPano.Model
{
    public class vwDeliverableHist
    {
        public int DeliverableID { get; set; }
        public int JobID { get; set; }
        public int OBID { get; set; }
        public string? myTask { get; set; }
        public string? DelGp1 { get; set; }
        public string? DelGp2 { get; set; }

        public string? DelGp3 { get; set; }
        public string? DelGp4 { get; set; }

        public string? DelName { get; set; }
        public string? DelComment { get; set; }
        public decimal DelHours { get; set; }
        public decimal DelCost { get; set; }
        public decimal DelPctCumul { get; set; }
        public decimal DelEarnedHrs { get; set; }
        public decimal DelEarnedCost { get; set; }
        public DateTime? ProgressDate { get; set; }
        public bool Direct { get; set; }
        public decimal DirPct { get; set; }

        public DateTime? PlanFinishDate { get; set; }
        
        public DateTime? PlanStartDate { get; set; }
        public string? DelNum { get; set; }
        public string? DelRev { get; set; }

    }
}
