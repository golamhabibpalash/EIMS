namespace SMS.Entities
{
    public class ClassRoom : CommonProps
    {
        public int RoomNo { get; set; }
        public int SeatCapacity { get; set; }
        public bool IsLab { get; set; } = false;
    }
}
