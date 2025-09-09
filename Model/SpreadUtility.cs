using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ProjectPano.Model.DAL;
using static System.Reflection.Metadata.BlobBuilder;

namespace ProjectPano.Model
{

    public static class SpreadUtility
    {
        public static List<ETCSpreadChartDto>  GetSpread(
            List<vwDiscETC> etcRecords,
            List<vwCurves> curveList,
            List<vwActuals_byProject_byWeek> actuals)
        {
            Dictionary<string, ETCSpreadChartDto> spreadDict = new();

            // Group curve data
            var groupedCurves = curveList
                .GroupBy(c => c.CurveID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(c => c.CurveSectionNum)
                          .Select(c => c.CurveSectionPct)
                          .ToList()
                );

            var sw = Stopwatch.StartNew();

            string MakeETCKey(DateTime we, string desc, string lead, string resStatus, string type, string client, string jobName, string stackedClient, decimal? wboh, decimal? wbohot, decimal? twboh, decimal? twbohot, int jobId)
            {
                return $"{we:yyyyMMdd}|{desc}|{lead}|{resStatus}|{type}|{client}|{jobName}|{stackedClient}|{wboh}|{wbohot}|{twboh}|{twbohot}|{jobId}";
            }

            // =============================
            // 🔹 Build capacity lookup by EmpResGroupDesc
            // =============================
            var etcCapacityByGroup = etcRecords
                .GroupBy(e => e.EmpResGroupDesc ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.First() // any ETC row is fine, OH/OHOT are the same
                );

            // =============================
            // 🔹 Process ETC
            // =============================

            foreach (var etc in etcRecords)
            {
                int durationWeeks = (int)((etc.PlanFinishWE - etc.PlanStartWE).TotalDays / 7) + 1;
                if (durationWeeks <= 0 || etc.ETCHrs <= 0) continue;
                if (!groupedCurves.TryGetValue(etc.CurveID, out var curvePcts)) continue;
                if (curvePcts.Count != 5) continue;

                // Step 1: Interpolate curve to match durationWeeks
                var scaledCurve = new List<decimal>();
                decimal totalPct = curvePcts.Sum();
                decimal accumulated = 0;

                for (int i = 0; i < durationWeeks; i++)
                {
                    decimal relativePos = (decimal)i / durationWeeks * 5;
                    int lower = (int)Math.Floor(relativePos);
                    int upper = Math.Min(lower + 1, 4);
                    decimal frac = relativePos - lower;
                    decimal value = (1 - frac) * curvePcts[lower] + frac * curvePcts[upper];
                    scaledCurve.Add(value);
                    accumulated += value;
                }

                // Step 2: Normalize to ensure percentages sum to 1
                if (accumulated > 0)
                {
                    scaledCurve = scaledCurve.Select(v => v / accumulated).ToList();
                }

                // Step 3: Apply to ETC hours and distribute across weeks
                for (int i = 0; i < durationWeeks; i++)
                {
                    DateTime weekEnding = GetWE.GetWeekEnding(etc.PlanStartWE.AddDays(i * 7));
                    decimal weeklyHours = etc.ETCHrs * scaledCurve[i];

                    string key = MakeETCKey(
                        weekEnding, etc.EmpResGroupDesc, etc.EmpResGroupLead,
                        etc.ResourceStatus, "ETC", etc.ClientNameShort, etc.JobName, etc.StackedAreaClient,
                        etc.WklyBillableOH, etc.WklyBillableOHOT, etc.TotalWklyBillableOH, etc.TotalWklyBillableOHOT, etc.JobID);


                    if (spreadDict.TryGetValue(key, out var existing))
                    {
                        existing.SpreadHrs += weeklyHours;
                    }
                    else
                    {
                        spreadDict[key] = new ETCSpreadChartDto
                        {
                            EmpResGroupDesc = etc.EmpResGroupDesc,
                            ResourceStatus = etc.ResourceStatus,
                            ActualETC = "ETC",
                            WklyBillableOH = etc.WklyBillableOH,
                            WklyBillableOHOT = etc.WklyBillableOHOT,
                            TotalWklyBillableOH = etc.TotalWklyBillableOH,
                            TotalWklyBillableOHOT = etc.TotalWklyBillableOHOT,
                            WeekEnding = weekEnding,
                            SpreadHrs = weeklyHours
                        };
                    }
                }
            }

            // Fast actuals processing using a short dictionary key
            Dictionary<string, ETCSpreadChartDto> actualsDict = new();

            foreach (var act in actuals)
            {
                //string key = $"{act.WeekEnd:yyyyMMdd}|{act.EmpGroupID}|{act.EmpResGroupDesc}|{act.JobID}|Actual";
                string key = $"{act.WeekEnd:yyyyMMdd}|{act.EmpResGroupDesc}|{act.JobID}|Actual";

                // lookup OH/OHOT capacity by group
                etcCapacityByGroup.TryGetValue(act.EmpResGroupDesc ?? "Unknown", out var cap);
                //decimal totalOH = cap?.TotalWklyBillableOH ?? 0;
                //decimal totalOHOT = cap?.TotalWklyBillableOHOT ?? 0;


                if (actualsDict.TryGetValue(key, out var existing))
                {
                    existing.SpreadHrs += act.BillQty;
                }
                else
                {
                    actualsDict[key] = new ETCSpreadChartDto
                    {
                        EmpResGroupDesc = act.EmpResGroupDesc ?? "Unknown",
                        WeekEnding = act.WeekEnd,
                        SpreadHrs = act.BillQty,
                        ResourceStatus = "Backlog",
                        ActualETC = "Actual"
                        //TotalWklyBillableOH = totalOH,
                        //TotalWklyBillableOHOT = totalOHOT
                    };
                }
            }

            // Merge actualsDict into main spreadDict
            foreach (var kvp in actualsDict)
            {
                if (spreadDict.TryGetValue(kvp.Key, out var existing))
                {
                    existing.SpreadHrs += kvp.Value.SpreadHrs;
                    //existing.TotalWklyBillableOH = kvp.Value.TotalWklyBillableOH;
                    //existing.TotalWklyBillableOHOT = kvp.Value.TotalWklyBillableOHOT;
                }
                else
                {
                    spreadDict[kvp.Key] = kvp.Value;
                }
            }

            sw.Stop();
            Console.WriteLine($"SpreadUtility.GetSpread took {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total SpreadWeeks: {spreadDict.Count}");

            var allRows = spreadDict.Values
                .GroupBy(r => r.WeekEnding)
                .Select(g => new ETCSpreadChartDto
                {
                    EmpResGroupDesc = "ALL",
                    ResourceStatus = "Capacity",
                    ActualETC = "Capacity",
                    WeekEnding = g.Key,
                    SpreadHrs = 0
                    //WklyBillableOH = g.Sum(x => x.WklyBillableOH ?? 0),
                    //WklyBillableOHOT = g.Sum(x => x.WklyBillableOHOT ?? 0),
                    //TotalWklyBillableOH = g.Sum(x => x.TotalWklyBillableOH ?? 0),
                    //TotalWklyBillableOHOT = g.Sum(x => x.TotalWklyBillableOHOT ?? 0)
                });


            return spreadDict.Values
                .OrderBy(r => r.WeekEnding)
                .ThenBy(r => r.EmpResGroupDesc)
                .ToList();

            //var finalList = spreadDict.Values
            //    .Concat(allRows)
            //    .OrderBy(r => r.WeekEnding)
            //    .ThenBy(r => r.EmpResGroupDesc)
            //    .ToList();

            //var originalList = spreadDict.Values
            //    .OrderBy(r => r.WeekEnding)
            //    .ThenBy(r => r.EmpResGroupDesc)
            //    .ToList();

            //return (finalList, originalList);
        }

    }

