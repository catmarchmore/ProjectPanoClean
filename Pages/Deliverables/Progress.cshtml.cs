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
    public class ProgressModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly DAL dal;
        public List<vwJob> OpenProj { get; set; } = new List<vwJob>();
        public SelectList JobSelectList { get; set; }
        public SelectList ProgressDateSelectList { get; set; }
        public DateTime CurrWeekEnding { get; set; } = DateTime.Today;
        public ProgressModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }

        [BindProperty(SupportsGet = true)]
        public int? JobId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ProgressDate { get; set; }

        [BindProperty]
        public List<vwDeliverableHist> Deliverables { get; set; } = new();

        [BindProperty]
        public tbDeliverableHist? NewDeliverable { get; set; }

        [BindProperty]
        public decimal DirPct { get; set; }

        [BindProperty]
        public List<int> SelectedFrom { get; set; } = new();



        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();

        public void OnGet()
        {
            var today = DateTime.Today;
            CurrWeekEnding = GetWE.GetWeekEnding(today);

            if (!ProgressDate.HasValue)
                ProgressDate = CurrWeekEnding;

            if (NewDeliverable == null)
                NewDeliverable = new tbDeliverableHist();

            NewDeliverable.JobID = JobId ?? 0; // make sure hidden input has value
            NewDeliverable.ProgressDate = ProgressDate.Value;

            OpenProj = dal.GetAllOpenProj(configuration);
            var filteredJobs = OpenProj
                .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1)
                .OrderBy(j => j.ClientJob)
                .ToList();
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", JobId);

            vwBudgetActuals = new List<vwBudgetActuals_REVISED>();

            if (JobId.HasValue)
            {
                NewDeliverable.JobID=JobId.Value;

                vwBudgetActuals = dal.GetVWBudgetActuals(JobId.Value, configuration);

                var dates = dal.GetProgressDatesByJob(JobId.Value, configuration)
                               .OrderByDescending(d => d)
                               .ToList();

                if (!ProgressDate.HasValue && dates.Any())
                    ProgressDate = dates.First();

                //ProgressDateSelectList = new SelectList(dates, ProgressDate);

                // Convert dates to yyyy-MM-dd strings for the dropdown
                ProgressDateSelectList = new SelectList(
                    dates.Select(d => new SelectListItem
                    {
                        Value = d.ToString("yyyy-MM-dd"),
                        Text = d.ToString("yyyy-MM-dd")
                    }),
                    "Value", "Text",
                    ProgressDate?.ToString("yyyy-MM-dd")  // set selected value
                );

                if (ProgressDate.HasValue)
                {
                    NewDeliverable.ProgressDate = ProgressDate.Value;
                    Deliverables = dal.GetDeliverablesByJobAndDate(
                        JobId.Value, ProgressDate.Value, configuration
                    );
                }
            }
        }


        public IActionResult OnPost(int? jobId, int? deleteId, int? editId, int? addNew)
        {
            if (deleteId.HasValue)
            {
                dal.DeleteDeliverableHist(deleteId.Value, configuration);
                return RedirectToPage(new { JobId, ProgressDate });
            }

            if (Deliverables != null)
            {
                foreach (var d in Deliverables)
                {
                    d.DirPct = DirPct;
                    d.ProgressDate = ProgressDate;
                    dal.UpdateDeliverableHist(d, configuration);
                }
            }

            //return RedirectToPage(new { JobId, ProgressDate });

            return RedirectToPage(new
            {
                //JobId = jobId,
                JobId,
                ProgressDate = ProgressDate?.ToString("yyyy-MM-dd")
            });

        }

        public async Task<IActionResult> OnPostAddNewAsync()
        {
            if (NewDeliverable != null)
            {
                //NewDeliverable.JobID = JobId ?? 0;
                //NewDeliverable.ProgressDate = ProgressDate ?? DateTime.Today;

                //delete this later 
                //Console.WriteLine($"Hidden JobID: {NewDeliverable.JobID}");
                //Console.WriteLine($"PageModel JobId: {JobId}");

                //Console.WriteLine($"JobID: {JobId}");
                //Console.WriteLine($"OBID: {NewDeliverable.OBID}");
                //Console.WriteLine($"DelName: {NewDeliverable.DelName}");
                //Console.WriteLine($"DelHours: {NewDeliverable.DelHours}");
                //Console.WriteLine($"DelCost: {NewDeliverable.DelCost}");
                //Console.WriteLine($"DelPctCumul: {NewDeliverable.DelPctCumul}");
                //Console.WriteLine($"ProgressDate: {NewDeliverable.ProgressDate}");
                //Console.WriteLine($"Direct: {NewDeliverable.Direct}");
                //Console.WriteLine($"DirPct: {NewDeliverable.DirPct}");
                //Console.WriteLine($"DelEarnedHrs: {NewDeliverable.DelEarnedHrs}");
                //Console.WriteLine($"DelEarnedCost: {NewDeliverable.DelEarnedCost}");

                // ? set defaults that could possibly be null
                if (NewDeliverable.Direct == null)
                    NewDeliverable.Direct = true;

                if (NewDeliverable.DelPctCumul == null)
                    NewDeliverable.DelPctCumul = 0;

                if (NewDeliverable.DelEarnedHrs == null)
                    NewDeliverable.DelEarnedHrs = 0;

                if (NewDeliverable.DelEarnedCost == null)
                    NewDeliverable.DelEarnedCost = 0;

                if (NewDeliverable.DirPct == null)
                    NewDeliverable.DirPct = 0;

                if (!NewDeliverable.ProgressDate.HasValue)
                    NewDeliverable.ProgressDate = ProgressDate; // fallback to page model, if needed

                NewDeliverable.Created = DateTime.Now;
                NewDeliverable.Modified = DateTime.Now;

                dal.InsertDeliverableHist(NewDeliverable, configuration);
            }

            //return RedirectToPage(new { JobId, ProgressDate });
            return RedirectToPage(new
            {
                JobId ,
                ProgressDate = ProgressDate?.ToString("yyyy-MM-dd")
            });
        }

        public async Task<IActionResult> OnPostCopyProgressAsync(DateTime? SelectedDateFrom, DateTime? SelectedDateTo)
        {
            Console.WriteLine($"CopyProgress called: From {SelectedDateFrom}, To {SelectedDateTo}");
            Console.WriteLine($"Looking for JobID={JobId}, Date={SelectedDateFrom.Value.Date}");

            if (!JobId.HasValue || !SelectedDateFrom.HasValue || !SelectedDateTo.HasValue)
                return RedirectToPage(new { JobId, ProgressDate });

            if (SelectedDateTo.Value.DayOfWeek != DayOfWeek.Saturday)
            {
                ModelState.AddModelError("", "Selected target date must be a Saturday.");
                return Page();
            }

            // --- Fetch records using only the date portion ---
            var existingDeliverables = dal.GetDeliverablesByJobAndDate(JobId.Value, SelectedDateFrom.Value.Date, configuration);

            if (existingDeliverables != null && existingDeliverables.Any())
            {
                var first = existingDeliverables.First();

                Console.WriteLine($"JobID: {JobId}");
                Console.WriteLine($"OBID: {first.OBID}");
                Console.WriteLine($"DelName: {first.DelName}");
                Console.WriteLine($"DelHours: {first.DelHours}");
                Console.WriteLine($"DelCost: {first.DelCost}");
                Console.WriteLine($"DelPctCumul: {first.DelPctCumul}");
                Console.WriteLine($"ProgressDate: {first.ProgressDate}");
                Console.WriteLine($"Direct: {first.Direct}");
                Console.WriteLine($"DirPct: {first.DirPct}");
                Console.WriteLine($"DelEarnedHrs: {first.DelEarnedHrs}");
                Console.WriteLine($"DelEarnedCost: {first.DelEarnedCost}");
            }

            if (existingDeliverables == null || !existingDeliverables.Any())
            {
                ModelState.AddModelError("", "No deliverables found for the selected date.");
                Console.WriteLine("existing deliverables is null");
                return Page();
            }

            foreach (var d in existingDeliverables)
            {
                var newDel = new tbDeliverableHist
                {
                    JobID = d.JobID,
                    OBID = d.OBID,
                    DelName = d.DelName,
                    DelHours = d.DelHours,
                    DelCost = d.DelCost,
                    DelPctCumul = d.DelPctCumul,
                    Direct = d.Direct,
                    DelComment = d.DelComment,
                    ProgressDate = SelectedDateTo.Value.Date, // force midnight
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    DirPct = d.DirPct,
                    DelEarnedHrs = d.DelEarnedHrs,
                    DelEarnedCost = d.DelEarnedCost
                };

                dal.InsertDeliverableHist(newDel, configuration);
            }

            //return RedirectToPage(new { JobId, ProgressDate = SelectedDateTo.Value.Date });
            return RedirectToPage(new
            {
                JobId,
                ProgressDate = SelectedDateTo?.ToString("yyyy-MM-dd")
            });

        }




    }
}
