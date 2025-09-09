using System.Security.Cryptography.X509Certificates;

namespace ProjectPano.Model
{
    public class NewJobsByMonthDto
    {
        public string myYearMonth { get; set; }
        public decimal MonthlyOrigAmt { get; set; }
        public decimal MonthlyChangeAmt { get; set; }
        public decimal MonthlyTarget { get; set; }
        public DateTime YearMonthDate{get;set;}  
    }


}
