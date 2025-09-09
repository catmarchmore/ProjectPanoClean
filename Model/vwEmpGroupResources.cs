using Microsoft.Graph.Models;

namespace ProjectPano.Model
{
    public class vwEmpGroupResources
    {
        //this is list of all available emps, resource grps, leads, target levels
        public int EmpGroupID { get; set; }
        public string EmpGroup { get; set; }
        public int EmpResGpID { get; set; }
        public string EmpName { get; set; }
        public string EmpResGroupLead { get; set; }
        public string EmpResGroupDesc { get; set; }
        public decimal WklyBillable { get; set; }
        public decimal WklyBillableOH { get; set; }
        public decimal WklyBillableOHOT { get;set;}
        public int EmpID { get; set; }

    }
}
