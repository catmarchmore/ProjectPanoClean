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
        public decimal CURRHRS { get; set; }
        public decimal CURRCOST { get; set; }

        public int? DelTypeID { get; set; }
        public decimal Step1Limit { get; set; }
        public decimal Step2Limit { get; set; }
        public decimal Step3Limit { get; set; }
        public decimal Step4Limit { get; set; }
        public decimal Step5Limit { get; set; }

        public int Step1StatusID { get; set; }
        public int Step2StatusID { get; set; }
        public int Step3StatusID { get; set; }
        public int Step4StatusID { get; set; }
        public int Step5StatusID { get; set; }
        public decimal Step1EV { get; set; }
        public decimal Step2EV { get; set; }
        public decimal Step3EV { get; set; }
        public decimal Step4EV { get; set; }
        public decimal Step5EV { get; set; }
        public decimal Step1PctCumul { get; set; }
        public decimal Step2PctCumul { get; set; }
        public decimal Step3PctCumul { get; set; }
        public decimal Step4PctCumul { get; set; }
        public decimal Step5PctCumul { get; set; }
        public int MaxSteps { get; set; }
        public string ActiveStep { get; set; }

    }
}
