namespace SMS.Entities
{
    public class StudentPaymentDetails : CommonProps
    {
        public int StudentPaymentId { get; set; }
        public int StudentFeeHeadId { get; set; }
        public double PaidAmount { get; set; }
        public int ClassFeeId { get; set; } = 0;
        public StudentFeeHead StudentFeeHead { get; set; }
        public StudentPayment StudentPayment { get; set; }

    }
}
