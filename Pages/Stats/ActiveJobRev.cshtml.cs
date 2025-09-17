using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.Stats
{
    public class ActiveJobRevModel : PageModel
    {
        private readonly IConfiguration configuration;
        public List<vwActiveJobClient> ListJobStatus = new List<vwActiveJobClient>();
        public ActiveJobRevModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public DateTime? MaxDateStamp { get; set; }
        public DateTime ThisWeekEnding { get; set; }

        public void OnGet()
        {
            DAL dal = new DAL();
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListJobStatus = dal.GetActiveJobClient(configuration);
            ThisWeekEnding = GetWE.GetWeekEnding(DateTime.Today);
        }
    }
}
