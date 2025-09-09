using ProjectPano.Model;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.ETC
{
    [IgnoreAntiforgeryToken]
    public class PMUIModel : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
        //public string budgetActualsJson { get; set; }
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwEmpGroupResources> ListEmpGroupResources { get; set; } = new();
        public string allEmpDataJson { get; set; }
        public List<vwCurves> ListCurves { get; set; } = new();
        public List<vwDiscETC> thisVWDiscETC { get; set; } = new List<vwDiscETC>();


        public SelectList JobSelectList { get; set; }
        public List<object> JobLiteList { get; set; }
        public string SerializedJobLiteList { get; set; }

        public SelectList PMSelectList { get; set; }
        public string? SelectedPMName { get; set; }
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

        public PMUIModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }
        public async Task<IActionResult> OnGetAsync(int? jobId, string? mgrName)
        {
            SelectedPMName = mgrName;
            var jobs = dal.GetAllOpenProj(configuration);
            JobSelectList = new SelectList(jobs, "JobID", "ClientJob");
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListResourceGroups = dal.GetResourceGroupSummaries(configuration);
            ListEmpGroupResources = dal.GetResourceGroups(configuration);
            //allEmpDataJson = JsonSerializer.Serialize(ListEmpGroupResources);
            allEmpDataJson = JsonSerializer.Serialize(
                ListEmpGroupResources,
                new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            ListCurves = dal.GetCurves(configuration);

            OpenProj = dal.GetAllOpenProj(configuration);

            var filteredJobs = OpenProj
                    .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1 && j.ResourceStatus=="Backlog")
                    .OrderBy(j => j.ClientJob)
                    .ToList();
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", jobId);

            // Distinct list of PMs (MgrName)
            var uniquePMs = filteredJobs
                .Where(j => !string.IsNullOrEmpty(j.MgrName))
                .Select(j => j.MgrName)
                .Distinct()
                .OrderBy(pm => pm)
                .ToList();

            //PMSelectList = new SelectList(uniquePMs);
            PMSelectList = new SelectList(uniquePMs, SelectedPMName);

            JobLiteList = filteredJobs
                .Select(j => new {
                    JobID = j.JobID,
                    ClientJob = j.ClientJob,
                    MgrName = j.MgrName
                })
                .Cast<object>() // necessary if JobLiteList is List<object>
                .ToList();

            SerializedJobLiteList = JsonSerializer.Serialize(
                JobLiteList,
            new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });


            if (jobId == null)
            {
                // Nothing selected yet, don’t load spread/chart/table
                thisVWDiscETC = new List<vwDiscETC>();
                vwBudgetActuals = new List<vwBudgetActuals_REVISED>();

                return Page();
            }
            else
            {
                var jobList = dal.GetThisWJob(jobId.Value, configuration);

                Console.WriteLine(thisVWJob.JobID);
                if (jobList.Count > 0)
                    thisVWJob = jobList[0];

                //thisVWJob = dal.GetVWJob(jobId.Value, configuration);
                vwBudgetActuals = dal.GetVWBudgetActuals(jobId.Value, configuration) ?? new List<vwBudgetActuals_REVISED>();
                thisVWDiscETC = dal.GetDiscETC(jobId.Value, configuration) ?? new List<vwDiscETC>();

                return Page();
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
                    //decimal etchrs = dto.ETCHrs;
                    //decimal etcCost = dto.ETCCost ?? 0m; 

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

                    //decimal etchrs = dto.ETCHrs;
                    //decimal etcCost = dto.ETCCost ?? 0m;

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
                        checkExistingCmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        checkExistingCmd.Parameters.AddWithValue("@RptWeekend", currentWeek);

                        if (isUpdate)
                            checkExistingCmd.Parameters.AddWithValue("@DiscEtcID", dto.DiscEtcID);

                        int existingCount = (int)await checkExistingCmd.ExecuteScalarAsync();
                        isFirstETCThisWeek = (existingCount == 0);
                    }

                    decimal etcCost = dto.ETCCost ?? 0m;
                    decimal eachrs = dto.ETCHrs;
                    decimal eacCost = etcCost;

                    if (isFirstETCThisWeek)
                    {
                        eachrs += prevWkCumulHrs;
                        eacCost += prevWkCumulCost;
                    }

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
                }

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
                Console.WriteLine("Exception (AddSingleETC): " + ex.Message);
                return StatusCode(500, new { message = "Server error", detail = ex.Message });

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

    }
}
