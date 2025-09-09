using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Configuration;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.ETC
{
    public class IndexModel : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;
        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void OnGet(int? jobId)
        {
            DAL dal = new DAL();
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListJobStatus = dal.GetJobStatus(configuration);

            if (jobId.HasValue)
            {
                // ? Fix: Get first job only
                var jobList = dal.GetThisWJob(jobId.Value, configuration);
                if (jobList.Count > 0)
                    thisVWJob = jobList[0];

                vwBudgetActuals = dal.GetVWBudgetActuals(jobId.Value, configuration);
            }
        }

    }
}
