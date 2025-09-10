using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectPano.Model;
using System.Data;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.Deliverables
{
    [IgnoreAntiforgeryToken]
    public class UploadModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        public SelectList JobSelectList { get; set; }
        public DateTime CurrWeekEnding { get; set; } = DateTime.Today;
        public UploadModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }

        [BindProperty(SupportsGet = true)]
        public int? JobId { get; set; }
        public bool HasDeliverables { get; set; } = false;
        public string DeliverableMessage { get; set; } = "";

        public async Task OnGetAsync(int? jobId)
        {
            var today = DateTime.Today;
            CurrWeekEnding = GetWE.GetWeekEnding(today);

            // Always populate the base list
            OpenProj = dal.GetAllOpenProj(configuration);
            var filteredJobs = OpenProj
                .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1)
                .OrderBy(j => j.ClientJob)
                .ToList();

            // If the query parameter was supplied use it (also works when model-binding sets JobId)
            if (jobId.HasValue)
                JobId = jobId.Value;

            // Build the SelectList — passing JobId (nullable) will pre-select if present
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", JobId);

            // Debug output (will appear in console/out window)
            //Console.WriteLine($"OnGet: param jobId={jobId}, bound JobId={JobId}, filteredJobs.Count={filteredJobs.Count}");

            // If we have a job, check deliverables
            if (JobId.HasValue)
            {
                using var con = new SqlConnection(configuration.GetConnectionString("DBCS"));
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM tbDeliverableHist WHERE JobID = @JobID", con);
                con.Open();
                cmd.Parameters.AddWithValue("@JobID", JobId.Value);
                int count = (int)cmd.ExecuteScalar();
                HasDeliverables = count > 0;
                if (HasDeliverables)
                    DeliverableMessage = "?? Deliverable records already exist for this job. Excel upload is disabled.";
            }

            // If we arrived via redirect with a flash message, show it
            if (TempData.ContainsKey("DeliverableMessage"))
            {
                DeliverableMessage = TempData["DeliverableMessage"]?.ToString();
            }
        }



        public async Task<IActionResult> OnPostUploadExcelAsync(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                DeliverableMessage = "? No file selected.";
                return Page();
            }

            var deliverables = new List<tbDeliverableHist>();
            var today = DateTime.Today;
            var jobId = JobId.GetValueOrDefault();
            CurrWeekEnding = GetWE.GetWeekEnding(today);

            int skippedCount = 0;

            // --- Parse Excel (same as you have) ---
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);               // or .Worksheet("Deliverables") if you want to require a name
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // skip header

                    foreach (var row in rows)
                    {
                        string myTask = row.Cell(5).GetString()?.Trim().ToUpper();
                        //Console.WriteLine($"Row {row.RowNumber()}: myTask='{myTask}' (length={myTask?.Length})");
                        //Console.WriteLine($"Row {row.RowNumber()}: myTask='{myTask}'");

                        var obidResult = dal.LookupOBID(jobId, myTask, configuration);
                        if (obidResult == null)
                        {
                            // collect/skipped if you want - here we just skip
                            skippedCount++;
                            continue;
                        }

                        decimal obhrs = obidResult.Value.OB_HRS;
                        decimal obcost = obidResult.Value.OB_COST;
                        decimal delRate = obhrs != 0 ? obcost / obhrs : 0;

                        deliverables.Add(new tbDeliverableHist
                        {
                            JobID = jobId,
                            OBID = obidResult.Value.OBID,
                            DelGp1 = row.Cell(1).GetString(),
                            DelGp2 = row.Cell(2).GetString(),
                            DelGp3 = row.Cell(3).GetString(),
                            DelGp4 = row.Cell(4).GetString(),
                            DelName = row.Cell(6).GetString(),
                            DelComment = row.Cell(7).GetString(),
                            DelHours = row.Cell(8).GetValue<decimal>(),
                            DelCost = delRate * row.Cell(8).GetValue<decimal>(),
                            Direct = row.Cell(9).GetValue<bool>(),
                            Created = DateTime.Now,
                            Modified = DateTime.Now,
                            DelPctCumul = 0,
                            DelEarnedHrs = 0,
                            DelEarnedCost = 0,
                            ProgressDate = CurrWeekEnding,
                            DirPct = 0,
                            DelNum = null,
                            PlanStartDate = null,
                            PlanFinishDate = null,
                            ActFinishDate = null,
                            FcastFinishDate = null,
                            DelRev = null
                        });
                    }
                }
            }

            // --- Insert with ADO.NET, count inserted rows ---
            int insertedCount = 0;

            using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await conn.OpenAsync();

                foreach (var d in deliverables)
                {
                    using var cmd = new SqlCommand(@"
                INSERT INTO tbDeliverableHist
                (JobID, OBID, DelGp1, DelGp2, DelGp3, DelGp4, DelName, DelComment,
                 DelHours, DelCost, Direct, Created, Modified,
                 DelPctCumul, DelEarnedHrs, DelEarnedCost, ProgressDate,
                 DirPct, DelNum, PlanStartDate, PlanFinishDate, ActFinishDate, FcastFinishDate, DelRev)
                VALUES
                (@JobID, @OBID, @DelGp1, @DelGp2, @DelGp3, @DelGp4, @DelName, @DelComment,
                 @DelHours, @DelCost, @Direct, @Created, @Modified,
                 @DelPctCumul, @DelEarnedHrs, @DelEarnedCost, @ProgressDate,
                 @DirPct, @DelNum, @PlanStartDate, @PlanFinishDate, @ActFinishDate, @FcastFinishDate, @DelRev);
            ", conn);

                    // explicit param typing for decimals/dates to avoid AddWithValue pitfalls
                    cmd.Parameters.AddWithValue("@JobID", d.JobID);
                    cmd.Parameters.AddWithValue("@OBID", d.OBID);
                    cmd.Parameters.AddWithValue("@DelGp1", (object?)d.DelGp1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp2", (object?)d.DelGp2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp3", (object?)d.DelGp3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp4", (object?)d.DelGp4 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelName", (object?)d.DelName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelComment", (object?)d.DelComment ?? DBNull.Value);

                    cmd.Parameters.Add("@DelHours", SqlDbType.Decimal).Value = d.DelHours;
                    cmd.Parameters.Add("@DelCost", SqlDbType.Decimal).Value = d.DelCost;

                    cmd.Parameters.AddWithValue("@Direct", d.Direct);
                    cmd.Parameters.Add("@Created", SqlDbType.DateTime).Value = (object?)d.Created ?? DBNull.Value;
                    cmd.Parameters.Add("@Modified", SqlDbType.DateTime).Value = (object?)d.Modified ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@DelPctCumul", d.DelPctCumul);
                    cmd.Parameters.AddWithValue("@DelEarnedHrs", d.DelEarnedHrs);
                    cmd.Parameters.AddWithValue("@DelEarnedCost", d.DelEarnedCost);

                    cmd.Parameters.Add("@ProgressDate", SqlDbType.Date).Value = (object?)d.ProgressDate ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@DirPct", d.DirPct);
                    cmd.Parameters.AddWithValue("@DelNum", (object?)d.DelNum ?? DBNull.Value);

                    cmd.Parameters.Add("@PlanStartDate", SqlDbType.Date).Value = (object?)d.PlanStartDate ?? DBNull.Value;
                    cmd.Parameters.Add("@PlanFinishDate", SqlDbType.Date).Value = (object?)d.PlanFinishDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ActFinishDate", SqlDbType.Date).Value = (object?)d.ActFinishDate ?? DBNull.Value;
                    cmd.Parameters.Add("@FcastFinishDate", SqlDbType.Date).Value = (object?)d.FcastFinishDate ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@DelRev", (object?)d.DelRev ?? DBNull.Value);

                    try
                    {
                        insertedCount += await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        // stamp the error in TempData and redirect back so OnGet can show it
                        //TempData["DeliverableMessage"] = $"? Insert failed: {ex.Message}";
                        //Console.WriteLine($"Insert failed: {ex}"); // server-side debug
                        TempData["DeliverableMessage"] = $"Read {deliverables.Count} deliverables from Excel. Skipped {skippedCount}.";
                        return RedirectToPage("/Deliverables/Upload", new { jobId = jobId });
                    }
                }
            }

            // Use TempData so message survives redirect
            //TempData["DeliverableMessage"] = $"? {insertedCount} deliverable record(s) inserted successfully.";
            TempData["DeliverableMessage"] = $"Read {deliverables.Count} deliverables from Excel. Skipped {skippedCount}.";

            //Console.WriteLine($"OnPost: insertedCount={insertedCount}, redirecting with jobId={jobId}");

            return RedirectToPage("/Deliverables/Upload", new { jobId = jobId });
        }


    }
}
