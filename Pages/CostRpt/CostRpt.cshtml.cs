using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.CostRpt
{
    public class IndexModel : PageModel
    {
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public DateTime? MaxDateStamp { get; set; }

        private readonly IConfiguration configuration;
        public vwJob thisVWJob { get; set; } = new vwJob();
        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

       public void OnGet()
{
    string idQuery = Request.Query["id"];
    try
    {
        if (int.TryParse(idQuery, out int jobId))
        {
            Debug.WriteLine("Job ID from query: " + jobId);

            DAL dal = new DAL();
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);

            // Fetch job info
            var jobList = dal.GetThisWJob(jobId, configuration);
            if (jobList.Count > 0)
                thisVWJob = jobList[0];

            // Fetch budget actuals
            vwBudgetActuals = dal.GetVWBudgetActuals(jobId, configuration);
        }
        else
        {
            Debug.WriteLine("Invalid or missing Job ID query parameter.");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine("Error: " + ex.Message);
    }
}

    }

}
