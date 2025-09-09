using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages.Shared.Partials
{
    [IgnoreAntiforgeryToken]
    public class EditETCHrsModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly DAL dal;

        public EditETCHrsModel(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.dal = new DAL();
        }

        public class ETCPopupFormModel 
        {
            public List<vwCurves> ListCurves { get; set; } = new();
            public List<vwDiscETC> FilteredDiscETC { get; set; }
            public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        }

        [BindProperty]
        public ETCPopupFormModel PopupFormModel { get; set; } = new();

        public void OnGet(int jobId,string empResGroupDesc)
        {
            PopupFormModel.FilteredDiscETC = dal.GetDiscETC_ByJobAndGroup(jobId, empResGroupDesc, configuration);
            PopupFormModel.ListCurves=dal.GetCurves(configuration);
            PopupFormModel.ListResourceGroups = dal.GetResourceGroupSummaries(configuration);
        }

    }
}
