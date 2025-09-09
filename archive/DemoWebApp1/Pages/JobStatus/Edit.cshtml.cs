using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjectPano.Pages.JobStatus
{
    public class EditModel : PageModel
    {
        public tbProjectProgress ProgressStatus = new tbProjectProgress();
        public string successMessage = string.Empty;
        public string errorMessage = string.Empty;

        private readonly IConfiguration configuration;
        public vwCumulSpendforWeekly_Brian JobFinanceInfo { get;set; } = new vwCumulSpendforWeekly_Brian();
        
        public EditModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void OnGet(int ID)
        {
            //string id = Request.Query.FirstOrDefault(kvp => kvp.Key.Equals("ID", StringComparison.OrdinalIgnoreCase)).Value;

            try
            {
                DAL dal = new DAL();
                ProgressStatus = dal.GetOrCreateProgressStatus(ID.ToString(), configuration);
                JobFinanceInfo = dal.GetJobStatusByID(ProgressStatus.ProjectProgID, configuration);
                //JobFinanceInfo = dal.GetJobStatusByID(ID,configuration);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }
        

        public void OnPost()
        {
            ProgressStatus.ProjectProgID = int.Parse(Request.Form["ProjectProgID"]);

            //cumul progress
            if (decimal.TryParse(Request.Form["CumulPeriodProgress"], out decimal percentInput))
            {
                // Convert from whole percent to decimal format for storage
                ProgressStatus.CumulPeriodProgress = percentInput / 100m;
            }
            else
            {
                // Optionally handle parse error (e.g., invalid number)
            }




            // Parse the posted date
            if (DateTime.TryParse(Request.Form["FcastFinishDate"], out DateTime selectedDate))
            {
                // Calculate Saturday of the selected week
                int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)selectedDate.DayOfWeek + 7) % 7;
                DateTime saturdayDate = selectedDate.AddDays(daysUntilSaturday);

                ProgressStatus.FcastFinishDate = saturdayDate;
            }
            else
            {
                // Handle invalid date input
            }



            ProgressStatus.Comment = Request.Form["Comment"];
            ProgressStatus.EAC_Info = decimal.Parse(Request.Form["EAC_Info"]);

            {
                DAL dal = new DAL();
                int i = dal.UpdateProgressStatus(ProgressStatus, configuration);
            }

            successMessage = "Job Status has been updated.";
            Response.Redirect("/JobIndex/Index");
        }
    }

}
