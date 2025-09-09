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
    public class Map2Model : PageModel
    {
        private readonly IConfiguration configuration;
        public Map2Model(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string ChartDataJson { get; set; }

        public void OnGet()
        {
            var data = new List<JobRevenueByState>();
            using (var con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                con.Open();
                var cmd = new SqlCommand(@"
SELECT 
    v.MapState AS StateCode,
    COUNT(*) AS JobCount,
    ROUND(SUM(v.JobTotalValue)/1000,0) AS Revenue,
    STUFF((
        SELECT ',' + t.ClientNameShort
        FROM (
            SELECT TOP (5) v2.ClientNameShort, SUM(v2.JobTotalValue) AS ClientRevenue
            FROM vwBubblesMapApp v2
            WHERE v2.MapState = v.MapState
            GROUP BY v2.ClientNameShort
            ORDER BY SUM(v2.JobTotalValue) DESC
        ) t
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS Clients
FROM vwBubblesMapApp v
WHERE v.MapState IS NOT NULL
GROUP BY v.MapState;
                    ", con);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        data.Add(new JobRevenueByState
                        {
                            StateCode = rdr["StateCode"].ToString(),
                            JobCount = (int)rdr["JobCount"],
                            Revenue = (decimal)rdr["Revenue"],
                            Clients = rdr["Clients"].ToString()
                        });
                    }
                }
            }
            ChartDataJson = JsonConvert.SerializeObject(data);
        }
    }
}
