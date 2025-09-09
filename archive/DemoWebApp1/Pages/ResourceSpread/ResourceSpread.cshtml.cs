using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.ResourceSpread
{
    [IgnoreAntiforgeryToken]
    public class ResourceSpreadModel : PageModel
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
        public ResourceSpreadModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //this is just for this histogram on this page
        public List<ETCSpreadChartDto> SpreadWeeks { get; set; } = new();

        //this gets all the data as on the deepdive page for debugging
        public List<ETCSpreadCheck> SpreadCheckList { get; set; }

        public class SpreadResult
        {
            public List<ETCSpreadChartDto> FinalList { get; set; }
            public List<ETCSpreadChartDto> OriginalList { get; set; }
        }


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

        public List<ETCSpreadChartDto> GetSpreadAll()
        {
            DAL dal = new DAL();
            var allEtcs = dal.GetAllDiscETCLabor(configuration);
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetLast4lWklyActuals(configuration);
            //var (spreadAll, spreadOriginal) = SpreadUtility.GetSpread(allEtcs, curves, actuals);


            // Optionally filter by current RptWE
            DateTime rptWE = ReportWE.GetReportWE();
            allEtcs = allEtcs.Where(e => e.PlanFinishWE >= rptWE).ToList();

            return SpreadUtility.GetSpread(allEtcs, curves, actuals);
            //return spreadOriginal;
        }

        public void OnGet()
        {
 
            {
                DAL dal = new DAL();
                var etcRecordsDebug = dal.GetAllDiscETC(configuration);
                var curvesDebug = dal.GetCurveSections(configuration);
                var actualsDebug = dal.GetLast4lWklyActuals(configuration);
                var tbJobs = dal.GetTbJob(configuration);

                ListEmpGroupResources = dal.GetResourceGroups(configuration);

                SpreadCheckList = SpreadDebugUtility.GetSpreadDebug(etcRecordsDebug, curvesDebug, actualsDebug, tbJobs);

                SpreadWeeks = GetSpreadAll();
            }
        }
    }
}
