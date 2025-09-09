namespace ProjectPano.Model
{
    public class tbProjectProgress
    {
        public int ProjectProgID { get; set; }
    public DateTime WeekEnd { get; set; }
        public decimal ProjectPeriodProgress {  get; set; }
        public string Status {  get; set; } 
        public int JobID {  get; set; }  
        public string Comment {  get; set; }    
        public DateTime? FcastFinishDate {  get; set; }    
        public decimal? ForecastHrs {  get; set; }    
        public decimal? CumulPeriodProgress {  get; set; }
        public DateTime Created {  get; set; }
        public DateTime Modified {  get; set; }   
        public decimal? EAC_Info {  get; set; }

    }
}
