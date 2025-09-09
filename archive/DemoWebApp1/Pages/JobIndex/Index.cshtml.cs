using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.JobIndex
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration configuration;
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus = new List<vwCumulSpendforWeekly_Brian>();
        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public DateTime? MaxDateStamp { get; set; }
        public DateTime ThisWeekEnding { get; set; }

        public void OnGet()
        {
            DAL dal = new DAL();
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListJobStatus = dal.GetJobStatus(configuration);
            ThisWeekEnding = GetWE.GetWeekEnding(DateTime.Today);
        }
    }
}
