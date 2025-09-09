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

namespace ProjectPano.Pages.ETC
{
    [IgnoreAntiforgeryToken]
    public class Index1Model : PageModel
    {
        //public tbProjectProgress ProgressStatus = new tbProjectProgress();
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        public string budgetActualsJson { get; set; }
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public string allEmpDataJson { get; set; }
        public List<vwCurves>ListCurves { get; set; } = new();
        public List<vwDiscETC> thisVWDiscETC { get; set; } = new List<vwDiscETC>();

        public List<ETCSpreadChartDto> JobSpreadWeeks { get; set; } = new();
        public List<vwActuals_byProject_byWeek_nogroup> JobActualsByWeek { get; set; } = new();

        public string spreadDataJson { get; set; }

        public List<ChartSummaryPoint> SummaryPoints =>
            JobActualsByWeek?
                .OrderBy(p => p.WeekEnd)
                .Select(p => new ChartSummaryPoint
                {
                    WeekEnding = p.WeekEnd,
                    BillwithAdminDisc = p.BillwithAdminDisc,
                    CURRENTCOST = p.CURRENTCOST
                }).ToList() ?? new List<ChartSummaryPoint>();

        public string summaryJson { get; set; }

        public DateTime CurrWeekEnding { get; set; } = DateTime.Today;
        public DateTime PrevWeekEnding { get; set; } = DateTime.Today.AddDays(-7);

        public class ChartSummaryPoint
        {
            public DateTime WeekEnding { get; set; }
            public decimal BillwithAdminDisc { get; set; }
            public decimal CURRENTCOST { get; set; }
        }


        public List<ETCSpreadChartDto> GetSpreadByJob(int jobId)
        {
            //DAL dal = new DAL();
            var allEtcs = dal.GetDiscETC(jobId,configuration);
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetWklyActualsbyJob(jobId, configuration);

            // Optionally filter by current RptWE
            DateTime rptWE = ReportWE.GetReportWE();
            allEtcs = allEtcs.Where(e => e.PlanFinishWE >= rptWE).ToList();

            return SpreadUtility.GetSpread(allEtcs, curves, actuals);
        }

        public List<ETCSpreadChartDto> SpreadData { get; set; }

        public SelectList JobSelectList { get; set; }
        public class ShiftRequest
        {
            public int Weeks { get; set; }
            public int JobID { get; set; }
            public DateTime RptWeekend { get; set; }
        }

        public class BulkScheduleDto
        {
            public List<string> DiscEtcIDs { get; set; } = new();
            public DateTime? PlanStartWE { get; set; }
            public DateTime? PlanFinishWE { get; set; }
            public int? CurveID { get; set; }
        }

        public bool CanUseEACEqualsCAB {  get; set; }
        public bool CanUseEACEqualsLast { get; set; }

        public Index1Model(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }

        [BindProperty]
        public List<DiscETCDto> Items { get; set; }

