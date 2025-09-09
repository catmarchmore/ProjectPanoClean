namespace ProjectPano.Model
{
    public class tbEmp
    {
        public int EmpID { get; set; }
        public string? EmpFirstName { get; set; }
        public string? EmpLastName { get; set; }
        public string? EmpName { get; set; }
        public string? EmpStatus { get; set; }
        public int? EmpMgr { get; set; }
        public int? EmpGroupID { get; set; }
        public string? LaborGroup { get; set; }
        public int? LaborCodeID { get; set; }
        public int? DivCodeID { get; set; }  
        public int? TegreOpsID { get; set; }
        public string? LaborGroupWithLeader { get; set; }
    }
}
