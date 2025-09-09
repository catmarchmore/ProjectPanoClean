using Microsoft.Graph.Models;

namespace ProjectPano.Model
{
    public class ResourceDetailGroups
    {
        //this is list of all available emps, resource grps, leads, target levels
        public int EmpGroupID { get; set; }
        public string EmpGroup { get; set; }
        public string EmpResGroupLead { get; set; }
        public string EmpResGroupDesc { get; set; }


    }
}