        [BindProperty]
        public DiscETCDto SingleItem { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? JobId { get; set; }

        //public IActionResult Index1(int jobId)
        //{
        //    // jobId parameter automatically bound from ?jobId=123
        //}

        public async Task<IActionResult> OnGetAsync(int? jobId)
        {
            var jobs = dal.GetAllOpenProj(configuration);
            JobSelectList = new SelectList(jobs, "JobID", "BigTimeJobDisplayName");
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListResourceGroups = dal.GetResourceGroupSummaries(configuration);
            ListEmpGroupResources = dal.GetResourceGroups(configuration);
            allEmpDataJson = JsonSerializer.Serialize(ListEmpGroupResources);
            var today = DateTime.Today;
            CurrWeekEnding = GetWE.GetWeekEnding(today);
            PrevWeekEnding = CurrWeekEnding.AddDays(-7);

            ListCurves = dal.GetCurves(configuration);
            OpenProj = dal.GetAllOpenProj(configuration);

            var filteredJobs = OpenProj
                    .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1)
                    .OrderBy(j => j.ClientJob)
                    .ToList();
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", jobId);

            if (jobId == null)
            {
                // Nothing selected yet, don’t load spread/chart/table
                thisVWDiscETC = new List<vwDiscETC>();
                JobActualsByWeek = new List<vwActuals_byProject_byWeek_nogroup>();
                vwBudgetActuals = new List<vwBudgetActuals_REVISED>();
                SpreadData = new List<ETCSpreadChartDto>(); // ? Prevents Razor null issues
                //SummaryPoints=new List<ChartSummaryPoint>();

                spreadDataJson = JsonSerializer.Serialize(SpreadData);
                summaryJson = JsonSerializer.Serialize(SummaryPoints);
                budgetActualsJson = JsonSerializer.Serialize(vwBudgetActuals);

                return Page();
            }

            var jobList = dal.GetThisWJob(jobId.Value, configuration);

            Console.WriteLine(thisVWJob.JobID);
            if (jobList.Count > 0)
                thisVWJob = jobList[0];

            //thisVWJob = dal.GetVWJob(jobId.Value, configuration);
            vwBudgetActuals = dal.GetVWBudgetActuals(jobId.Value, configuration);
            budgetActualsJson = JsonSerializer.Serialize(vwBudgetActuals);

            thisVWDiscETC = dal.GetDiscETC(jobId.Value, configuration);
            JobActualsByWeek = dal.GetWklyActualsbyJobNoGroup(jobId.Value, configuration);

            SpreadData = GetSpreadByJob(jobId.Value);
            spreadDataJson = JsonSerializer.Serialize(SpreadData);

            //SummaryPoints = GetSummaryPoints(jobId.Value);
            summaryJson = JsonSerializer.Serialize(SummaryPoints);

            CanUseEACEqualsCAB = vwBudgetActuals.All(x => x.BILLWITHADMIN <= x.CURRCOST);
            CanUseEACEqualsLast = vwBudgetActuals.All(x => x.BILLWITHADMIN <= x.EAC_Cost);

            return Page();
        }


