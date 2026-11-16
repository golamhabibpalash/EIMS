using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Entities;
using SMS_App.Utilities.Reports;
using Xunit;

namespace SMS.Tests.Reports
{
    /// <summary>Testimonial and Transfer Certificate must print on a single page, signature included.</summary>
    public class CertificatePdfLayoutTests
    {
        static CertificatePdfLayoutTests() => QuestPDF.Settings.License = LicenseType.Community;

        // 1x1 PNG, standing in for the logo and signature images.
        private static readonly byte[] Png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

        private static Institute Institute(int repeat) => new()
        {
            Name = "Bangladesh Rifles School and College",
            Address = string.Concat(Enumerable.Repeat("Pilkhana, Dhaka-1205, Bangladesh. ", repeat)),
            Email = "info@school.edu.bd", Phone1 = "01700000000", EIIN = "108000"
        };

        private static StudentCertificate Cert(int repeat) => new()
        {
            StudentNameSnapshot = string.Concat(Enumerable.Repeat("Md. Abdullah Al Mamun ", repeat)),
            FatherNameSnapshot = "Md. Abdur Rahman Khan", MotherNameSnapshot = "Mst. Rokeya Begum",
            SessionNameSnapshot = "2025-2026", ClassNameSnapshot = "Ten", SectionNameSnapshot = "Science",
            ClassRollSnapshot = 12, UniqueIdSnapshot = "2025100012", DobSnapshot = new DateTime(2010, 1, 5),
            GenderNameSnapshot = "Male", SignatoryName = "Md. Head Teacher", IssueDate = new DateTime(2026, 11, 16)
        };

        private static int Pages(IDocument doc) => doc.GenerateImages(new ImageGenerationSettings { RasterDpi = 20 }).Count();

        [Theory]
        [InlineData(1)]   // typical record
        [InlineData(12)]  // very long names / address: must shrink, not spill onto page 2
        public void Testimonial_is_one_page(int repeat) =>
            Assert.Equal(1, Pages(new TestimonialPdfBuilder(Institute(repeat), Png, Png, Cert(repeat))));

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        public void Transfer_certificate_is_one_page(int repeat) =>
            Assert.Equal(1, Pages(new TransferCertificatePdfBuilder(Institute(repeat), Png, Png, Cert(repeat))));
    }
}
