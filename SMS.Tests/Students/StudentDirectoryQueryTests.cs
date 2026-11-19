using System.Collections.Generic;
using System.Linq;
using SMS.Entities;
using SMS_App.Utilities.Students;
using Xunit;

namespace SMS.Tests.Students
{
    /// <summary>The mobile staff student list: session scope, filters, search, order and paging.</summary>
    public class StudentDirectoryQueryTests
    {
        private static readonly AcademicClass Six = new() { Id = 6, Name = "Six", ClassSerial = 6 };
        private static readonly AcademicClass Seven = new() { Id = 7, Name = "Seven", ClassSerial = 7 };

        private static Student S(int id, AcademicClass cls, int roll, int session = 1, int? section = null,
            string name = "x", string phone = null) => new()
        {
            Id = id,
            AcademicClass = cls,
            AcademicClassId = cls.Id,
            AcademicSectionId = section,
            AcademicSessionId = session,
            ClassRoll = roll,
            Name = name,
            UniqueId = "U" + id,
            PhoneNo = phone,
        };

        private static readonly List<Student> All = new()
        {
            S(1, Seven, 2, name: "Rahim"),
            S(2, Six, 3, section: 10, name: "Karim"),
            S(3, Six, 1, section: 11, name: "Salma", phone: "1711000000"),
            S(4, Six, 2, session: 2, name: "Old session"),
        };

        [Fact]
        public void Scopes_to_session_and_orders_by_class_serial_then_roll()
        {
            var (page, total) = StudentDirectoryQuery.Apply(All, 1, null, null, null, 1, 20);
            Assert.Equal(3, total);
            Assert.Equal(new[] { 3, 2, 1 }, page.Select(s => s.Id));
        }

        [Fact]
        public void Filters_by_class_and_section()
        {
            Assert.Equal(2, StudentDirectoryQuery.Apply(All, 1, 6, null, null, 1, 20).TotalCount);
            var (page, total) = StudentDirectoryQuery.Apply(All, 1, 6, 10, null, 1, 20);
            Assert.Equal(1, total);
            Assert.Equal(2, page.Single().Id);
        }

        [Theory]
        [InlineData("salma", 3)]
        [InlineData("U1", 1)]
        [InlineData("17110", 3)]
        public void Searches_name_unique_id_and_phone(string term, int expectedId)
        {
            var (page, _) = StudentDirectoryQuery.Apply(All, 1, null, null, term, 1, 20);
            Assert.Contains(page, s => s.Id == expectedId);
        }

        [Fact]
        public void Pages_and_clamps_page_size()
        {
            var (page, total) = StudentDirectoryQuery.Apply(All, 1, null, null, null, 2, 2);
            Assert.Equal(3, total);
            Assert.Equal(1, page.Single().Id);

            var many = Enumerable.Range(1, 80).Select(i => S(i, Six, i)).ToList();
            Assert.Equal(StudentDirectoryQuery.MaxPageSize,
                StudentDirectoryQuery.Apply(many, 1, null, null, null, 1, 500).Page.Count);
        }

        [Fact]
        public void Pads_phone_to_eleven_digits()
        {
            Assert.Equal("01711000000", StudentDirectoryQuery.Phone("1711000000"));
            Assert.Null(StudentDirectoryQuery.Phone(" "));
        }
    }
}
