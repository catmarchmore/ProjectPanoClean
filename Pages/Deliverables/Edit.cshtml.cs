using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ProjectPano.Pages.Deliverables
{
    public class EditModel : PageModel
    {
        private readonly IConfiguration configuration;

        public EditModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [BindProperty]
        public tbDeliverable? Deliverable { get; set; }
        public string? StatusMessage { get; set; }

        public IActionResult OnGet(int deliverableId)
        {
            if (deliverableId <= 0)
            {
                return RedirectToPage("/Deliverables/Index"); // fallback if invalid
            }

            DAL dal = new DAL();
            Deliverable = dal.GetDeliverableById(deliverableId, configuration);

            if (Deliverable == null || Deliverable.DeliverableID == 0)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // redisplay form with validation errors
            }

            DAL dal = new DAL();
            int rowsAffected = dal.UpdateDeliverable(Deliverable, configuration);

            if (rowsAffected > 0)
            {
                StatusMessage = "Deliverable updated successfully.";
                return RedirectToPage("/Deliverables/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Update failed. Please try again.");
                return Page();
            }

        }
    }
}
