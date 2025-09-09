using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectPano.Pages.User
{
    public class TestModel : PageModel
    {
        private readonly IConfiguration configuration;
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus = new List<vwCumulSpendforWeekly_Brian>();
        public TestModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void OnGet()
        {
            DAL dal = new DAL();
            ListJobStatus = dal.GetJobStatus(configuration);
        }
    }
}
