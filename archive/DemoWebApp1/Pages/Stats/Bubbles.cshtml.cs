using ProjectPano.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text.Json;


namespace ProjectPano.Pages.Stats
{
    public class BubblesModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public string BubbleDataJson { get; set; }
        public List<int> Years { get; set; } = new();

        private readonly IConfiguration configuration;
        public BubblesModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public void OnGet()
        {
            var bubbles = new List<Bubbles>();
            using (var con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                con.Open();
                var cmd = new SqlCommand(@"
select StreamDesc,ProjectTypeDesc,myJobStartYear,round(sum(JobTotalValue)/1000,0) as Revenue
from vwBubblesMapApp
where StreamDesc is not null and ProjectTypeDesc is not null
group by StreamDesc,ProjectTypeDesc,myJobStartYear;
                    ", con);

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        bubbles.Add(new Bubbles
                        {
                            Stream = rdr["StreamDesc"].ToString(),
                            ProjectType = rdr["ProjectTypeDesc"].ToString(),
                            Revenue = (decimal)rdr["Revenue"],
                            myYear = (int)rdr["myJobStartYear"]
                        });
                    }
                }
            }
            Years = bubbles.Select(b => b.myYear).Distinct().OrderByDescending(y => y).ToList();

            //var filtered=bubbles.Where(b=>b.myYear==SelectedYear).ToList();
            var filtered = bubbles
                .Where(b => b.myYear == SelectedYear)
                .OrderByDescending(b => b.myYear)  // or Stream order
                .ThenByDescending(b => b.Revenue)
                .ToList();

            BubbleDataJson = JsonConvert.SerializeObject(filtered);

        }
    }
}
