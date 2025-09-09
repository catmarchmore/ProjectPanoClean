//using ProjectPano.Model;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using System.Collections.Generic;
//using static ProjectPano.Model.DAL;

//namespace ProjectPano.Pages.Shared.Partials
//{
//    [IgnoreAntiforgeryToken]
//    public class AddETCHrsModel : PageModel
//    {
//        private readonly IConfiguration configuration;
//        private readonly DAL dal;

//        public AddETCHrsModel(IConfiguration configuration)
//        {
//            this.configuration = configuration;
//            this.dal = new DAL();
//        }

//        public class AddETCPopupFormModel
//        {
//            public List<vwCurves> ListCurves { get; set; } = new();
//            public List<vwDiscETC> FilteredDiscETC { get; set; }
//            public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
//            public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();
//            public DateTime RptWE { get; set; }

//        }

//        [BindProperty]
//        public AddETCPopupFormModel PopupFormModel { get; set; } = new();

//        public void OnGet(int jobId, string empResGroupDesc)
//        {
//            DateTime rptWE = ReportWE.GetReportWE();
//            PopupFormModel.RptWE=rptWE;
//            PopupFormModel.FilteredDiscETC = dal.GetDiscETC_ByJobAndGroup(jobId, empResGroupDesc, configuration);
//            PopupFormModel.ListCurves = dal.GetCurves(configuration);
//            PopupFormModel.ListResourceGroups = dal.GetResourceGroupSummaries(configuration);
//            PopupFormModel.vwBudgetActuals = dal.GetVWBudgetActuals(jobId, configuration);
//        }
//    }
//}
