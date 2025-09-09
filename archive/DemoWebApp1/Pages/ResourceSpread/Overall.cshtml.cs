using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using static ProjectPano.Model.DAL;
using static System.Reflection.Metadata.BlobBuilder;

namespace ProjectPano.Pages.ResourceSpread
{
    [IgnoreAntiforgeryToken]

    public class OverallModel : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;
        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public List<vwCurves> ListCurves { get; set; } = new();
        public SelectList JobSelectList { get; set; }
        public OverallModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public List<ETCSpreadStackedAreaDto> SpreadWeeks { get; set; } = new();
        private Dictionary<int, List<decimal>> GetCurveDictionary(List<vwCurves> curveList)
        {
            var grouped = curveList
                .GroupBy(c => c.CurveID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(c => c.CurveSectionNum)
                          .Select(c => c.CurveSectionPct)
                          .ToList()
                );

            foreach (var kvp in grouped)
            {
                Console.WriteLine($"CurveID {kvp.Key} has {kvp.Value.Count} sections with total pct = {kvp.Value.Sum()}");
            }

            return grouped;
        }

        public List<ETCSpreadStackedAreaDto> GetSpreadAll()
        {
            DAL dal = new DAL();
            var allEtcs = dal.GetAllDiscETC(configuration);
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetLast4lWklyActualsClient(configuration);
            var tbJobs = dal.GetTbJob(configuration);

            // Optionally filter by current RptWE
            DateTime rptWE = ReportWE.GetReportWE();
            allEtcs = allEtcs.Where(e => e.PlanFinishWE >= rptWE).ToList();

            return StackedAreaUtility.GetSpread(allEtcs, curves, actuals, tbJobs);
        }

        public void OnGet()
        {

            {
                SpreadWeeks = GetSpreadAll();
            }

        }

    }
}
