using ProjectPano.Model;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using static ProjectPano.Pages.Stats.Bubbles1Model;

namespace ProjectPano.Pages.Stats
{
    public class TreeMapModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public List<int> Years { get; set; } = new();

        private readonly IConfiguration configuration;
        public TreeMapModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string TreemapJson { get; set; } = "{}";

        public class TmNode
        {
            public string name { get; set; } = "";
            public decimal? value { get; set; } // value in thousands (k)
            public List<TmNode> children { get; set; } = new();
        }

        public void OnGet()
        {
            using var con = new SqlConnection(configuration.GetConnectionString("DBCS"));
            con.Open();

            // 1) Years for dropdown (most recent first)
            using (var yearCmd = new SqlCommand(@"
                SELECT DISTINCT myJobStartYear
                FROM vwBubblesMapApp
                WHERE myJobStartYear IS NOT NULL
                ORDER BY myJobStartYear DESC;", con))
            using (var rdr = yearCmd.ExecuteReader())
            {
                while (rdr.Read()) Years.Add(rdr.GetInt32(0));
            }

            if (Years.Count == 0)
            {
                TreemapJson = JsonConvert.SerializeObject(new { name = "Business", children = new List<TmNode>() });
                return;
            }

            // If no year provided or it's not in the dataset, pick the latest in the data
            if (SelectedYear == 0 || !Years.Contains(SelectedYear))
                SelectedYear = Years.First();

            // 2) Query treemap data per Stream + ProjectType for the selected year
            // IMPORTANT: If vwBubblesMapApp has multiple rows per Job (causing double count),
            // switch to the "dedupe by JobID" version below.
            var sql = @"
                WITH Mapped AS (
	                SELECT 
		                CASE 
			                WHEN StreamDesc = 'Commercial' THEN 'Specialty Chemicals/Commercial'
			                WHEN StreamDesc = 'Specialty Chemicals' THEN 'Specialty Chemicals/Commercial'
			                ELSE StreamDesc
		                END AS StreamDesc,
		                ProjectTypeDesc,
		                myJobStartYear,
		                JobTotalValue
	                FROM vwBubblesMapApp
	                WHERE StreamDesc IS NOT NULL
	                  AND ProjectTypeDesc IS NOT NULL
	                  AND myJobStartYear = @year
                )
                SELECT StreamDesc, ProjectTypeDesc,
	                   SUM(JobTotalValue) / 1000.0 AS RevenueK
                FROM Mapped
                GROUP BY StreamDesc, ProjectTypeDesc
                ORDER BY StreamDesc, ProjectTypeDesc;";

            // ----- If you suspect duplicate rows per job, use this safer alternative -----
            // var sql = @"
            //     WITH Mapped AS (
            //         SELECT 
            //             JobID,
            //             CASE 
            //                 WHEN StreamDesc = 'Commercial' THEN 'Specialty Chemicals/Commercial'
            //                 WHEN StreamDesc = 'Specialty Chemicals' THEN 'Specialty Chemicals/Commercial'
            //                 ELSE StreamDesc
            //             END AS StreamDesc,
            //             ProjectTypeDesc,
            //             myJobStartYear,
            //             MAX(JobTotalValue) AS JobTotalValue
            //         FROM vwBubblesMapApp
            //         WHERE StreamDesc IS NOT NULL
            //           AND ProjectTypeDesc IS NOT NULL
            //           AND myJobStartYear = @year
            //         GROUP BY JobID, 
            //                  CASE 
            //                     WHEN StreamDesc = 'Commercial' THEN 'Specialty Chemicals/Commercial'
            //                     WHEN StreamDesc = 'Specialty Chemicals' THEN 'Specialty Chemicals/Commercial'
            //                     ELSE StreamDesc
            //                  END,
            //                  ProjectTypeDesc,
            //                  myJobStartYear
            //     )
            //     SELECT StreamDesc, ProjectTypeDesc,
            //            SUM(JobTotalValue) / 1000.0 AS RevenueK
            //     FROM Mapped
            //     GROUP BY StreamDesc, ProjectTypeDesc
            //     ORDER BY StreamDesc, ProjectTypeDesc;";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@year", SelectedYear);

            var streams = new Dictionary<string, TmNode>(StringComparer.OrdinalIgnoreCase);

            using (var rdr2 = cmd.ExecuteReader())
            {
                while (rdr2.Read())
                {
                    var stream = rdr2["StreamDesc"].ToString() ?? "";
                    var ptype = rdr2["ProjectTypeDesc"].ToString() ?? "";
                    var revK = Convert.ToDecimal(rdr2["RevenueK"]);

                    if (!streams.TryGetValue(stream, out var sNode))
                    {
                        sNode = new TmNode { name = stream };
                        streams[stream] = sNode;
                    }

                    sNode.children.Add(new TmNode
                    {
                        name = ptype,
                        value = revK // store thousands for display (…k)
                    });
                }
            }

            // Ensure standard 4 streams exist (even if empty), so layout is stable
            var allStreams = new[] { "Upstream", "Midstream", "Downstream", "Specialty Chemicals/Commercial" };
            var root = new TmNode { name = "Business" };
            foreach (var s in allStreams)
            {
                if (streams.TryGetValue(s, out var existing))
                    root.children.Add(existing);
                else
                    root.children.Add(new TmNode { name = s, children = new List<TmNode>() });
            }

            TreemapJson = JsonConvert.SerializeObject(root);
        }
    }
}
