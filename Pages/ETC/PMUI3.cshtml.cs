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
    public class PMUI3Model : PageModel
    {


        public DateTime? MaxDateStamp { get; set; }
        private readonly ILogger<PMUI3Model> _logger;
        private readonly IConfiguration configuration;
        private readonly DAL dal;

        public PMUI3Model(IConfiguration configuration,ILogger<PMUI3Model>logger)
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

        public List<vwDiscETCOneLineNEW> FilteredRows { get; set; } = new List<vwDiscETCOneLineNEW>();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public string allEmpDataJson { get; set; }
        public List<vwCurves> ListCurves { get; set; } = new();
        public List<vwDiscETC> thisVWDiscETC { get; set; } = new List<vwDiscETC>();
        public List<string> AllMgrNames { get; set; }
        public string? SelectedMgrName { get; set; }
        //public List<vwDiscETCOneLine> FilteredRows { get; set; }
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

        public async Task OnGetAsync(string? mgrName,int?jobId)
        {
            var today = DateTime.Today;
            var weekDay = (int)today.DayOfWeek; // Sunday = 0, Monday = 1, etc.

            DateTime weekEnding;

            if (weekDay == 0 || weekDay == 1) // Sunday or Monday
            {
                // Last week's Saturday
                weekEnding = today.AddDays(-(weekDay + 1));
                ForecastMessage = $"You are updating forecasts starting last week (week ending {weekEnding:MM/dd})";
                ForecastAlertClass = "alert-danger";  // <-- danger for last week
            }
            else
            {
                // This week's Saturday
                weekEnding = today.AddDays(6 - weekDay);
                ForecastMessage = $"You are updating forecasts starting this week (week ending {weekEnding:MM/dd})";
                ForecastAlertClass = "alert-info";    // <-- info for this week
            }


            DateTime rptWE = ReportWE.GetReportWE();
            JobProgressDict = dal.GetProjectProgressForCurrentWeek(configuration,rptWE);
            AllETC = dal.GetAllDiscETC(configuration);
            ListCurves = dal.GetCurves(configuration);

            SelectedMgrName = mgrName;
            var (rows, mgrs) = dal.GetAllDiscETCOneLineWithMgrsNEW(configuration);

            FilteredRows = string.IsNullOrEmpty(mgrName)
                ? new List<vwDiscETCOneLineNEW>() // or Enumerable.Empty<YourRowType>().ToList()
                : rows.Where(x => x.MgrName == mgrName).ToList();
            FilteredRows=FilteredRows
                .OrderByDescending(r=>r.SimpleFlag)
                .ThenBy(r=>r.ClientJob)
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

        public async Task<IActionResult> OnPostSaveETCHrsInlineAsync(string? mgrName)
        {
            DateTime rptWE = ReportWE.GetReportWE();

            // 0) Rehydrate the same data you build in OnGet, so POST has the same context
            AllETC = dal.GetAllDiscETC(configuration);
            ListCurves = dal.GetCurves(configuration);

            SelectedMgrName = mgrName; // keep it around for redirect
            var (rows, mgrs) = dal.GetAllDiscETCOneLineWithMgrsNEW(configuration);

            FilteredRows = string.IsNullOrEmpty(mgrName)
                ? new List<vwDiscETCOneLineNEW>()
                : rows.Where(x => x.MgrName == mgrName).ToList();
            FilteredRows = FilteredRows
                    .OrderByDescending(r=>r.SimpleFlag)
                    .ThenBy(r => r.ClientJob)
                    .ToList();

            // (Optional) also rebuild MgrSelectList if you intend to return Page()
            MgrSelectList = mgrs
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem { Value = x, Text = x, Selected = x == mgrName })
                .ToList();

            // now build validRows AFTER rehydration
            var validRows = (FilteredRows ?? new List<vwDiscETCOneLineNEW>())
                .Where(r => r.SimpleFlag == 1)
                .ToList();

            var etcUpdates = new List<(int DiscEtcID, decimal ETCHrs, DateTime? PlanStartWE, DateTime? PlanFinishWE, int? CurveID)>();

            var jobIds = new HashSet<int>();

            var jobProgressUpdates = new List<(int JobID, decimal PctComplete)>();


            // Loop over form keys that start with "ETCHrs_"
            foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("ETCHrs_") && k.Length>7))
            {
                var discEtcIDStr = key.Split('_')[1];

                if (!int.TryParse(discEtcIDStr, out int discEtcId) ||
                    !decimal.TryParse(Request.Form[key], out decimal etcHrs))
                    continue;

                // Find the row in FilteredRows
                var row = validRows.FirstOrDefault(r => r.DiscETCID_PM == discEtcId
                                                        || r.DiscETCID_Engr == discEtcId
                                                        || r.DiscETCID_Design == discEtcId
                                                        || r.DiscETCID_EIC == discEtcId);

                if (row == null || row.SimpleFlag == 0)
                    continue; // skip non-simple jobs immediately

                // Determine which resource group this DiscEtcID belongs to
                string resGroup = null;
                if (row.DiscETCID_PM == discEtcId) resGroup = "PM";
                else if (row.DiscETCID_Engr == discEtcId) resGroup = "Engr";
                else if (row.DiscETCID_Design == discEtcId) resGroup = "Design";
                else if (row.DiscETCID_EIC == discEtcId) resGroup = "EIC";

                // Grab other form data
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

                // ? Move JobID extraction here, inside the loop
                var jobIdKey = $"JobID_{discEtcIDStr}";
                if (Request.Form.ContainsKey(jobIdKey) &&
                    int.TryParse(Request.Form[jobIdKey], out int jobId))
                {
                    jobIds.Add(jobId);
                }
            }

            //if (!etcUpdates.Any() || !jobIds.Any())

            // new: allow progress-only saves OR etc-only saves
            var hasEtc = etcUpdates.Any();
            var jobIdsFromEtc = new HashSet<int>(jobIds);

            // Build job progress from the POST (see #4)
            //var jobProgressUpdates = new List<(int JobID, decimal PctComplete)>();
            // ... fill it (next section) ...
            var hasProgress = jobProgressUpdates.Any();

            // If neither ETC nor Progress changed, nothing to do
            if (!hasEtc && !hasProgress)
            {
                // Nothing to update or no job IDs found
                return Page();
            }

            //// after building etcUpdates list, only leave the simple jobs
            //etcUpdates = etcUpdates
            //    .Where(u =>
            //        AllETC.Any(e => e.DiscEtcID == u.DiscEtcID &&
            //                        e.JobID != 0 &&
            //                        // only allow if SimpleFlag = 1
            //                        validRows.Any(r => r.JobID == e.JobID )))
            //    .ToList();

            etcUpdates = etcUpdates
                .Where(u =>
                    AllETC.Any(e => e.DiscEtcID == u.DiscEtcID &&
                        validRows.Any(r => r.JobID == e.JobID)))
                .ToList();


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

            //// Build TVP DataTable for JobIDs for stored procedure
            //var jobIdTable = new DataTable();
            //jobIdTable.Columns.Add("JobID", typeof(int));
            //foreach (var id in jobIds)
            //{
            //    if (FilteredRows != null && FilteredRows.Any(r => r.JobID == id && r.SimpleFlag == 1))
            //    {
            //        jobIdTable.Rows.Add(id);
            //    }
            //}

            // ? Build JobID TVP only for valid jobs
            var jobIdTable = new DataTable();
            jobIdTable.Columns.Add("JobID", typeof(int));
            foreach (var id in jobIds.Where(id => validRows.Any(r => r.JobID == id)))
            {
                jobIdTable.Rows.Add(id);
            }

            //foreach (var jobId in jobIds)
            //{
            //    var pctKey = $"PctComplete_{jobId}";
            //    if (Request.Form.ContainsKey(pctKey))
            //    {
            //        var pctStr = Request.Form[pctKey];
            //        if (decimal.TryParse(pctStr, out var pctVal))
            //        {
            //            jobProgressUpdates.Add((jobId, pctVal / 100m)); // Convert % to decimal
            //        }
            //    }
            //}
            foreach (var jobId in jobIds)
            {
                var pctKey = $"PctComplete_{jobId}";
                if (Request.Form.ContainsKey(pctKey) &&
                    decimal.TryParse(Request.Form[pctKey], out var pctVal))
                {
                    jobProgressUpdates.Add((jobId, pctVal / 100m));
                }
            }

            //filter out the nonsimple jobs
            jobProgressUpdates = jobProgressUpdates
                .Where(u => validRows.Any(r => r.JobID == u.JobID ))
                .ToList();

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
                SET 
                    d.ETCHrs = u.ETCHrs,
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

            JobProgressDict = dal.GetProjectProgressForCurrentWeek(configuration, rptWE);
            AllETC = dal.GetAllDiscETC(configuration);
            ListCurves = dal.GetCurves(configuration);

            SelectedMgrName = mgrName;
            //var (rows, mgrs) = dal.GetAllDiscETCOneLineWithMgrs(configuration);

            FilteredRows = string.IsNullOrEmpty(mgrName)
                ? new List<vwDiscETCOneLineNEW>() // or Enumerable.Empty<YourRowType>().ToList()
                : rows.Where(x => x.MgrName == mgrName).ToList();
            FilteredRows = FilteredRows
                .OrderByDescending(r=>r.SimpleFlag)
                .ThenBy(r => r.ClientJob)
                .ToList();

            MgrSelectList = mgrs
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new SelectListItem { Value = x, Text = x, Selected = x == mgrName })
                .ToList();

            //return Page(); // NOT RedirectToPage

            return RedirectToPage(new { mgrName = SelectedMgrName });
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
