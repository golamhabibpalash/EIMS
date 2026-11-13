using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMS_App.Controllers.Api
{
    public class DataTableRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string searchValue { get; set; }
        public string orderColumn { get; set; }
        public string orderDirection { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class StudentFeeAllocationApiController : ControllerBase
    {
        private readonly IStudentFeeAllocationManager _studentFeeAllocationManager;

        public StudentFeeAllocationApiController(IStudentFeeAllocationManager studentFeeAllocationManager)
        {
            _studentFeeAllocationManager = studentFeeAllocationManager;
        }

        [HttpPost("GetDataTableData")]
        [Authorize(Policy = "IndexStudentFeeAllocationsPolicy")]
        public IActionResult GetDataTableData([FromBody] DataTableRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            int draw = request.draw;
            int start = request.start;
            int length = request.length;
            string searchValue = request.searchValue ?? "";
            string orderColumn = request.orderColumn;
            string orderDirection = request.orderDirection;

            if (string.IsNullOrEmpty(orderColumn))
                orderColumn = "classroll";
            if (string.IsNullOrEmpty(orderDirection))
                orderDirection = "asc";

            try
            {
                var result = _studentFeeAllocationManager.GetDataTableDataAsync(
                    searchValue, orderColumn, orderDirection, start, length).GetAwaiter().GetResult();

                var response = new
                {
                    draw = draw,
                    recordsTotal = result.totalRecord,
                    recordsFiltered = result.filteredRecord,
                    data = result.data.Select(s => new
                    {
                        id = s.Id,
                        studentId = s.StudentId,
                        student = s.Student != null ? new
                        {
                            id = s.Student.Id,
                            name = s.Student.Name,
                            classRoll = s.Student.ClassRoll,
                            academicClassId = s.Student.AcademicClassId
                        } : null,
                        studentFeeHeadId = s.StudentFeeHeadId,
                        studentFeeHead = s.StudentFeeHead != null ? new
                        {
                            id = s.StudentFeeHead.Id,
                            name = s.StudentFeeHead.Name
                        } : null,
                        allocatedAmount = s.AllocatedAmount,
                        isActive = s.IsActive,
                        feeAllocationApplication = s.FeeAllocationApplication,
                        editedAt = s.EditedAt,
                        editedBy = s.EditedBy,
                        classFeeList = s.ClassFeeList != null ? new
                        {
                            id = s.ClassFeeList.Id,
                            academicSession = s.ClassFeeList.AcademicSession != null ? new
                            {
                                id = s.ClassFeeList.AcademicSession.Id,
                                name = s.ClassFeeList.AcademicSession.Name
                            } : null
                        } : null
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}