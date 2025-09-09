using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using static ProjectPano.Model.DAL;
using ClosedXML.Excel;

namespace ProjectPano.Pages.ResourceSpread
{
    [IgnoreAntiforgeryToken]
    public class OverallDetailsModel : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public List<vwCurves> ListCurves { get; set; } = new();
        public SelectList JobSelectList { get; set; }
        public List<ETCSpreadCheck> SpreadCheckList { get; set; }
        public OverallDetailsModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void OnGet()
    {
            DAL dal = new DAL();
            var etcRecords = dal.GetAllDiscETC(configuration);
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetLast4lWklyActuals(configuration);
            var tbJobs = dal.GetTbJob(configuration);
            SpreadCheckList = SpreadDebugUtility.GetSpreadDebug(etcRecords,curves,actuals, tbJobs);
    }



    public async Task<FileResult> OnGetDownloadSpreadAsync()
    {
            DAL dal = new DAL();
            var etcRecords = dal.GetAllDiscETCLabor(configuration);
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetLast4lWklyActuals(configuration);
            var tbJobs = dal.GetTbJob(configuration);
            var spreadList = SpreadDebugUtility.GetSpreadDebug(etcRecords,curves,actuals,tbJobs);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ETCSpreadCheck");

        // Header row
        ws.Cell(1, 1).Value = "WeekEnding";
        ws.Cell(1, 2).Value = "EmpResGroupDesc";
        ws.Cell(1, 3).Value = "ResourceStatus";
        //ws.Cell(1, 4).Value = "ActualETC";
        ws.Cell(1, 4).Value = "SpreadHrs";
        //ws.Cell(1, 6).Value = "WklyBillableOH";
        //ws.Cell(1, 7).Value = "WklyBillableOHOT";
        //ws.Cell(1, 8).Value = "TotalWklyBillableOH";
        //ws.Cell(1, 9).Value = "TotalWklyBillableOHOT";
        ws.Cell(1, 5).Value = "JobID";
        ws.Cell(1, 6).Value = "JobName";
        ws.Cell(1, 7).Value = "ClientNameShort";
        ws.Cell(1, 8).Value = "SpreadHrsProb";
        ws.Cell(1, 9).Value = "Probability";

        // Rows
        for (int i = 0; i < spreadList.Count; i++)
        {
            var s = spreadList[i];
            ws.Cell(i + 2, 1).Value = s.WeekEnding;
            ws.Cell(i + 2, 2).Value = s.EmpResGroupDesc;
            ws.Cell(i + 2, 3).Value = s.ResourceStatus;
            //ws.Cell(i + 2, 4).Value = s.ActualETC;
            ws.Cell(i + 2, 4).Value = s.SpreadHrs;
            //ws.Cell(i + 2, 6).Value = s.WklyBillableOH;
            //ws.Cell(i + 2, 7).Value = s.WklyBillableOHOT;
            //ws.Cell(i + 2, 8).Value = s.TotalWklyBillableOH;
            //ws.Cell(i + 2, 9).Value = s.TotalWklyBillableOHOT;
            ws.Cell(i + 2, 5).Value = s.JobID;
            ws.Cell(i + 2, 6).Value = s.JobName;
            ws.Cell(i + 2, 7).Value = s.ClientNameShort;
            ws.Cell(i + 2, 8).Value = s.SpreadHrsProb;
            ws.Cell(i + 2, 9).Value = s.Probability;
            }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ETCSpreadCheck.xlsx");
    }
    }
}
