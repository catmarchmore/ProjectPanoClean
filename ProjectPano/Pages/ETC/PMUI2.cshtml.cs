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


namespace ProjectPano.Pages.ETC
{
    [IgnoreAntiforgeryToken]
    public class PMUI2Model : PageModel
    {


        public DateTime? MaxDateStamp { get; set; }
        private readonly ILogger<PMUI2Model> _logger;
        private readonly IConfiguration configuration;
        private readonly DAL dal;

        public PMUI2Model(IConfiguration configuration,ILogger<PMUI2Model>logger)
        {
            _logger = logger;
            this.configuration = configuration;
            this.dal = new DAL();
        }

        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        //public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();

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

        [BindProperty]
        public List<DiscETCDto> Items { get; set; }

        [BindProperty]
        public DiscETCDto SingleItem { get; set; }

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

        public class PopupFormViewModel
        {
            public DiscETCDto DiscETC { get; set; }
            public List<vwCurves> CurveOptions { get; set; } = new();
            //public List<vwDiscETC> FilteredDiscETC { get; set; }
            public List<vwEmpGroupResources> EmpGroupOptions { get; set; } = new();
            public List<vwBudgetActuals_REVISED> OBIDOptions { get; set; } = new List<vwBudgetActuals_REVISED>();
        }

        public async Task OnGetAsync(string? mgrName)
        {
            {
                DateTime rptWE = ReportWE.GetReportWE();
                AllETC = dal.GetAllDiscETC(configuration);

                SelectedMgrName = mgrName;
                var (rows, mgrs) = dal.GetAllDiscETCOneLineWithMgrs(configuration);
                FilteredRows = string.IsNullOrEmpty(mgrName) ? rows : rows.Where(x => x.MgrName == mgrName).ToList();

                MgrSelectList = mgrs
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new SelectListItem { Value = x, Text = x, Selected = x == mgrName })
                    .ToList();
            }
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




        public PartialViewResult OnGetAddETCHrsPartial(int jobId, string empResGroupDesc)
        {
            var obidList = dal.GetVWBudgetActuals(jobId, configuration); // List<vwBudgetActuals_Revised>
            var curveList = dal.GetCurves(configuration);               // List<vwCurves>
            var empGroupList = dal.GetResourceGroupSummaries(configuration); // List<ResourceDetailGroups>
            DateTime rptWE = ReportWE.GetReportWE();

            // Map EmpResGroupDesc to default EmpGroupID (if needed)
            var empGroupMatch = empGroupList
                .FirstOrDefault(x => x.EmpResGroupDesc?.Trim().Equals(empResGroupDesc?.Trim(), StringComparison.OrdinalIgnoreCase) == true);

            int? defaultEmpGroupID = empGroupMatch?.EmpGroupID;

            // Prefill a new DiscETCDto for the form
            var vm = new DiscETCDto
            {
                JobID = jobId,
                OBID = 0,
                RptWeekend = rptWE,
                ETCHrs = 0,
                ETCCost = 0,
                EACHrs = 0,
                EACCost = 0,
                ETCComment = "",
                PlanStartWE = rptWE,
                PlanFinishWE = rptWE,
                EmpID = 999,
                EmpGroupID = defaultEmpGroupID,
                CurveID = 1,
                ETCRate = 150,
                ETCRateTypeID = 3
            };

            var popupModel = new AddETCPopupFormModel
            {
                vwBudgetActuals = obidList,
                ListCurves = curveList,
                ListResourceGroups = empGroupList,
                RptWE = rptWE,
                DiscETC = vm
            };

            return Partial("_AddETCHrs", popupModel);
        }