    public static class StackedAreaUtility
    {
        public static List<ETCSpreadStackedAreaDto> GetSpread(
            List<vwDiscETC> etcRecords,
            List<vwCurves> curveList,
            List<vwActuals_byProject_byWeek> actuals,
            List<tbJob> tbJobs)
        {
            Dictionary<string, ETCSpreadStackedAreaDto> spreadDict = new();

            var jobProbabilities = tbJobs
                .Where(j => j.JobID != 0)
                .ToDictionary(j => j.JobID, j => j.Probability ?? 1m);  // fallback to 1 if null

            var groupedCurves = curveList
                .GroupBy(c => c.CurveID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(c => c.CurveSectionNum)
                          .Select(c => c.CurveSectionPct)
                          .ToList()
                );

            var sw = Stopwatch.StartNew();

            string MakeETCKey(DateTime we, string stackedClient, string actualEtc, string resStatus)
            {
                return $"{we:yyyyMMdd}|{stackedClient}|{actualEtc}|{resStatus}";
            }

            foreach (var etc in etcRecords)
            {
                int durationWeeks = (int)((etc.PlanFinishWE - etc.PlanStartWE).TotalDays / 7) + 1;
                if (durationWeeks <= 0 || etc.ETCHrs <= 0) continue;
                if (!groupedCurves.TryGetValue(etc.CurveID, out var curvePcts)) continue;
                if (curvePcts.Count != 5) continue;

                decimal? probability = jobProbabilities.TryGetValue(etc.JobID, out var prob) ? prob : (decimal?)null;

                var scaledCurve = new List<decimal>();
                decimal totalPct = curvePcts.Sum();
                decimal accumulated = 0;

                for (int i = 0; i < durationWeeks; i++)
                {
                    decimal relativePos = (decimal)i / durationWeeks * 5;
                    int lower = (int)Math.Floor(relativePos);
                    int upper = Math.Min(lower + 1, 4);
                    decimal frac = relativePos - lower;
                    decimal value = (1 - frac) * curvePcts[lower] + frac * curvePcts[upper];
                    scaledCurve.Add(value);
                    accumulated += value;
                }

                if (accumulated > 0)
                {
                    scaledCurve = scaledCurve.Select(v => v / accumulated).ToList();
                }

                for (int i = 0; i < durationWeeks; i++)
                {
                    DateTime weekEnding = GetWE.GetWeekEnding(etc.PlanStartWE.AddDays(i * 7));
                    decimal weeklyHours = etc.ETCHrs * scaledCurve[i];
                    decimal? spreadHrsProb = probability.HasValue ? weeklyHours * probability.Value : (decimal?)null;

                    string key = MakeETCKey(
                        weekEnding,
                        etc.StackedAreaClient,
                        "ETC",
                        etc.ResourceStatus);

                    if (spreadDict.TryGetValue(key, out var existing))
                    {
                        existing.SpreadHrs += weeklyHours;
                        existing.SpreadHrsProb += spreadHrsProb ?? 0;
                    }
                    else
                    {
                        spreadDict[key] = new ETCSpreadStackedAreaDto
                        {
                            StackedAreaClient = etc.StackedAreaClient,
                            ResourceStatus = etc.ResourceStatus,
                            ActualETC = "ETC",
                            WklyBillableOH = etc.WklyBillableOH,
                            WklyBillableOHOT = etc.WklyBillableOHOT,
                            TotalWklyBillableOH = etc.TotalWklyBillableOH,
                            TotalWklyBillableOHOT = etc.TotalWklyBillableOHOT,
                            WeekEnding = weekEnding,
                            SpreadHrs = weeklyHours,
                            SpreadHrsProb= spreadHrsProb,
                            Probability = etc.Probability
                        };
                    }
                }
            }

            // Build a capacity lookup per client from any ETC record (all values are valid)
            var etcCapacityByClient = etcRecords
                .GroupBy(e => e.StackedAreaClient)
                .ToDictionary(
                    g => g.Key,
                    g => g.First() // pick any ETC record, since OH and OHOT are the same for all weeks
                );

            foreach (var act in actuals)
            {
                string client = act.StackedAreaClient ?? "Unknown";
                string key = MakeETCKey(act.WeekEnd, client, "Actual", "Backlog");

                etcCapacityByClient.TryGetValue(client, out var cap);

                decimal totalOH = cap?.TotalWklyBillableOH ?? 0;
                decimal totalOHOT = cap?.TotalWklyBillableOHOT ?? 0;

                if (spreadDict.TryGetValue(key, out var existing) && existing.ActualETC == "Actual")
                {
                    // ✅ Only merge with an existing Actual record
                    existing.SpreadHrs += act.BillQty;
                    existing.SpreadHrsProb += act.BillQty;   // Always = SpreadHrs
                    existing.TotalWklyBillableOH = totalOH;
                    existing.TotalWklyBillableOHOT = totalOHOT;
                }
                else
                {
                    // ✅ Always create a fresh Actual record
                    spreadDict[key] = new ETCSpreadStackedAreaDto
                    {
                        StackedAreaClient = client,
                        WeekEnding = act.WeekEnd,
                        SpreadHrs = act.BillQty,
                        SpreadHrsProb = act.BillQty, // Probability always = 1 for actuals
                        ResourceStatus = "Backlog",
                        ActualETC = "Actual",
                        TotalWklyBillableOH = totalOH,
                        TotalWklyBillableOHOT = totalOHOT,
                        Probability = 1
                    };
                }
            }

            sw.Stop();
            Console.WriteLine($"StackedAreaUtility.GetSpread took {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total SpreadWeeks: {spreadDict.Count}");

            return spreadDict.Values
                .OrderBy(r => r.WeekEnding)
                .ThenBy(r => r.StackedAreaClient)
                .ToList();
        }
    }

