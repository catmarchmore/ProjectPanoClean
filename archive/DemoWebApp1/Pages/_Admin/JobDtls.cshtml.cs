using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace ProjectPano.Pages._Admin
{
    [IgnoreAntiforgeryToken]
    public class JobDtlsModel : PageModel
    {
        private readonly DAL dal;
        private readonly IConfiguration configuration;

        public JobDtlsModel(DAL dal, IConfiguration configuration)
        {
            this.dal = dal;
            this.configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public int JobId { get; set; }

        [BindProperty]
        public List<tbAdminFee> AdminFees { get; set; } = new();

        [BindProperty]
        public tbAdminFee NewFee { get; set; } = new();

        public void OnGet()
        {
            AdminFees = dal.GetAdminFee(JobId, configuration);
        }

        public IActionResult OnPostUpdate(int id)
        {
            var fee = AdminFees.FirstOrDefault(f => f.AdminFeeID == id);
            if (fee != null)
            {
                dal.UpdateAdminFee(fee, configuration);
            }

            return RedirectToPage("/_Admin/JobDtls", new { jobId = JobId });
        }

        public IActionResult OnPostDelete(int id)
        {
            dal.DeleteAdminFee(id, configuration);
            return RedirectToPage("/_Admin/JobDtls", new { jobId = JobId });
        }

        public IActionResult OnPostAdd()
        {
            if (NewFee != null &&
                NewFee.AdminFeeStart != null &&
                NewFee.AdminFeeFinish != null &&
                NewFee.AdminFeeAmt != 0)
            {
                dal.InsertAdminFee(
                    JobId,
                    NewFee.AdminFeeStart.Value,
                    NewFee.AdminFeeFinish.Value,
                    NewFee.AdminFeeAmt,
                    configuration
                );
            }

            return RedirectToPage("/_Admin/JobDtls", new { jobId = JobId });
        }
    }
}
