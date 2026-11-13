using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    /// <summary>
    /// Where a ticket sits in the developer's workflow.
    /// </summary>
    public enum TicketStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Working")]
        Working = 2,

        [Display(Name = "Solved")]
        Solved = 3,

        [Display(Name = "Rejected")]
        Rejected = 4
    }

    /// <summary>
    /// What the user is bringing to the developer - a fault, an idea, a question.
    /// </summary>
    public enum TicketType
    {
        [Display(Name = "Issue")]
        Issue = 1,

        [Display(Name = "Plan")]
        Plan = 2,

        [Display(Name = "Curiosity")]
        Curiosity = 3,

        [Display(Name = "Fixation")]
        Fixation = 4,

        [Display(Name = "Other")]
        Other = 99
    }
}