    public static class SpreadDebugUtility
    {
        public static List<ETCSpreadCheck> GetSpreadDebug(
            List<vwDiscETC> etcRecords,
            List<vwCurves> curveList,
            List<vwActuals_byProject_byWeek> actuals,
            List<tbJob> tbJobs)
        {
            List<ETCSpreadCheck> result = new();

            var groupedCurves = curveList
                .GroupBy(c => c.CurveID)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(c => c.CurveSectionNum)
                          .Select(c => c.CurveSectionPct)
                          .ToList()
                );

            var jobProbabilities = tbJobs
                .Where(j => j.JobID != 0)
                .ToDictionary(j => j.JobID, j => j.Probability ?? 1m);  // fallback to 1 if null

            var sw = Stopwatch.StartNew();

            foreach (var etc in etcRecords)
            {
                int durationWeeks = (int)((etc.PlanFinishWE - etc.PlanStartWE).TotalDays / 7) + 1;
                if (durationWeeks <= 0 || etc.ETCHrs <= 0) continue;
                if (!groupedCurves.TryGetValue(etc.CurveID, out var curvePcts)) continue;
                if (curvePcts.Count != 5) continue;

                decimal? probability = jobProbabilities.TryGetValue(etc.JobID, out var prob) ? prob : (decimal?)null;

                var scaledCurve = new List<decimal>();
                decimal totalPct = curvePcts.Sum();
                decimal accumulated = 0;

                for (int i = 0; i < durationWeeks; i++)
                {
                    decimal relativePos = (decimal)i / durationWeeks * 5;
                    int lower = (int)Math.Floor(relativePos);
                    int upper = Math.Min(lower + 1, 4);
                    decimal frac = relativePos - lower;
                    decimal value = (1 - frac) * curvePcts[lower] + frac * curvePcts[upper];
                    scaledCurve.Add(value);
                    accumulated += value;
                }

                if (accumulated > 0)
                {
                    scaledCurve = scaledCurve.Select(v => v / accumulated).ToList();
                }

                for (int i = 0; i < durationWeeks; i++)
                {
                    DateTime weekEnding = GetWE.GetWeekEnding(etc.PlanStartWE.AddDays(i * 7));
                    decimal weeklyHours = etc.ETCHrs * scaledCurve[i];
                    decimal? spreadHrsProb = probability.HasValue ? weeklyHours * probability.Value : (decimal?)null;

                    result.Add(new ETCSpreadCheck
                    {
                        WeekEnding = weekEnding,
                        EmpResGroupDesc = etc.EmpResGroupDesc,
                        ResourceStatus = etc.ResourceStatus,
                        ActualETC = "ETC",
                        SpreadHrs = weeklyHours,
                        SpreadHrsProb=spreadHrsProb,
                        WklyBillableOH = etc.WklyBillableOH,
                        WklyBillableOHOT = etc.WklyBillableOHOT,
                        TotalWklyBillableOH = etc.TotalWklyBillableOH,
                        TotalWklyBillableOHOT = etc.TotalWklyBillableOHOT,
                        JobID = etc.JobID,
                        JobName=etc.JobName,
                        ClientNameShort = etc.ClientNameShort,
                        Probability= probability
                    });
                }
            }

            foreach (var act in actuals)
            {
                result.Add(new ETCSpreadCheck
                {
                    WeekEnding = act.WeekEnd,
                    EmpResGroupDesc = act.EmpResGroupDesc ?? "Unknown",
                    ResourceStatus = "Backlog",
                    ActualETC = "Actual",
                    SpreadHrs = act.BillQty,
                    SpreadHrsProb=act.BillQty,
                    JobID = act.JobID,
                    JobName=act.JobName,
                    ClientNameShort=act.ClientNameShort,
                    Probability= 1
                });
            }

            sw.Stop();
            Console.WriteLine($"SpreadDebugUtility.GetSpreadDebug took {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total SpreadRows: {result.Count}");

            return result.OrderBy(r => r.WeekEnding).ThenBy(r => r.EmpResGroupDesc).ToList();
        }
    }
}