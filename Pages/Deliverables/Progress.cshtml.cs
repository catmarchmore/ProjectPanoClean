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

        public void OnGet(int? JobId)
        {
            var today = DateTime.Today;
            CurrWeekEnding = GetWE.GetWeekEnding(today);

            // Always populate the base list
            OpenProj = dal.GetAllOpenProj(configuration);
            var filteredJobs = OpenProj
                .Where(j => j.BillingStatus == "In Process" && j.CorpID == 1)
                .OrderBy(j => j.ClientJob)
                .ToList();
            JobSelectList = new SelectList(filteredJobs, "JobID", "ClientJob", JobId);

            if (JobId.HasValue)
            {
                var dates = dal.GetProgressDatesByJob(JobId.Value, configuration);
                ProgressDateSelectList = new SelectList(dates, ProgressDate);

                if (ProgressDate.HasValue)
                {
                    Deliverables = dal.GetDeliverablesByJobAndDate(JobId.Value, ProgressDate.Value, configuration);
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

            if (addNew.HasValue && NewDeliverable != null)
            {
                // fill in required context fields
                NewDeliverable.JobID = jobId.Value;
                NewDeliverable.ProgressDate = ProgressDate;

                dal.InsertDeliverableHist(NewDeliverable, configuration);
                return RedirectToPage(new { JobId, ProgressDate });
            }

            if (Deliverables != null)
            {
                foreach (var d in Deliverables)
                {
                    dal.UpdateDeliverableHist(d, configuration);
                }
            }

            return RedirectToPage(new { JobId, ProgressDate });
        }

    }
}
