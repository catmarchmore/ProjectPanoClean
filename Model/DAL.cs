using DocumentFormat.OpenXml.Wordprocessing;
using Humanizer;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Graph.Drives.Item.Items.Item.Workbook.Functions.Weekday;
using Microsoft.Graph.Models.ExternalConnectors;
using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;

namespace ProjectPano.Model
{
    public class DAL
    {
        public static class GetWE
        {
            public static DateTime GetWeekEnding(DateTime date)
            {
                int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)date.DayOfWeek + 7) % 7;
                return date.AddDays(daysUntilSaturday);
            }
        }

        public static class ReportWE
        {
            // Returns the date of the upcoming Saturday
            public static DateTime GetReportWE()
            {
                DateTime today = DateTime.Today;
                //int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
                //DateTime nextSaturday = today.AddDays(daysUntilSaturday);
                //return nextSaturday;

                var weekDay = (int)today.DayOfWeek; // Sunday = 0, Monday = 1, etc.

                DateTime weekEnding;

                if (weekDay == 0 || weekDay == 1) // Sunday or Monday
                {
                    // Last week's Saturday
                    weekEnding = today.AddDays(-(weekDay + 1));
                    //ForecastMessage = $"You are updating forecasts starting last week (week ending {weekEnding:MM/dd})";
                    return weekEnding;
                }
                else
                {
                    // This week's Saturday
                    weekEnding = today.AddDays(6 - weekDay);
                    //ForecastMessage = $"You are updating forecasts starting this week (week ending {weekEnding:MM/dd})";
                    return weekEnding;
                }

            }
        }

        public static class maxMyDateStamp
        {
            public static DateTime? GetMaxDateStamp(IConfiguration configuration)
            {
                DateTime? maxDate = null;

                using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
                {
                    string query = "SELECT MAX(myDateStamp) FROM tbLabExp";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != DBNull.Value && result != null)
                        {
                            maxDate = Convert.ToDateTime(result);
                        }
                    }
                }

                return maxDate;
            }
        }

        public List<Users> GetUsers(IConfiguration configuration)
        {
            List<Users> ListUsers = new List<Users>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from TbUsersToDelete", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Users user = new Users();
                        user.ID = Convert.ToString(dt.Rows[i]["ID"]);
                        user.FirstName = Convert.ToString(dt.Rows[i]["FirstName"]);
                        user.LastName = Convert.ToString(dt.Rows[i]["LastName"]);
                        ListUsers.Add(user);
                    }
                }
            }
            return ListUsers;
        }

        public List<vwCumulSpendforWeekly_Brian> GetJobStatus(IConfiguration configuration)
        {
            List<vwCumulSpendforWeekly_Brian> ListJobStatus = new List<vwCumulSpendforWeekly_Brian>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
                        {
                SqlDataAdapter da = new SqlDataAdapter("Select * from vwCumulSpendforWeekly_Brian", con);
                //SqlDataAdapter da = new SqlDataAdapter("Select v.jobid,v.mgrname,v.client,v.clientjob,v.Current_Budget," +
                //    "v.Current_Cumulative_Spend,v.PERCENT_SPENT,v.EACCost,v.FinishDate,v.comment,v.ProjectProgID,p.WeekEnd " +
                //    "from vwCumulSpendforWeekly_Brian join tbProjectProgress p on p.projectprogid=v.projectprogid", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        vwCumulSpendforWeekly_Brian JobStatus = new vwCumulSpendforWeekly_Brian();
                        JobStatus.jobid = Convert.ToString(dt.Rows[i]["jobid"]);
                        JobStatus.MgrName = Convert.ToString(dt.Rows[i]["MgrName"]);
                        JobStatus.Client = Convert.ToString(dt.Rows[i]["Client"]);
                        JobStatus.ClientJob = Convert.ToString(dt.Rows[i]["ClientJob"]);
                        JobStatus.OriginalBudget = Convert.ToDecimal(dt.Rows[i]["Original_Budget"]);
                        JobStatus.CurrentBudget = Convert.ToDecimal(dt.Rows[i]["Current_Budget"]);
                        JobStatus.CurrentCumulativeSpend = Convert.ToDecimal(dt.Rows[i]["Current_Cumulative_Spend"]);
                        JobStatus.PercentSpent = Convert.ToDecimal(dt.Rows[i]["PERCENT_SPENT"]);
                        JobStatus.PercentComplete = Convert.ToDecimal(dt.Rows[i]["PctComplete"]);
                        JobStatus.EACCost = dt.Rows[i]["EACCost"] as decimal? ?? 0m;
                        JobStatus.FinishDate = dt.Rows[i].IsNull("FinishDate")
                            ? DateTime.MinValue
                            : Convert.ToDateTime(dt.Rows[i]["FinishDate"]);
                        JobStatus.comment = Convert.ToString(dt.Rows[i]["comment"]);
                        JobStatus.ProjectProgID = dt.Rows[i]["ProjectProgID"] as int? ?? 0;
                        //JobStatus.WeekEnd = Convert.ToDateTime(dt.Rows[i]["WeekEnd"]);
                        ListJobStatus.Add(JobStatus);
                    }
                }
            }
            return ListJobStatus;
        }

        public List<vwNewJobs> GetNewJobs(IConfiguration configuration)
        {
            List<vwNewJobs> ListNewJobs = new List<vwNewJobs>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from vwNewJobs", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        vwNewJobs NewJobs = new vwNewJobs();
                        NewJobs.myYearMonth = Convert.ToString(dt.Rows[i]["myYearMonth"]);
                        NewJobs.JobID = Convert.ToInt32(dt.Rows[i]["JobID"]);
                        NewJobs.ClientName = Convert.ToString(dt.Rows[i]["ClientName"]);
                        NewJobs.JobName = Convert.ToString(dt.Rows[i]["JobName"]);
                        NewJobs.JobNum = Convert.ToString(dt.Rows[i]["JobNum"]);
                        NewJobs.Amount = Convert.ToDecimal(dt.Rows[i]["Amount"]);
                        NewJobs.Status = Convert.ToString(dt.Rows[i]["Status"]);
                        NewJobs.MgrName = Convert.ToString(dt.Rows[i]["MgrName"]);
                        NewJobs.AwardDate = Convert.ToDateTime(dt.Rows[i]["AwardDate"]);
                        NewJobs.ProjectTypeDesc = Convert.ToString(dt.Rows[i]["ProjectTypeDesc"]);
                        ListNewJobs.Add(NewJobs);
                    }
                }
            }
            return ListNewJobs;
        }

        public List<vwNewJobsByMonth> GetNewJobsByMonth(IConfiguration configuration)
        {
            List<vwNewJobsByMonth> ListNewJobsByMonth = new List<vwNewJobsByMonth>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from vwNewJobsByMonth where myYearMonth like '%2024%' or myYearMonth like '%2025%'", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        vwNewJobsByMonth NewJobsByMonth = new vwNewJobsByMonth();
                        NewJobsByMonth.myYearMonth = Convert.ToString(dt.Rows[i]["myYearMonth"]);
                        NewJobsByMonth.MonthlyOrigAmt = Convert.ToDecimal(dt.Rows[i]["MonthlyOrigAmt"]);
                        NewJobsByMonth.MonthlyChangeAmt = Convert.ToDecimal(dt.Rows[i]["MonthlyChangeAmt"]);
                        NewJobsByMonth.MonthlyTarget = Convert.ToDecimal(dt.Rows[i]["MonthlyTarget"]);
                        ListNewJobsByMonth.Add(NewJobsByMonth);
                    }
                }
            }
            return ListNewJobsByMonth;
        }

        public List<tbProjectProgress> GetProgressStatus(IConfiguration configuration)
        {
            List<tbProjectProgress> ListProgressStatus = new List<tbProjectProgress>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from tbProjectProgress", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        tbProjectProgress ProgressStatus = new tbProjectProgress();
                        //ProgressStatus.ProjectProgID = Convert.ToString(dt.Rows[i]["ProjectProgID"]);
                        ProgressStatus.ProjectProgID = Convert.ToInt32(dt.Rows[i]["ProjectProgID"]);
                        ProgressStatus.WeekEnd = Convert.ToDateTime(dt.Rows[i]["Weekend"]);
                        ProgressStatus.ProjectPeriodProgress = Convert.ToDecimal(dt.Rows[i]["ProjectPeriodProgress"]);
                        ProgressStatus.Status = Convert.ToString(dt.Rows[i]["Status"]);
                        ProgressStatus.JobID = Convert.ToInt32(dt.Rows[i]["JobID"]);
                        ProgressStatus.Comment = Convert.ToString(dt.Rows[i]["Comment"]);
                        ProgressStatus.FcastFinishDate = Convert.ToDateTime(dt.Rows[i]["FcastFinishDate"]);
                        ProgressStatus.ForecastHrs = Convert.ToDecimal(dt.Rows[i]["ForecastHrs"]);
                        ProgressStatus.CumulPeriodProgress = Convert.ToDecimal(dt.Rows[i]["CumulPeriodProgress"]);
                        ProgressStatus.Created = Convert.ToDateTime(dt.Rows[i]["Created"]);
                        ProgressStatus.Modified = Convert.ToDateTime(dt.Rows[i]["Modified"]);
                        ProgressStatus.EAC_Info = Convert.ToDecimal(dt.Rows[i]["EAC_Info"]);
                        ListProgressStatus.Add(ProgressStatus);
                    }
                }
            }
            return ListProgressStatus;
        }
        public List<vwCurves> GetCurveSections(IConfiguration configuration)
        {
            List<vwCurves> curveSections = new List<vwCurves>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                // No GROUP BY, get all rows (all sections)
                SqlDataAdapter da = new SqlDataAdapter("SELECT CurveID, CurveSectionNum, CurveSectionPct FROM vwCurves ORDER BY CurveID, CurveSectionNum", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwCurves curveSection = new vwCurves
                    {
                        CurveID = row["CurveID"] != DBNull.Value ? Convert.ToInt32(row["CurveID"]) : 0,
                        CurveSectionNum = row["CurveSectionNum"] != DBNull.Value ? Convert.ToInt32(row["CurveSectionNum"]) : 0,
                        CurveSectionPct = row["CurveSectionPct"] != DBNull.Value ? Convert.ToDecimal(row["CurveSectionPct"]) : 0m
                    };
                    curveSections.Add(curveSection);
                }
            }

            return curveSections;
        }

        public List<vwCurves> GetCurves(IConfiguration configuration)
        {
            List<vwCurves> ListCurves = new List<vwCurves>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select CurveID,CurveName from vwCurves group by CurveID, CurveName", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        vwCurves Curves = new vwCurves();
                        Curves.CurveID = Convert.ToInt32(dt.Rows[i]["CurveID"]);
                        Curves.CurveName = Convert.ToString(dt.Rows[i]["CurveName"]);

                        ListCurves.Add(Curves);
                    }
                }
            }
            return ListCurves;
        }

        List<DateTime> GetWeekEndingDates(DateTime start, DateTime end)
        {
            var dates = new List<DateTime>();
            var current = start;
            while (current <= end)
            {
                dates.Add(current);
                current = current.AddDays(7);
            }
            return dates;
        }

        public List<tblabExp> GetLabExp(IConfiguration configuration)
        {
            List<tblabExp> ListLabExp = new List<tblabExp>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from tbLabExp", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        tblabExp LabExp = new tblabExp();
                        //ProgressStatus.ProjectProgID = Convert.ToString(dt.Rows[i]["ProjectProgID"]);
                        LabExp.LabExpID = Convert.ToInt32(dt.Rows[i]["LabExpID"]);
                        LabExp.JobID = Convert.ToInt32(dt.Rows[i]["JobID"]);
                        LabExp.EmpID = Convert.ToInt32(dt.Rows[i]["EmpID"]);
                        LabExp.BigTimeJobDisplayName = Convert.ToString(dt.Rows[i]["BigTimeJobDisplayName"]);
                        LabExp.EmpName = Convert.ToString(dt.Rows[i]["EmpName"]);
                        LabExp.Category = Convert.ToString(dt.Rows[i]["Category"]);
                        LabExp.BillDate = Convert.ToDateTime(dt.Rows[i]["BillDate"]);
                        LabExp.Task = Convert.ToString(dt.Rows[i]["Task"]);
                        LabExp.Notes = Convert.ToString(dt.Rows[i]["Notes"]);
                        LabExp.InvNumber = Convert.ToString(dt.Rows[i]["InvNumber"]);
                        LabExp.WeekEnd = Convert.ToDateTime(dt.Rows[i]["Weekend"]);
                        LabExp.InputCost = Convert.ToDecimal(dt.Rows[i]["InputCost"]);
                        LabExp.BillableCost = Convert.ToDecimal(dt.Rows[i]["BillableCost"]);
                        LabExp.BillableQty = Convert.ToDecimal(dt.Rows[i]["BillableQty"]);
                        LabExp.BigTimeBillRate = Convert.ToDecimal(dt.Rows[i]["BigTimeBillRate"]);
                        LabExp.BillableWithAdmin = Convert.ToDecimal(dt.Rows[i]["BillableWithAdmin"]);
                        LabExp.AdminFee = Convert.ToDecimal(dt.Rows[i]["AdminFee"]);
                        LabExp.BillableWithAdminDiscount = Convert.ToDecimal(dt.Rows[i]["BillableWithAdminDiscount"]);
                        LabExp.DiscountAmt = Convert.ToDecimal(dt.Rows[i]["DiscountAmt"]);
                        LabExp.InputQty = Convert.ToDecimal(dt.Rows[i]["InputQty"]);
                        LabExp.LabExp = Convert.ToString(dt.Rows[i]["LabExp"]);
                        LabExp.NC = Convert.ToString(dt.Rows[i]["NC"]);
                        LabExp.myTask = Convert.ToString(dt.Rows[i]["myTask"]);
                        LabExp.myDateStamp = Convert.ToDateTime(dt.Rows[i]["myDateStamp"]);
                        LabExp.DataOrigin = Convert.ToString(dt.Rows[i]["DataOrigin"]); 
                        LabExp.OvertimeCheck = Convert.ToString(dt.Rows[i]["OvertimeCheck"]);
                        LabExp.WorkType = Convert.ToString(dt.Rows[i]["WorkType"]);
                        LabExp.TotalCost = Convert.ToDecimal(dt.Rows[i]["TotalCost"]);
                        LabExp.NavajoNationAmt = Convert.ToDecimal(dt.Rows[i]["NavajoNationAmt"]);
                        LabExp.BillablewithAdminNavajo = Convert.ToDecimal(dt.Rows[i]["BillablewithAdminNavajo"]);
                        LabExp.NewMexAmt = Convert.ToDecimal(dt.Rows[i]["NewMexAmt"]);
                        LabExp.BillablewithAdminNewMex = Convert.ToDecimal(dt.Rows[i]["BillablewithAdminNewMex"]);
                        LabExp.CorpID = Convert.ToInt32(dt.Rows[i]["CorpID"]);
                        LabExp.JobTeamRole = Convert.ToString(dt.Rows[i]["JobTeamRole"]);
                        LabExp.Created = Convert.ToDateTime(dt.Rows[i]["Created"]);
                        LabExp.Modified = Convert.ToDateTime(dt.Rows[i]["Modified"]);
                        LabExp.WorkTypeDavid = Convert.ToString(dt.Rows[i]["WorkTypeDavid"]);
                        ListLabExp.Add(LabExp);
                    }
                }
            }
            return ListLabExp;
        }

        public (int OBID, decimal OB_HRS, decimal OB_COST)? LookupOBID(int jobId, string myTask, IConfiguration configuration)
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"SELECT OBID, OB_HRS, OB_COST 
                         FROM vwBudgetActuals_REVISED 
                         WHERE JobID = @JobID AND MYTASK = @myTask";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@JobID", jobId);
                    cmd.Parameters.AddWithValue("@myTask", myTask);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // explicitly cast values to match tuple signature
                            int obid = reader["OBID"] != DBNull.Value ? Convert.ToInt32(reader["OBID"]) : 0;
                            decimal obhrs = reader["OB_HRS"] != DBNull.Value ? Convert.ToDecimal(reader["OB_HRS"]) : 0m;
                            decimal obcost = reader["OB_COST"] != DBNull.Value ? Convert.ToDecimal(reader["OB_COST"]) : 0m;

                            return (obid, obhrs, obcost);
                        }
                    }
                }
            }

            return null; // no row found
        }


        public List<vwBudgetActuals_REVISED> GetVWBudgetActuals(int jobId, IConfiguration configuration)
        {
            List<vwBudgetActuals_REVISED> ListVWBudgetActuals = new List<vwBudgetActuals_REVISED>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM vwBudgetActuals_REVISED WHERE JobID = @JobID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@JobID", jobId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        vwBudgetActuals_REVISED record = new vwBudgetActuals_REVISED
                        {
                            JobID = Convert.ToInt32(reader["JobID"]),
                            BigTimeJobDisplayName = reader["BigTimeJobDisplayName"].ToString(),
                            OBID = Convert.ToInt32(reader["OBID"]),
                            MYTASK = reader["MYTASK"].ToString(),
                            MAXBILLDATE = Convert.ToDateTime(reader["MAXBILLDATE"]),
                            OB_HRS = Convert.ToDecimal(reader["OB_HRS"]),
                            OB_COST = Convert.ToDecimal(reader["OB_COST"]),
                            ApprovedCNHRS = Convert.ToDecimal(reader["ApprovedCNHRS"]),
                            ApprovedCNCOST = Convert.ToDecimal(reader["ApprovedCNCOST"]),
                            UnApprovedCNHRS = Convert.ToDecimal(reader["UnApprovedCNHRS"]),
                            UnApprovedCNCOST = Convert.ToDecimal(reader["UnApprovedCNCOST"]),
                            CURRHRS = Convert.ToDecimal(reader["CURRHRS"]),
                            CURRCOST = Convert.ToDecimal(reader["CURRCOST"]),
                            BILLCOST = Convert.ToDecimal(reader["BILLCOST"]),
                            BILLQTY = Convert.ToDecimal(reader["BILLQTY"]),
                            BILLWITHADMIN = Convert.ToDecimal(reader["BILLWITHADMIN"]),
                            ADMIN_FEE = Convert.ToDecimal(reader["ADMIN_FEE"]),
                            BILLWITHADMINDISC = Convert.ToDecimal(reader["BILLWITHADMINDISC"]),
                            DISCOUNT_AMT = Convert.ToDecimal(reader["DISCOUNT_AMT"]),
                            PERCENT_SPENT = Convert.ToDecimal(reader["PERCENT_SPENT"]),
                            DiscCode = reader["DiscCode"].ToString(),
                            DiscGroup = reader["DiscGroup"].ToString(),
                            DiscGroupSort = Convert.ToInt32(reader["DiscGroupSort"]),
                            DiscSort = Convert.ToInt32(reader["DiscSort"]),
                            MgrName = reader["MgrName"].ToString(),
                            //ProgressDate = reader["ProgressDate"] == DBNull.Value ? null : (DateTime?)reader["ProgressDate"];
                            PctCompl = Convert.ToDecimal(reader["PctCompl"]),
                            EAC_Hrs = Convert.ToDecimal(reader["EAC_Hrs"]),
                            EAC_Cost = Convert.ToDecimal(reader["EAC_Cost"]),
                            BillingStatus = reader["BillingStatus"].ToString(),
                            JobLevel = reader["JobLevel"].ToString(),
                            //DiscDesc = reader["DiscDesc"].ToString()
                            ResourceStatus = reader["ResourceStatus"].ToString(),
                            DefaultEmpGroupID = Convert.ToInt32(reader["DefaultEmpGroupID"]),
                            CurrWkEnding = Convert.ToDateTime(reader["CurrWkEnding"]),
                            PrevWkEnding = Convert.ToDateTime(reader["PrevWkEnding"]),
                            PrevWkCumulHrs = Convert.ToDecimal(reader["PrevWkCumulHrs"]),
                            PrevWkCumulCost = Convert.ToDecimal(reader["PrevWkCumulCost"]),
                            CurrRate = Convert.ToDecimal(reader["CurrRate"]),
                            ActRate = Convert.ToDecimal(reader["ActRate"]),
                            ETC_Hrs = Convert.ToDecimal(reader["ETC_Hrs"]),
                            ETC_Cost = Convert.ToDecimal(reader["ETC_Cost"]),
                            CurrWkHrs = Convert.ToDecimal(reader["CurrWkHrs"]),
                            CurrWkCost = Convert.ToDecimal(reader["CurrWkCost"])
                        };

                        ListVWBudgetActuals.Add(record);
                    }
                }
            }

            return ListVWBudgetActuals;
        }

        public List<vwJobGroups> GetVWJobGroups(int jobGp1Id, IConfiguration configuration)
        {
            List<vwJobGroups> ListVWJobGps = new List<vwJobGroups>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM vwJobGroups WHERE JobGp1ID = @JobGp1Id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@JobGp1Id", jobGp1Id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        vwJobGroups record = new vwJobGroups
                        {
                            JobGp1ID = Convert.ToInt32(reader["JobGp1ID"]),
                            JobGp1Desc = reader["JobGp1Desc"].ToString(),
                            JobGp2ID = Convert.ToInt32(reader["JobGp2ID"]),
                            JobGp2Desc = reader["JobGp2Desc"].ToString(),
                            ClientJob = reader["ClientJob"].ToString(),
                            JobID = Convert.ToInt32(reader["JobID"]),
                            MYTASK = reader["MYTASK"].ToString(),
                            OBID = Convert.ToInt32(reader["OBID"]),
                            OB_HRS = Convert.ToDecimal(reader["OB_HRS"]),
                            OB_COST = Convert.ToDecimal(reader["OB_COST"]),
                            ApprovedCNHRS = Convert.ToDecimal(reader["ApprovedCNHRS"]),
                            ApprovedCNCOST = Convert.ToDecimal(reader["ApprovedCNCOST"]),
                            UnApprovedCNHRS = Convert.ToDecimal(reader["UnApprovedCNHRS"]),
                            UnApprovedCNCOST = Convert.ToDecimal(reader["UnApprovedCNCOST"]),
                            CURRHRS = Convert.ToDecimal(reader["CURRHRS"]),
                            CURRCOST = Convert.ToDecimal(reader["CURRCOST"]),
                            BILLQTY = Convert.ToDecimal(reader["BILLQTY"]),
                            BILLWITHADMINDISC = Convert.ToDecimal(reader["BILLWITHADMINDISC"]),
                            PrevWkCumulHrs = Convert.ToDecimal(reader["PrevWkCumulHrs"]),
                            PrevWkCumulCost = Convert.ToDecimal(reader["PrevWkCumulCost"]),
                            CurrWkHrs = Convert.ToDecimal(reader["CurrWkHrs"]),
                            CurrWkCost = Convert.ToDecimal(reader["CurrWkCost"]),
                            ETC_Hrs = Convert.ToDecimal(reader["ETC_Hrs"]),
                            ETC_Cost = Convert.ToDecimal(reader["ETC_Cost"]),
                            EAC_Hrs = Convert.ToDecimal(reader["EAC_Hrs"]),
                            EAC_Cost = Convert.ToDecimal(reader["EAC_Cost"]),
                            CorpID = Convert.ToInt32(reader["CorpID"]),
                            CorpDesc = reader["JobGp2Desc"].ToString(),
                            DiscGroupSort = Convert.ToInt32(reader["DiscGroupSort"]),
                            DiscSort = Convert.ToInt32(reader["DiscSort"]),
                            DiscGroup = reader["DiscGroup"].ToString()
                        };

                        ListVWJobGps.Add(record);
                    }
                }
            }

            return ListVWJobGps;
        }

        public List<vwJobGroups> GetDistinctJobGp1List(IConfiguration configuration)
        {
            List<vwJobGroups> list = new List<vwJobGroups>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT DISTINCT JobGp1ID, JobGp1Desc FROM vwJobGroups ORDER BY JobGp1Desc";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new vwJobGroups
                        {
                            JobGp1ID = Convert.ToInt32(reader["JobGp1ID"]),
                            JobGp1Desc = reader["JobGp1Desc"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public List<vwBudgetActuals_REVISED> GetVWBudgetActualsAllActiveJobs(IConfiguration configuration)
        {
            List<vwBudgetActuals_REVISED> ListVWBudgetActualsAllActive = new List<vwBudgetActuals_REVISED>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "select * from vwBudgetActuals_revised where billingstatus like '%process%'";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    //cmd.Parameters.AddWithValue("@JobID", jobId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        vwBudgetActuals_REVISED record = new vwBudgetActuals_REVISED
                        {
                            JobID = Convert.ToInt32(reader["JobID"]),
                            BigTimeJobDisplayName = reader["BigTimeJobDisplayName"].ToString(),
                            OBID = Convert.ToInt32(reader["OBID"]),
                            MYTASK = reader["MYTASK"].ToString(),
                            MAXBILLDATE = Convert.ToDateTime(reader["MAXBILLDATE"]),
                            OB_HRS = Convert.ToDecimal(reader["OB_HRS"]),
                            OB_COST = Convert.ToDecimal(reader["OB_COST"]),
                            ApprovedCNHRS = Convert.ToDecimal(reader["ApprovedCNHRS"]),
                            ApprovedCNCOST = Convert.ToDecimal(reader["ApprovedCNCOST"]),
                            UnApprovedCNHRS = Convert.ToDecimal(reader["UnApprovedCNHRS"]),
                            UnApprovedCNCOST = Convert.ToDecimal(reader["UnApprovedCNCOST"]),
                            CURRHRS = Convert.ToDecimal(reader["CURRHRS"]),
                            CURRCOST = Convert.ToDecimal(reader["CURRCOST"]),
                            BILLCOST = Convert.ToDecimal(reader["BILLCOST"]),
                            BILLQTY = Convert.ToDecimal(reader["BILLQTY"]),
                            BILLWITHADMIN = Convert.ToDecimal(reader["BILLWITHADMIN"]),
                            ADMIN_FEE = Convert.ToDecimal(reader["ADMIN_FEE"]),
                            BILLWITHADMINDISC = Convert.ToDecimal(reader["BILLWITHADMINDISC"]),
                            DISCOUNT_AMT = Convert.ToDecimal(reader["DISCOUNT_AMT"]),
                            PERCENT_SPENT = Convert.ToDecimal(reader["PERCENT_SPENT"]),
                            DiscCode = reader["DiscCode"].ToString(),
                            DiscGroup = reader["DiscGroup"].ToString(),
                            DiscGroupSort = Convert.ToInt32(reader["DiscGroupSort"]),
                            DiscSort = Convert.ToInt32(reader["DiscSort"]),
                            MgrName = reader["MgrName"].ToString(),
                            //ProgressDate = reader["ProgressDate"] == DBNull.Value ? null : (DateTime?)reader["ProgressDate"];
                            PctCompl = Convert.ToDecimal(reader["PctCompl"]),
                            EAC_Hrs = Convert.ToDecimal(reader["EAC_Hrs"]),
                            EAC_Cost = Convert.ToDecimal(reader["EAC_Cost"]),
                            BillingStatus = reader["BillingStatus"].ToString(),
                            JobLevel = reader["JobLevel"].ToString(),
                            //DiscDesc = reader["DiscDesc"].ToString()
                            ResourceStatus = reader["ResourceStatus"].ToString(),
                            DefaultEmpGroupID = Convert.ToInt32(reader["DefaultEmpGroupID"]),
                            CurrWkEnding = Convert.ToDateTime(reader["CurrWkEnding"]),
                            PrevWkEnding = Convert.ToDateTime(reader["PrevWkEnding"]),
                            PrevWkCumulHrs = Convert.ToDecimal(reader["PrevWkCumulHrs"]),
                            PrevWkCumulCost = Convert.ToDecimal(reader["PrevWkCumulCost"]),
                            CurrRate = Convert.ToDecimal(reader["CurrRate"]),
                            ActRate = Convert.ToDecimal(reader["ActRate"]),
                            ETC_Hrs = Convert.ToDecimal(reader["ETC_Hrs"]),
                            ETC_Cost = Convert.ToDecimal(reader["ETC_Cost"]),
                            CurrWkHrs = Convert.ToDecimal(reader["CurrWkHrs"]),
                            CurrWkCost = Convert.ToDecimal(reader["CurrWkCost"])
                        };

                        ListVWBudgetActualsAllActive.Add(record);
                    }
                }
            }

            return ListVWBudgetActualsAllActive;
        }

        public List<vwActuals_byProject_byWeek> GetLast4lWklyActuals(IConfiguration configuration)
        {
            List<vwActuals_byProject_byWeek> WklyActuals4 = new List<vwActuals_byProject_byWeek>();

            DateTime reportWE = ReportWE.GetReportWE();
            DateTime startDate = reportWE.AddDays(-28);  // last 4 weeks

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT WeekEnd, EmpGroupID, EmpResGroupDesc, JobID, SUM(BillQty) as BillQty, ClientNameShort, JobName
            FROM vwActuals_byProject_byWeek
            WHERE BillQty > 0
              AND Weekend >= @StartDate
              AND Weekend < @ReportWE   -- exclude the current forecast week
            GROUP BY Weekend, EmpGroupID, EmpResGroupDesc, JobID, ClientNameShort, JobName
        ", con);

                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@ReportWE", reportWE);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwActuals_byProject_byWeek item = new vwActuals_byProject_byWeek
                    {
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        BillQty = row["BillQty"] != DBNull.Value ? Convert.ToDecimal(row["BillQty"]) : 0,
                        WeekEnd = row["WeekEnd"] != DBNull.Value ? Convert.ToDateTime(row["WeekEnd"]) : DateTime.MinValue,
                        EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
                        EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                        JobName = row["JobName"]?.ToString(),
                    };

                    WklyActuals4.Add(item);
                }
            }

            Console.WriteLine($"Loaded {WklyActuals4.Count} records from vwActuals_byProject_byWeek.");
            return WklyActuals4;
        }


        //public List<vwActuals_byProject_byWeek> GetLast4lWklyActuals(IConfiguration configuration)
        //{
        //    List<vwActuals_byProject_byWeek> WklyActuals4 = new List<vwActuals_byProject_byWeek>();

        //    using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter(@"
        //            SELECT WeekEnd,EmpGroupID,EmpResGroupDesc,JobID,sum(BillQty) as BillQty,ClientNameShort,JobName
        //            FROM vwActuals_byProject_byWeek
        //            WHERE BillQty>0 AND
        //                weekend >= CAST(
        //                DATEADD(
        //                    DAY, 
        //                    (7 - DATEPART(WEEKDAY, GETDATE())) % 7, 
        //                    GETDATE() - 28
        //                ) AS DATE
        //            )
        //            AND weekend <> CAST(
        //                DATEADD(
        //                    DAY, 
        //                    (7 - DATEPART(WEEKDAY, GETDATE())) % 7, 
        //                    GETDATE()
        //                ) AS DATE
        //            )                     GROUP BY Weekend,EmpGroupID,EmpResGroupDesc,JobID,ClientNameShort,JobName
        //        ", con);

        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        foreach (DataRow row in dt.Rows)
        //        {
        //            vwActuals_byProject_byWeek item = new vwActuals_byProject_byWeek
        //            {
        //                JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
        //                BillQty = row["BillQty"] != DBNull.Value ? Convert.ToDecimal(row["BillQty"]) : 0,
        //                WeekEnd = row["WeekEnd"] != DBNull.Value ? Convert.ToDateTime(row["WeekEnd"]) : DateTime.MinValue,
        //                EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
        //                EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
        //                ClientNameShort = row["ClientNameShort"]?.ToString(),
        //                JobName = row["JobName"]?.ToString(),
        //            };

        //            WklyActuals4.Add(item);
        //        }
        //        Console.WriteLine($"Loaded {WklyActuals4.Count} records from vwActuals_byProject_byWeek.");

        //    }

        //    return WklyActuals4;
        //}

        public List<vwActuals_byProject_byWeek> GetLast4lWklyActualsClient(IConfiguration configuration)
        {
            List<vwActuals_byProject_byWeek> WklyActuals4Client = new List<vwActuals_byProject_byWeek>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT WeekEnd,StackedAreaClient,sum(BillQty) as BillQty,ClientNameShort,JobName,JobID,ClientNameShort,JobName
                    FROM vwActuals_byProject_byWeek
                    WHERE BillQty>0 AND
                        weekend >= CAST(
                        DATEADD(
                            DAY, 
                            (7 - DATEPART(WEEKDAY, GETDATE())) % 7, 
                            GETDATE() - 28
                        ) AS DATE
                    )
                    AND weekend <> CAST(
                        DATEADD(
                            DAY, 
                            (7 - DATEPART(WEEKDAY, GETDATE())) % 7, 
                            GETDATE()
                        ) AS DATE
                    )                     GROUP BY Weekend,StackedAreaClient,ClientNameShort,JobName,JobID,ClientNameShort,JobName
                ", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwActuals_byProject_byWeek item = new vwActuals_byProject_byWeek
                    {
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        BillQty = row["BillQty"] != DBNull.Value ? Convert.ToDecimal(row["BillQty"]) : 0,
                        WeekEnd = row["WeekEnd"] != DBNull.Value ? Convert.ToDateTime(row["WeekEnd"]) : DateTime.MinValue,
                        StackedAreaClient = row["StackedAreaClient"]?.ToString(),
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                        JobName = row["JobName"]?.ToString()
                    };

                    WklyActuals4Client.Add(item);
                }
                Console.WriteLine($"Loaded {WklyActuals4Client.Count} records from vwActuals_byProject_byWeek.");

            }

            return WklyActuals4Client;
        }

        public List<vwActuals_byProject_byWeek> GetWklyActualsbyJob(int jobId,IConfiguration configuration)
        {
            List<vwActuals_byProject_byWeek> WklyActualsJob = new List<vwActuals_byProject_byWeek>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT WeekEnd,EmpGroupID,EmpResGroupDesc,JobID,sum(BillQty) as BillQty,JobName,ClientNameShort
                    FROM vwActuals_byProject_byWeek
                    WHERE JobID = @JobID                    
                    GROUP BY WeekEnd,EmpGroupID,EmpResGroupDesc,JobID,JobName,ClientNameShort
                ", con);

                da.SelectCommand.CommandTimeout = 60;

                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwActuals_byProject_byWeek item = new vwActuals_byProject_byWeek
                    {
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        BillQty = row["BillQty"] != DBNull.Value ? Convert.ToDecimal(row["BillQty"]) : 0,
                        WeekEnd = row["WeekEnd"] != DBNull.Value ? Convert.ToDateTime(row["WeekEnd"]) : DateTime.MinValue,
                        EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
                        EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
                        JobName = row["JobName"]?.ToString(),
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                    };

                    WklyActualsJob.Add(item);
                }
                Console.WriteLine($"Loaded {WklyActualsJob.Count} records from vwActuals_byProject_byWeek.");

            }

            return WklyActualsJob;
        }

        public List<vwActuals_byProject_byWeek_nogroup> GetWklyActualsbyJobNoGroup(int jobId, IConfiguration configuration)
        {
            List<vwActuals_byProject_byWeek_nogroup> WklyActualsJobNoGroup = new List<vwActuals_byProject_byWeek_nogroup>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT *
                    FROM vwActuals_byProject_byWeek_nogroup
                    WHERE JobID = @JobID                    
                ", con);

                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwActuals_byProject_byWeek_nogroup item = new vwActuals_byProject_byWeek_nogroup
                    {
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        BigTimeJobDisplayName = row["BigTimeJobDisplayName"]?.ToString(),
                        BillQty = row["BillQty"] != DBNull.Value ? Convert.ToDecimal(row["BillQty"]) : 0,
                        BillwithAdminDisc = row["BillwithAdminDisc"] != DBNull.Value ? Convert.ToDecimal(row["BillwithAdminDisc"]) : 0,
                        WeekEnd = row["WeekEnd"] != DBNull.Value ? Convert.ToDateTime(row["WeekEnd"]) : DateTime.MinValue,
                        CURRENTCOST = row["CURRENTCOST"] != DBNull.Value ? Convert.ToDecimal(row["CURRENTCOST"]) : 0,
                        PeriodPctSpent = row["PeriodPctSpent"] != DBNull.Value ? Convert.ToDecimal(row["PeriodPctSpent"]) : 0,
                    };

                    WklyActualsJobNoGroup.Add(item);
                }
                Console.WriteLine($"Loaded {WklyActualsJobNoGroup.Count} records from vwActuals_byProject_byWeek_nogroup.");

            }

            return WklyActualsJobNoGroup;
        }

        public List<vwDiscETC> GetAllDiscETCLabor(IConfiguration configuration)
        {
            List<vwDiscETC> discETC = new List<vwDiscETC>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vwDiscETC where empresgroupdesc <>'expense'", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwDiscETC item = new vwDiscETC
                    {
                        DiscEtcID = row["DiscEtcID"] != DBNull.Value ? Convert.ToInt32(row["DiscEtcID"]) : 0,
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        OBID = row["OBID"] != DBNull.Value ? Convert.ToInt32(row["OBID"]) : 0,
                        myTask = row["myTask"]?.ToString(),
                        RptWeekend = row["RptWeekend"] != DBNull.Value ? Convert.ToDateTime(row["RptWeekend"]) : DateTime.MinValue,
                        ETCHrs = row["ETCHrs"] != DBNull.Value ? Convert.ToDecimal(row["ETCHrs"]) : 0,
                        ETCCost = row["ETCCost"] != DBNull.Value ? Convert.ToDecimal(row["ETCCost"]) : 0,
                        EACHrs = row["EACHrs"] != DBNull.Value ? Convert.ToDecimal(row["EACHrs"]) : 0,
                        EACCost = row["EACCost"] != DBNull.Value ? Convert.ToDecimal(row["EACCost"]) : 0,
                        ETCComment = row["ETCComment"]?.ToString(),
                        Created = row["Created"] != DBNull.Value ? Convert.ToDateTime(row["Created"]) : DateTime.MinValue,
                        Modified = row["Modified"] != DBNull.Value ? Convert.ToDateTime(row["Modified"]) : DateTime.MinValue,
                        PlanStartWE = row["PlanStartWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanStartWE"]) : DateTime.MinValue,
                        PlanFinishWE = row["PlanFinishWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanFinishWE"]) : DateTime.MinValue,
                        EmpID = row["EmpID"] != DBNull.Value ? Convert.ToInt32(row["EmpID"]) : 0,
                        EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
                        CurveID = row["CurveID"] != DBNull.Value ? Convert.ToInt32(row["CurveID"]) : 0,
                        CurveName = row["CurveName"]?.ToString(),
                        ResourceStatus = row["ResourceStatus"]?.ToString(),
                        EmpResGroupLead = row["EmpResGroupLead"]?.ToString(),
                        EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                        JobName = row["JobName"]?.ToString(),
                        StackedAreaClient = row["StackedAreaClient"]?.ToString(),
                        WklyBillable = row["WklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillable"]) : 0,
                        WklyBillableOH = row["WklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOH"]) : 0,
                        WklyBillableOHOT = row["WklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOHOT"]) : 0,
                        TotalWklyBillable = row["TotalWklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillable"]) : 0,
                        TotalWklyBillableOH = row["TotalWklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOH"]) : 0,
                        TotalWklyBillableOHOT = row["TotalWklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOHOT"]) : 0,
                        ETCRate = row["ETCRate"] != DBNull.Value ? Convert.ToDecimal(row["ETCRate"]) : 0,
                        ETCRateTypeDesc = row["ETCRateTypeDesc"]?.ToString(),
                        ETCRateTypeID = row["ETCRateTypeID"] != DBNull.Value ? Convert.ToInt32(row["ETCRateTypeID"]) : 0,
                        MapChk = row["MapChk"] != DBNull.Value ? Convert.ToInt32(row["MapChk"]) : 0,
                        Probability = row["Probability"] != DBNull.Value ? Convert.ToDecimal(row["Probability"]) : 0,
                    };

                    discETC.Add(item);
                }
                Console.WriteLine($"Loaded {discETC.Count} records from vwDiscETC.");

            }

            return discETC;
        }

        public List<vwDiscETC> GetAllDiscETC(IConfiguration configuration)
        {
            List<vwDiscETC> discETC = new List<vwDiscETC>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vwDiscETC", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwDiscETC item = new vwDiscETC
                    {
                        DiscEtcID = row["DiscEtcID"] != DBNull.Value ? Convert.ToInt32(row["DiscEtcID"]) : 0,
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        OBID = row["OBID"] != DBNull.Value ? Convert.ToInt32(row["OBID"]) : 0,
                        myTask = row["myTask"]?.ToString(),
                        RptWeekend = row["RptWeekend"] != DBNull.Value ? Convert.ToDateTime(row["RptWeekend"]) : DateTime.MinValue,
                        ETCHrs = row["ETCHrs"] != DBNull.Value ? Convert.ToDecimal(row["ETCHrs"]) : 0,
                        ETCCost = row["ETCCost"] != DBNull.Value ? Convert.ToDecimal(row["ETCCost"]) : 0,
                        EACHrs = row["EACHrs"] != DBNull.Value ? Convert.ToDecimal(row["EACHrs"]) : 0,
                        EACCost = row["EACCost"] != DBNull.Value ? Convert.ToDecimal(row["EACCost"]) : 0,
                        ETCComment = row["ETCComment"]?.ToString(),
                        Created = row["Created"] != DBNull.Value ? Convert.ToDateTime(row["Created"]) : DateTime.MinValue,
                        Modified = row["Modified"] != DBNull.Value ? Convert.ToDateTime(row["Modified"]) : DateTime.MinValue,
                        PlanStartWE = row["PlanStartWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanStartWE"]) : DateTime.MinValue,
                        PlanFinishWE = row["PlanFinishWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanFinishWE"]) : DateTime.MinValue,
                        EmpID = row["EmpID"] != DBNull.Value ? Convert.ToInt32(row["EmpID"]) : 0,
                        EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
                        CurveID = row["CurveID"] != DBNull.Value ? Convert.ToInt32(row["CurveID"]) : 0,
                        CurveName = row["CurveName"]?.ToString(),
                        ResourceStatus = row["ResourceStatus"]?.ToString(),
                        EmpResGroupLead = row["EmpResGroupLead"]?.ToString(),
                        EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                        JobName = row["JobName"]?.ToString(),
                        StackedAreaClient = row["StackedAreaClient"]?.ToString(),
                        WklyBillable = row["WklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillable"]) : 0,
                        WklyBillableOH = row["WklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOH"]) : 0,
                        WklyBillableOHOT = row["WklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOHOT"]) : 0,
                        TotalWklyBillable = row["TotalWklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillable"]) : 0,
                        TotalWklyBillableOH = row["TotalWklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOH"]) : 0,
                        TotalWklyBillableOHOT = row["TotalWklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOHOT"]) : 0,
                        ETCRate = row["ETCRate"] != DBNull.Value ? Convert.ToDecimal(row["ETCRate"]) : 0,
                        ETCRateTypeDesc = row["ETCRateTypeDesc"]?.ToString(),
                        ETCRateTypeID = row["ETCRateTypeID"] != DBNull.Value ? Convert.ToInt32(row["ETCRateTypeID"]) : 0,
                        MapChk = row["MapChk"] != DBNull.Value ? Convert.ToInt32(row["MapChk"]) : 0,
                        Probability = row["Probability"] != DBNull.Value ? Convert.ToDecimal(row["Probability"]) : 0,
                    };

                    discETC.Add(item);
                }
                Console.WriteLine($"Loaded {discETC.Count} records from vwDiscETC.");

            }

            return discETC;
        }
        public List<vwDiscETC> GetDiscETC_ByJobAndGroup(int jobId, string empResGroupDesc, IConfiguration configuration)
        {
            List<vwDiscETC> discETCList = new List<vwDiscETC>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT * FROM vwDiscETC WHERE JobID = @JobID AND EmpResGroupDesc = @EmpResGroupDesc", con);
                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);
                da.SelectCommand.Parameters.AddWithValue("@EmpResGroupDesc", empResGroupDesc);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwDiscETC item = new vwDiscETC
                    {
                        // Fill properties here based on your model definition
                        DiscEtcID = Convert.ToInt32(row["DiscEtcID"]),
                        OBID = Convert.ToInt32(row["OBID"]),
                        JobID = Convert.ToInt32(row["JobID"]),
                        myTask = Convert.ToString(row["myTask"]),
                        RptWeekend= Convert.ToDateTime(row["RptWeekend"]),
                        EmpResGroupDesc = row["EmpResGroupDesc"].ToString(),
                        ETCHrs = Convert.ToDecimal(row["ETCHrs"]),
                        ETCCost = Convert.ToDecimal(row["ETCCost"]),
                        EACHrs = Convert.ToDecimal(row["EACHrs"]),
                        EACCost = Convert.ToDecimal(row["EACCost"]),
                        ETCComment= row["ETCComment"].ToString(),
                        PlanStartWE= Convert.ToDateTime(row["PlanStartWE"]),
                        PlanFinishWE= Convert.ToDateTime(row["PlanFinishWE"]),
                        EmpID = Convert.ToInt32(row["EmpID"]),
                        EmpGroupID = Convert.ToInt32(row["EmpGroupID"]),
                        CurveID = Convert.ToInt32(row["CurveID"]),
                        CurveName= row["CurveName"].ToString(),
                        ETCRate= Convert.ToDecimal(row["ETCRate"]),
                        ETCRateTypeDesc = row["ETCRateTypeDesc"].ToString(),
                        ETCRateTypeID = Convert.ToInt32(row["ETCRateTypeID"]),
                        MapChk = Convert.ToInt32(row["MapChk"]),
                        Probability = Convert.ToDecimal(row["Probability"]),
                    };

                    discETCList.Add(item);
                }
            }

            return discETCList;
        }



        public List<vwDiscETC> GetDiscETC(int id, IConfiguration configuration)
        {
            List<vwDiscETC> discETC = new List<vwDiscETC>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vwDiscETC WHERE JobID = @JobID", con);
                da.SelectCommand.Parameters.AddWithValue("@JobID", id);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwDiscETC item = new vwDiscETC
                    {
                        DiscEtcID = row["DiscEtcID"] != DBNull.Value ? Convert.ToInt32(row["DiscEtcID"]) : 0,
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        OBID = row["OBID"] != DBNull.Value ? Convert.ToInt32(row["OBID"]) : 0,
                        myTask = row["myTask"]?.ToString(),
                        RptWeekend = row["RptWeekend"] != DBNull.Value ? Convert.ToDateTime(row["RptWeekend"]) : DateTime.MinValue,
                        ETCHrs = row["ETCHrs"] != DBNull.Value ? Convert.ToDecimal(row["ETCHrs"]) : 0,
                        ETCCost = row["ETCCost"] != DBNull.Value ? Convert.ToDecimal(row["ETCCost"]) : 0,
                        EACHrs = row["EACHrs"] != DBNull.Value ? Convert.ToDecimal(row["EACHrs"]) : 0,
                        EACCost = row["EACCost"] != DBNull.Value ? Convert.ToDecimal(row["EACCost"]) : 0,
                        ETCComment = row["ETCComment"]?.ToString(),
                        Created = row["Created"] != DBNull.Value ? Convert.ToDateTime(row["Created"]) : DateTime.MinValue,
                        Modified = row["Modified"] != DBNull.Value ? Convert.ToDateTime(row["Modified"]) : DateTime.MinValue,
                        PlanStartWE = row["PlanStartWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanStartWE"]) : DateTime.MinValue,
                        PlanFinishWE = row["PlanFinishWE"] != DBNull.Value ? Convert.ToDateTime(row["PlanFinishWE"]) : DateTime.MinValue,
                        EmpID = row["EmpID"] != DBNull.Value ? Convert.ToInt32(row["EmpID"]) : 0,
                        EmpGroupID = row["EmpGroupID"] != DBNull.Value ? Convert.ToInt32(row["EmpGroupID"]) : 0,
                        CurveID = row["CurveID"] != DBNull.Value ? Convert.ToInt32(row["CurveID"]) : 0,
                        CurveName = row["CurveName"]?.ToString(),
                        ResourceStatus = row["ResourceStatus"]?.ToString(),
                        EmpResGroupDesc = row["EmpResGroupDesc"]?.ToString(),
                        EmpResGroupLead = row["EmpResGroupLead"]?.ToString(),
                        ClientNameShort = row["ClientNameShort"]?.ToString(),
                        JobName = row["JobName"]?.ToString(),
                        StackedAreaClient = row["StackedAreaClient"]?.ToString(),
                        WklyBillable = row["WklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillable"]) : 0,
                        WklyBillableOH = row["WklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOH"]) : 0,
                        WklyBillableOHOT = row["WklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["WklyBillableOHOT"]) : 0,
                        TotalWklyBillable = row["TotalWklyBillable"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillable"]) : 0,
                        TotalWklyBillableOH = row["TotalWklyBillableOH"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOH"]) : 0,
                        TotalWklyBillableOHOT = row["TotalWklyBillableOHOT"] != DBNull.Value ? Convert.ToDecimal(row["TotalWklyBillableOHOT"]) : 0,
                        ETCRate = row["ETCRate"] != DBNull.Value ? Convert.ToDecimal(row["ETCRate"]) : 0,
                        ETCRateTypeDesc = row["ETCRateTypeDesc"]?.ToString(),
                        ETCRateTypeID = row["ETCRateTypeID"] != DBNull.Value ? Convert.ToInt32(row["ETCRateTypeID"]) : 0,
                        MapChk = row["MapChk"] != DBNull.Value ? Convert.ToInt32(row["MapChk"]) : 0,
                        Probability = row["Probability"] != DBNull.Value ? Convert.ToDecimal(row["Probability"]) : 0
                    };

                    discETC.Add(item);
                }
            }

            return discETC;
        }

        public (List<vwDiscETCOneLine> ETCItems, List<string> MgrNames) GetAllDiscETCOneLineWithMgrs(IConfiguration configuration)
        {
            var discETCOneLine = new List<vwDiscETCOneLine>();
            var mgrNames = new List<string>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string sql = @"
            SELECT * FROM vwDiscETCOneLine;

            SELECT DISTINCT MgrName 
            FROM vwDiscETCOneLine 
            WHERE MgrName IS NOT NULL 
            ORDER BY MgrName;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set: ETC rows
                        while (reader.Read())
                        {
                            var item = new vwDiscETCOneLine
                            {
                                JobID = reader["JobID"] != DBNull.Value ? Convert.ToInt32(reader["JobID"]) : 0,
                                MgrName = reader["MgrName"]?.ToString(),
                                ClientNameShort = reader["ClientNameShort"]?.ToString(),
                                ClientJob = reader["ClientJob"]?.ToString(),
                                SimpleFlag = reader["SimpleFlag"] != DBNull.Value ? Convert.ToInt32(reader["SimpleFlag"]) : 0,

                                ETCHrs_PM = reader["ETCHrs_PM"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_PM"]) : 0,
                                PlanStartWE_PM = reader["PlanStartWE_PM"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_PM"]) : DateTime.MinValue,
                                PlanFinishWE_PM = reader["PlanFinishWE_PM"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_PM"]) : DateTime.MinValue,
                                CurveID_PM = reader["CurveID_PM"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_PM"]) : 0,
                                CurveName_PM = reader["CurveName_PM"]?.ToString(),
                                OBID_PM = reader["OBID_PM"] != DBNull.Value ? Convert.ToInt32(reader["OBID_PM"]) : 0,
                                DiscETCID_PM = reader["DiscETCID_PM"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_PM"]) : 0,

                                ETCHrs_Engr = reader["ETCHrs_Engr"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_Engr"]) : 0,
                                PlanStartWE_Engr = reader["PlanStartWE_Engr"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_Engr"]) : DateTime.MinValue,
                                PlanFinishWE_Engr = reader["PlanFinishWE_Engr"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_Engr"]) : DateTime.MinValue,
                                CurveID_Engr = reader["CurveID_Engr"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_Engr"]) : 0,
                                CurveName_Engr = reader["CurveName_Engr"]?.ToString(),
                                OBID_Engr = reader["OBID_Engr"] != DBNull.Value ? Convert.ToInt32(reader["OBID_Engr"]) : 0,
                                DiscETCID_Engr = reader["DiscETCID_Engr"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_Engr"]) : 0,

                                ETCHrs_EIC = reader["ETCHrs_EIC"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_EIC"]) : 0,
                                PlanStartWE_EIC = reader["PlanStartWE_EIC"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_EIC"]) : DateTime.MinValue,
                                PlanFinishWE_EIC = reader["PlanFinishWE_EIC"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_EIC"]) : DateTime.MinValue,
                                CurveID_EIC = reader["CurveID_EIC"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_EIC"]) : 0,
                                CurveName_EIC = reader["CurveName_EIC"]?.ToString(),
                                OBID_EIC = reader["OBID_EIC"] != DBNull.Value ? Convert.ToInt32(reader["OBID_EIC"]) : 0,
                                DiscETCID_EIC = reader["DiscETCID_EIC"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_EIC"]) : 0,

                                ETCHrs_Design = reader["ETCHrs_Design"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_Design"]) : 0,
                                PlanStartWE_Design = reader["PlanStartWE_Design"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_Design"]) : DateTime.MinValue,
                                PlanFinishWE_Design = reader["PlanFinishWE_Design"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_Design"]) : DateTime.MinValue,
                                CurveID_Design = reader["CurveID_Design"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_Design"]) : 0,
                                CurveName_Design = reader["CurveName_Design"]?.ToString(),
                                OBID_Design = reader["OBID_Design"] != DBNull.Value ? Convert.ToInt32(reader["OBID_Design"]) : 0,
                                DiscETCID_Design = reader["DiscETCID_Design"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_Design"]) : 0,
                            };

                            discETCOneLine.Add(item);
                        }

                        // Move to second result set: MgrNames
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                mgrNames.Add(reader["MgrName"].ToString());
                            }
                        }
                    }
                }
            }

            return (discETCOneLine, mgrNames);
        }

        public (List<vwDiscETCOneLineNEW> ETCItems, List<string> MgrNames) GetAllDiscETCOneLineWithMgrsNEW(IConfiguration configuration)
        {
            var discETCOneLine = new List<vwDiscETCOneLineNEW>();
            var mgrNames = new List<string>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string sql = @"
            SELECT * FROM vwDiscETCOneLineNEW;

            SELECT DISTINCT MgrName 
            FROM vwDiscETCOneLine 
            WHERE MgrName IS NOT NULL 
            ORDER BY MgrName;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // First result set: ETC rows
                        while (reader.Read())
                        {
                            var item = new vwDiscETCOneLineNEW
                            {
                                JobID = reader["JobID"] != DBNull.Value ? Convert.ToInt32(reader["JobID"]) : 0,
                                MgrName = reader["MgrName"]?.ToString(),
                                ClientNameShort = reader["ClientNameShort"]?.ToString(),
                                ClientJob = reader["ClientJob"]?.ToString(),
                                SimpleFlag = reader["SimpleFlag"] != DBNull.Value ? Convert.ToInt32(reader["SimpleFlag"]) : 0,

                                ETCHrs_PM = reader["ETCHrs_PM"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_PM"]) : 0,
                                PlanStartWE_PM = reader["PlanStartWE_PM"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_PM"]) : DateTime.MinValue,
                                PlanFinishWE_PM = reader["PlanFinishWE_PM"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_PM"]) : DateTime.MinValue,
                                CurveID_PM = reader["CurveID_PM"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_PM"]) : 0,
                                CurveName_PM = reader["CurveName_PM"]?.ToString(),
                                OBID_PM = reader["OBID_PM"] != DBNull.Value ? Convert.ToInt32(reader["OBID_PM"]) : 0,
                                DiscETCID_PM = reader["DiscETCID_PM"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_PM"]) : 0,

                                ETCHrs_Engr = reader["ETCHrs_Engr"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_Engr"]) : 0,
                                PlanStartWE_Engr = reader["PlanStartWE_Engr"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_Engr"]) : DateTime.MinValue,
                                PlanFinishWE_Engr = reader["PlanFinishWE_Engr"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_Engr"]) : DateTime.MinValue,
                                CurveID_Engr = reader["CurveID_Engr"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_Engr"]) : 0,
                                CurveName_Engr = reader["CurveName_Engr"]?.ToString(),
                                OBID_Engr = reader["OBID_Engr"] != DBNull.Value ? Convert.ToInt32(reader["OBID_Engr"]) : 0,
                                DiscETCID_Engr = reader["DiscETCID_Engr"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_Engr"]) : 0,

                                ETCHrs_EIC = reader["ETCHrs_EIC"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_EIC"]) : 0,
                                PlanStartWE_EIC = reader["PlanStartWE_EIC"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_EIC"]) : DateTime.MinValue,
                                PlanFinishWE_EIC = reader["PlanFinishWE_EIC"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_EIC"]) : DateTime.MinValue,
                                CurveID_EIC = reader["CurveID_EIC"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_EIC"]) : 0,
                                CurveName_EIC = reader["CurveName_EIC"]?.ToString(),
                                OBID_EIC = reader["OBID_EIC"] != DBNull.Value ? Convert.ToInt32(reader["OBID_EIC"]) : 0,
                                DiscETCID_EIC = reader["DiscETCID_EIC"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_EIC"]) : 0,

                                ETCHrs_Design = reader["ETCHrs_Design"] != DBNull.Value ? Convert.ToDecimal(reader["ETCHrs_Design"]) : 0,
                                PlanStartWE_Design = reader["PlanStartWE_Design"] != DBNull.Value ? Convert.ToDateTime(reader["PlanStartWE_Design"]) : DateTime.MinValue,
                                PlanFinishWE_Design = reader["PlanFinishWE_Design"] != DBNull.Value ? Convert.ToDateTime(reader["PlanFinishWE_Design"]) : DateTime.MinValue,
                                CurveID_Design = reader["CurveID_Design"] != DBNull.Value ? Convert.ToDecimal(reader["CurveID_Design"]) : 0,
                                CurveName_Design = reader["CurveName_Design"]?.ToString(),
                                OBID_Design = reader["OBID_Design"] != DBNull.Value ? Convert.ToInt32(reader["OBID_Design"]) : 0,
                                DiscETCID_Design = reader["DiscETCID_Design"] != DBNull.Value ? Convert.ToInt32(reader["DiscETCID_Design"]) : 0,
                            };

                            discETCOneLine.Add(item);
                        }

                        // Move to second result set: MgrNames
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                mgrNames.Add(reader["MgrName"].ToString());
                            }
                        }
                    }
                }
            }

            return (discETCOneLine, mgrNames);
        }


        public List<vwJob> GetAllOpenProj(IConfiguration configuration)
        {
            List<vwJob> AllOpenProj = new List<vwJob>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM vwJob";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    //cmd.Parameters.AddWithValue("@JobID", jobId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        vwJob record = new vwJob
                        {
                            JobID = reader["JobID"] != DBNull.Value ? Convert.ToInt32(reader["JobID"]) : 0,
                            ClientID = reader["ClientID"] != DBNull.Value ? Convert.ToInt32(reader["ClientID"]) : 0,
                            ClientNameShort = reader["ClientNameShort"]?.ToString(),
                            BigTimeJobDisplayName = reader["BigTimeJobDisplayName"]?.ToString(),
                            JobName = reader["JobName"]?.ToString(),
                            JobNum = reader["JobNum"]?.ToString(),
                            JobStartDate = reader["JobStartDate"] != DBNull.Value ? Convert.ToDateTime(reader["JobStartDate"]) : DateTime.MinValue,
                            MgrName = reader["MgrName"]?.ToString(),
                            JobFinishDate = reader["JobFinishDate"] != DBNull.Value ? Convert.ToDateTime(reader["JobFinishDate"]) : DateTime.MinValue,
                            BillingStatus = reader["BillingStatus"]?.ToString(),
                            RateSheetName = reader["RateSheetName"]?.ToString(),
                            AFE = reader["AFE"]?.ToString(),
                            ClientPM = reader["ClientPM"]?.ToString(),
                            CorpID = reader["CorpID"] != DBNull.Value ? Convert.ToInt32(reader["CorpID"]) : 0,
                            ProjectProgramID = reader["ProjectProgramID"] != DBNull.Value ? Convert.ToInt32(reader["ProjectProgramID"]) : 0,
                            RegionID = reader["RegionID"] != DBNull.Value ? Convert.ToInt32(reader["RegionID"]) : 0,
                            StreamID = reader["StreamID"] != DBNull.Value ? Convert.ToInt32(reader["StreamID"]) : 0,
                            ProjectTypeID = reader["ProjectTypeID"] != DBNull.Value ? Convert.ToInt32(reader["ProjectTypeID"]) : 0,
                            ResourceStatus = reader["ResourceStatus"]?.ToString(),
                            ClientJob = reader["ClientJob"]?.ToString(),
                            CURRCOST = reader.IsDBNull(reader.GetOrdinal("CURRCOST")) ? 0 : reader.GetDecimal(reader.GetOrdinal("CURRCOST")),
                            Probability = reader.IsDBNull(reader.GetOrdinal("Probability")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Probability"))
                        };

                        AllOpenProj.Add(record);
                    }
                }
            }

            return AllOpenProj;
        }

        public List<vwJob> GetThisWJob(int jobId, IConfiguration configuration)
        {
            List<vwJob> thisVWJob = new List<vwJob>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM vwJob WHERE JobID = @JobID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@JobID", jobId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        vwJob record = new vwJob
                        {
                            JobID = reader["JobID"] != DBNull.Value ? Convert.ToInt32(reader["JobID"]) : 0,
                            ClientID = reader["ClientID"] != DBNull.Value ? Convert.ToInt32(reader["ClientID"]) : 0,
                            ClientNameShort = reader["ClientNameShort"]?.ToString(),
                            BigTimeJobDisplayName = reader["BigTimeJobDisplayName"]?.ToString(),
                            JobName = reader["JobName"]?.ToString(),
                            JobNum = reader["JobNum"]?.ToString(),
                            JobStartDate = reader["JobStartDate"] != DBNull.Value ? Convert.ToDateTime(reader["JobStartDate"]) : DateTime.MinValue,
                            MgrName = reader["MgrName"]?.ToString(),
                            JobFinishDate = reader["JobFinishDate"] != DBNull.Value ? Convert.ToDateTime(reader["JobFinishDate"]) : DateTime.MinValue,
                            BillingStatus = reader["BillingStatus"]?.ToString(),
                            RateSheetName = reader["RateSheetName"]?.ToString(),
                            AFE = reader["AFE"]?.ToString(),
                            ClientPM = reader["ClientPM"]?.ToString(),
                            CorpID = reader["CorpID"] != DBNull.Value ? Convert.ToInt32(reader["CorpID"]) : 0,
                            ProjectProgramID = reader["ProjectProgramID"] != DBNull.Value ? Convert.ToInt32(reader["ProjectProgramID"]) : 0,
                            RegionID = reader["RegionID"] != DBNull.Value ? Convert.ToInt32(reader["RegionID"]) : 0,
                            StreamID = reader["StreamID"] != DBNull.Value ? Convert.ToInt32(reader["StreamID"]) : 0,
                            ProjectTypeID = reader["ProjectTypeID"] != DBNull.Value ? Convert.ToInt32(reader["ProjectTypeID"]) : 0,
                            ResourceStatus = reader["ResourceStatus"]?.ToString(),
                            ClientJob = reader["ClientJob"]?.ToString(),
                            CURRCOST = reader.IsDBNull(reader.GetOrdinal("CURRCOST")) ? 0 : reader.GetDecimal(reader.GetOrdinal("CURRCOST")),
                            Probability = reader.IsDBNull(reader.GetOrdinal("Probability")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Probability"))
                        };

                        thisVWJob.Add(record);
                    }
                }
            }

            return thisVWJob;
        }

        public int AddUser(Users user, IConfiguration configuration)
        {
            int i = 0;
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlCommand cmd = new SqlCommand("INSERT into tbUsersToDelete(FirstName,LastName) VALUES ('" + user.FirstName + "','" + user.LastName + "')", con);
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }
            return i;
        }

        public Users GetUser(string id, IConfiguration configuration)
        {
            {
                Users user = new Users();
                using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select * from TbUsersToDelete WHERE ID='" + id + "' ", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {

                        user.ID = Convert.ToString(dt.Rows[0]["ID"]);
                        user.FirstName = Convert.ToString(dt.Rows[0]["FirstName"]);
                        user.LastName = Convert.ToString(dt.Rows[0]["LastName"]);
                    }
                }
                return user;
            }
        }

        public tbProjectProgress GetProgressStatus(string id, IConfiguration configuration)
        {
            {
                tbProjectProgress ProgressStatus = new tbProjectProgress();
                using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select * from tbProjectProgress WHERE ProjectProgID='" + id + "' ", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        ProgressStatus.ProjectProgID = Convert.ToInt32(dt.Rows[0]["ProjectProgID"]);
                        ProgressStatus.WeekEnd = Convert.ToDateTime(dt.Rows[0]["Weekend"]);
                        ProgressStatus.ProjectPeriodProgress = Convert.ToDecimal(dt.Rows[0]["ProjectPeriodProgress"]);
                        ProgressStatus.Status = Convert.ToString(dt.Rows[0]["Status"]);
                        ProgressStatus.JobID = Convert.ToInt32(dt.Rows[0]["JobID"]);
                        ProgressStatus.Comment = Convert.ToString(dt.Rows[0]["Comment"]);
                        ProgressStatus.FcastFinishDate = Convert.ToDateTime(dt.Rows[0]["FcastFinishDate"]);
                        ProgressStatus.ForecastHrs = Convert.ToDecimal(dt.Rows[0]["ForecastHrs"]);
                        ProgressStatus.CumulPeriodProgress = Convert.ToDecimal(dt.Rows[0]["CumulPeriodProgress"]);
                        ProgressStatus.Created = Convert.ToDateTime(dt.Rows[0]["Created"]);
                        ProgressStatus.Modified = Convert.ToDateTime(dt.Rows[0]["Modified"]);
                        ProgressStatus.EAC_Info = Convert.ToDecimal(dt.Rows[0]["EAC_Info"]);
                    }
                }
                return ProgressStatus;
            }
        }

        public List<vwDeliverable> GetDelList(int id, IConfiguration configuration)
        {
            List<vwDeliverable> delList = new List<vwDeliverable>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vwDeliverable WHERE JobID = @JobID", con);
                da.SelectCommand.Parameters.AddWithValue("@JobID", id);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    vwDeliverable item = new vwDeliverable
                    {
                        DeliverableID = row["DeliverableID"] != DBNull.Value ? Convert.ToInt32(row["DeliverableID"]) : 0,
                        OBID = row["OBID"] != DBNull.Value ? Convert.ToInt32(row["OBID"]) : 0,
                        DelNum = row["DelNum"]?.ToString(),
                        DelName = row["DelName"]?.ToString(),
                        DelHours = row["DelHours"] != DBNull.Value ? Convert.ToDecimal(row["DelHours"]) : 0,
                        DelCost = row["DelCost"] != DBNull.Value ? Convert.ToDecimal(row["DelCost"]) : 0,
                        //myTIMESTAMP = row["myTIMESTAMP"] != DBNull.Value ? Convert.ToDateTime(row["myTIMESTAMP"]) : DateTime.MinValue,
                        PlanFinishDate = row["PlanFinishDate"] != DBNull.Value ? Convert.ToDateTime(row["PlanFinishDate"]) : DateTime.MinValue,
                        DelComment = row["DelComment"]?.ToString(),
                        Created = row["Created"] != DBNull.Value ? Convert.ToDateTime(row["Created"]) : DateTime.MinValue,
                        Modified = row["Modified"] != DBNull.Value ? Convert.ToDateTime(row["Modified"]) : DateTime.MinValue,
                        JobID = row["JobID"] != DBNull.Value ? Convert.ToInt32(row["JobID"]) : 0,
                        PlanStartDate = row["PlanStartDate"] != DBNull.Value ? Convert.ToDateTime(row["PlanStartDate"]) : DateTime.MinValue,
                        DelRev = row["DelRev"]?.ToString(),
                        DelGp1 = row["DelGp1"]?.ToString(),
                        DelGp2 = row["DelGp2"]?.ToString(),
                        DelGp3 = row["DelGp3"]?.ToString(),
                        DelGp4 = row["DelGp4"]?.ToString(),
                        myTask = row["myTask"]?.ToString(),
                        DISCDESC = row["DISCDESC"]?.ToString(),
                        TASKDESC = row["TASKDESC"]?.ToString(),
                        DiscSort = row["DiscSort"] != DBNull.Value ? Convert.ToInt32(row["DiscSort"]) : 0,
                        DiscGroup = row["DiscGroup"]?.ToString()
                    };

                    delList.Add(item);
                }
            }

            return delList;
        }


        public vwCumulSpendforWeekly_Brian GetJobStatusList(string id, IConfiguration configuration)
        {
            {
                vwCumulSpendforWeekly_Brian JobStatus = new vwCumulSpendforWeekly_Brian();
                using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select * from vwCumulSpendforWeekly_Brian WHERE ID='" + id + "' ", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        JobStatus.jobid = Convert.ToString(dt.Rows[0]["jobid"]);
                        JobStatus.MgrName = Convert.ToString(dt.Rows[0]["MgrName"]);
                        JobStatus.Client = Convert.ToString(dt.Rows[0]["Client"]);
                        JobStatus.ClientJob = Convert.ToString(dt.Rows[0]["ClientJob"]);
                        JobStatus.OriginalBudget = Convert.ToDecimal(dt.Rows[0]["Original_Budget"]);
                        JobStatus.CurrentBudget = Convert.ToDecimal(dt.Rows[0]["Current_Budget"]);
                        JobStatus.CurrentCumulativeSpend = Convert.ToDecimal(dt.Rows[0]["Current_Cumulative_Spend"]);
                        JobStatus.PercentSpent = Convert.ToDecimal(dt.Rows[0]["PERCENT_SPENT"]);
                        JobStatus.PercentComplete = Convert.ToDecimal(dt.Rows[0]["PctComplete"]);
                        JobStatus.EACCost = Convert.ToDecimal(dt.Rows[0]["EACCost"]);
                        JobStatus.FinishDate = Convert.ToDateTime(dt.Rows[0]["FinishDate"]);
                        JobStatus.comment = Convert.ToString(dt.Rows[0]["comment"]);
                        JobStatus.ProjectProgID = Convert.ToInt32(dt.Rows[0]["ProjectProgID"]);
                    }
                }
                return JobStatus;
            }
        }

        public int UpdateUser(Users user, IConfiguration configuration)
        {
            int i = 0;
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlCommand cmd = new SqlCommand("UPDATE tbUsersToDelete SET FirstName= '" + user.FirstName + "',LastName='" + user.LastName + "' WHERE ID='" + user.ID + "' ", con);
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }
            return i;
        }

        public int UpdateProgressStatus(tbProjectProgress ProgressStatus, IConfiguration configuration)
        {
            int i = 0;
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlCommand cmd = new SqlCommand("UPDATE tbProjectProgress SET Comment= '" + ProgressStatus.Comment + "',CumulPeriodProgress='" + ProgressStatus.CumulPeriodProgress + "',FcastFinishDate='" + ProgressStatus.FcastFinishDate + "', EAC_Info='" + ProgressStatus.EAC_Info + "' WHERE ProjectProgID='" + ProgressStatus.ProjectProgID + "' ", con);
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }
            return i;
        }

        public int DeleteUser(string id, IConfiguration configuration)
        {
            int i = 0;
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlCommand cmd = new SqlCommand("DELETE from tbUsersToDelete WHERE ID='" + id + "' ", con);
                con.Open();
                i = cmd.ExecuteNonQuery();
                con.Close();
            }
            return i;
        }

        public tbProjectProgress GetOrCreateProgressStatus(string id, IConfiguration configuration)
        {
            tbProjectProgress ProgressStatus = new tbProjectProgress();
            DateTime reportWE = ReportWE.GetReportWE(); // upcoming Saturday
            string connectionString = configuration.GetConnectionString("DBCS");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Step 1: Select existing record
                SqlCommand selectCmd = new SqlCommand("SELECT * FROM tbProjectProgress WHERE ProjectProgID = @id", con);
                selectCmd.Parameters.AddWithValue("@id", id);
                SqlDataAdapter da = new SqlDataAdapter(selectCmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Step 2: Map to ProgressStatus object
                    ProgressStatus.ProjectProgID = Convert.ToInt32(dt.Rows[0]["ProjectProgID"]);
                    ProgressStatus.WeekEnd = Convert.ToDateTime(dt.Rows[0]["Weekend"]);
                    ProgressStatus.ProjectPeriodProgress = Convert.ToDecimal(dt.Rows[0]["ProjectPeriodProgress"]);
                    ProgressStatus.Status = Convert.ToString(dt.Rows[0]["Status"]);
                    ProgressStatus.JobID = Convert.ToInt32(dt.Rows[0]["JobID"]);
                    ProgressStatus.Comment = Convert.ToString(dt.Rows[0]["Comment"]);
                    ProgressStatus.FcastFinishDate = Convert.ToDateTime(dt.Rows[0]["FcastFinishDate"]);
                    ProgressStatus.ForecastHrs = Convert.ToDecimal(dt.Rows[0]["ForecastHrs"]);
                    ProgressStatus.CumulPeriodProgress = Convert.ToDecimal(dt.Rows[0]["CumulPeriodProgress"]);
                    ProgressStatus.Created = Convert.ToDateTime(dt.Rows[0]["Created"]);
                    ProgressStatus.Modified = Convert.ToDateTime(dt.Rows[0]["Modified"]);
                    ProgressStatus.EAC_Info = Convert.ToDecimal(dt.Rows[0]["EAC_Info"]);

                    // Step 3: If WeekEnd doesn't match, insert new record and return it
                    if (ProgressStatus.WeekEnd.Date != reportWE.Date)
                    {
                        SqlCommand insertCmd = new SqlCommand(@"
                    INSERT INTO tbProjectProgress
                    (Weekend, ProjectPeriodProgress, Status, JobID, Comment, 
                     FcastFinishDate, ForecastHrs, CumulPeriodProgress, Created, Modified, EAC_Info)
                    VALUES
                    (@Weekend, @ProjectPeriodProgress, @Status, @JobID, @Comment, 
                     @FcastFinishDate, @ForecastHrs, @CumulPeriodProgress, @Created, @Modified, @EAC_Info);

                    SELECT SCOPE_IDENTITY();", con); // returns the new int identity

                        insertCmd.Parameters.AddWithValue("@Weekend", reportWE);
                        insertCmd.Parameters.AddWithValue("@ProjectPeriodProgress", ProgressStatus.ProjectPeriodProgress);
                        insertCmd.Parameters.AddWithValue("@Status", ProgressStatus.Status);
                        insertCmd.Parameters.AddWithValue("@JobID", ProgressStatus.JobID);
                        insertCmd.Parameters.AddWithValue("@Comment", ProgressStatus.Comment);
                        insertCmd.Parameters.AddWithValue("@FcastFinishDate", ProgressStatus.FcastFinishDate);
                        insertCmd.Parameters.AddWithValue("@ForecastHrs", ProgressStatus.ForecastHrs);
                        insertCmd.Parameters.AddWithValue("@CumulPeriodProgress", ProgressStatus.CumulPeriodProgress);
                        insertCmd.Parameters.AddWithValue("@Created", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@Modified", DateTime.Now);
                        insertCmd.Parameters.AddWithValue("@EAC_Info", ProgressStatus.EAC_Info);

                        // ✅ ExecuteScalar to get the new ID
                        object result = insertCmd.ExecuteScalar();
                        int newID = Convert.ToInt32(result);

                        // Step 4: Retrieve the inserted record by new ID
                        SqlCommand newSelectCmd = new SqlCommand("SELECT * FROM tbProjectProgress WHERE ProjectProgID = @newID", con);
                        newSelectCmd.Parameters.AddWithValue("@newID", newID);
                        SqlDataAdapter newDa = new SqlDataAdapter(newSelectCmd);
                        DataTable newDt = new DataTable();
                        newDa.Fill(newDt);

                        if (newDt.Rows.Count > 0)
                        {
                            ProgressStatus.ProjectProgID = newID;
                            ProgressStatus.WeekEnd = Convert.ToDateTime(newDt.Rows[0]["Weekend"]);
                            ProgressStatus.Created = Convert.ToDateTime(newDt.Rows[0]["Created"]);
                            ProgressStatus.Modified = Convert.ToDateTime(newDt.Rows[0]["Modified"]);
                            // Copy others if needed
                        }
                    }
                }
            }

            return ProgressStatus;
        }

        public tbProjectProgress GetOrCreateProgressStatusByJobID(int jobId, IConfiguration configuration)
        {
            tbProjectProgress ProgressStatus = new tbProjectProgress();
            DateTime reportWE = ReportWE.GetReportWE(); // upcoming Saturday
            string connectionString = configuration.GetConnectionString("DBCS");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Step 1: Try to get current week's record
                SqlCommand selectCurrentCmd = new SqlCommand("SELECT * FROM tbProjectProgress WHERE JobID = @jobId AND Weekend = @weekend", con);
                selectCurrentCmd.Parameters.AddWithValue("@jobId", jobId);
                selectCurrentCmd.Parameters.AddWithValue("@weekend", reportWE);
                SqlDataAdapter da = new SqlDataAdapter(selectCurrentCmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Found it — return it
                    ProgressStatus.ProjectProgID = Convert.ToInt32(dt.Rows[0]["ProjectProgID"]);
                    ProgressStatus.JobID = jobId;
                    ProgressStatus.WeekEnd = reportWE;
                    ProgressStatus.EAC_Info = Convert.ToDecimal(dt.Rows[0]["EAC_Info"]);
                    ProgressStatus.CumulPeriodProgress = Convert.ToDecimal(dt.Rows[0]["CumulPeriodProgress"]);
                    ProgressStatus.FcastFinishDate = Convert.ToDateTime(dt.Rows[0]["FcastFinishDate"]);
                    return ProgressStatus;
                }

                // Step 2: Get most recent past record
                SqlCommand selectLatestCmd = new SqlCommand("SELECT TOP 1 * FROM tbProjectProgress WHERE JobID = @jobId ORDER BY Weekend DESC", con);
                selectLatestCmd.Parameters.AddWithValue("@jobId", jobId);
                SqlDataAdapter latestDa = new SqlDataAdapter(selectLatestCmd);
                DataTable latestDt = new DataTable();
                latestDa.Fill(latestDt);

                if (latestDt.Rows.Count > 0)
                {
                    // Copy from latest and insert new
                    var last = latestDt.Rows[0];
                    SqlCommand insertCmd = new SqlCommand(@"
                INSERT INTO tbProjectProgress
                (Weekend, ProjectPeriodProgress, Status, JobID, Comment, FcastFinishDate,
                 ForecastHrs, CumulPeriodProgress, Created, Modified, EAC_Info)
                VALUES
                (@Weekend, @ProjectPeriodProgress, @Status, @JobID, @Comment, @FcastFinishDate,
                 @ForecastHrs, @CumulPeriodProgress, @Created, @Modified, @EAC_Info);
                SELECT SCOPE_IDENTITY();", con);

                    insertCmd.Parameters.AddWithValue("@Weekend", reportWE);
                    insertCmd.Parameters.AddWithValue("@ProjectPeriodProgress", last["ProjectPeriodProgress"]);
                    insertCmd.Parameters.AddWithValue("@Status", last["Status"]);
                    insertCmd.Parameters.AddWithValue("@JobID", jobId);
                    insertCmd.Parameters.AddWithValue("@Comment", last["Comment"]);
                    insertCmd.Parameters.AddWithValue("@FcastFinishDate", last["FcastFinishDate"]);
                    insertCmd.Parameters.AddWithValue("@ForecastHrs", last["ForecastHrs"]);
                    insertCmd.Parameters.AddWithValue("@CumulPeriodProgress", last["CumulPeriodProgress"]);
                    insertCmd.Parameters.AddWithValue("@Created", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@Modified", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@EAC_Info", last["EAC_Info"]);

                    var newId = Convert.ToInt32(insertCmd.ExecuteScalar());

                    ProgressStatus.ProjectProgID = newId;
                    ProgressStatus.JobID = jobId;
                    ProgressStatus.WeekEnd = reportWE;
                    ProgressStatus.ProjectPeriodProgress = Convert.ToDecimal(last["ProjectPeriodProgress"]);
                    ProgressStatus.Status = Convert.ToString(last["Status"]);
                    ProgressStatus.Comment = Convert.ToString(last["Comment"]);
                    ProgressStatus.FcastFinishDate = last["FcastFinishDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(last["FcastFinishDate"]);
                    ProgressStatus.ForecastHrs = last["ForecastHrs"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(last["ForecastHrs"]);
                    ProgressStatus.CumulPeriodProgress = last["CumulPeriodProgress"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(last["CumulPeriodProgress"]);
                    ProgressStatus.EAC_Info = last["EAC_Info"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(last["EAC_Info"]);
                }
                else
                {
                    // No past record — insert blank/default
                    SqlCommand insertBlankCmd = new SqlCommand(@"
                INSERT INTO tbProjectProgress
                (Weekend, JobID, Created, Modified)
                VALUES (@Weekend, @JobID, @Created, @Modified);
                SELECT SCOPE_IDENTITY();", con);

                    insertBlankCmd.Parameters.AddWithValue("@Weekend", reportWE);
                    insertBlankCmd.Parameters.AddWithValue("@JobID", jobId);
                    insertBlankCmd.Parameters.AddWithValue("@Created", DateTime.Now);
                    insertBlankCmd.Parameters.AddWithValue("@Modified", DateTime.Now);

                    var newId = Convert.ToInt32(insertBlankCmd.ExecuteScalar());

                    ProgressStatus.ProjectProgID = newId;
                    ProgressStatus.JobID = jobId;
                    ProgressStatus.WeekEnd = reportWE;
                }
            }

            return ProgressStatus;
        }


        public vwCumulSpendforWeekly_Brian GetJobStatusByID(int id, IConfiguration configuration)
        {
            vwCumulSpendforWeekly_Brian jobFinanceInfo = null;
            string connectionString = configuration.GetConnectionString("DBCS");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                Console.WriteLine($"🟡 Running query with ProjectProgID = {id}");
                SqlCommand cmd = new SqlCommand("SELECT * FROM vwCumulSpendforWeekly_Brian WHERE ProjectProgID = @id", con);


                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                Console.WriteLine($"🟢 Rows returned: {dt.Rows.Count}");

                if (dt.Rows.Count > 0)
                {
                    jobFinanceInfo = new vwCumulSpendforWeekly_Brian
                    {
                        ProjectProgID = Convert.ToInt32(dt.Rows[0]["ProjectProgID"]),
                        CurrentBudget = Convert.ToDecimal(dt.Rows[0]["Current_Budget"]),
                        CurrentCumulativeSpend = Convert.ToDecimal(dt.Rows[0]["Current_Cumulative_Spend"]),
                        MgrName = Convert.ToString(dt.Rows[0]["MgrName"]),
                        ClientJob = Convert.ToString(dt.Rows[0]["ClientJob"]),
                        Client = Convert.ToString(dt.Rows[0]["Client"])
                    };
                }
                else
                {
                    Console.WriteLine("🔴 No rows found for that ProjectProgID.");
                }

            }

            return jobFinanceInfo;
        }

        public List<vwEmpGroupResources> GetResourceGroups(IConfiguration configuration)
        {
            List<vwEmpGroupResources> ListEmpGroupResources = new List<vwEmpGroupResources>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter("Select * from vwEmpGroupResources", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        vwEmpGroupResources EmpGroupRes = new vwEmpGroupResources();
                        EmpGroupRes.EmpGroupID = Convert.ToInt32(dt.Rows[i]["EmpGroupID"]);
                        EmpGroupRes.EmpGroup = Convert.ToString(dt.Rows[i]["EmpGroup"]);
                        EmpGroupRes.EmpResGpID = Convert.ToInt32(dt.Rows[i]["EmpResGpID"]);
                        EmpGroupRes.EmpName = Convert.ToString(dt.Rows[i]["EmpName"]);
                        EmpGroupRes.EmpResGroupLead = Convert.ToString(dt.Rows[i]["EmpResGroupLead"]);
                        EmpGroupRes.EmpResGroupDesc = Convert.ToString(dt.Rows[i]["EmpResGroupDesc"]);
                        EmpGroupRes.WklyBillable = Convert.ToDecimal(dt.Rows[i]["WklyBillable"]);
                        EmpGroupRes.WklyBillableOH = Convert.ToDecimal(dt.Rows[i]["WklyBillableOH"]);
                        EmpGroupRes.WklyBillableOHOT = Convert.ToDecimal(dt.Rows[i]["WklyBillableOHOT"]);
                        EmpGroupRes.EmpID = Convert.ToInt32(dt.Rows[i]["EmpID"]);
                        ListEmpGroupResources.Add(EmpGroupRes);
                    }
                }
            }
            return ListEmpGroupResources;
        }

        public List<ResourceDetailGroups> GetResourceGroupSummaries(IConfiguration configuration)
        {
            List<ResourceDetailGroups> groupSummaries = new List<ResourceDetailGroups>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT EmpGroupID, EmpGroup, EmpResGroupLead,EmpResGroupDesc
            FROM vwEmpGroupResources
            GROUP BY EmpGroupID, EmpGroup, EmpResGroupLead,EmpResGroupDesc", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var summary = new ResourceDetailGroups
                    {
                        EmpGroupID = Convert.ToInt32(row["EmpGroupID"]),
                        EmpGroup = Convert.ToString(row["EmpGroup"]),
                        EmpResGroupDesc = Convert.ToString(row["EmpResGroupDesc"]),
                        EmpResGroupLead = Convert.ToString(row["EmpResGroupLead"])
                    };
                    groupSummaries.Add(summary);
                }
            }
            return groupSummaries;
        }

        public List<vwEmpGroupResources> GetResCapGroupEmp(IConfiguration configuration)
        {
            List<vwEmpGroupResources> ResCapGroupEmp = new List<vwEmpGroupResources>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT EmpResGroupDesc, EmpName, WklyBillable,WklyBillableOH,WklyBillableOHOT
            FROM vwEmpGroupResources
            GROUP BY EmpResGroupDesc, EmpName", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var summary = new vwEmpGroupResources
                    {
                        EmpResGroupDesc = Convert.ToString(row["EmpResGroupDesc"]),
                        EmpName = Convert.ToString(row["EmpName"]),
                        WklyBillable = Convert.ToDecimal(row["WklyBillable"]),
                        WklyBillableOH = Convert.ToDecimal(row["WklyBillableOH"]),
                        WklyBillableOHOT = Convert.ToDecimal(row["WklyBillableOHOT"]),
                    };
                    ResCapGroupEmp.Add(summary);
                }
            }
            return ResCapGroupEmp;
        }

        public List<vwEmpGroupResources> GetResCapGroup(IConfiguration configuration)
        {
            List<vwEmpGroupResources> ResCapGroup = new List<vwEmpGroupResources>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT EmpResGroupDesc, sum(WklyBillable) as WklyBillable,sum(WklyBillableOH) as WklyBillableOH,sum(WklyBillableOHOT) as WklyBillableOHOT
            FROM vwEmpGroupResources
            GROUP BY EmpResGroupDesc", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var summary = new vwEmpGroupResources
                    {
                        EmpResGroupDesc = Convert.ToString(row["EmpResGroupDesc"]),
                        WklyBillable = Convert.ToDecimal(row["WklyBillable"]),
                        WklyBillableOH = Convert.ToDecimal(row["WklyBillableOH"]),
                        WklyBillableOHOT = Convert.ToDecimal(row["WklyBillableOHOT"]),
                    };
                    ResCapGroup.Add(summary);
                }
            }
            return ResCapGroup;
        }

        public List<vwEmpGroupResources> GetResCapTotal (IConfiguration configuration)
        {
            List<vwEmpGroupResources> ResCapTotal = new List<vwEmpGroupResources>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS").ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT sum(WklyBillable) as WklyBillable,sum(WklyBillableOH) as WklyBillableOH,sum(WklyBillableOHOT) as WklyBillableOHOT
            FROM vwEmpGroupResources", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var summary = new vwEmpGroupResources
                    {
                        WklyBillable = Convert.ToDecimal(row["WklyBillable"]),
                        WklyBillableOH = Convert.ToDecimal(row["WklyBillableOH"]),
                        WklyBillableOHOT = Convert.ToDecimal(row["WklyBillableOHOT"]),
                    };
                    ResCapTotal.Add(summary);
                }
            }
            return ResCapTotal;
        }
        public List<vwBudgetActuals_REVISED> GetBudgetRecords(int jobId, IConfiguration configuration)
        {
            List<vwBudgetActuals_REVISED> list = new List<vwBudgetActuals_REVISED>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM vwBudgetActuals_REVISED WHERE JobID = @JobID", con);
                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var record = new vwBudgetActuals_REVISED
                    {
                        JobID = Convert.ToInt32(row["JobID"]),
                        OBID = Convert.ToInt32(row["OBID"]),
                        MYTASK = Convert.ToString(row["MYTASK"])
                    };
                    list.Add(record);
                }
            }
            return list;
        }


        public List<tbForecast> GetForecastRecords(int jobId, IConfiguration configuration, DateTime forecastDate)
        {
            List<tbForecast> list = new List<tbForecast>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT * FROM tbForecast WHERE JobID = @JobID AND ForecastDateWE = @forecastDate", con);

                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);
                da.SelectCommand.Parameters.AddWithValue("@forecastDate", forecastDate);

                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var record = new tbForecast
                    {
                        ForecastID = Convert.ToInt32(row["ForecastID"]),
                        OBID = Convert.ToInt32(row["OBID"]),
                        ForecastDateWE = Convert.ToDateTime(row["ForecastDateWE"]),
                        PctComplete = Convert.ToDecimal(row["PctComplete"]),
                        EAC_Hrs = Convert.ToDecimal(row["EAC_Hrs"]),
                        EAC_Cost = Convert.ToDecimal(row["EAC_Cost"]),
                        DiscForecastComment = row["DiscForecastComment"]?.ToString(),
                        JobID = Convert.ToInt32(row["JobID"]),
                        EmpGroupID = Convert.ToInt32(row["EmpGroupID"])
                    };
                    list.Add(record);
                }
            }
            return list;
        }

        public List<tbForecast> GetOrCreateForecastRecords(int jobId, IConfiguration configuration)
        {
            List<tbForecast> forecastList = new List<tbForecast>();
            DateTime reportWE = ReportWE.GetReportWE(); // upcoming Saturday

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                con.Open();

                // Step 1: Try to fetch forecasts for this JobID and this reportWE
                SqlCommand selectCmd = new SqlCommand("SELECT * FROM tbForecast WHERE JobID = @JobID AND ForecastDateWE = @ForecastDateWE", con);
                selectCmd.Parameters.AddWithValue("@JobID", jobId);
                selectCmd.Parameters.AddWithValue("@ForecastDateWE", reportWE);

                using (SqlDataAdapter da = new SqlDataAdapter(selectCmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        // Forecasts already exist for this JobID + ReportWE
                        foreach (DataRow row in dt.Rows)
                        {
                            forecastList.Add(new tbForecast
                            {
                                ForecastID = Convert.ToInt32(row["ForecastID"]),
                                OBID = Convert.ToInt32(row["OBID"]),
                                ForecastDateWE = Convert.ToDateTime(row["ForecastDateWE"]),
                                PctComplete = Convert.ToDecimal(row["PctComplete"]),
                                EAC_Hrs = Convert.ToDecimal(row["EAC_Hrs"]),
                                EAC_Cost = Convert.ToDecimal(row["EAC_Cost"]),
                                DiscForecastComment = Convert.ToString(row["DiscForecastComment"]),
                                JobID = Convert.ToInt32(row["JobID"]),
                                EmpGroupID = Convert.ToInt32(row["EmpGroupID"])
                            });
                        }

                        return forecastList; // ✅ Done
                    }
                }

                // Step 2: No matching forecasts found — get latest available set
                SqlCommand latestDateCmd = new SqlCommand("SELECT MAX(ForecastDateWE) FROM tbForecast WHERE JobID = @JobID", con);
                latestDateCmd.Parameters.AddWithValue("@JobID", jobId);
                object maxDateObj = latestDateCmd.ExecuteScalar();

                if (maxDateObj != DBNull.Value)
                {
                    DateTime latestDate = Convert.ToDateTime(maxDateObj);

                    SqlCommand getLatestCmd = new SqlCommand("SELECT * FROM tbForecast WHERE JobID = @JobID AND ForecastDateWE = @LatestDate", con);
                    getLatestCmd.Parameters.AddWithValue("@JobID", jobId);
                    getLatestCmd.Parameters.AddWithValue("@LatestDate", latestDate);

                    using (SqlDataAdapter latestDa = new SqlDataAdapter(getLatestCmd))
                    {
                        DataTable latestDt = new DataTable();
                        latestDa.Fill(latestDt);

                        foreach (DataRow row in latestDt.Rows)
                        {
                            SqlCommand insertCmd = new SqlCommand(@"
                        INSERT INTO tbForecast
                        (OBID, ForecastDateWE, PctComplete, EAC_Hrs, EAC_Cost, DiscForecastComment, JobID, EmpGroupID)
                        VALUES
                        (@OBID, @ForecastDateWE, @PctComplete, @EAC_Hrs, @EAC_Cost, @DiscForecastComment, @JobID, @EmpGroupID);

                        SELECT SCOPE_IDENTITY();", con);

                            insertCmd.Parameters.AddWithValue("@OBID", Convert.ToInt32(row["OBID"]));
                            insertCmd.Parameters.AddWithValue("@ForecastDateWE", reportWE);
                            insertCmd.Parameters.AddWithValue("@PctComplete", Convert.ToDecimal(row["PctComplete"]));
                            insertCmd.Parameters.AddWithValue("@EAC_Hrs", Convert.ToDecimal(row["EAC_Hrs"]));
                            insertCmd.Parameters.AddWithValue("@EAC_Cost", Convert.ToDecimal(row["EAC_Cost"]));
                            insertCmd.Parameters.AddWithValue("@DiscForecastComment", Convert.ToString(row["DiscForecastComment"] ?? ""));
                            insertCmd.Parameters.AddWithValue("@JobID", Convert.ToInt32(row["JobID"]));
                            insertCmd.Parameters.AddWithValue("@EmpGroupID", Convert.ToInt32(row["EmpGroupID"]));

                            int newId = Convert.ToInt32(insertCmd.ExecuteScalar());

                            forecastList.Add(new tbForecast
                            {
                                ForecastID = newId,
                                OBID = Convert.ToInt32(row["OBID"]),
                                ForecastDateWE = reportWE,
                                PctComplete = Convert.ToDecimal(row["PctComplete"]),
                                EAC_Hrs = Convert.ToDecimal(row["EAC_Hrs"]),
                                EAC_Cost = Convert.ToDecimal(row["EAC_Cost"]),
                                DiscForecastComment = Convert.ToString(row["DiscForecastComment"] ?? ""),
                                JobID = Convert.ToInt32(row["JobID"]),
                                EmpGroupID = Convert.ToInt32(row["EmpGroupID"])
                            });
                        }
                    }
                }
            }

            return forecastList;
        }

        public List<tbDeliverableHist> GetOrCreateDelHist(int jobId, IConfiguration configuration)
        {
            var delListHist = new List<tbDeliverableHist>();
            DateTime reportWE = ReportWE.GetReportWE();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                con.Open();

                // Step 1: Try to fetch current records
                SqlCommand selectCmd = new SqlCommand("SELECT * FROM tbDeliverableHist WHERE JobID = @JobID AND ProgressDate = @ForecastDateWE", con);
                selectCmd.Parameters.AddWithValue("@JobID", jobId);
                selectCmd.Parameters.AddWithValue("@ForecastDateWE", reportWE);

                using (SqlDataAdapter da = new SqlDataAdapter(selectCmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            delListHist.Add(new tbDeliverableHist
                            {
                                DeliverableID = row["DeliverableID"] as int? ?? 0,
                                OBID = row["OBID"] as int? ?? 0,
                                DelNum = row["DelNum"]?.ToString(),
                                DelName = row["DelName"]?.ToString(),
                                DelHours = row["DelHours"] as decimal? ?? 0,
                                DelCost = row["DelCost"] as decimal? ?? 0,
                                DelPctCumul = row["DelPctCumul"] as decimal? ?? 0,
                                DelEarnedHrs = row["DelEarnedHrs"] as decimal? ?? 0,
                                DelEarnedCost = row["DelEarnedCost"] as decimal? ?? 0,
                                ProgressDate = row["ProgressDate"] as DateTime? ?? DateTime.MinValue,
                                Direct = row.IsNull("Direct") ? false : Convert.ToBoolean(row["Direct"]),
                                JobID = row["JobID"] as int? ?? 0,
                                DirPct = row["DirPct"] as decimal? ?? 0,
                                PlanFinishDate = row["PlanFinishDate"] as DateTime? ?? DateTime.MinValue,
                                ActFinishDate = row["ActFinishDate"] as DateTime? ?? DateTime.MinValue,
                                FcastFinishDate = row["FcastFinishDate"] as DateTime? ?? DateTime.MinValue,
                                DelComment = row["DelComment"]?.ToString(),
                                //Created = row["Created"] as DateTime? ?? DateTime.MinValue,
                                //Modified = row["Modified"] as DateTime? ?? DateTime.MinValue,
                                PlanStartDate = row["PlanStartDate"] as DateTime? ?? DateTime.MinValue,
                                DelRev = row["DelRev"]?.ToString(),
                                DelGp1 = row["DelGp1"]?.ToString(),
                                DelGp2 = row["DelGp2"]?.ToString(),
                                DelGp3 = row["DelGp3"]?.ToString(),
                                DelGp4 = row["DelGp4"]?.ToString()
                            });
                        }

                        return delListHist;
                    }
                }

                // Step 2: Get latest snapshot and copy it for this ReportWE
                SqlCommand latestDateCmd = new SqlCommand("SELECT MAX(ProgressDate) FROM tbDeliverableHist WHERE JobID = @JobID", con);
                latestDateCmd.Parameters.AddWithValue("@JobID", jobId);
                object maxDateObj = latestDateCmd.ExecuteScalar();

                if (maxDateObj != DBNull.Value)
                {
                    DateTime latestDate = Convert.ToDateTime(maxDateObj);

                    SqlCommand getLatestCmd = new SqlCommand("SELECT * FROM tbDeliverableHist WHERE JobID = @JobID AND ProgressDate = @LatestDate", con);
                    getLatestCmd.Parameters.AddWithValue("@JobID", jobId);
                    getLatestCmd.Parameters.AddWithValue("@LatestDate", latestDate);

                    using (SqlDataAdapter latestDa = new SqlDataAdapter(getLatestCmd))
                    {
                        DataTable latestDt = new DataTable();
                        latestDa.Fill(latestDt);

                        foreach (DataRow row in latestDt.Rows)
                        {
                            SqlCommand insertCmd = new SqlCommand(@"
                        INSERT INTO tbDeliverableHist
                        (DeliverableID, OBID, DelNum, DelName, DelHours,DelCost, DelPctCumul, DelEarnedHrs, DelEarnedCost, ProgressDate,  Direct, JobID, DirPct, PlanFinishDate, ActFinishDate, FcastFinishDate,  DelComment, Created, Modified, PlanStartDate, DelRev, DelGp1, DelGp2, DelGp3, DelGp4)
                        VALUES
                        (@DeliverableID, @OBID, @DelNum, @DelName, @DelHours,@DelCost, @DelPctCumul, @DelEarnedHrs, @DelEarnedCost, @ProgressDate,  @Direct, @JobID, @DirPct, @PlanFinishDate, @ActFinishDate, @FcastFinishDate,  @DelComment, @Created, @Modified, @PlanStartDate, @DelRev, @DelGp1, @DelGp2, @DelGp3, @DelGp4);
                        SELECT SCOPE_IDENTITY();", con);

                            insertCmd.Parameters.AddWithValue("@DeliverableID", row["DeliverableID"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@OBID", row["OBID"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelNum", row["DelNum"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelName", row["DelName"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelHours", row["DelHours"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelCost", row["DelCost"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelPctCumul", row["DelPctCumul"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelEarnedHrs", row["DelEarnedHrs"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelEarnedCost", row["DelEarnedCost"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@ProgressDate", reportWE);
                            insertCmd.Parameters.AddWithValue("@Direct", row["Direct"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@JobID", row["JobID"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DirPct", row["DirPct"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@PlanFinishDate", row["PlanFinishDate"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@ActFinishDate", row["ActFinishDate"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@FcastFinishDate", row["FcastFinishDate"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelComment", row["DelComment"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@Created", row["Created"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@Modified", row["Modified"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@PlanStartDate", row["PlanStartDate"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelRev", row["DelRev"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelGp1", row["DelGp1"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelGp2", row["DelGp2"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelGp3", row["DelGp3"] ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@DelGp4", row["DelGp4"] ?? DBNull.Value);

                            int newId = Convert.ToInt32(insertCmd.ExecuteScalar());

                            // Add to list if needed (optional)
                        }
                    }
                }
            }

            return delListHist;
        }

        public tbDeliverable GetDeliverableById(int deliverableId, IConfiguration configuration)
        {
            tbDeliverable deliverable = null;

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM tbDeliverableHist WHERE DeliverableID = @DeliverableID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@DeliverableID", deliverableId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    deliverable = new tbDeliverable
                    {
                        DeliverableID = reader.GetInt32(reader.GetOrdinal("DeliverableID")),
                        OBID = reader.GetInt32(reader.GetOrdinal("OBID")),
                        DelNum = reader.IsDBNull(reader.GetOrdinal("DelNum")) ? null : reader.GetString(reader.GetOrdinal("DelNum")),
                        DelName = reader.IsDBNull(reader.GetOrdinal("DelName")) ? null : reader.GetString(reader.GetOrdinal("DelName")),
                        DelHours = reader.IsDBNull(reader.GetOrdinal("DelHours")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DelHours")),
                        DelCost = reader.IsDBNull(reader.GetOrdinal("DelCost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DelCost")),
                        DelComment = reader.IsDBNull(reader.GetOrdinal("DelComment")) ? null : reader.GetString(reader.GetOrdinal("DelComment")),
                        PlanFinishDate = reader.IsDBNull(reader.GetOrdinal("PlanFinishDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("PlanFinishDate")),
                        Created = reader.IsDBNull(reader.GetOrdinal("Created")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Created")),
                        Modified = reader.IsDBNull(reader.GetOrdinal("Modified")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Modified")),
                        JobID = reader.GetInt32(reader.GetOrdinal("JobID")),
                        PlanStartDate = reader.IsDBNull(reader.GetOrdinal("PlanStartDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("PlanStartDate")),
                        DelRev = reader.IsDBNull(reader.GetOrdinal("DelRev")) ? null : reader.GetString(reader.GetOrdinal("DelRev")),
                        DelGp1 = reader.IsDBNull(reader.GetOrdinal("DelGp1")) ? null : reader.GetString(reader.GetOrdinal("DelGp1")),
                        DelGp2 = reader.IsDBNull(reader.GetOrdinal("DelGp2")) ? null : reader.GetString(reader.GetOrdinal("DelGp2")),
                        DelGp3 = reader.IsDBNull(reader.GetOrdinal("DelGp3")) ? null : reader.GetString(reader.GetOrdinal("DelGp3")),
                        DelGp4 = reader.IsDBNull(reader.GetOrdinal("DelGp4")) ? null : reader.GetString(reader.GetOrdinal("DelGp4")),
                    };
                }

                reader.Close();
            }

            return deliverable;
        }

        public int UpdateDeliverable(tbDeliverable d, IConfiguration configuration)
        {
            int rowsAffected = 0;

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string sql = @"
            UPDATE tbDeliverableHist
            SET 
                DelNum = @DelNum,
                DelName = @DelName,
                DelHours = @DelHours,
                DelCost = @DelCost,
                DelComment = @DelComment,
                PlanFinishDate = @PlanFinishDate,
                PlanStartDate = @PlanStartDate,
                DelRev = @DelRev,
                DelGp1 = @DelGp1,
                DelGp2 = @DelGp2,
                DelGp3 = @DelGp3,
                DelGp4 = @DelGp4,
                Modified = GETDATE()
            WHERE DeliverableID = @DeliverableID";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@DelNum", (object?)d.DelNum ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelName", (object?)d.DelName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelHours", d.DelHours);
                    cmd.Parameters.AddWithValue("@DelCost", d.DelCost);
                    cmd.Parameters.AddWithValue("@DelComment", (object?)d.DelComment ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PlanFinishDate", d.PlanFinishDate);
                    cmd.Parameters.AddWithValue("@PlanStartDate", d.PlanStartDate);
                    cmd.Parameters.AddWithValue("@DelRev", (object?)d.DelRev ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp1", (object?)d.DelGp1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp2", (object?)d.DelGp2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp3", (object?)d.DelGp3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DelGp4", (object?)d.DelGp4 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeliverableID", d.DeliverableID);

                    con.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public int UpdateThisDiscETCOneline(DiscETCDto d, IConfiguration configuration)
        {
            int rowsAffected = 0;

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string sql = @"
                -- 1. Update tbDiscETC
                UPDATE tbDiscETC
                SET 
                    JobID=@JobID,
                    RptWeekend=@RptWeekend,
                    ETCHrs = @ETCHrs,
                    ETCCost = @ETCCost,
                    EACHrs = @EACHrs,
                    EACCost = @EACCost,
                    ETCComment = @ETCComment,
                    PlanStartWE = @PlanStartWE,
                    PlanFinishWE = @PlanFinishWE,
                    CurveID = @CurveID,
                    ETCRate = @ETCRate,
                    ETCRateTypeID = @ETCRateTypeID

                WHERE DiscEtcID = @DiscEtcID;

                -- 2. Recalculate EAC_Info and FcastFinishDate for the job
                UPDATE tbProjectProgress
                SET 
                    EAC_Info = ISNULL((
                        SELECT SUM(EACCost) 
                        FROM tbDiscETC 
                        WHERE JobID = @JobID AND RptWeekend = @RptWeekend
                    ), 0),
                    FcastFinishDate = (
                        SELECT MAX(PlanFinishWE)
                        FROM tbDiscETC 
                        WHERE JobID = @JobID AND RptWeekend = @RptWeekend
                    )
                WHERE JobID = @JobID;
                ";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ETCHrs", SqlDbType.Decimal).Value = (object?)d.ETCHrs ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@ETCCost", SqlDbType.Decimal).Value = (object?)d.ETCCost ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@EACHrs", SqlDbType.Decimal).Value = (object?)d.EACHrs ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@EACCost", SqlDbType.Decimal).Value = (object?)d.EACCost ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@ETCComment", d.ETCComment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PlanStartWE", (object?)d.PlanStartWE ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PlanFinishWE", (object?)d.PlanFinishWE ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@CurveID", d.CurveID);
                    cmd.Parameters.AddWithValue("@ETCRate", (object?)d.ETCRate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ETCRateTypeID", d.ETCRateTypeID);

                    cmd.Parameters.AddWithValue("@DiscEtcID", d.DiscEtcID);
                    cmd.Parameters.AddWithValue("@JobID", d.JobID);
                    cmd.Parameters.AddWithValue("@RptWeekend", (object?)d.RptWeekend ?? DBNull.Value);

                    con.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        public List<tbClient> GetClientList(IConfiguration configuration)
        {
            var clients = new List<tbClient>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
            SELECT ClientID, ClientCode, ClientName, ClientAbb, ClientNameShort 
            FROM tbClient 
            ORDER BY ClientName";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new tbClient
                            {
                                ClientID = reader.GetInt32(reader.GetOrdinal("ClientID")),
                                ClientCode = reader.IsDBNull(reader.GetOrdinal("ClientCode")) ? null : reader.GetString(reader.GetOrdinal("ClientCode")),
                                ClientName = reader.IsDBNull(reader.GetOrdinal("ClientName")) ? null : reader.GetString(reader.GetOrdinal("ClientName")),
                                ClientAbb = reader.IsDBNull(reader.GetOrdinal("ClientAbb")) ? null : reader.GetString(reader.GetOrdinal("ClientAbb")),
                                ClientNameShort = reader.IsDBNull(reader.GetOrdinal("ClientNameShort")) ? null : reader.GetString(reader.GetOrdinal("ClientNameShort"))
                            });
                        }
                    }
                }
            }

            return clients;
        }

        public List<tbJob> GetTbJob(IConfiguration configuration)
        {
            var jobs = new List<tbJob>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            JobID, ClientID, JobName, BigTimeJobDisplayName, JobNum,
            JobStartDate, MgrID, JobFinishDate, TASKS, BillingStatus,
            ProjectValue, NewMexTaxAmt, RateSheetID, JobComments, AFE,
            ClientPM, CorpID, OTPctJob, IndustrySectorID, ProjectProgramID,
            RegionID, StreamID, ResourceStatus, SubClientID, ProjectTypeID,TASKS,Probability,BacklogStartDate
        FROM tbJob";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jobs.Add(new tbJob
                            {
                                JobID = reader.GetInt32(reader.GetOrdinal("JobID")), // assuming always NOT NULL
                                ClientID = reader.IsDBNull(reader.GetOrdinal("ClientID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ClientID")),
                                JobName = reader["JobName"]?.ToString(),
                                BigTimeJobDisplayName = reader["BigTimeJobDisplayName"]?.ToString(),
                                JobNum = reader["JobNum"]?.ToString(),
                                JobStartDate = reader["JobStartDate"] as DateTime?,
                                MgrID = reader.IsDBNull(reader.GetOrdinal("MgrID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("MgrID")),
                                JobFinishDate = reader["JobFinishDate"] as DateTime?,
                                TASKS = reader.IsDBNull(reader.GetOrdinal("TASKS")) ? false : reader.GetBoolean(reader.GetOrdinal("TASKS")),
                                BillingStatus = reader["BillingStatus"]?.ToString(),
                                ProjectValue = reader.IsDBNull(reader.GetOrdinal("ProjectValue")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("ProjectValue")),
                                //NewMexTaxAmt = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("NewMexTaxAmt"))),
                                NewMexTaxAmt = reader.IsDBNull(reader.GetOrdinal("NewMexTaxAmt")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("NewMexTaxAmt")),
                                RateSheetID = reader.IsDBNull(reader.GetOrdinal("RateSheetID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RateSheetID")),
                                JobComments = reader["JobComments"]?.ToString(),
                                AFE = reader["AFE"]?.ToString(),
                                ClientPM = reader["ClientPM"]?.ToString(),
                                CorpID = reader.IsDBNull(reader.GetOrdinal("CorpID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CorpID")),
                                //OTPctJob = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("OTPctJob"))),
                                OTPctJob = reader.IsDBNull(reader.GetOrdinal("OTPctJob")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("OTPctJob")),
                                IndustrySectorID = reader.IsDBNull(reader.GetOrdinal("IndustrySectorID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("IndustrySectorID")),
                                ProjectProgramID = reader.IsDBNull(reader.GetOrdinal("ProjectProgramID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ProjectProgramID")),
                                RegionID = reader.IsDBNull(reader.GetOrdinal("RegionID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("RegionID")),
                                StreamID = reader.IsDBNull(reader.GetOrdinal("StreamID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StreamID")),
                                ResourceStatus = reader["ResourceStatus"]?.ToString(),
                                SubClientID = reader.IsDBNull(reader.GetOrdinal("SubClientID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("SubClientID")),
                                ProjectTypeID = reader.IsDBNull(reader.GetOrdinal("ProjectTypeID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ProjectTypeID")),
                                Probability = reader.IsDBNull(reader.GetOrdinal("Probability")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Probability")),
                                BacklogStartDate = reader["BacklogStartDate"] as DateTime?
                                //MgrName = reader["MgrName"]?.ToString()
                            });
                        }
                    }
                }
            }

            return jobs;
        }

        public JobFormDto? GetJobById(int jobId, IConfiguration configuration)
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = "SELECT * FROM tbJob WHERE JobID = @JobID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@JobID", jobId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        return new JobFormDto
                        {
                            JobID = Convert.ToInt32(reader["JobID"]),
                            JobNum = reader["JobNum"] as string,
                            BigTimeJobDisplayName = reader["BigTimeJobDisplayName"] as string,
                            //JobStartDate = reader["JobStartDate"] as DateTime?,
                            //JobFinishDate = reader["JobFinishDate"] as DateTime?,
                            JobStartDate = reader["JobStartDate"] != DBNull.Value ? (DateTime?)reader["JobStartDate"] : null,
                            JobFinishDate = reader["JobFinishDate"] != DBNull.Value ? (DateTime?)reader["JobFinishDate"] : null,
                            ClientID = reader["ClientID"] as int?,
                            ClientPM = reader["ClientPM"] as string,
                            AFE = reader["AFE"] as string,
                            MgrID = reader["MgrID"] as int?,
                            BillingStatus = reader["BillingStatus"] as string,
                            RateSheetID = reader["RateSheetID"] as int?,
                            ProjectValue = reader["ProjectValue"] as decimal?,
                            NewMexTaxAmt = reader["NewMexTaxAmt"] as decimal?,
                            CorpID = reader["CorpID"] as int?,
                            IndustrySectorID = reader["IndustrySectorID"] as int?,
                            ProjectProgramID = reader["ProjectProgramID"] as int?,
                            RegionID = reader["RegionID"] as int?,
                            StreamID = reader["StreamID"] as int?,
                            ResourceStatus = reader["ResourceStatus"] as string,
                            SubClientID = reader["SubClientID"] as int?,
                            ProjectTypeID = reader["ProjectTypeID"] as int?,
                            JobComments = reader["JobComments"] as string,
                            OTPctJob = reader["OTPctJob"] as decimal?,
                            TASKS = reader["TASKS"] != DBNull.Value ? Convert.ToBoolean(reader["TASKS"]) : true,
                            Probability = reader["Probability"] as decimal?,
                            BacklogStartDate = reader["BacklogStartDate"] != DBNull.Value ? (DateTime?)reader["BacklogStartDate"] : null


                        };
                    }
                }
            }

            return null;
        }

        public List<tbAdminFee> GetAdminFee(int jobId, IConfiguration configuration)
        {
            List<tbAdminFee> list = new List<tbAdminFee>();
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM tbAdminFee WHERE JobID = @JobID", con);
                da.SelectCommand.Parameters.AddWithValue("@JobID", jobId);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    var record = new tbAdminFee
                    {
                        AdminFeeID = Convert.ToInt32(row["AdminFeeID"]),
                        JobID = Convert.ToInt32(row["JobID"]),
                        AdminFeeStart = Convert.ToDateTime(row["AdminFeeStart"]),
                        AdminFeeFinish = Convert.ToDateTime(row["AdminFeeFinish"]),
                        AdminFeeAmt = Convert.ToDecimal(row["AdminFeeAmt"])
                    };
                    list.Add(record);
                }
            }
            return list;
        }

        public void UpdateAdminFee(tbAdminFee fee, IConfiguration configuration)
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            using (SqlCommand cmd = new SqlCommand(@"UPDATE tbAdminFee
                                             SET AdminFeeStart = @Start,
                                                 AdminFeeFinish = @Finish,
                                                 AdminFeeAmt = @Amt
                                             WHERE AdminFeeID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Start", fee.AdminFeeStart);
                cmd.Parameters.AddWithValue("@Finish", fee.AdminFeeFinish);
                cmd.Parameters.AddWithValue("@Amt", fee.AdminFeeAmt);
                cmd.Parameters.AddWithValue("@ID", fee.AdminFeeID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAdminFee(int id, IConfiguration configuration)
        {
            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            using (SqlCommand cmd = new SqlCommand("DELETE FROM tbAdminFee WHERE AdminFeeID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public JobFormDto InsertJob(JobFormDto job, IConfiguration configuration)
        {
            JobFormDto newJob = new JobFormDto(); // The object to return
            string connectionString = configuration.GetConnectionString("DBCS");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand insertCmd = new SqlCommand(@"
            INSERT INTO tbJob
            (JobNum, BigTimeJobDisplayName, JobStartDate, JobFinishDate, ClientID, ClientPM,
             AFE, MgrID, BillingStatus, RateSheetID, ProjectValue, NewMexTaxAmt, CorpID, IndustrySectorID,
             ProjectProgramID, RegionID, StreamID, ResourceStatus, SubClientID, ProjectTypeID, JobComments, OTPctJob,Probability,BacklogStartDate)
            VALUES
            (@JobNum, @BigTimeJobDisplayName, @JobStartDate, @JobFinishDate, @ClientID, @ClientPM,
             @AFE, @MgrID, @BillingStatus, @RateSheetID, @ProjectValue, @NewMexTaxAmt, @CorpID, @IndustrySectorID,
             @ProjectProgramID, @RegionID, @StreamID, @ResourceStatus, @SubClientID, @ProjectTypeID, @JobComments, @OTPctJob,@Probability,@BacklogStartDate);
            SELECT SCOPE_IDENTITY();", con);

                insertCmd.Parameters.AddWithValue("@JobNum", (object?)job.JobNum ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@BigTimeJobDisplayName", (object?)job.BigTimeJobDisplayName ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@JobStartDate", (object?)job.JobStartDate ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@JobFinishDate", (object?)job.JobFinishDate ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ClientID", (object?)job.ClientID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ClientPM", (object?)job.ClientPM ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@AFE", (object?)job.AFE ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@MgrID", (object?)job.MgrID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@BillingStatus", (object?)job.BillingStatus ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@RateSheetID", (object?)job.RateSheetID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ProjectValue", (object?)job.ProjectValue ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@NewMexTaxAmt", (object?)job.NewMexTaxAmt ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@CorpID", (object?)job.CorpID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@IndustrySectorID", (object?)job.IndustrySectorID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ProjectProgramID", (object?)job.ProjectProgramID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@RegionID", (object?)job.RegionID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@StreamID", (object?)job.StreamID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ResourceStatus", (object?)job.ResourceStatus ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@SubClientID", (object?)job.SubClientID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@ProjectTypeID", (object?)job.ProjectTypeID ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@JobComments", (object?)job.JobComments ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@OTPctJob", (object?)job.OTPctJob ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@TASKS", (object?)job.TASKS ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Probability", (object?)job.Probability ?? DBNull.Value);
                insertCmd.Parameters.AddWithValue("@BacklogStartDate", (object?)job.BacklogStartDate ?? DBNull.Value);

                int newId = Convert.ToInt32(insertCmd.ExecuteScalar());

                newJob = job;
                newJob.JobID = newId;
            }

            return newJob;
        }

        public void EditJob(JobFormDto dto, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand updateCmd = new SqlCommand(@"
        UPDATE tbJob SET
            JobNum = @JobNum,
            BigTimeJobDisplayName = @BigTimeJobDisplayName,
            JobStartDate = @JobStartDate,
            JobFinishDate = @JobFinishDate,
            ClientID = @ClientID,
            ClientPM = @ClientPM,
            AFE = @AFE,
            MgrID = @MgrID,
            BillingStatus = @BillingStatus,
            RateSheetID = @RateSheetID,
            ProjectValue = @ProjectValue,
            NewMexTaxAmt = @NewMexTaxAmt,
            CorpID = @CorpID,
            IndustrySectorID = @IndustrySectorID,
            ProjectProgramID = @ProjectProgramID,
            RegionID = @RegionID,
            StreamID = @StreamID,
            ResourceStatus = @ResourceStatus,
            SubClientID = @SubClientID,
            ProjectTypeID = @ProjectTypeID,
            JobComments = @JobComments,
            OTPctJob = @OTPctJob,
            TASKS=@TASKS,
            Probability=@Probability,
            BacklogStartDate=@BacklogStartDate
        WHERE JobID = @JobID", con);

                updateCmd.Parameters.AddWithValue("@JobID", dto.JobID);
                updateCmd.Parameters.AddWithValue("@JobNum", (object?)dto.JobNum ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@BigTimeJobDisplayName", (object?)dto.BigTimeJobDisplayName ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@JobStartDate", (object?)dto.JobStartDate ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@JobFinishDate", (object?)dto.JobFinishDate ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ClientID", (object?)dto.ClientID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ClientPM", (object?)dto.ClientPM ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@AFE", (object?)dto.AFE ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@MgrID", (object?)dto.MgrID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@BillingStatus", (object?)dto.BillingStatus ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@RateSheetID", (object?)dto.RateSheetID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ProjectValue", (object?)dto.ProjectValue ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@NewMexTaxAmt", (object?)dto.NewMexTaxAmt ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@CorpID", (object?)dto.CorpID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@IndustrySectorID", (object?)dto.IndustrySectorID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ProjectProgramID", (object?)dto.ProjectProgramID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@RegionID", (object?)dto.RegionID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@StreamID", (object?)dto.StreamID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ResourceStatus", (object?)dto.ResourceStatus ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@SubClientID", (object?)dto.SubClientID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@ProjectTypeID", (object?)dto.ProjectTypeID ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@JobComments", (object?)dto.JobComments ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@OTPctJob", (object?)dto.OTPctJob ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@TASKS", (object?)dto.TASKS ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@Probability", (object?)dto.Probability ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@BacklogStartDate", (object?)dto.BacklogStartDate ?? DBNull.Value);

                updateCmd.ExecuteNonQuery();
            }
        }

        public decimal? GetAdminFeeAmtForRateSheet(int? rateSheetId,IConfiguration configuration)
        {
            if (rateSheetId == null) return null;

            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 Rate
                FROM tbratesheetrate
                WHERE ClassID = 55 AND RateSheetID = @RateSheetID;", conn))
            {
                cmd.Parameters.AddWithValue("@RateSheetID", rateSheetId);
                conn.Open();

                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? null : (decimal?)result;
            }
        }

        //public void InsertDefaultOBID(int jobId,IConfiguration configuration)
        //{
        //    string connectionString = configuration.GetConnectionString("DBCS");

        //    using (var conn = new SqlConnection(connectionString))
        //    using (var cmd = new SqlCommand(@"
        //        INSERT INTO tbOB (JobID, TaskID)
        //        SELECT @JobID,TaskID
        //        FROM tbTask
        //        where TaskID IN (21,22,1270,1272);", conn))
        //    {
        //        cmd.Parameters.AddWithValue("@JobID", jobId);

        //        conn.Open();
        //        cmd.ExecuteNonQuery();
        //    }
        //}

        public void InsertDefaultOBID(int jobId, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                -- First insert
                    INSERT INTO tbOB (JobID, TaskID)
                    SELECT @JobID, TaskID
                    FROM tbTask
                    WHERE TaskID IN (21,22,1270,1272);

                -- Second insert
                    INSERT INTO tbOB (JobID, TaskID, MapPM, MapEngr, MapDesign, MapEIC)
                    VALUES
                    (@JobID, 3, 1, 0, 0, 0),
                    (@JobID, 1209, 0, 1, 0, 0),
                    (@JobID, 1210, 0, 0, 1, 0),
                    (@JobID, 1211, 0, 0, 0, 1);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public void InsertAdminFee(int jobId, DateTime start, DateTime finish, decimal? amt,IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tbAdminFee (JobID, AdminFeeStart, AdminFeeFinish, AdminFeeAmt)
                VALUES (@JobID, @Start, @Finish, @Amt);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@Finish", finish);
                cmd.Parameters.AddWithValue("@Amt", (object?)amt ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertDiscount(int jobId, DateTime start, DateTime finish,IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tbDiscount (JobID, DiscountStart, DiscountFinish, DiscountAmt, BaseLaborOnly)
                VALUES (@JobID, @Start, @Finish, @Amt,@BaseLaborOnly);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@Finish", finish);
                cmd.Parameters.AddWithValue("@Amt", 0);
                cmd.Parameters.AddWithValue("@BaseLaborOnly", 0);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertProjProg(int jobId, DateTime currWE, DateTime ThisJobFinishDate,IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tbProjectProgress (JobID, WeekEnd, Status,FcastFinishDate,Created,Modified)
                VALUES (@JobID, @CurrWE, @Status,@ThisJobFinishDate,@Created,@Modified);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@CurrWE", currWE);
                cmd.Parameters.AddWithValue("@Status", "Actual");
                cmd.Parameters.AddWithValue("@ThisJobFinishDate", ThisJobFinishDate);

                var now = DateTime.Now; // or DateTime.UtcNow if you want UTC time
                cmd.Parameters.AddWithValue("@Created", now);
                cmd.Parameters.AddWithValue("@Modified", now);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertOT(int jobId, DateTime start, DateTime finish,IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tbOTAllowed (JobID, OTAllowStart, OTAllowFinish, OTAllowedYN)
                VALUES (@JobID, @Start, @Finish, @Amt);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@Finish", finish);
                cmd.Parameters.AddWithValue("@Amt", 1);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void InsertNavNat(int jobId, DateTime start, DateTime finish,IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBCS");

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
                INSERT INTO tbNavajoNationFee (JobID, NavajoNationStart, NavajoNationFinish, NavajoNationAmt)
                VALUES (@JobID, @Start, @Finish, @Amt);", conn))
            {
                cmd.Parameters.AddWithValue("@JobID", jobId);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@Finish", finish);
                cmd.Parameters.AddWithValue("@Amt", 0);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<tbEmp> GetEmpList(IConfiguration configuration)
        {
            var emps = new List<tbEmp>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            EmpID, EmpFirstName, EmpLastName, EmpName, EmpStatus,
            EmpMgr, EmpGroupID, LaborGroup, LaborCodeID, DivCodeID,
            TegreOpsID, LaborGroupWithLeader
        FROM tbEmp
        where empstatus='active'
        ";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emps.Add(new tbEmp
                            {
                                EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")), // required
                                EmpFirstName = reader.IsDBNull(reader.GetOrdinal("EmpFirstName")) ? null : reader.GetString(reader.GetOrdinal("EmpFirstName")),
                                EmpLastName = reader.IsDBNull(reader.GetOrdinal("EmpLastName")) ? null : reader.GetString(reader.GetOrdinal("EmpLastName")),
                                EmpName = reader.IsDBNull(reader.GetOrdinal("EmpName")) ? null : reader.GetString(reader.GetOrdinal("EmpName")),
                                EmpStatus = reader.IsDBNull(reader.GetOrdinal("EmpStatus")) ? null : reader.GetString(reader.GetOrdinal("EmpStatus")),
                                EmpMgr = reader.IsDBNull(reader.GetOrdinal("EmpMgr")) ? null : reader.GetInt32(reader.GetOrdinal("EmpMgr")),
                                EmpGroupID = reader.IsDBNull(reader.GetOrdinal("EmpGroupID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("EmpGroupID")),
                                LaborGroup = reader.IsDBNull(reader.GetOrdinal("LaborGroup")) ? null : reader.GetString(reader.GetOrdinal("LaborGroup")),
                                LaborCodeID = reader.IsDBNull(reader.GetOrdinal("LaborCodeID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("LaborCodeID")),
                                LaborGroupWithLeader = reader.IsDBNull(reader.GetOrdinal("LaborGroupWithLeader")) ? null : reader.GetString(reader.GetOrdinal("LaborGroupWithLeader"))
                            });
                        }
                    }
                }
            }

            return emps;
        }

        public List<tbRateSheets> GetRateSheetList(IConfiguration configuration)
        {
            var rateSheets = new List<tbRateSheets>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            RateSheetID, RateSheetName, Active
        FROM tbRateSheets where Active=1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rateSheets.Add(new tbRateSheets
                            {
                                RateSheetID = reader.GetInt32(reader.GetOrdinal("RateSheetID")),
                                RateSheetName = reader["RateSheetName"]?.ToString(),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active"))
                            });
                        }
                    }
                }
            }

            return rateSheets;
        }

        public List<tbCorp> GetCorpList(IConfiguration configuration)
        {
            var corps = new List<tbCorp>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            CorpID, CorpName
        FROM tbCorp";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            corps.Add(new tbCorp
                            {
                                CorpID = reader.GetInt32(reader.GetOrdinal("CorpID")),
                                CorpName = reader["CorpName"]?.ToString()
                            });
                        }
                    }
                }
            }

            return corps;
        }

        public List<tbIndustrySector> GetIndSectorList(IConfiguration configuration)
        {
            var indSector = new List<tbIndustrySector>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            IndustrySectorID, IndustrySector
        FROM tbIndustrySector";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            indSector.Add(new tbIndustrySector
                            {
                                IndustrySectorID = reader.GetInt32(reader.GetOrdinal("IndustrySectorID")),
                                IndustrySector = reader["IndustrySector"]?.ToString()
                            });
                        }
                    }
                }
            }

            return indSector;
        }

        public List<tbSubClient> GetSubClientList(IConfiguration configuration)
        {
            var subClient = new List<tbSubClient>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            SubClientID, SubClientName
        FROM tbSubClient";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subClient.Add(new tbSubClient
                            {
                                SubClientID = reader.GetInt32(reader.GetOrdinal("SubClientID")),
                                SubClientName = reader["SubClientName"]?.ToString()
                            });
                        }
                    }
                }
            }

            return subClient;
        }

        public List<tbStream> GetStreamList(IConfiguration configuration)
        {
            var stream = new List<tbStream>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            StreamID, StreamDesc
        FROM tbStream";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stream.Add(new tbStream
                            {
                                StreamID = reader.GetInt32(reader.GetOrdinal("StreamID")),
                                StreamDesc = reader["StreamDesc"]?.ToString()
                            });
                        }
                    }
                }
            }

            return stream;
        }

        public List<tbRegion> GetRegionList(IConfiguration configuration)
        {
            var region = new List<tbRegion>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            RegionID, RegionDesc
        FROM tbRegion";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            region.Add(new tbRegion
                            {
                                RegionID = reader.GetInt32(reader.GetOrdinal("RegionID")),
                                RegionDesc = reader["RegionDesc"]?.ToString()
                            });
                        }
                    }
                }
            }

            return region;
        }

        public List<tbProjectType> GetProjTypeList(IConfiguration configuration)
        {
            var projType = new List<tbProjectType>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
        SELECT 
            ProjectTypeID, ProjectTypeDesc
        FROM tbProjectType";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projType.Add(new tbProjectType
                            {
                                ProjectTypeID = reader.GetInt32(reader.GetOrdinal("ProjectTypeID")),
                                ProjectTypeDesc = reader["ProjectTypeDesc"]?.ToString()
                            });
                        }
                    }
                }
            }

            return projType;
        }

        public Dictionary<int, decimal?> GetProjectProgressForCurrentWeek(IConfiguration configuration, DateTime rptWE)
        {
            var dict = new Dictionary<int, decimal?>();

            using (SqlConnection con = new SqlConnection(configuration.GetConnectionString("DBCS")))
            {
                string query = @"
            SELECT JobID, CumulPeriodProgress
            FROM tbProjectProgress
            WHERE WeekEnd = @RptWeekend";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RptWeekend", rptWE);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int jobId = reader.GetInt32(reader.GetOrdinal("JobID"));

                            decimal? progress = reader.IsDBNull(reader.GetOrdinal("CumulPeriodProgress"))
                                ? (decimal?)null
                                : reader.GetDecimal(reader.GetOrdinal("CumulPeriodProgress"));

                            dict[jobId] = progress;
                        }
                    }
                }
            }

            return dict;
        }

        public List<NewJobsByMonthDto> GetNewJobsByMonthRolling(IConfiguration config)
        {
            var result = new List<NewJobsByMonthDto>();

            using (var conn = new SqlConnection(config.GetConnectionString("DBCS")))
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT 
                myYearMonth, 
                SUM(MonthlyOrigAmt) as MonthlyOrigAmt,
                SUM(MonthlyChangeAmt) as MonthlyChangeAmt,
                SUM(MonthlyTarget) as MonthlyTarget,
                YearMonthDate
            FROM vwNewJobsByMonth
            GROUP BY myYearMonth,YearMonthDate
            ORDER BY myYearMonth", conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            //string ym = rdr["YearMonthDate"].ToString();

                            //var parsed = rdr.GetDateTime(rdr.GetOrdinal("YearMonthDate"));

                            result.Add(new NewJobsByMonthDto
                            {
                                myYearMonth = rdr.GetString(rdr.GetOrdinal("myYearMonth")),
                                MonthlyOrigAmt = rdr.GetDecimal(rdr.GetOrdinal("MonthlyOrigAmt")),
                                MonthlyChangeAmt = rdr.GetDecimal(rdr.GetOrdinal("MonthlyChangeAmt")),
                                MonthlyTarget = rdr.GetDecimal(rdr.GetOrdinal("MonthlyTarget")),
                                YearMonthDate=rdr.GetDateTime(rdr.GetOrdinal("YearMonthDate"))
                            });
                        }
                    }
                }
            }

            return result;
        }






    }
}
