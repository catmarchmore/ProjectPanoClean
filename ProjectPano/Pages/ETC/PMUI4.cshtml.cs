using ProjectPano.Model;
using ProjectPano.Pages.Shared.Partials;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using System.Linq;
using System.Text.Json;
using static ProjectPano.Model.DAL;
//using static ProjectPano.Pages.Shared.Partials.AddETCHrsModel;
using static ProjectPano.Pages.Shared.Partials.EditETCHrsModel;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace ProjectPano.Pages.ETC
{
    [IgnoreAntiforgeryToken]
    public class PMUI4Model : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly ILogger<PMUI4Model> _logger;
        private readonly IConfiguration configuration;
        private readonly DAL dal;

        public PMUI4Model(IConfiguration configuration, ILogger<PMUI4Model> logger)
        {
            _logger = logger;
            this.configuration = configuration;
            this.dal = new DAL();
        }

        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        //public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public List<vwBudgetActuals_REVISED> vwBudgetActualsAllActive { get; set; } = new List<vwBudgetActuals_REVISED>();
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public string allEmpDataJson { get; set; }
        public List<vwCurves> ListCurves { get; set; } = new();
        public List<vwDiscETC> thisVWDiscETC { get; set; } = new List<vwDiscETC>();
        public List<string> AllMgrNames { get; set; }
        public string? SelectedMgrName { get; set; }
        public List<vwDiscETCOneLine> FilteredRows { get; set; }
        public List<SelectListItem> MgrSelectList { get; set; }
        public List<vwDiscETC> AllETC { get; set; } = new List<vwDiscETC>();
        public string ForecastMessage { get; private set; }
        public string ForecastAlertClass { get; private set; } = "alert-info";

        [BindProperty]
        public List<DiscETCDto> Items { get; set; }
        //[BindProperty]
        //public DiscETCDto SingleItem { get; set; }
        [BindProperty]
        public AddETCPopupFormModel InputModel { get; set; }

        public static class ETCGroupMapping
        {
            public static readonly Dictionary<string, string> RoleToEmpResGroup = new()
            {
                ["PM"] = "PM",
                ["Engr"] = "Engineering",
                ["EIC"] = "EIC Engr & Design",
                ["Design"] = "Mechanical Design"
            };
        }

        public static class EmpGroupMapping
        {
            public static readonly Dictionary<string, int> DefaultEmpGroupID = new()
            {
                ["PM"] = 12,
                ["Engineering"] = 10,
                ["EIC Engr & Design"] = 5,
                ["Mechanical Design"] = 9
            };
        }

        public Dictionary<int, decimal?> JobProgressDict { get; set; } = new();

        public class PopupFormViewModel
        {
            public DiscETCDto DiscETC { get; set; }
            public List<vwCurves> CurveOptions { get; set; } = new();
            //public List<vwDiscETC> FilteredDiscETC { get; set; }
            public List<vwEmpGroupResources> EmpGroupOptions { get; set; } = new();
            public List<vwBudgetActuals_REVISED> OBIDOptions { get; set; } = new List<vwBudgetActuals_REVISED>();
        }

        public async Task OnGetAsync(string? mgrName, int? jobId)
        {
            var today = DateTime.Today;
            var weekDay = (int)today.DayOfWeek; // Sunday = 0, Monday = 1, etc.
            DateTime weekEnding;

            if (weekDay == 0 || weekDay == 1) // Sunday or Monday
            {
                // Last week's Saturday
                weekEnding = today.AddDays(-(weekDay + 1));
                ForecastMessage = $"You are updating forecasts starting last week (week ending {weekEnding:MM/dd})";
                ForecastAlertClass = "alert-danger"; // <-- danger for last week
            }
            else
            {
                // This week's Saturday
                weekEnding = today.AddDays(6 - weekDay);
                ForecastMessage = $"You are updating forecasts starting this week (week ending {weekEnding:MM/dd})";
                ForecastAlertClass = "alert-info"; // <-- info for this week
            }

            DateTime rptWE = ReportWE.GetReportWE();
            JobProgressDict = dal.GetProjectProgressForCurrentWeek(configuration, rptWE);
            AllETC = dal.GetAllDiscETC(configuration);
            ListCurves = dal.GetCurves(configuration);
            SelectedMgrName = mgrName;

            var (rows, mgrs) = dal.GetAllDiscETCOneLineWithMgrs(configuration);

            FilteredRows = string.IsNullOrEmpty(mgrName)
                ? new List<vwDiscETCOneLine>() // or Enumerable.Empty<YourRowType>().ToList()
                : rows.Where(x => x.MgrName == mgrName).ToList();

            FilteredRows = FilteredRows
                .OrderBy(r => r.ClientJob)
                .ToList();

            MgrSelectList = mgrs
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem { Value = x, Text = x, Selected = x == mgrName })
                .ToList();
        }

        public IActionResult OnGetEditETCHrs(int jobId, string role)
        {
            var group = ETCGroupMapping.RoleToEmpResGroup.GetValueOrDefault(role);
            var etcList = AllETC
                .Where(x => x.JobID == jobId && x.EmpResGroupDesc == group)
                .ToList();

            ViewData["Curves"] = ListCurves; // Needed for dropdown
            return Partial("_EditETCHrs", etcList);
        }

        public double GetETCHrsForRole(int? jobId, string role)
        {
            if (!jobId.HasValue) return 0;
            var group = ETCGroupMapping.RoleToEmpResGroup.GetValueOrDefault(role);
            return AllETC
                .Where(x => x.JobID == jobId && x.EmpResGroupDesc == group)
                .Sum(x => (double)x.ETCHrs);
        }

        public async Task<IActionResult> OnPostSaveETCHrsInlineAsync()
        {
            DateTime rptWE = ReportWE.GetReportWE();
            var submittedBy = Request.Form["SubmittedBy"].ToString();

            var etcUpdates = new List<(int DiscEtcID, decimal ETCHrs, DateTime? PlanStartWE, DateTime? PlanFinishWE, int? CurveID)>();
            var jobIds = new HashSet<int>();
            var jobProgressUpdates = new List<(int JobID, decimal PctComplete)>();

            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("ETCHrs_")))
            {
                var discEtcIDStr = key.Split('_')[1];
                if (int.TryParse(discEtcIDStr, out int discEtcId) && decimal.TryParse(Request.Form[key], out decimal etcHrs))
                {
                    DateTime? startWE = null;
                    DateTime? finishWE = null;
                    int? curveID = null;

                    var startStr = Request.Form[$"Start_{discEtcIDStr}"];
                    if (DateTime.TryParse(startStr, out var startDt)) startWE = startDt;

                    var finishStr = Request.Form[$"Finish_{discEtcIDStr}"];
                    if (DateTime.TryParse(finishStr, out var finishDt)) finishWE = finishDt;

                    var curveStr = Request.Form[$"CurveID_{discEtcIDStr}"];
                    if (int.TryParse(curveStr, out var cId)) curveID = cId;

                    etcUpdates.Add((discEtcId, etcHrs, startWE, finishWE, curveID));
                }

                // Extract JobID related to this DiscEtcID
                var jobIdKey = $"JobID_{discEtcIDStr}";
                if (Request.Form.ContainsKey(jobIdKey))
                {
                    var jobIdValue = Request.Form[jobIdKey].ToString();
                    if (int.TryParse(jobIdValue, out int jobId))
                    {
                        jobIds.Add(jobId);
                    }
                }
            }

            if (!etcUpdates.Any() || !jobIds.Any())
            {
                // Nothing to update or no job IDs found
                return Page();
            }

            // Build TVP DataTable for update
            var etcUpdateTable = new DataTable();
            etcUpdateTable.Columns.Add("DiscEtcID", typeof(int));
            etcUpdateTable.Columns.Add("ETCHrs", typeof(decimal));
            etcUpdateTable.Columns.Add("PlanStartWE", typeof(DateTime));
            etcUpdateTable.Columns.Add("PlanFinishWE", typeof(DateTime));
            etcUpdateTable.Columns.Add("CurveID", typeof(int));

            foreach (var item in etcUpdates)
            {
                etcUpdateTable.Rows.Add(
                    item.DiscEtcID,
                    item.ETCHrs,
                    item.PlanStartWE.HasValue ? (object)item.PlanStartWE.Value : DBNull.Value,
                    item.PlanFinishWE.HasValue ? (object)item.PlanFinishWE.Value : DBNull.Value,
                    item.CurveID.HasValue ? (object)item.CurveID.Value : DBNull.Value
                );
            }

            // Build TVP DataTable for JobIDs for stored procedure
            var jobIdTable = new DataTable();
            jobIdTable.Columns.Add("JobID", typeof(int));
            foreach (var id in jobIds)
            {
                jobIdTable.Rows.Add(id);
            }

            foreach (var jobId in jobIds)
            {
                var pctKey = $"PctComplete_{jobId}";
                if (Request.Form.ContainsKey(pctKey))
                {
                    var pctStr = Request.Form[pctKey];
                    if (decimal.TryParse(pctStr, out var pctVal))
                    {
                        jobProgressUpdates.Add((jobId, pctVal / 100m)); // Convert % to decimal
                    }
                }
            }

            // build tvp table for progress
            var progressTable = new DataTable();
            progressTable.Columns.Add("JobID", typeof(int));
            progressTable.Columns.Add("PctComplete", typeof(decimal));
            foreach (var item in jobProgressUpdates)
            {
                progressTable.Rows.Add(item.JobID, item.PctComplete);
            }

            var connString = configuration.GetConnectionString("DBCS");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();

                // 1) Update the tbDiscETC rows with user inputs, ETCCost, and ETCRateTypeID defaults
                var updateSql = @"
                    -- Assume @EtcUpdates is TVP with DiscEtcID, ETCHrs
                    UPDATE d
                    SET d.ETCHrs = u.ETCHrs,
                        d.PlanStartWE = u.PlanStartWE,
                        d.PlanFinishWE = u.PlanFinishWE,
                        d.CurveID = u.CurveID,
                        d.ETCRate = CASE WHEN d.ETCRate IS NULL OR d.ETCRate = 0 THEN 150 ELSE d.ETCRate END,
                        d.ETCCost = u.ETCHrs * CASE WHEN d.ETCRate IS NULL OR d.ETCRate = 0 THEN 150 ELSE d.ETCRate END,
                        d.ETCRateTypeID = CASE WHEN d.ETCRateTypeID IS NULL OR d.ETCRateTypeID = 0 THEN 3 ELSE d.ETCRateTypeID END
                    FROM tbDiscETC d
                    INNER JOIN @EtcUpdates u ON d.DiscEtcID = u.DiscEtcID;

                    UPDATE jp
                    SET jp.CumulPeriodProgress = p.PctComplete
                    FROM tbProjectProgress jp
                    INNER JOIN @JobProgressUpdates p ON jp.JobID = p.JobID
                    WHERE jp.WeekEnd=@rptWE;
                ";

                using (var updateCmd = new SqlCommand(updateSql, conn))
                {
                    var etcUpdatesParam = new SqlParameter
                    {
                        ParameterName = "@EtcUpdates",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.EtcUpdatesType_v2",
                        Value = etcUpdateTable
                    };
                    updateCmd.Parameters.Add(etcUpdatesParam);

                    var progressParam = new SqlParameter
                    {
                        ParameterName = "@JobProgressUpdates",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.JobProgressType",
                        Value = progressTable
                    };
                    updateCmd.Parameters.Add(progressParam);

                    var WEParam = new SqlParameter
                    {
                        ParameterName = "@rptWE",
                        SqlDbType = SqlDbType.DateTime,
                        Value = rptWE
                    };
                    updateCmd.Parameters.Add(WEParam);

                    await updateCmd.ExecuteNonQueryAsync();
                }

                // 2) Call your existing stored procedure for further processing
                using (var spCmd = new SqlCommand("dbo.spUpdateSimpleETC_EAC", conn))
                {
                    spCmd.CommandType = CommandType.StoredProcedure;

                    var jobIdsParam = new SqlParameter
                    {
                        ParameterName = "@JobIDList",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IntTableType",
                        Value = jobIdTable
                    };
                    spCmd.Parameters.Add(jobIdsParam);

                    await spCmd.ExecuteNonQueryAsync();
                }
            }

            //await SendNotificationEmail(submittedBy);

            return RedirectToPage();
        }

        private async Task SendNotificationEmail(string submittedBy)
        {
            var smtpClient = new SmtpClient("smtp.companyname.com")  // e.g., smtp.gmail.com, might be TegreCorp.onmicrosoft.com
            {
                Port = 587,
                Credentials = new NetworkCredential("santa.claus@djfkdls;lj.com", "ajskdlf;jldkjflad;jlk"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("santa.claus@jkdlf;sdlkjf.com"),
                Subject = "ETC Form Saved",
                Body = $"The ETC form was submitted by: {submittedBy} at {DateTime.Now}",
                IsBodyHtml = false,
            };

            mailMessage.To.Add("santa.claus@sjdkfl;ldskj.com");

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
