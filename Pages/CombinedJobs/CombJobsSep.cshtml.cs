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

namespace ProjectPano.Pages.CombJobsSep
{
    [IgnoreAntiforgeryToken]
    public class CombJobsSepModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public CombJobsSepModel(IConfiguration configuration)
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

        public IEnumerable<JobGroup> JobGroups { get; set; }


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
                vwBudgetActuals = dal.GetVWJobGroups(jobGp1Id.Value, configuration) ?? new List<vwJobGroups>();

                // Build Job -> DiscGroup -> Items hierarchy (Items are the original vwJobGroups rows)
                JobGroups = vwBudgetActuals
                    .GroupBy(x => new { JobID = x.JobID.Value, x.ClientJob })                       // group by job
                    .OrderBy(g => g.Key.JobID)
                    .Select(jobGroup => new JobGroup
                    {
                        JobID = jobGroup.Key.JobID,
                        //JobName = jobGroup.Key.ClientJob ?? string.Empty,
                        JobName =
                            (jobGroup.Key.ClientJob ?? string.Empty).Contains(": ")
                                ? (jobGroup.Key.ClientJob ?? string.Empty).Substring((jobGroup.Key.ClientJob ?? string.Empty).IndexOf(": ") + 2)
                                : (jobGroup.Key.ClientJob ?? string.Empty),
                        DiscGroups = jobGroup
                            .GroupBy(x => new { x.DiscGroupSort, x.DiscGroup })      // group by disc group inside job
                            .OrderBy(dg => dg.Key.DiscGroupSort)
                            .Select(dg => new DiscGroup
                            {
                                DiscGroupSort = dg.Key.DiscGroupSort,
                                DiscGroupName = dg.Key.DiscGroup ?? string.Empty,
                                Items = dg.OrderBy(x => x.DiscSort).ToList()          // keep raw rows for rendering
                            })
                            .ToList()
                    })
                    .ToList();

                foreach (var job in JobGroups)
                {
                    System.Diagnostics.Debug.WriteLine($"Grouped JobID={job.JobID}, JobName={job.JobName}");
                }

            }
            else
            {
                vwBudgetActuals = new List<vwJobGroups>();
                JobGroups = new List<JobGroup>();
            }

            return Page();
        }

        public class DiscGroup
        {
            public int? DiscGroupSort { get; set; }
            public string DiscGroupName { get; set; } = string.Empty;

            // Items are the raw rows from your DAL (vwJobGroups)
            public List<vwJobGroups> Items { get; set; } = new List<vwJobGroups>();

            // handy aggregated totals (use whatever fields you need; added a couple as example)
            public decimal Total_OB_HRS => Items.Sum(i => i.OB_HRS.GetValueOrDefault());
            public decimal Total_OB_COST => Items.Sum(i => i.OB_COST.GetValueOrDefault());
            public decimal Total_ApprovedCNHRS => Items.Sum(i => i.ApprovedCNHRS.GetValueOrDefault());
            public decimal Total_ApprovedCNCOST => Items.Sum(i => i.ApprovedCNCOST.GetValueOrDefault());
            public decimal Total_UnApprovedCNHRS => Items.Sum(i => i.UnApprovedCNHRS.GetValueOrDefault());
            public decimal Total_UnApprovedCNCOST => Items.Sum(i => i.UnApprovedCNCOST.GetValueOrDefault());
            public decimal Total_CURRHRS => Items.Sum(i => i.CURRHRS.GetValueOrDefault());
            public decimal Total_CURRCOST => Items.Sum(i => i.CURRCOST.GetValueOrDefault());
            public decimal Total_BILLQTY => Items.Sum(i => i.BILLQTY.GetValueOrDefault());
            public decimal Total_BILLWITHADMINDISC => Items.Sum(i => i.BILLWITHADMINDISC.GetValueOrDefault());
            public decimal Total_PrevWkCumulHrs => Items.Sum(i => i.PrevWkCumulHrs.GetValueOrDefault());
            public decimal Total_PrevWkCumulCost => Items.Sum(i => i.PrevWkCumulCost.GetValueOrDefault());
            public decimal Total_CurrWkHrs => Items.Sum(i => i.CurrWkHrs.GetValueOrDefault());
            public decimal Total_CurrWkCost => Items.Sum(i => i.CurrWkCost.GetValueOrDefault());

            public decimal Total_ETC_HRS => Items.Sum(i => i.ETC_Hrs.GetValueOrDefault());
            public decimal Total_ETC_COST => Items.Sum(i => i.ETC_Cost.GetValueOrDefault());

            public decimal Total_EAC_Hrs => Items.Sum(i => i.EAC_Hrs.GetValueOrDefault());
            public decimal Total_EAC_Cost => Items.Sum(i => i.EAC_Cost.GetValueOrDefault());
        }

        public class JobGroup
        {
            public int? JobID { get; set; }
            public string JobName { get; set; } = string.Empty;
            public List<DiscGroup> DiscGroups { get; set; } = new List<DiscGroup>();

            // job totals derived from DiscGroups
            public decimal Total_OB_HRS => DiscGroups.Sum(g => g.Total_OB_HRS);
            public decimal Total_OB_COST => DiscGroups.Sum(g => g.Total_OB_COST);
            public decimal Total_ApprovedCNHRS => DiscGroups.Sum(g => g.Total_ApprovedCNHRS);
            public decimal Total_ApprovedCNCOST => DiscGroups.Sum(g => g.Total_ApprovedCNCOST);
            public decimal Total_UnApprovedCNHRS => DiscGroups.Sum(g => g.Total_UnApprovedCNHRS);
            public decimal Total_UnApprovedCNCOST => DiscGroups.Sum(g => g.Total_UnApprovedCNCOST);
            public decimal Total_CURRHRS => DiscGroups.Sum(g => g.Total_CURRHRS);
            public decimal Total_CURRCOST => DiscGroups.Sum(g => g.Total_CURRCOST);
            public decimal Total_BILLQTY => DiscGroups.Sum(g => g.Total_BILLQTY);
            public decimal Total_BILLWITHADMINDISC => DiscGroups.Sum(g => g.Total_BILLWITHADMINDISC);

            public decimal Total_PrevWkCumulHrs => DiscGroups.Sum(g => g.Total_PrevWkCumulHrs);
            public decimal Total_PrevWkCumulCost => DiscGroups.Sum(g => g.Total_PrevWkCumulCost);
            public decimal Total_CurrWkHrs => DiscGroups.Sum(g => g.Total_CurrWkHrs);
            public decimal Total_CurrWkCost => DiscGroups.Sum(g => g.Total_CurrWkCost);

            public decimal Total_ETC_HRS => DiscGroups.Sum(g => g.Total_ETC_HRS);
            public decimal Total_ETC_COST => DiscGroups.Sum(g => g.Total_ETC_COST);
            public decimal Total_EAC_Hrs => DiscGroups.Sum(g => g.Total_EAC_Hrs);
            public decimal Total_EAC_Cost => DiscGroups.Sum(g => g.Total_EAC_Cost);
        }
    }
}
