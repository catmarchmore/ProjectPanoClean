using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectPano.Pages.NewJobs
{
    public class SummaryModel : PageModel
    {

        private readonly IConfiguration configuration;
        public List<vwNewJobs> ListNewJobs = new List<vwNewJobs>();
        //public List<vwNewJobsByMonth> ListNewJobsByMonth = new List<vwNewJobsByMonth>();

        public SummaryModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void OnGet(List<string>? selectedMonths)
        {
            DAL dal = new DAL();
            var allJobs = dal.GetNewJobs(configuration);

            if (selectedMonths != null && selectedMonths.Count > 0)
            {
                ListNewJobs = allJobs
                    .Where(x => selectedMonths.Contains(x.myYearMonth))
                    .OrderByDescending(x => x.myYearMonth)
                    .ToList();
            }
            else
            {
                ListNewJobs = allJobs.OrderByDescending(x => x.myYearMonth).ToList();
            }
        }

    }
}
