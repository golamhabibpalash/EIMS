namespace SMS.Entities.Enums
{
    /// <summary>
    /// Lifecycle of a Transfer Certificate. A Draft can be freely edited/deleted; once Issued it
    /// becomes an official record that is locked from edits and can only be Cancelled (never
    /// hard-deleted). A printable certificate is only produced for Issued records.
    /// </summary>
    public enum TransferCertificateStatus
    {
        Draft = 0,
        Issued = 1,
        Cancelled = 2,
    }
}
