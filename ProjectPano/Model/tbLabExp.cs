namespace ProjectPano.Model
{
    public class tblabExp
    {
        public int LabExpID { get; set; }
        public int JobID { get; set; }
        public int EmpID { get; set; }
        public string BigTimeJobDisplayName { get; set; }
        public string EmpName { get; set; }
        public string Category { get; set; }
        public DateTime BillDate { get; set; }

        public string Task { get; set; }

        public string Notes { get; set; }
        public string InvNumber { get; set; }
        public DateTime WeekEnd { get; set; }
        public decimal InputCost {  get; set; }
        public decimal BillableCost {  get; set; }    
        public decimal BillableQty {  get; set; }
        public decimal BigTimeBillRate { get; set; }
        public decimal BillableWithAdmin { get; set; }
        public decimal AdminFee { get; set; }
        public decimal BillableWithAdminDiscount { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal InputQty { get; set; }
        public string LabExp { get; set; }
        public string NC { get; set; }
        public string myTask { get; set; }
        public DateTime myDateStamp { get; set; }
        public string DataOrigin { get; set; }
        public string OvertimeCheck { get; set; }
        public string WorkType { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NavajoNationAmt { get; set; }
        public decimal BillablewithAdminNavajo { get; set; }
        public decimal NewMexAmt { get; set; }
        public decimal BillablewithAdminNewMex { get; set; }
        public int CorpID { get; set; }
        public string JobTeamRole { get; set; }
        public DateTime Created {  get; set; }
        public DateTime Modified {  get; set; }
        public string WorkTypeDavid { get; set; }

    }
}
