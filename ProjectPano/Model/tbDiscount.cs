namespace ProjectPano.Model
{
    public class tbDiscount
    {
        public int DiscountID { get; set; }
        public int? JobID { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountFinishFinish { get; set; }
        public decimal? DiscountAmt { get; set; }
        public bool? BaseLaborOnly {  get; set; }


    }
}
