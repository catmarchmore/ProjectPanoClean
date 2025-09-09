using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.Deliverables
{

    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        public DateTime? MaxDateStamp { get; set; }
        private readonly IConfiguration configuration;

        public vwJob thisVWJob { get; set; } = new vwJob();
        public List<vwDeliverable> delList { get; set; } = new List<vwDeliverable>();
        public List<vwCumulSpendforWeekly_Brian> ListJobStatus { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();

        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void OnGet(int? jobId)
        {
            DAL dal = new DAL();
            MaxDateStamp = maxMyDateStamp.GetMaxDateStamp(configuration);
            ListJobStatus = dal.GetJobStatus(configuration);

            if (jobId.HasValue)
            {
                var jobList = dal.GetThisWJob(jobId.Value, configuration);
                if (jobList.Count > 0)
                    thisVWJob = jobList[0];

                vwBudgetActuals = dal.GetVWBudgetActuals(jobId.Value, configuration);
                delList = dal.GetDelList(jobId.Value, configuration);
            }
        }

        public async Task<JsonResult> OnGetGetDeliverableAsync([FromQuery] int deliverableId)
        {
            Console.WriteLine("HANDLER HIT: deliverableId = " + deliverableId);

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await con.OpenAsync();

                var query = "SELECT * FROM tbDeliverable WHERE DeliverableID = @DeliverableID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DeliverableID", deliverableId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                DeliverableID = reader["DeliverableID"],
                                OBID = reader["OBID"],
                                JobID = reader["JobID"],
                                DelNum = reader["DelNum"] == DBNull.Value ? null : reader["DelNum"].ToString(),
                                DelName = reader["DelName"] == DBNull.Value ? null : reader["DelName"].ToString(),
                                DelHours = reader["DelHours"],
                                DelCost = reader["DelCost"],
                                DelComment = reader["DelComment"] == DBNull.Value ? null : reader["DelComment"].ToString(),
                                PlanStartDate = reader["PlanStartDate"] == DBNull.Value ? null : ((DateTime)reader["PlanStartDate"]).ToString("yyyy-MM-dd"),
                                PlanFinishDate = reader["PlanFinishDate"] == DBNull.Value ? null : ((DateTime)reader["PlanFinishDate"]).ToString("yyyy-MM-dd"),
                                DelRev = reader["DelRev"] == DBNull.Value ? null : reader["DelRev"].ToString(),
                                DelGp1 = reader["DelGp1"] == DBNull.Value ? null : reader["DelGp1"].ToString(),
                                DelGp2 = reader["DelGp2"] == DBNull.Value ? null : reader["DelGp2"].ToString(),
                                DelGp3 = reader["DelGp3"] == DBNull.Value ? null : reader["DelGp3"].ToString(),
                                DelGp4 = reader["DelGp4"] == DBNull.Value ? null : reader["DelGp4"].ToString()
                            };

                            return new JsonResult(result);
                        }
                    }
                }
            }

            return new JsonResult(new { error = "Deliverable not found." });
        }

        public IActionResult OnPostUpdateDeliverable([FromBody] DeliverableUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    Console.WriteLine($"Key: {kvp.Key}");
                    foreach (var err in kvp.Value.Errors)
                    {
                        Console.WriteLine($"  Error: {err.ErrorMessage}");
                    }
                }

                return BadRequest(new { message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors) });
            }


            try
            {
                Console.WriteLine($"Received DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");

                using (var conn = new SqlConnection(configuration.GetConnectionString("DBCS")))
                {
                    conn.Open();

                    using (var cmd = new SqlCommand(@"
                UPDATE tbDeliverable SET
                    OBID = @OBID,
                    JobID = @JobID,
                    DelNum = @DelNum,
                    DelName = @DelName,
                    DelHours = @DelHours,
                    DelCost = @DelCost,
                    DelComment = @DelComment,
                    PlanStartDate = @PlanStartDate,
                    PlanFinishDate = @PlanFinishDate,
                    DelRev = @DelRev,
                    DelGp1 = @DelGp1,
                    DelGp2 = @DelGp2,
                    DelGp3 = @DelGp3,
                    DelGp4 = @DelGp4
                WHERE DeliverableID = @DeliverableID", conn))
                    {
                        cmd.Parameters.AddWithValue("@DeliverableID", dto.DeliverableID);
                        cmd.Parameters.AddWithValue("@OBID", dto.OBID);
                        cmd.Parameters.AddWithValue("@JobID", dto.JobID);
                        cmd.Parameters.AddWithValue("@DelNum", (object?)dto.DelNum ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelName", (object?)dto.DelName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelHours", dto.DelHours);
                        cmd.Parameters.AddWithValue("@DelCost", dto.DelCost);
                        cmd.Parameters.AddWithValue("@DelComment", (object?)dto.DelComment ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@PlanStartDate",
                            dto.PlanStartDate.HasValue ? (object)dto.PlanStartDate.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@PlanFinishDate",
                            dto.PlanFinishDate.HasValue ? (object)dto.PlanFinishDate.Value : DBNull.Value);


                        cmd.Parameters.AddWithValue("@DelRev", (object?)dto.DelRev ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelGp1", (object?)dto.DelGp1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelGp2", (object?)dto.DelGp2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelGp3", (object?)dto.DelGp3 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DelGp4", (object?)dto.DelGp4 ?? DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new JsonResult(new { success = true });
                        }
                        else
                        {
                            return NotFound(new { message = "Deliverable not found or not updated." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during update: " + ex.Message);
                return StatusCode(500, new { message = "Server error", details = ex.Message });
            }
        }

    }
}
