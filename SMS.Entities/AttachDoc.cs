namespace SMS.Entities;

public class AttachDoc:CommonProps
{
    public string DocumentsName { get; set; }
    public string Image { get; set; }
    public int? StudentId { get; set; }
    public Student Student { get; set; }

    public int? EmployeeId { get; set; }
    public Employee Employee { get; set; }
    public int AttachDocTypeId { get; set; }
    public AttachDocType AttachDocType { get; set; }
}
