using ProjectPano.Model;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.CombinedJobs
{
    [IgnoreAntiforgeryToken]
    public class CombinedJobsModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public CombinedJobsModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }

        public SelectList JobSelectList { get; set; }

        public List<vwJobGroups> vwBudgetActuals { get; set; } // Table data

        [BindProperty(SupportsGet = true)]
        public int? JobGp1Id { get; set; }

        public DateTime CurrWeekEnding { get; set; } = DateTime.Today;
        public DateTime PrevWeekEnding { get; set; } = DateTime.Today.AddDays(-7);

        public async Task<IActionResult> OnGetAsync(int? jobGp1Id)
        {
            var today = DateTime.Today;
            CurrWeekEnding = GetWE.GetWeekEnding(today);
            PrevWeekEnding = CurrWeekEnding.AddDays(-7);

            // Populate dropdown
            var jobGroups = dal.GetDistinctJobGp1List(configuration);
            JobSelectList = new SelectList(jobGroups, "JobGp1ID", "JobGp1Desc", jobGp1Id);

            if (jobGp1Id.HasValue)
            {
                // Load table data for selected JobGp1ID
                vwBudgetActuals = dal.GetVWJobGroups(jobGp1Id.Value, configuration);
            }
            else
            {
                vwBudgetActuals = new List<vwJobGroups>(); // empty table initially
            }

            return Page();
        }
    }
}
