using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ProjectPano.Controllers
{
    [Route("Deliverables")]
    public class DeliverablesController : Controller
    {
        private readonly IConfiguration _configuration;

        public DeliverablesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Deliverables/GetDeliverable?deliverableId=123
        [HttpGet("GetDeliverable")]
        public IActionResult GetDeliverable(int deliverableId)
        {
            if (deliverableId <= 0)
                return BadRequest("Invalid ID");

            DAL dal = new DAL();
            var deliverable = dal.GetDeliverableById(deliverableId, _configuration);
            if (deliverable == null)
                return NotFound();

            return Json(deliverable);
        }

        // POST: /Deliverables/UpdateDeliverable
        [HttpPost("UpdateDeliverable")]
        public IActionResult UpdateDeliverable([FromForm] tbDeliverable deliverable)
        {
            if (deliverable == null || deliverable.DeliverableID <= 0)
                return BadRequest("Invalid data");

            DAL dal = new DAL();
            dal.UpdateDeliverable(deliverable, _configuration);

            return Ok();
        }
    }
}
