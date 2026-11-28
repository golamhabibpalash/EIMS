using System.ComponentModel.DataAnnotations;

namespace SMS.Entities.Enums
{
    /// <summary>
    /// The kind of external link an institute publishes. "Other" deliberately covers any
    /// social platform or website not listed here; such links are distinguished by their title,
    /// so an institute can hold any number of them. Stored as the underlying int.
    /// </summary>
    public enum InstituteLinkType
    {
        Website = 0,
        Facebook = 1,
        YouTube = 2,
        Instagram = 3,
        LinkedIn = 4,
        [Display(Name = "X (Twitter)")] Twitter = 5,
        WhatsApp = 6,
        Telegram = 7,
        Other = 99
    }
}
