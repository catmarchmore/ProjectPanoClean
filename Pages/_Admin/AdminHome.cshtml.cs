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
    public class AdminHomeModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
