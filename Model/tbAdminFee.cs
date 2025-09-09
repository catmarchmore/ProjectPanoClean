namespace ProjectPano.Model
{
    public class tbAdminFee
    {
        public int AdminFeeID { get; set; }
        public int? JobID { get; set; }
        public DateTime? AdminFeeStart { get; set; }
        public DateTime? AdminFeeFinish { get; set; }
        public decimal? AdminFeeAmt { get; set; }

    }
}