        public async Task<IActionResult> OnPostEditFormAsync(List<DiscETCDto> items, DiscETCDto singleItem)
        {
            if (singleItem != null)
            {
                items = new List<DiscETCDto> { singleItem };
            }

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"ModelState error in '{kvp.Key}': {error.ErrorMessage}");
                    }
                }

                return BadRequest(new
                {
                    message = "Invalid input",
                    errors = ModelState.Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                });
            }

            try
            {
                using var conn = new SqlConnection(configuration.GetConnectionString("DBCS"));
                await conn.OpenAsync();

                DateTime currentWeek = ReportWE.GetReportWE().Date;

                foreach (var dto in Items)
                {
                    // Validate default values
                    if (dto.EmpID == null || dto.EmpID == 0)
                        dto.EmpID = 999;
                    if (dto.ETCRateTypeID == null || dto.ETCRateTypeID == 0)
                        dto.ETCRateTypeID = 3;

                    // Get previous cumulative hours and cost for the OBID
                    decimal prevWkCumulHrs = 0;
                    decimal prevWkCumulCost = 0;

                    using (var cmd = new SqlCommand("SELECT PrevWkCumulHrs, PrevWkCumulCost FROM vwBudgetActuals_REVISED WHERE OBID = @OBID", conn))
                    {
                        cmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0)) prevWkCumulHrs = reader.GetDecimal(0);
                            if (!reader.IsDBNull(1)) prevWkCumulCost = reader.GetDecimal(1);
                        }
                    }

                    bool isUpdate = false;
                    if (dto.DiscEtcID > 0)
                    {
                        using var checkCmd = new SqlCommand("SELECT RptWeekend FROM tbDiscETC WHERE DiscEtcID = @DiscEtcID", conn);
                        checkCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);
                        var result = await checkCmd.ExecuteScalarAsync();

                        if (result != null && DateTime.TryParse(result.ToString(), out DateTime existingWeek))
                        {
                            if (existingWeek.Date == currentWeek.Date)
                            {
                                isUpdate = true;
                            }
                        }
                    }

                    // Check if first ETC for this week
                    bool isFirstETCThisWeek = false;
                    using (var checkExistingCmd = new SqlCommand($@"
                SELECT COUNT(*) FROM tbDiscETC
                WHERE OBID = @OBID AND RptWeekend = @RptWeekend
                {(isUpdate ? "AND DiscEtcID <> @DiscEtcID" : "")}", conn))
                    {
                        checkExistingCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        checkExistingCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);
                        if (isUpdate)
                            checkExistingCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);

                        int existingCount = (int)await checkExistingCmd.ExecuteScalarAsync();
                        isFirstETCThisWeek = existingCount == 0;
                    }

                    decimal etcCost = dto.ETCCost ?? 0m;
                    decimal eachrs = dto.ETCHrs;
                    decimal eacCost = etcCost;

                    if (isFirstETCThisWeek)
                    {
                        eachrs += prevWkCumulHrs;
                        eacCost += prevWkCumulCost;
                    }

                    if (isUpdate)
                    {
                        using var updateCmd = new SqlCommand(@"
                    UPDATE tbDiscETC SET
                        OBID = @OBID,
                        ETCHrs = @ETCHrs,
                        ETCCost = @ETCCost,
                        EACHrs = @EACHrs,
                        EACCost = @EACCost,
                        ETCComment = @ETCComment,
                        PlanStartWE = @PlanStartWE,
                        PlanFinishWE = @PlanFinishWE,
                        EmpID = @EmpID,
                        EmpGroupID = @EmpGroupID,
                        CurveID = @CurveID,
                        ETCRate = @ETCRate,
                        ETCRateTypeID = @ETCRateTypeID
                    WHERE DiscEtcID = @DiscEtcID", conn);

                        updateCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);
                        updateCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        updateCmd.Parameters.AddWithValue("@ETCHrs", dto.ETCHrs);
                        updateCmd.Parameters.AddWithValue("@ETCCost", etcCost);
                        updateCmd.Parameters.AddWithValue("@EACHrs", eachrs);
                        updateCmd.Parameters.AddWithValue("@EACCost", eacCost);
                        updateCmd.Parameters.AddWithValue("@ETCComment", (object?)dto.ETCComment ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@PlanStartWE", (object?)dto.PlanStartWE ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@PlanFinishWE", (object?)dto.PlanFinishWE ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@EmpID", (object?)dto.EmpID ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@EmpGroupID", dto.EmpGroupID);
                        updateCmd.Parameters.AddWithValue("@CurveID", dto.CurveID);
                        updateCmd.Parameters.AddWithValue("@ETCRate", (object?)dto.ETCRate ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ETCRateTypeID", dto.ETCRateTypeID);

                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        using var insertCmd = new SqlCommand(@"
                    INSERT INTO tbDiscETC (
                        OBID, RptWeekend, ETCHrs, ETCCost, EACHrs, EACCost,
                        ETCComment, PlanStartWE, PlanFinishWE, EmpID, EmpGroupID, JobID, CurveID, ETCRate, ETCRateTypeID
                    )
                    VALUES (
                        @OBID, @RptWeekend, @ETCHrs, @ETCCost, @EACHrs, @EACCost,
                        @ETCComment, @PlanStartWE, @PlanFinishWE, @EmpID, @EmpGroupID, @JobID, @CurveID, @ETCRate, @ETCRateTypeID
                    )", conn);

                        insertCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        insertCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);
                        insertCmd.Parameters.AddWithValue("@ETCHrs", dto.ETCHrs);
                        insertCmd.Parameters.AddWithValue("@ETCCost", etcCost);
                        insertCmd.Parameters.AddWithValue("@EACHrs", eachrs);
                        insertCmd.Parameters.AddWithValue("@EACCost", eacCost);
                        insertCmd.Parameters.AddWithValue("@ETCComment", (object?)dto.ETCComment ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@PlanStartWE", (object?)dto.PlanStartWE ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@PlanFinishWE", (object?)dto.PlanFinishWE ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@EmpID", (object?)dto.EmpID ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@EmpGroupID", dto.EmpGroupID);
                        insertCmd.Parameters.AddWithValue("@JobID", dto.JobID);
                        insertCmd.Parameters.AddWithValue("@CurveID", dto.CurveID);
                        insertCmd.Parameters.AddWithValue("@ETCRate", (object?)dto.ETCRate ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ETCRateTypeID", dto.ETCRateTypeID);

                        await insertCmd.ExecuteNonQueryAsync();

                        //FilteredRows = string.IsNullOrEmpty(mgrName) ? rows : rows.Where(x => x.MgrName == mgrName).ToList();
                    }
                }

                // Update project progress after all records processed
                DAL dal = new DAL();
                var allEtcs = dal.GetDiscETC(Items.FirstOrDefault()?.JobID ?? 0, configuration);

                decimal totalEAC = allEtcs.Sum(e => e.EACCost);
                DateTime? latestFinish = allEtcs.Max(e => e.PlanFinishWE);

                var progress = dal.GetOrCreateProgressStatusByJobID(Items.FirstOrDefault()?.JobID ?? 0, configuration);
                progress.EAC_Info = totalEAC;
                progress.FcastFinishDate = latestFinish ?? progress.FcastFinishDate;
                dal.UpdateProgressStatus(progress, configuration);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception (EditForm batch): " + ex.Message);
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEditFormSingleAsync(DiscETCDto dto)
        {

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"ModelState error in '{kvp.Key}': {error.ErrorMessage}");
                    }
                }

                return BadRequest(new
                {
                    message = "Invalid input",
                    errors = ModelState.Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                });
            }

            try
            {
                using var conn = new SqlConnection(configuration.GetConnectionString("DBCS"));
                await conn.OpenAsync();

                DateTime currentWeek = ReportWE.GetReportWE().Date;

                Console.WriteLine($"?? Handling save for JobID: {dto.JobID}");


                //foreach (var dto in Items)
                //{
                    // Validate default values
                    if (dto.EmpID == null || dto.EmpID == 0)
                        dto.EmpID = 999;
                    if (dto.ETCRateTypeID == null || dto.ETCRateTypeID == 0)
                        dto.ETCRateTypeID = 3;

                    // Get previous cumulative hours and cost for the OBID
                    decimal prevWkCumulHrs = 0;
                    decimal prevWkCumulCost = 0;

                    using (var cmd = new SqlCommand("SELECT PrevWkCumulHrs, PrevWkCumulCost FROM vwBudgetActuals_REVISED WHERE OBID = @OBID", conn))
                    {
                        cmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        using var reader = await cmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0)) prevWkCumulHrs = reader.GetDecimal(0);
                            if (!reader.IsDBNull(1)) prevWkCumulCost = reader.GetDecimal(1);
                        }
                    }

                    bool isUpdate = false;
                    if (dto.DiscEtcID > 0)
                    {
                        using var checkCmd = new SqlCommand("SELECT RptWeekend FROM tbDiscETC WHERE DiscEtcID = @DiscEtcID", conn);
                        checkCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);
                        var result = await checkCmd.ExecuteScalarAsync();

                        if (result != null && DateTime.TryParse(result.ToString(), out DateTime existingWeek))
                        {
                            if (existingWeek.Date == currentWeek.Date)
                            {
                                isUpdate = true;
                            }
                        }
                    }

                    // Check if first ETC for this week
                    bool isFirstETCThisWeek = false;
                    using (var checkExistingCmd = new SqlCommand($@"
                SELECT COUNT(*) FROM tbDiscETC
                WHERE OBID = @OBID AND RptWeekend = @RptWeekend
                {(isUpdate ? "AND DiscEtcID <> @DiscEtcID" : "")}", conn))
                    {
                        checkExistingCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        checkExistingCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);
                        if (isUpdate)
                            checkExistingCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);

                        int existingCount = (int)await checkExistingCmd.ExecuteScalarAsync();
                        isFirstETCThisWeek = existingCount == 0;
                    }

                    decimal etcCost = dto.ETCCost ?? 0m;
                    decimal eachrs = dto.ETCHrs;
                    decimal eacCost = etcCost;

                    if (isFirstETCThisWeek)
                    {
                        eachrs += prevWkCumulHrs;
                        eacCost += prevWkCumulCost;
                    }

                    if (isUpdate)
                    {
                        using var updateCmd = new SqlCommand(@"
                    UPDATE tbDiscETC SET
                        OBID = @OBID,
                        ETCHrs = @ETCHrs,
                        ETCCost = @ETCCost,
                        EACHrs = @EACHrs,
                        EACCost = @EACCost,
                        ETCComment = @ETCComment,
                        PlanStartWE = @PlanStartWE,
                        PlanFinishWE = @PlanFinishWE,
                        EmpID = @EmpID,
                        EmpGroupID = @EmpGroupID,
                        CurveID = @CurveID,
                        ETCRate = @ETCRate,
                        ETCRateTypeID = @ETCRateTypeID
                    WHERE DiscEtcID = @DiscEtcID", conn);

                        updateCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);
                        updateCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        updateCmd.Parameters.AddWithValue("@ETCHrs", dto.ETCHrs);
                        updateCmd.Parameters.AddWithValue("@ETCCost", etcCost);
                        updateCmd.Parameters.AddWithValue("@EACHrs", eachrs);
                        updateCmd.Parameters.AddWithValue("@EACCost", eacCost);
                        updateCmd.Parameters.AddWithValue("@ETCComment", (object?)dto.ETCComment ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@PlanStartWE", (object?)dto.PlanStartWE ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@PlanFinishWE", (object?)dto.PlanFinishWE ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@EmpID", (object?)dto.EmpID ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@EmpGroupID", dto.EmpGroupID);
                        updateCmd.Parameters.AddWithValue("@CurveID", dto.CurveID);
                        updateCmd.Parameters.AddWithValue("@ETCRate", (object?)dto.ETCRate ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ETCRateTypeID", dto.ETCRateTypeID);

                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        using var insertCmd = new SqlCommand(@"
                    INSERT INTO tbDiscETC (
                        OBID, RptWeekend, ETCHrs, ETCCost, EACHrs, EACCost,
                        ETCComment, PlanStartWE, PlanFinishWE, EmpID, EmpGroupID, JobID, CurveID, ETCRate, ETCRateTypeID
                    )
                    VALUES (
                        @OBID, @RptWeekend, @ETCHrs, @ETCCost, @EACHrs, @EACCost,
                        @ETCComment, @PlanStartWE, @PlanFinishWE, @EmpID, @EmpGroupID, @JobID, @CurveID, @ETCRate, @ETCRateTypeID
                    )", conn);

                        insertCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        insertCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);
                        insertCmd.Parameters.AddWithValue("@ETCHrs", dto.ETCHrs);
                        insertCmd.Parameters.AddWithValue("@ETCCost", etcCost);
                        insertCmd.Parameters.AddWithValue("@EACHrs", eachrs);
                        insertCmd.Parameters.AddWithValue("@EACCost", eacCost);
                        insertCmd.Parameters.AddWithValue("@ETCComment", (object?)dto.ETCComment ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@PlanStartWE", (object?)dto.PlanStartWE ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@PlanFinishWE", (object?)dto.PlanFinishWE ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@EmpID", (object?)dto.EmpID ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@EmpGroupID", dto.EmpGroupID);
                        insertCmd.Parameters.AddWithValue("@JobID", dto.JobID);
                        insertCmd.Parameters.AddWithValue("@CurveID", dto.CurveID);
                        insertCmd.Parameters.AddWithValue("@ETCRate", (object?)dto.ETCRate ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ETCRateTypeID", dto.ETCRateTypeID);

                        await insertCmd.ExecuteNonQueryAsync();

                        //FilteredRows = string.IsNullOrEmpty(mgrName) ? rows : rows.Where(x => x.MgrName == mgrName).ToList();
                    }
                //}

                // Update project progress after all records processed
                DAL dal = new DAL();
                //var allEtcs = dal.GetDiscETC(Items.FirstOrDefault()?.JobID ?? 0, configuration);

                //int jobId = DiscETC.JobID ?? 0;
                int jobId = dto.JobID;

                if (jobId == 0)
                {
                    Console.WriteLine("? Invalid or missing JobID in submitted items.");
                    return StatusCode(500, new { message = "Missing or invalid JobID in posted data." });
                }

                var allEtcs = dal.GetDiscETC(jobId, configuration);

                if (allEtcs == null || !allEtcs.Any())
                {
                    Console.WriteLine($"?? No ETC records found for JobID {jobId}.");
                    return StatusCode(500, new { message = "No ETC records found for JobID." });
                }


                decimal totalEAC = allEtcs.Sum(e => e.EACCost);
                DateTime? latestFinish = allEtcs.Max(e => e.PlanFinishWE);

                var progress = dal.GetOrCreateProgressStatusByJobID(jobId, configuration);

                progress.EAC_Info = totalEAC;
                progress.FcastFinishDate = latestFinish ?? progress.FcastFinishDate;
                dal.UpdateProgressStatus(progress, configuration);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error in OnPostEditFormAsync");
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }


    }
}
