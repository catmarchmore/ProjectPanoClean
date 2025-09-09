using ProjectPano.Model;
using ProjectPano.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using static ProjectPano.Model.DAL;

namespace ProjectPano.Pages._Admin
{
    [IgnoreAntiforgeryToken]
    public class StoredProceduresModel : PageModel
    {
        private readonly IConfiguration configuration;
        private readonly GraphSharePointService _graphService;

        public DateTime? LastProgressWeekend { get; set; }

        public DateTime? LastDiscETCWkEndBacklog { get; set; }
        public DateTime? LastDiscETCWkEndOpps { get; set; }

        public StoredProceduresModel(IConfiguration configuration, GraphSharePointService graphService)
        {
            this.configuration = configuration;
            _graphService = graphService;
        }

        public List<ListItem> MyRegions { get; set; }

        public async Task OnGetAsync()
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SELECT MAX(Weekend) FROM tbProjectProgress", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {
                        LastProgressWeekend = (DateTime)result;
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    select max(tbDiscETC.rptweekend)
                    from tbDiscETC
                    join vwJob on vwJob.jobid=tbDiscETC.JobID
                    where vwJob.BillingStatus='in process' and vwJob.ResourceStatus='backlog'", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {
                        LastDiscETCWkEndBacklog = (DateTime)result;
                    }
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    select max(tbDiscETC.rptweekend)
                    from tbDiscETC
                    join vwJob on vwJob.jobid=tbDiscETC.JobID
                    where vwJob.BillingStatus='in process' and vwJob.ResourceStatus<>'backlog'", con))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {
                        LastDiscETCWkEndOpps = (DateTime)result;
                    }
                }
            }
        }


        public async Task<IActionResult> OnPostMakeNewDiscETCAsync()
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await con.OpenAsync();

                //// First stored procedure
                //using (SqlCommand cmd1 = new SqlCommand("spUpdateDiscETC_Complex", con))
                //{
                //    cmd1.CommandType = CommandType.StoredProcedure;
                //    await cmd1.ExecuteNonQueryAsync();
                //}

                // Second stored procedure
                using (SqlCommand cmd2 = new SqlCommand("spMakeNewWeekDiscETC_SimpleR3", con))
                {
                    cmd2.CommandType = CommandType.StoredProcedure;
                    await cmd2.ExecuteNonQueryAsync();
                }
            }

            TempData["SuccessMessage"] = "ETC Complex and Complex procedures successfully updated using [spMakeNewWeekDiscETC_SimpleR3].";

            return RedirectToPage(); // or RedirectToPage("Index1")
        }

        public async Task<IActionResult> OnPostMakeNewProgressWeekAsync()
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await con.OpenAsync();

                using (SqlCommand cmd2 = new SqlCommand("spMakeNewWeekProjectProgress", con))
                {
                    cmd2.CommandType = CommandType.StoredProcedure;
                    await cmd2.ExecuteNonQueryAsync();
                }
            }

            TempData["SuccessMessage"] = "New Progress week added using [spMakeNewWeekProjectProgress].";

            return RedirectToPage(); // or RedirectToPage("Index1")
        }

        public async Task<IActionResult> OnPostMakeNewDiscETCMappedAsync()
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                using (SqlCommand cmd = new SqlCommand("spMakeNewWeekDiscETC_REV1_mappedjobs", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync(); // No results expected
                }
            }

            TempData["SuccessMessage"] = "New Recordset successfully added.";

            return RedirectToPage(); // or RedirectToPage("Index1") if you're on a subpage
        }

        public async Task<IActionResult> OnPostMakeNewOppsETCAsync()
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                await con.OpenAsync();

                using (SqlCommand cmd2 = new SqlCommand("spCopyLastWeekOpportunitiesToCurrWk", con))
                {
                    cmd2.CommandType = CommandType.StoredProcedure;
                    await cmd2.ExecuteNonQueryAsync();
                }
            }

            TempData["SuccessMessage"] = "Opportunities from last week copied to this week using [spCopyLastWeekOpportunitiesToCurrWk].";

            return RedirectToPage(); // or RedirectToPage("Index1")
        }

    }
}
