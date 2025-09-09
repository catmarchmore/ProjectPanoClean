using ProjectPano.Model;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Linq;


namespace ProjectPano.Pages.Stats
{
    public class Bubbles1Model : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public List<int> Years { get; set; } = new();
        public string CircleDataJson { get; set; }

        private readonly IConfiguration configuration;
        public Bubbles1Model(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Parent level (Stream)
        public class StreamNode
        {
            public string name { get; set; } = "";
            public decimal value { get; set; }
            public List<ProjectTypeNode> children { get; set; } = new List<ProjectTypeNode>();
        }

        // Child level (ProjectType under each Stream)
        public class ProjectTypeNode
        {
            public string name { get; set; } = "";
            public decimal value { get; set; }
            public List<ProjectNode> children { get; set; } = new List<ProjectNode>();
        }

        // Grandchild (actual project or just year-based grouping)
        public class ProjectNode
        {
            public string name { get; set; } = "";
            public decimal value { get; set; }
        }
        public List<StreamNode> ChartData { get; set; } = new();


        public void OnGet()
        {
            using var con = new SqlConnection(configuration.GetConnectionString("DBCS"));
            con.Open();

            // 1. Get all distinct years for dropdown
            var yearCmd = new SqlCommand(@"
        SELECT DISTINCT myJobStartYear
        FROM vwBubblesMapApp
        WHERE myJobStartYear IS NOT NULL
        ORDER BY myJobStartYear DESC", con);

            using (var yearRdr = yearCmd.ExecuteReader())
            {
                Years = new List<int>();
                while (yearRdr.Read())
                {
                    Years.Add(yearRdr.GetInt32(0));
                }
            }

            // Default to most recent if not chosen
            if (SelectedYear == 0 && Years.Count > 0)
                SelectedYear = Years.First();

            // 2. Now get the filtered data for the selected year
            var cmd = new SqlCommand(@"
    with group1 as (
        SELECT case 
            when StreamDesc = 'Commercial' then 'Specialty Chemicals/Commercial'
            when StreamDesc = 'Specialty Chemicals' then 'Specialty Chemicals/Commercial'
            else StreamDesc end as StreamDesc,
            ProjectTypeDesc, myJobStartYear, 
            ROUND(SUM(JobTotalValue)/1000,0) AS Revenue
        FROM vwBubblesMapApp
        WHERE StreamDesc IS NOT NULL 
          AND ProjectTypeDesc IS NOT NULL
          AND myJobStartYear = @year
        GROUP BY StreamDesc, ProjectTypeDesc, myJobStartYear
    )
    select StreamDesc, ProjectTypeDesc, myJobStartYear, sum(Revenue) as Revenue
    from group1
    group by StreamDesc, ProjectTypeDesc, myJobStartYear;
", con);

            cmd.Parameters.AddWithValue("@year", SelectedYear);

            var rdr = cmd.ExecuteReader();

            var streams = new Dictionary<string, StreamNode>();

            while (rdr.Read())
            {
                string stream = rdr["StreamDesc"].ToString()!;
                string projectType = rdr["ProjectTypeDesc"].ToString()!;
                string year = rdr["myJobStartYear"].ToString()!;
                decimal revenue = rdr.GetDecimal(rdr.GetOrdinal("Revenue"));

                if (!streams.ContainsKey(stream))
                {
                    streams[stream] = new StreamNode { name = stream };
                }

                var streamNode = streams[stream];

                var projectTypeNode = streamNode.children
                    .FirstOrDefault(pt => pt.name == projectType);

                if (projectTypeNode == null)
                {
                    projectTypeNode = new ProjectTypeNode { name = projectType };
                    streamNode.children.Add(projectTypeNode);
                }

                projectTypeNode.children.Add(new ProjectNode
                {
                    name = year,
                    value = revenue
                });

                // accumulate totals
                //projectTypeNode.value += revenue;
                //streamNode.value += revenue;
            }

            // Populate Years list for dropdown
            //Years = streams.Values
            //    .SelectMany(s => s.children)               // ProjectTypeNodes
            //    .SelectMany(pt => pt.children)             // ProjectNodes
            //    .Select(p => int.TryParse(p.name, out int y) ? y : 0)
            //    .Where(y => y != 0)
            //    .Distinct()
            //    .OrderByDescending(y => y)
            //    .ToList();

            // Default to current year if none selected
            if (SelectedYear == 0 && Years.Count > 0)
                SelectedYear = Years.First();

            // Filter project nodes for the selected year
            foreach (var streamNode in streams.Values)
            {
                foreach (var ptNode in streamNode.children)
                {
                    ptNode.children = ptNode.children
                        .Where(p => int.TryParse(p.name, out int y) && y == SelectedYear)
                        .ToList();

                    // Recompute project type totals
                    ptNode.value = ptNode.children.Sum(p => p.value);
                }

                // Recompute stream totals
                streamNode.value = streamNode.children.Sum(pt => pt.value);
            }

            // Ensure all four streams exist even if no data
            var allStreams = new[] { "Upstream", "Downstream", "Midstream", "Specialty Chemicals/Commercial" };
            foreach (var s in allStreams)
            {
                if (!streams.ContainsKey(s))
                {
                    streams[s] = new StreamNode
                    {
                        name = s,
                        value = 0,
                        children = new List<ProjectTypeNode>()
                    };
                }
            }

            ChartData = streams.Values.ToList();

            // Serialize for D3
            CircleDataJson = JsonConvert.SerializeObject(new { name = "Business", children = ChartData });
        }
    }


}
