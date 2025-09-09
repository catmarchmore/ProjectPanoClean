using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.NewJobs
{
    public class IndexModel : PageModel
    {

        private readonly IConfiguration configuration;
        public List<vwNewJobs> ListNewJobs = new List<vwNewJobs>();
        public List<vwNewJobsByMonth> ListNewJobsByMonth = new List<vwNewJobsByMonth>();
        public List<NewJobsByMonthDto> ListNewJobsByMonthRolling = new List<NewJobsByMonthDto>();

        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void OnGet()
        {
            DAL dal = new DAL();

            ListNewJobs = dal.GetNewJobs(configuration)
                .OrderByDescending(x=> x.myYearMonth)
                .ToList();

            var cutoff = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-12);
            var allMonths=Enumerable.Range(0,13)
                .Select(i=>cutoff.AddMonths(i))
                .ToList();

            var raw= dal.GetNewJobsByMonthRolling(configuration)
                .Where(x=>x.YearMonthDate>=cutoff)
                .ToList();

            ListNewJobsByMonthRolling = allMonths
                .Select(m => raw.FirstOrDefault(x => x.YearMonthDate == m) ?? new NewJobsByMonthDto
                {
                    myYearMonth = m.ToString("yyyy-MM"),
                    YearMonthDate=m,
                    MonthlyOrigAmt = 0,
                    MonthlyChangeAmt = 0,
                    MonthlyTarget = 0
                })
                .OrderBy(x => x.YearMonthDate)
                .ToList();
        }
    }
}
