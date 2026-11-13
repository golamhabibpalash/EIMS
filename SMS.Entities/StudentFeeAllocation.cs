namespace SMS.Entities
{
    public class StudentFeeAllocation : CommonProps
    {
        public int StudentId { get; set; }
        public int StudentFeeHeadId { get; set; }
        public double AllocatedAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public string FeeAllocationApplication { get; set; } = string.Empty;
        public string UniqueId { get; set; }
        public int? ClassFeeListId { get; set; }

        //Navigations

        public StudentFeeHead StudentFeeHead { get; set; }
        public Student Student { get; set; }
        public ClassFeeList ClassFeeList { get; set; }
    }
}