        public async Task<IActionResult> OnPostAddInitialETCAsync([FromBody] JobRequest request)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
                return BadRequest(new
                {
                    message = "Invalid input",
                    errors = ModelState.Select(kvp => new {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                }); ;
            }

            try
            {
                int jobId = request.JobId;
                var reportWE = ReportWE.GetReportWE();

                DAL dal = new DAL();
                var jobList = dal.GetThisWJob(jobId, configuration);
                if (jobList.Count == 0)
                    return NotFound(new { message = "Job not found" });

                var job = jobList[0];
                //var startWE = GetWE.GetWeekEnding(job.JobStartDate);
                var startWE = reportWE;
                var finishWE = GetWE.GetWeekEnding(job.JobFinishDate);
                var budgetActuals = dal.GetVWBudgetActuals(jobId, configuration);

                // TODO: Replace with values from form once added
                int empID = 999;
                int etcRateTypeID = 1;// Placeholder
                //int empGroupID = 1;      // Placeholder
                int curveID = 1;      // Placeholder

                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                {
                    conn.Open();

                    // ? Delete existing ETC records for this JobID + RptWeekend
                    using (var deleteCmd = new SqlCommand(@"
                        DELETE FROM tbDiscETC 
                        WHERE JobID = @JobID AND RptWeekend = @RptWeekend", conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@JobID", jobId);
                        deleteCmd.Parameters.AddWithValue("@RptWeekend", reportWE);
                        await deleteCmd.ExecuteNonQueryAsync();
                    }

                    // ? Now insert new ETC records
                    foreach (var ba in budgetActuals)
                    {
                        //var avgRate = (ba.BILLQTY == 0)
                        //    ? (ba.CURRHRS != 0 ? ba.CURRCOST / ba.CURRHRS : 0)
                        //    : ba.BILLWITHADMINDISC / ba.BILLQTY;
                        var avgRate = (ba.ActRate == 0) ? ba.CurrRate : ba.ActRate;
                        var etcCost = ba.CURRCOST - ba.BILLWITHADMINDISC;
                        var etcHrs = (avgRate != 0) ? etcCost / avgRate : 0;
                        var eachrs = etcHrs + ba.BILLQTY;
                        var eacCost = ba.CURRCOST;

                        using (var insertcmd = new SqlCommand(@"
                            INSERT INTO tbDiscETC (
                                OBID, RptWeekend, ETCHrs, ETCCost, EACHrs, EACCost,
                                ETCComment, PlanStartWE, PlanFinishWE, EmpID, EmpGroupID, JobID,CurveID,ETCRate,ETCRateTypeID
                                )
                            VALUES (
                                @OBID, @RptWeekend, @ETCHrs, @ETCCost, @EACHrs, @EACCost,
                                @ETCComment, @PlanStartWE, @PlanFinishWE, @EmpID, @EmpGroupID, @JobID,@CurveID,@ETCRate,@ETCRateTypeID
                                )", conn))
                        {
                            insertcmd.Parameters.AddWithValue("@OBID", ba.OBID);
                            insertcmd.Parameters.AddWithValue("@RptWeekend", reportWE);
                            insertcmd.Parameters.AddWithValue("@ETCHrs", etcHrs);
                            insertcmd.Parameters.AddWithValue("@ETCCost", etcCost);
                            insertcmd.Parameters.AddWithValue("@EACHrs", eachrs);
                            insertcmd.Parameters.AddWithValue("@EACCost", eacCost);
                            insertcmd.Parameters.AddWithValue("@ETCComment", "");
                            insertcmd.Parameters.AddWithValue("@PlanStartWE", startWE);
                            insertcmd.Parameters.AddWithValue("@PlanFinishWE", finishWE);
                            insertcmd.Parameters.AddWithValue("@EmpID", empID);
                            //insertcmd.Parameters.AddWithValue("@EmpGroupID", empGroupID);
                            insertcmd.Parameters.AddWithValue("@EmpGroupID", ba.DefaultEmpGroupID);
                            insertcmd.Parameters.AddWithValue("@JobID", jobId);
                            insertcmd.Parameters.AddWithValue("@CurveID", curveID);
                            insertcmd.Parameters.AddWithValue("@ETCRate", avgRate);
                            insertcmd.Parameters.AddWithValue("@ETCRateTypeID", etcRateTypeID);

                            await insertcmd.ExecuteNonQueryAsync();
                        }

                        if (etcHrs == 0)
                        {
                            Console.WriteLine($"Skipping OBID {ba.OBID} — avgRate={avgRate}, etcCost={etcCost}");
                            continue;
                        }

                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostAddSingleETCAsync([FromBody] DiscETCDto dto)
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
                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                {
                    await conn.OpenAsync();

                    DateTime currentWeek = ReportWE.GetReportWE().Date; // Get the "this week's" Saturday
                    DateTime incomingWeek = dto.RptWeekend ?? currentWeek;

                    decimal prevWkCumulHrs = 0;
                    decimal prevWkCumulCost = 0;

                    Console.WriteLine($"OBID = {dto.OBID}, ETCHrs = {dto.ETCHrs}, ETCCost = {dto.ETCCost}");
                    using (var cmd = new SqlCommand("SELECT PrevWkCumulHrs, PrevWkCumulCost FROM vwBudgetActuals_REVISED WHERE OBID = @OBID", conn))
                    {
                        cmd.Parameters.AddWithValue("@OBID", dto.OBID);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(0)) prevWkCumulHrs = reader.GetDecimal(0);
                                if (!reader.IsDBNull(1)) prevWkCumulCost = reader.GetDecimal(1);
                            }
                        }
                    }
                    Console.WriteLine($"PrevWkCumulHrs = {prevWkCumulHrs}, PrevWkCumulCost = {prevWkCumulCost}");

                    bool isUpdate = false;

                    // If a DiscEtcID was provided, check if it matches the current week
                    if (dto.DiscEtcID > 0)
                    {
                        Console.WriteLine($"Checking tbDiscETC where DiscEtcID = {dto.DiscEtcID}");

                        using (var checkCmd = new SqlCommand("SELECT RptWeekend FROM tbDiscETC WHERE DiscEtcID = @DiscEtcID", conn))
                        {
                            checkCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);
                            var result = await checkCmd.ExecuteScalarAsync();

                            if (result != null && DateTime.TryParse(result.ToString(), out DateTime existingWeek))
                            {
                                Console.WriteLine($"Existing RptWeekend: {existingWeek.Date}, Current RptWeekend: {currentWeek.Date}");

                                if (existingWeek.Date == currentWeek.Date)
                                {
                                    isUpdate = true;
                                }
                            }
                        }
                    }

                    bool isFirstETCThisWeek = false;

                    using (var checkExistingCmd = new SqlCommand(@"
                        SELECT COUNT(*) FROM tbDiscETC
                        WHERE OBID = @OBID AND RptWeekend = @RptWeekend
                        " + (isUpdate ? "AND DiscEtcID <> @DiscEtcID" : ""), conn))

                    {
                        checkExistingCmd.Parameters.AddWithValue("@OBID",dto.OBID);
                        checkExistingCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);

                        if (isUpdate)
                            checkExistingCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);

                        int existingCount = (int)await checkExistingCmd.ExecuteScalarAsync();
                        isFirstETCThisWeek=(existingCount == 0);
                    }

                    decimal etcCost = dto.ETCCost ?? 0m;
                    decimal eachrs = dto.ETCHrs;
                    decimal eacCost = etcCost;

                    if (isFirstETCThisWeek)
                    {
                        eachrs += prevWkCumulHrs;
                        eacCost += prevWkCumulCost;
                    }

                    // Default missing EmpID to 999
                    if (dto.EmpID == null || dto.EmpID == 0)
                    {
                        dto.EmpID = 999;
                    }

                    if (dto.ETCRateTypeID == null || dto.ETCRateTypeID == 0)
                    {
                        dto.ETCRateTypeID = 3;
                    }

                    if (isUpdate)
                    {
                        // ? UPDATE existing record
                        using (var updateCmd = new SqlCommand(@"
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
                        ETCRate=@ETCRate,
                        ETCRateTypeID=@ETCRateTypeID
                    WHERE DiscEtcID = @DiscEtcID", conn))
                        {
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
                    }
                    else
                    {
                        // ? INSERT new record for current week
                        using (var insertCmd = new SqlCommand(@"
                    INSERT INTO tbDiscETC (
                        OBID, RptWeekend, ETCHrs, ETCCost, EACHrs, EACCost,
                        ETCComment, PlanStartWE, PlanFinishWE, EmpID, EmpGroupID, JobID, CurveID,ETCRate,ETCRateTypeID
                    )
                    VALUES (
                        @OBID, @RptWeekend, @ETCHrs, @ETCCost, @EACHrs, @EACCost,
                        @ETCComment, @PlanStartWE, @PlanFinishWE, @EmpID, @EmpGroupID, @JobID, @CurveID, @ETCRate,@ETCRateTypeID
                    )", conn))
                        {
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
                        }
                    }

                    // ?? Recalculate EACCost across all DiscETCIDs for this OBID & current week
                    using (var recalcCmd = new SqlCommand(@"
                        SELECT DiscEtcID, ETCHrs, ETCRate 
                        FROM tbDiscETC 
                        WHERE OBID = @OBID AND RptWeekend = @RptWeekend", conn))
                    {
                        recalcCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        recalcCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);

                        var etcRows = new List<(int DiscEtcID, decimal ETCHrs, decimal Rate)>();

                        using (var reader = await recalcCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                etcRows.Add((
                                    reader.GetInt32(0),
                                    reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                                    reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                                ));
                            }
                        }

                        decimal totalHrs = etcRows.Sum(x => x.ETCHrs);
                        decimal totalCost = etcRows.Sum(x => x.ETCHrs * x.Rate);
                        decimal newEAC = totalCost + prevWkCumulCost;

                        int rowCount = etcRows.Count; // count of DiscEtcIDs for this OBID/week

                        using (var updateEACCmd = new SqlCommand(
                            "UPDATE tbDiscETC SET EACCost = @EACCost, EACHrs = @EACHrs WHERE DiscEtcID = @DiscEtcID", conn))
                        {
                            updateEACCmd.Parameters.Add("@EACCost", SqlDbType.Decimal);
                            updateEACCmd.Parameters.Add("@EACHrs", SqlDbType.Decimal);
                            updateEACCmd.Parameters.Add("@DiscEtcID", SqlDbType.Int);

                            foreach (var row in etcRows)
                            {
                                decimal proportionalEAC;
                                decimal proportionalEACHrs;

                                if (rowCount == 1)
                                {
                                    proportionalEAC = newEAC;
                                    proportionalEACHrs = totalHrs + prevWkCumulHrs;
                                }
                                else if (totalHrs > 0)
                                {
                                    proportionalEAC = (row.ETCHrs / totalHrs) * newEAC;
                                    proportionalEACHrs = (row.ETCHrs / totalHrs) * (totalHrs + prevWkCumulHrs);
                                }
                                else
                                {
                                    proportionalEAC = newEAC / rowCount;
                                    proportionalEACHrs = (totalHrs + prevWkCumulHrs) / rowCount;
                                }

                                updateEACCmd.Parameters["@EACCost"].Value = proportionalEAC;
                                updateEACCmd.Parameters["@EACHrs"].Value = proportionalEACHrs;
                                updateEACCmd.Parameters["@DiscEtcID"].Value = row.DiscEtcID;
                                await updateEACCmd.ExecuteNonQueryAsync();
                            }
                        }


                    }

                }

                // ? update the latest tbProjectProgress record

                DAL dal = new DAL();

                // Load all ETCs for this job (same logic as in your page model)
                var allEtcs = dal.GetDiscETC(dto.JobID, configuration);

                decimal totalEAC = allEtcs.Sum(e => e.EACCost);
                DateTime? latestFinish = allEtcs.Max(e => e.PlanFinishWE);

                //get EAC (spend) for obid's with no forecast
                decimal spentCost = 0m;
                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                {
                    await conn.OpenAsync(); // <-- Missing in your code

                    using (var spentCmd = new SqlCommand(@"
                        SELECT SUM(SpentCost) 
                        FROM vwActualsWithNoForecast 
                        WHERE JobID = @JobID", conn))
                    {
                        spentCmd.Parameters.AddWithValue("@JobID", dto.JobID);
                        var result = await spentCmd.ExecuteScalarAsync();
                        if (result != DBNull.Value && result != null)
                            spentCost = Convert.ToDecimal(result);
                    }
                }

                // Get or create the progress record for the current week
                var progress = dal.GetOrCreateProgressStatusByJobID(dto.JobID, configuration);

                // Update and save
                progress.EAC_Info = totalEAC+spentCost;
                progress.FcastFinishDate = latestFinish ?? progress.FcastFinishDate;
                dal.UpdateProgressStatus(progress, configuration);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception (AddSingleETC): " + ex.Message);
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
                Console.WriteLine("error updating EAC:" + ex.Message);

            }
        }

        public async Task<IActionResult> OnPostAddNewETCAsync([FromBody] DiscETCDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //var avgRate = 1.0m; // Placeholder, you may want to calculate it
                var avgRate = dto.ETCRate;
                var etcCost = avgRate * dto.ETCHrs;
                var eachrs = dto.ETCHrs; // Simplified, adjust if needed
                var eacCost = etcCost;   // Simplified need a variable for billwithadmindisc (cumul spend)

                using var conn = new SqlConnection(configuration.GetConnectionString("DBCS"));
                await conn.OpenAsync();

                using var insertCmd = new SqlCommand(@"
            INSERT INTO tbDiscETC (
                OBID, RptWeekend, ETCHrs, ETCCost, EACHrs, EACCost,
                ETCComment, PlanStartWE, PlanFinishWE, EmpID, EmpGroupID, JobID, CurveID,ETCRate,ETCRateTypeID)
            VALUES (
                @OBID, @RptWeekend, @ETCHrs, @ETCCost, @EACHrs, @EACCost,
                @ETCComment, @PlanStartWE, @PlanFinishWE, @EmpID, @EmpGroupID, @JobID, @CurveID,@ETCRate,@ETCRateTypeID)", conn);

                insertCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                insertCmd.Parameters.AddWithValue("@RptWeekend", dto.RptWeekend);
                insertCmd.Parameters.AddWithValue("@ETCHrs", dto.ETCHrs);
                insertCmd.Parameters.AddWithValue("@ETCCost", etcCost);
                insertCmd.Parameters.AddWithValue("@EACHrs", eachrs);
                insertCmd.Parameters.AddWithValue("@EACCost", eacCost);
                insertCmd.Parameters.AddWithValue("@ETCComment", (object?)dto.ETCComment ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@PlanStartWE", dto.PlanStartWE);
                insertCmd.Parameters.AddWithValue("@PlanFinishWE", dto.PlanFinishWE);
                insertCmd.Parameters.AddWithValue("@EmpID", (object?)dto.EmpID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@EmpGroupID", dto.EmpGroupID);
                insertCmd.Parameters.AddWithValue("@JobID", dto.JobID);
                insertCmd.Parameters.AddWithValue("@CurveID", dto.CurveID);
                insertCmd.Parameters.AddWithValue("@ETCRate", dto.ETCRate);
                insertCmd.Parameters.AddWithValue("@ETCRateTypeID", dto.ETCRateTypeID);

                await insertCmd.ExecuteNonQueryAsync();

                // ? update the latest tbProjectProgress record

                DAL dal = new DAL();

                // Load all ETCs for this job (same logic as in your page model)
                var allEtcs = dal.GetDiscETC(dto.JobID, configuration);

                decimal totalEAC = allEtcs.Sum(e => e.EACCost);
                DateTime? latestFinish = allEtcs.Max(e => e.PlanFinishWE);

                // Get or create the progress record for the current week
                var progress = dal.GetOrCreateProgressStatusByJobID(dto.JobID, configuration);

                // Update and save
                progress.EAC_Info = totalEAC;
                progress.FcastFinishDate = latestFinish ?? progress.FcastFinishDate;
                dal.UpdateProgressStatus(progress, configuration);

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert error: " + ex.Message);
                return StatusCode(500, new { message = "Error adding ETC record", detail = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteETCAsync([FromBody] int discEtcId)
        {
            var connString = configuration.GetConnectionString("DBCS");

            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand("DELETE FROM tbDiscETC WHERE DiscEtcID = @DiscEtcID", conn);

            cmd.Parameters.AddWithValue("@DiscEtcID", discEtcId);

            try
            {
                await conn.OpenAsync();
                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No matching ETC found." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostShowSpreadAsync(int jobId)
        {
            DAL dal = new DAL();

            var etcs = dal.GetAllDiscETC(configuration).Where(e => e.JobID == jobId).ToList();
            var curves = dal.GetCurveSections(configuration);
            var actuals = dal.GetLast4lWklyActuals(configuration).Where(a => a.JobID == jobId).ToList();

            JobSpreadWeeks = SpreadUtility.GetSpread(etcs, curves, actuals);

            // Preserve JobSelectList if needed
            var filteredJobs = dal.GetAllOpenProj(configuration)
                                  .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1)
                                  .OrderBy(j => j.ClientJob)
                                  .ToList();
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", jobId);

            return Page();
        }

        public async Task<IActionResult> OnPostBulkScheduleAsync([FromBody] BulkScheduleDto dto)
        {
            try
            {
                if (dto.DiscEtcIDs == null || dto.DiscEtcIDs.Count == 0)
                    return new JsonResult(new { success = false, message = "No records selected." });

                List<string> setClauses = new();
                List<SqlParameter> parameters = new();

                if (dto.PlanStartWE.HasValue)
                {
                    setClauses.Add("PlanStartWE = @PlanStartWE");
                    parameters.Add(new SqlParameter("@PlanStartWE", dto.PlanStartWE.Value));
                }

                if (dto.PlanFinishWE.HasValue)
                {
                    setClauses.Add("PlanFinishWE = @PlanFinishWE");
                    parameters.Add(new SqlParameter("@PlanFinishWE", dto.PlanFinishWE.Value));
                }

                if (dto.CurveID.HasValue)
                {
                    setClauses.Add("CurveID = @CurveID");
                    parameters.Add(new SqlParameter("@CurveID", dto.CurveID.Value));
                }

                if (!setClauses.Any())
                    return new JsonResult(new { success = false, message = "No schedule fields provided." });

                // Generate parameterized IN clause
                var idParams = dto.DiscEtcIDs.Select((id, index) => {
                    var param = new SqlParameter($"@id{index}", System.Data.SqlDbType.Int) { Value = int.Parse(id) };
                    parameters.Add(param);
                    return param.ParameterName;
                });

                string sql = $@"
            UPDATE tbDiscETC
            SET {string.Join(", ", setClauses)}
            WHERE DiscEtcID IN ({string.Join(",", idParams)});
        ";

                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await conn.OpenAsync();
                    int rows = await cmd.ExecuteNonQueryAsync();

                    return new JsonResult(new { success = true, message = $"{rows} ETC record(s) updated." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> OnPostShiftDatesByWeeksAsync([FromBody] ShiftRequest req)
        {
            try
            {
                if (req.Weeks == 0)
                    return new JsonResult(new { success = false, message = "Weeks must be non-zero" });

                // Calculate shift in days
                int daysToShift = req.Weeks * 7;

                string sql = @"
                UPDATE tbDiscETC
                    SET PlanStartWE = DATEADD(DAY, @Days, PlanStartWE),
                    PlanFinishWE = DATEADD(DAY, @Days, PlanFinishWE)
                    WHERE JobID = @JobID AND RptWeekend = @RptWeekend;
                ";

                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Days", daysToShift);
                    cmd.Parameters.AddWithValue("@JobID", req.JobID);
                    cmd.Parameters.AddWithValue("@RptWeekend", req.RptWeekend);

                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return new JsonResult(new { success = true, message = $"{rowsAffected} rows updated" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class JobRequest
        {
            public int JobId { get; set; }
        }

        public async Task<IActionResult> OnPostEditFormAsync(List<DiscETCDto> items,DiscETCDto singleItem)
        {
            if(singleItem!=null)
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

        public async Task<IActionResult> OnPostUpdateThisJobFcastAsync(int JobId)
        {
            if (JobId == null)
            {
                // Maybe show a message to select a job first
                return Page();
            }

            using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
            using (var cmd = new SqlCommand("spMakeNewWeekDiscETC_SimpleR3_byJob", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@myJobid", JobId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // Optional: reload data after procedure runs
            return RedirectToPage("/ETC/Index1", new { jobId = JobId });
        }


    }
}
