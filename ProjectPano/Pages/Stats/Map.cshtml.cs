using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectPano.Model;
using static ProjectPano.Model.DAL;
using Newtonsoft.Json; // for JSON serialization
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace ProjectPano.Pages.Stats
{
    public class MapModel : PageModel
    {
        private readonly IConfiguration configuration;
        public MapModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string ChartDataJson { get; set; }

        public void OnGet()
        {
            var data = new List<JobRevenueByState>();
            using (var con=new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    select MapState as StateCode,count(*) as JobCount,sum(JobTotalValue) as Revenue
                        FROM vwBubblesMapApp
                        where mapstate is not null
                        group by MapState 
                    ", con);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        data.Add(new JobRevenueByState
                        {
                            StateCode = rdr["StateCode"].ToString(),
                            JobCount = (int)rdr["JobCount"],
                            Revenue = (decimal)rdr["Revenue"]
                        });
                    }
                }
            }
            ChartDataJson=JsonConvert.SerializeObject(data);
        }
    }
}
