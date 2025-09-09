namespace ProjectPano.Model
{
    public class tbDeliverable
    {
        public int DeliverableID { get; set; }
        public int OBID { get; set; }

        public string? DelNum { get; set; }
        public string? DelName { get; set; }
        public decimal DelHours { get; set; }
        public decimal DelCost { get; set; }

       public string? DelComment { get; set; }
        public DateTime? PlanFinishDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int JobID { get; set; }
        public DateTime? PlanStartDate { get; set; }

        public string? DelRev { get; set; }

        public string? DelGp1 { get; set; }
        public string? DelGp2 { get; set; }


        public string? DelGp3 { get; set; }
        public string? DelGp4 { get; set; }

    }
}
