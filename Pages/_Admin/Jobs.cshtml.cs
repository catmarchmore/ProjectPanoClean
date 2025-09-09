using ProjectPano.Model;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages._Admin
{
    [IgnoreAntiforgeryToken]

    public class JobsModel : PageModel
    {
        private readonly DAL dal;
        private readonly IConfiguration configuration;

        [BindProperty]
        public JobFormDto JobForm { get; set; }
        public List<tbJob> JobList { get; set; }  //this was using vwJob
        public SelectList ClientList { get; set; }
        public SelectList MgrList { get; set; }
        public SelectList RateSheetList { get; set; }
        public SelectList CorpList { get; set; }
        public SelectList IndustrySectorList { get; set; }
        public SelectList SubClientList { get; set; }
        public SelectList ProjectTypeList { get; set; }
        public SelectList RegionList { get; set; }
        public SelectList StreamList { get; set; }
        public DateTime ThisWeekEnding { get; set; }

        public JobsModel(DAL dal,IConfiguration configuration)
        {
            this.dal = dal;
            this.configuration = configuration;
        }


        public void OnGet()
        {
            ThisWeekEnding = GetWE.GetWeekEnding(DateTime.Today);

            //var today = DateTime.Today;
            var jobs = dal.GetTbJob(configuration);

            var mgrs = dal.GetEmpList(configuration)
                .OrderBy(c => c.EmpName)
                .ToList();
            var mgrDict = mgrs.ToDictionary(m => m.EmpID, m => m.EmpName);

            //TodayDate = today;
            JobList = jobs
                .OrderByDescending(j => j.JobID)
                .Select(c => new tbJob
                {
                    JobID = c.JobID,
                    ClientID=c.ClientID,
                    //ClientNameShort = c.ClientNameShort,
                    BigTimeJobDisplayName = c.BigTimeJobDisplayName,
                    JobNum=c.JobNum,
                    JobStartDate = c.JobStartDate,
                    MgrID=c.MgrID,
                    MgrName = c.MgrID.HasValue && mgrDict.TryGetValue(c.MgrID.Value, out var empName)
                        ? empName
                        : null,
                    JobFinishDate = c.JobFinishDate,
                    BillingStatus = c.BillingStatus,
                    //RateSheetName = c.RateSheetName,
                    RateSheetID=c.RateSheetID,
                    AFE = c.AFE,
                    ClientPM = c.ClientPM,
                    CorpID = c.CorpID,
                    ResourceStatus = c.ResourceStatus,
                    OTPctJob=c.OTPctJob
                    //ClientJob=c.ClientJob,
                    //CURRCOST = c.CURRCOST
                })
                .ToList();

            //clientlist
            var clients = dal.GetClientList(configuration)
                .OrderBy(c => c.ClientName)
                .ToList();
            ClientList = new SelectList(clients, "ClientID", "ClientName");

            //mgrlist
            var mgrList = dal.GetEmpList(configuration)
                .OrderBy(c => c.EmpName)
                .ToList();
            MgrList = new SelectList(mgrList, "EmpID", "EmpName");

            //ratesheets
            var rateSheets = dal.GetRateSheetList(configuration)
                .OrderBy(c => c.RateSheetName)
                .ToList();
            RateSheetList = new SelectList(rateSheets, "RateSheetID", "RateSheetName");

            //corplist
            var corps = dal.GetCorpList(configuration)
                .OrderBy(c => c.CorpName)
                .ToList();
            CorpList = new SelectList(corps, "CorpID", "CorpName");

            //ind sect list
            var indSect = dal.GetIndSectorList(configuration)
                .OrderBy(c => c.IndustrySector)
                .ToList();
            IndustrySectorList = new SelectList(indSect, "IndustrySectorID", "IndustrySector");

            //region list
            var region = dal.GetRegionList(configuration)
                .OrderBy(c => c.RegionDesc)
                .ToList();
            RegionList = new SelectList(region, "RegionID", "RegionDesc");

            //stream list
            var stream = dal.GetStreamList(configuration)
                .OrderBy(c => c.StreamDesc)
                .ToList();
            StreamList = new SelectList(stream, "StreamID", "StreamDesc");

            //project type list
            var projType = dal.GetProjTypeList(configuration)
                .OrderBy(c => c.ProjectTypeDesc)
                .ToList();
            ProjectTypeList = new SelectList(projType, "ProjectTypeID", "ProjectTypeDesc");

            //subclient list
            var subClient = dal.GetSubClientList(configuration)
                .OrderBy(c => c.SubClientName)
                .ToList();
            SubClientList = new SelectList(subClient, "SubClientID", "SubClientName");


        }

        public async Task<IActionResult> OnPostAsync()
        {
            ThisWeekEnding = GetWE.GetWeekEnding(DateTime.Today);

            if (!ModelState.IsValid) return Page();

            JobFormDto savedJob;

            if (JobForm.JobID == 0)
            {
                savedJob = dal.InsertJob(JobForm, configuration);
                var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var finish = new DateTime(2030, 12, 31);
                var currWE = ThisWeekEnding;
                var ThisJobFinishDate = savedJob.JobFinishDate.HasValue
                    ? GetWE.GetWeekEnding(savedJob.JobFinishDate.Value)
                    : currWE; // or some default

                //add default tbOB records    
                dal.InsertDefaultOBID(savedJob.JobID,configuration);

                //add tbAdminFeeRecord
                var amt = dal.GetAdminFeeAmtForRateSheet(savedJob.RateSheetID,configuration);
                dal.InsertAdminFee(savedJob.JobID, start, finish, amt,configuration);

                //add default tbNavajoNationFee record
                dal.InsertNavNat(savedJob.JobID, start, finish,configuration);

                //add default tbOTAllowed record
                dal.InsertOT(savedJob.JobID, start, finish,configuration);

                //add default tbdiscount record    
                dal.InsertDiscount(savedJob.JobID, start, finish,configuration);

                //add default tbProjectProgress record
                dal.InsertProjProg(savedJob.JobID, currWE, ThisJobFinishDate,configuration);

                //dal.InsertJob(JobForm, configuration);
            }
            else
            {
                dal.EditJob(JobForm, configuration);
                savedJob = JobForm;
            }
            return RedirectToPage(); // or return Page() to reload
        }


        public JsonResult OnGetLoadJob(int id)
        {
            var job = dal.GetJobById(id,configuration); 
            return new JsonResult(job);
        }

    }
}
