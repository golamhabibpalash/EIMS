
// Global Delete function
function DeleteExam(id) {
    if (!id) {
        alertify.error('Invalid exam ID');
        return false;
    }
    
    if (!confirm("Are you sure you want to delete this exam?")) {
        return false;
    }
    
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    
    $.ajax({
        url: '/AcademicExams/Delete?Id=' + id,
        type: 'POST',
        success: function (response) {
            if (response && response.success) {
                var $row = $('button[data-id="' + id + '"]').closest('tr');
                $row.fadeOut(300, function() {
                    $(this).remove();
                });
                alertify.success(response.message);
            } else {
                alertify.error(response?.message || "Failed to delete");
            }
        },
        error: function (xhr) {
            alertify.error(xhr.responseJSON?.message || "Error deleting exam");
        }
    });
    return false;
}

$(document).ready(function () {
// Select2 initialization will be handled by the global initialization in _HeadPartial.cshtml
// This ensures consistent configuration across all select2 elements

$('.lockBtn').click(function () {
    if (confirm("Are you sure you want to unlock this?")) {
        let btnId = $(this).prop('id');
        unlockExam($(this).data("id"), btnId);
    }
    else {
        console.log("No");
        return false;
    }
});

    $('.unLockBtn').click(function () {
        if (confirm("Are you sure you want to unlock this?")) {
            let btnId = $(this).prop('id');
            lockExam($(this).data("id"), btnId);
        }
        else {
            console.log("No");
            return false;
        }
    });
    //$('.examEditBtn').click(function () {
    //    let id = $(this).data('examid');
    //    let marks = $('#editBtn_' + id).data('marks');
    //    let groupId = $('#editBtn_' + id).data('groupid');
    //    let classId = $('#editBtn_' + id).data('classid');
    //    let subjectid = $('#editBtn_' + id).data('subjectid');
    //    let sectionId = $('#editBtn_' + id).data('sectionid');
    //    let teacherId = $('#editBtn_' + id).data('employeeid');
    //    let isActive = $('#editBtn_' + id).data('status');

    //    $('#Id').val(id);
    //    $('#AcademicExamGroupId').val(groupId).trigger('change');

    //    $('#AcademicClassId').val(classId).trigger('change');

    //    $('#TotalMarks').val(marks);
    //    $('#EmployeeId').val(teacherId).trigger('change');
    //    $('#AcademicSubjectId').val(subjectid).trigger('change');
    //    $('#AcademicSectionId').val(sectionId).trigger('change');

    //    if (isActive == "True") {
    //        $('#Status').prop('checked', true);
    //    }
    //    else {
    //        $('#Status').prop('checked', false);
    //    }
    //    //document.getElementById('AcademicExamGroupId').disabled = "disabled";
    //    //document.getElementById('AcademicClassId').disabled = "disabled";
    //});
});
function unlockExam(id, btnId) {
    $.ajax({
        type: "post",
        url: '/AcademicExams/UnlockExam',
        data: "exId=" + id,
        success: function (response) {
            location.reload(true);
        },
        error: {

        }
    });
}

function lockExam(id, btnId) {
    $.ajax({
        type: "post",
        url: '/AcademicExams/lockExam',
        data: "exId=" + id,
        success: function (response) {
            location.reload(true);
        },
        error: {

        }
    });
}
$('#AcademicClassId').change(function () {
    let id = $('#AcademicClassId option:selected').val();

    let subjectsLoaded = $.ajax({
        url: '/AcademicSubjects/GetSubjectsByClassId?classId=' + id,
        method: 'POST',
        dataType: 'JSON',
        success: function (data) {
            $('#AcademicSubjectId').empty();
            $.each(data, function (i, obj) {
                var op = "<option value='" + obj.id + "'>" + obj.subjectName + "</option>";
                $('#AcademicSubjectId').append(op);
            });
        },
        error: function () { }
    });

    let sectionsLoaded = $.ajax({
        url: "/api/academicsections/getbyclasswithsessionId?classId=" + id + "&sessionId=" + null,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            var $sel = $('#AcademicSectionId');
            $sel.empty();

            // Add "All Sections" as default option
            $sel.append('<option value="0">All Sections</option>');

            if (data && data.length > 0) {
                // Don't add an empty "All Section" option for multiselect
                $.each(data, function (i, obj) {
                    var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                    $sel.append(op);
                });
            } else {
                var o = '<option disabled selected>Section Not Found</option>';
                $sel.append(o);
            }

            // Ensure multiple attribute is present
            $sel.prop('multiple', true);

            // Fully destroy any previous Select2 instance and its container
            try {
                if ($sel.data('select2')) {
                    $sel.select2('destroy');
                    $sel.next('.select2-container').remove();
                }
            } catch (ex) {
                console.warn('Error destroying select2:', ex);
            }

            // Use modal .modal-content as dropdown parent for better positioning
            var dropdownParent = $('#createUpdateModal .modal-content');

            // Reinitialize Select2 after populating options
            $sel.select2({
                theme: 'bootstrap-5',
                placeholder: 'Select Section(s)',
                allowClear: true,
                width: '100%',
                dropdownParent: dropdownParent.length ? dropdownParent : null,
                multiple: true
            });

            // Set "All Sections" (value 0) as default selection
            $sel.val(['0']).trigger('change.select2');

            // Trigger update
            $sel.trigger('change.select2');
            console.log('AcademicSectionId reinitialized with', $sel.find('option').length, 'options');
        },
        error: function (err) {
            console.log(err);
        }
    });

    // 🔔 When BOTH ajax calls complete, trigger event
    $.when(subjectsLoaded, sectionsLoaded).done(function () {
        $(document).trigger('classDataLoaded');
    });
});

function validateForm() {
    let marks = document.getElementById('TotalMarks').value;
    let groupId = document.getElementById('AcademicExamGroupId').value;
    let classId = document.getElementById('AcademicClassId').value;
    let subjectid = document.getElementById('AcademicSubjectId').value;
    let teacherId = document.getElementById('EmployeeId').value;
    let selectedSectionId = $('#AcademicSectionId').val();
    if (groupId <= 0) {
        alert("Please Select Academic Exam Group");
        document.getElementById('AcademicExamGroupId').focus();
        return false;
    }
    else if (classId <=0) {
        alert("Please select class");
        document.getElementById('AcademicClassId').focus();
        return false;
    }
    else if (subjectid <= 0) {
        alert("Please select Subject from list");
        document.getElementById('AcademicSubjectId').focus();
        return false;
    }
    else if (!selectedSectionId || selectedSectionId.length === 0) {
        alert("Please select Section(s)");
        document.getElementById('AcademicSectionId').focus();
        return false;
    }
    else if (teacherId <= 0) {
        alert("Please select Teacher from list");
        document.getElementById('EmployeeId').focus();
        return false;
    }
    else if (marks < 1 || marks > 100) {
        alert("Total Marks Field is not valid");
        document.getElementById('TotalMarks').focus();
        return false;
    }
    return true;
}

function ClearBtnClicked() {
    $("#detailsTable > tbody").empty();
}
function removeRow(button) {
    var row = button.parentNode.parentNode;
    row.parentNode.removeChild(row);
}


async function ExamAddBtnClick() {
    let isValidate = validateForm();
    if (isValidate) {
        let marks = document.getElementById('TotalMarks').value;

        let groupId = document.getElementById('AcademicExamGroupId').value;

        var classIdElement = document.getElementById("AcademicClassId");
        var classIdOption = classIdElement.options[classIdElement.selectedIndex];
        let classId = classIdOption.value;
        let classIdText = classIdOption.text;

        let examCategoryDD = document.getElementById('ExamCategory');
        let examCategory = examCategoryDD.options[examCategoryDD.selectedIndex];

        let subjectidElement = document.getElementById('AcademicSubjectId');
        let subjectidOption = subjectidElement.options[subjectidElement.selectedIndex];
        let subjectid = subjectidOption.value;
        let subjectIdText = subjectidOption.text;

        // Get selected section from Select2
        let selectedSectionId = $('#AcademicSectionId').val();

        let teacherIdElement = document.getElementById('EmployeeId');
        let teacherIdOption = teacherIdElement.options[teacherIdElement.selectedIndex];
        let teacherId = teacherIdOption.value;
        let teacherIdText = teacherIdOption.text;

        let tableBody = document.getElementById('tableBodyId');
        var indexCount = $("#detailsTable > tbody").children().length;

        // If value is "0" or empty, treat as "All Sections"
        let isAllSections = !selectedSectionId || selectedSectionId.length === 0 || selectedSectionId.includes('0');
        
        let sectionsToAdd = [];
        if (isAllSections) {
            sectionsToAdd.push({ id: '', text: 'All Sections' });
        } else {
            selectedSectionId.forEach(sectionId => {
                if (sectionId !== '0') {
                    let sectionOption = Array.from(document.getElementById('AcademicSectionId').options).find(opt => opt.value === sectionId);
                    sectionsToAdd.push({ id: sectionId, text: sectionOption ? sectionOption.text : 'Unknown' });
                }
            });
        }

        // Validate duplicates against existing rows in table for all selected sections
        let existingRows = $("#detailsTable > tbody tr");
        let duplicateSections = [];
        
        sectionsToAdd.forEach(section => {
            let isDuplicate = false;
            existingRows.each(function() {
                let row = $(this);
                let rowGroupId = row.find('input[name$=".AcademicExamGroupId"]').val();
                let rowClassId = row.find('input[name$=".AcademicClassId"]').val();
                let rowSubjectId = row.find('input[name$=".AcademicSubjectId"]').val();
                let rowSectionId = row.find('input[name$=".AcademicSectionId"]').val();
                let rowCategory = row.find('input[name$=".ExamCategory"]').val();
                
                let rowIsAllSections = rowSectionId === '' || rowSectionId === '0' || rowSectionId === null;
                let currentIsAllSections = section.id === '' || section.id === '0';
                
                if (rowGroupId == groupId && rowClassId == classId && rowSubjectId == subjectid && rowCategory == examCategory.value) {
                    if ((currentIsAllSections && rowIsAllSections) || rowSectionId == section.id) {
                        isDuplicate = true;
                        return false;
                    }
                }
            });
            if (isDuplicate) {
                duplicateSections.push(section.text);
            }
        });

        if (duplicateSections.length > 0) {
            alertify.error('Duplicate entry exists in pending table for: ' + duplicateSections.join(', '));
            return;
        }

        // Server-side validation for duplicates in database
        try {
            let duplicateCheckData = {
                ExamGroupId: parseInt(groupId),
                ClassId: parseInt(classId),
                SubjectId: parseInt(subjectid),
                ExamCategory: examCategory.value
            };

            if (isAllSections) {
                duplicateCheckData.SectionId = null;
            } else {
                duplicateCheckData.SectionIds = selectedSectionId.filter(s => s !== '0').map(s => parseInt(s));
            }

            const response = await $.ajax({
                url: '/AcademicExams/CheckDuplicates',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(duplicateCheckData)
            });

            if (response.results) {
                const duplicates = response.results.filter(r => r.isDuplicate);
                if (duplicates.length > 0) {
                    const dupSectionNames = duplicates.map(d => {
                        const sectionOpt = Array.from(document.getElementById('AcademicSectionId').options).find(opt => opt.value === d.sectionId.toString());
                        return sectionOpt ? sectionOpt.text : 'Section ' + d.sectionId;
                    });
                    alertify.error('Duplicate exam(s) already exist for: ' + dupSectionNames.join(', '));
                    return;
                }
            } else if (response.isDuplicate) {
                alertify.error('Duplicate exam already exists in database for this combination.');
                return;
            }
        } catch (xhr) {
            console.error('Error checking duplicates:', xhr);
        }

        // Add a row for each selected section
        sectionsToAdd.forEach(section => {
            let serial_td = '<td><input type="hidden" name="AcademicExams[' + indexCount + '].AcademicExamGroupId" value="' + groupId + '" />' + (indexCount + 1) + '</td>';
            let subject_td = '<td>  <input type="hidden" name="AcademicExams[' + indexCount + '].AcademicSubjectId" value="' + subjectid + '" />' + subjectIdText + '</td>';
            let examCategory_td = '<td><input type="hidden" name="AcademicExams[' + indexCount + '].ExamCategory" value="' + examCategory.value + '" /> ' + examCategory.value + '</td>';
            let class_td = '<td> <input type="hidden" name="AcademicExams[' + indexCount + '].AcademicClassId" value="' + classId + '" />' + classIdText + '</td>';
            let section_td = '<td> <input type="hidden" name="AcademicExams[' + indexCount + '].AcademicSectionId" value="' + section.id + '" /><span class="badge bg-info">' + section.text + '</span></td>';
            let teacher_td = '<td> <input type="hidden" name="AcademicExams[' + indexCount + '].EmployeeId" value="' + teacherId + '" />' + teacherIdText + '</td>';
            let marks_td = '<td class="text-end"> <input type="hidden" name="AcademicExams[' + indexCount + '].TotalMarks" value="' + marks + '" />' + marks + '</td>';
            let action_td = '<td><button onclick="removeRow(this)" class="removeBtn btn btn-sm btn-warning" value="Remove">Remove</button></td>';
            let tr = '<tr>' + serial_td + subject_td + examCategory_td + class_td + section_td + teacher_td + marks_td + action_td + '</tr>';
            tableBody.innerHTML += tr;
            indexCount++;
        });
        
        alertify.success(sectionsToAdd.length > 1 ? 'Exams added for ' + sectionsToAdd.length + ' sections.' : (isAllSections ? 'Exam added for All Sections.' : 'Exam added successfully.'));
    }
}

$('#submitBtn').click(function (e) {
    e.preventDefault();
    e.stopPropagation();
    showLoader();
    
    // Start progress animation
    let progress = 0;
    let progressInterval = setInterval(function() {
        progress += Math.random() * 15;
        if (progress > 90) progress = 90;
        $('#loading-progress').css('width', progress + '%');
        $('#loading-count').text(Math.round(progress) + '%');
    }, 500);
    
    var $form = $('#submitForm');
    
    // Get selected sections (multiple select)
    var selectedSections = $('#AcademicSectionId').val() || [];
    var formData = $form.serializeArray();
    
    // Add sections as JSON string
    if (selectedSections.length > 0 && selectedSections[0] != "0") {
        formData.push({ name: 'AcademicExams[0].AcademicSectionIdList', value: JSON.stringify(selectedSections) });
    }
    
    // Hide modal immediately and show loading
    $('#createUpdateModal').modal('hide');
    document.getElementById("loading").style.display = "block";
    $('#loading-status').text('Creating exam records for students...');
    
    // Submit the form via AJAX
    $.ajax({
        type: 'POST',
        url: $form.attr('action') || '/AcademicExams/Create',
        data: formData,
        success: function (response) {
            clearInterval(progressInterval);
            $('#loading-progress').css('width', '100%');
            $('#loading-count').text('100%');
            $('#loading-status').text('Exam created successfully!');
            
            // Clear the table
            $("#detailsTable > tbody").empty();
            
            // Reset the form
            $form[0].reset();
            $('#AcademicSectionId').val([]).trigger('change.select2');
            
            setTimeout(function () {
                hideLoader();
                document.getElementById("loading").style.display = "none";
                location.reload();
            }, 1000);
        },
        error: function (xhr, status, error) {
            clearInterval(progressInterval);
            hideLoader();
            document.getElementById("loading").style.display = "none";
            alertify.error('Error submitting form. Please try again.');
        }
    });
});
function DeleteExam(id) {
    if (!id) {
        console.log('No ID provided');
        alertify.error('Invalid exam ID');
        return false;
    }
    
    var result = confirm("Are you sure you want to delete this exam?");
    if (result) {
        console.log('Deleting exam id:', id);
        $.ajax({
            url: '/AcademicExams/Delete?Id=' + id,
            type: 'POST',
            success: function (response) {
                console.log('Delete response:', response);
                if (response && response.success) {
                    $('button[data-id="' + id + '"]').closest('tr').hide();
                    alertify.success(response.message);
                    setTimeout(function() {
                        location.reload();
                    }, 1000);
                } else {
                    alertify.error(response?.message || "Failed to delete");
                }
            },
            error: function (xhr) {
                console.log('Delete error:', xhr);
                var msg = xhr.responseJSON?.message || "Error deleting exam";
                alertify.error(msg);
            }
        });
    }
    return false;
}

async function EditExamClick(id) {

    //examGroup dropdown 
    let groupId = $('#editBtn_' + id).data('groupid');
    $('#AcademicExamGroupId').val(groupId).trigger('change');

    //Class Dropdown
    let classId = $('#editBtn_' + id).data('classid');
    $('#AcademicClassId').val(classId).trigger('change');

    // 🕒 Wait for both subjects and sections to load
    await new Promise(resolve => $(document).one('classDataLoaded', resolve));

    //Now safely set dependent dropdowns
    let sectionId = $('#editBtn_' + id).data('sectionid');
    
    // For Select2, we need to set value differently for multiple selects
    if (sectionId) {
        $('#AcademicSectionId').val([sectionId]).trigger('change.select2');
    }

    let subjectid = $('#editBtn_' + id).data('subjectid');
    $('#AcademicSubjectId').val(subjectid).trigger('change');

    //Mark input fields
    let marks = $('#editBtn_' + id).data('marks');
    $('#TotalMarks').val(marks);

    //Exam Category
    let cat = $('#editBtn_' + id).data('category');
    $('#ExamCategory').val(cat);

    //Teacher
    let teacherId = $('#editBtn_' + id).data('employeeid');
    $('#EmployeeId').val(teacherId).trigger('change');

    let isActive = $('#editBtn_' + id).data('status');
    $('#Id').val(id);
    $('#Status').prop('checked', isActive === "True");

    // UI adjustments
    document.getElementById('editupdatemodalfooter').style.display = 'none';
    document.getElementById('createUpdateModalLabel').innerHTML = "Edit Exam Info";
    document.getElementById('examAddBtn').style.display = 'none';
    document.getElementById('updateFormSubmitBtn').style.display = 'block';
}



$('#updateFormSubmitBtn').click(function () {
    /*modalFooter*/
    let isValidate = validateForm();
    if (isValidate) {
        alertify.confirm("This is a confirm dialog.",
            function () {
                alertify.success('Ok');
            },
            function () {
                alertify.error('Cancel');
            });
    }

});

function examinationAddButtonClicked(groupId) {

    let modalFooter = document.getElementById('editupdatemodalfooter');
    modalFooter.style.display = 'block';
    var dropdown = document.getElementById("AcademicExamGroupId");

    if (dropdown) {
        dropdown.value = groupId; // set selected value
    }
    dropdown.dispatchEvent(new Event('change'));
    
    // Reset form fields
    document.getElementById('AcademicSubjectId').value = '';
    document.getElementById('TotalMarks').value = '';
    document.getElementById('EmployeeId').value = '';
    document.getElementById('ExamCategory').value = '';
    document.getElementById('Id').value = '';
    
    // Set "All Sections" (value 0) as default selection
    $('#AcademicSectionId').val('0').trigger('change.select2');
    
    let examAddBtn = document.getElementById('examAddBtn');
    examAddBtn.style.display = 'block';

    let modalUpdateBtn = document.getElementById('updateFormSubmitBtn');
    modalUpdateBtn.style.display = 'none';
}
function showLoader() {
    document.getElementById('loading').classList.remove('d-none');
}
function hideLoader() {
    document.getElementById('loading').classList.add('d-none');
}

// ============================================
// OCR Mark Import Functions
// ============================================

async function processOCRImage() {
    const fileInput = document.getElementById('ocrImageInput');
    const file = fileInput.files[0];
    
    if (!file) {
        alertify.error('Please select an image file first');
        return;
    }
    
    const examType = document.getElementById('ocrExamType').value;
    const examId = document.querySelector('input[name="Id"]').value;
    
    // Show progress
    const progressDiv = document.getElementById('ocrProgress');
    const progressBar = document.getElementById('ocrProgressBar');
    const statusText = document.getElementById('ocrStatus');
    
    progressDiv.style.display = 'block';
    progressBar.style.width = '0%';
    progressBar.textContent = '0%';
    statusText.textContent = 'Initializing OCR...';
    
    try {
        // Initialize Tesseract worker
        progressBar.style.width = '10%';
        progressBar.textContent = '10%';
        statusText.textContent = 'Loading OCR engine...';
        
        const worker = await Tesseract.createWorker('eng', 1, {
            logger: m => {
                if (m.status === 'recognizing text') {
                    const percent = Math.round(m.progress * 80 + 10);
                    progressBar.style.width = percent + '%';
                    progressBar.textContent = percent + '%';
                    statusText.textContent = 'Processing image: ' + Math.round(m.progress * 100) + '%';
                }
            }
        });
        
        progressBar.style.width = '20%';
        progressBar.textContent = '20%';
        statusText.textContent = 'Recognizing text...';
        
        // Perform OCR
        const { data: { text } } = await worker.recognize(file);
        
        await worker.terminate();
        
        progressBar.style.width = '90%';
        progressBar.textContent = '90%';
        statusText.textContent = 'Parsing results...';
        
        // Log raw OCR text for debugging
        console.log('Raw OCR Text:', text);
        console.log('Lines found:', text.split('\n').length);
        
        // Parse the OCR text to extract roll numbers and marks
        const extractedData = parseOCRText(text);
        
        console.log('Extracted Data:', extractedData);
        
        if (extractedData.length === 0) {
            // Show raw text in alert for debugging
            console.log('Full OCR output:', text);
            alertify.warning('No marks could be extracted. Check browser console (F12) for raw OCR text.');
            statusText.textContent = 'Extracted: 0 rows - see console for debug';
            return;
        }
        
        // Auto-fill marks in the table
        const filledCount = autoFillMarksFromOCR(extractedData, examType);
        
        progressBar.style.width = '100%';
        progressBar.textContent = '100%';
        statusText.textContent = 'Completed! ' + filledCount + ' marks filled.';
        
        alertify.success(filledCount + ' marks filled successfully from OCR');
        
    } catch (error) {
        console.error('OCR Error:', error);
        statusText.textContent = 'Error: ' + error.message;
        alertify.error('OCR processing failed: ' + error.message);
    }
}

function parseOCRText(text) {
    const results = [];
    const lines = text.split('\n');
    
    for (let i = 0; i < lines.length; i++) {
        let line = lines[i].trim();
        if (!line) continue;
        
        // Skip header lines and short lines
        if (line.match(/^(Class)?Roll/i) || line.match(/Student\s*Name/i) || 
            line.match(/^CQ/i) || line.match(/^MCQ/i) || line.match(/^Prac/i) ||
            line.match(/^Total/i) || line.match(/^---/) || line.length < 5) {
            continue;
        }
        
        // Extract all numbers from the line
        const numbers = line.match(/\d+/g);
        
        if (!numbers) continue;
        
        // Format: rollNumber StudentName mark 
        // e.g., "2608001 Jamil Mia 15" -> numbers: [2608001, 15]
        if (numbers.length >= 2) {
            // First number is roll (could be 5-7 digits like 2608001)
            const roll = parseInt(numbers[0]);
            
            // Allow larger roll numbers (school code + roll)
            if (roll > 0 && roll < 100000) {
                // Find last number as mark (most likely)
                const mark = parseInt(numbers[numbers.length - 1]);
                
                if (mark >= 0 && mark <= 100) {
                    results.push({ roll: roll, mark: mark });
                }
            }
        }
    }
    
    return results;
}

function autoFillMarksFromOCR(extractedData, examType) {
    let filledCount = 0;
    
    const firstInput = document.querySelector('.markInput');
    const totalMarks = firstInput ? parseInt(firstInput.getAttribute('max')) || 100 : 100;
    
    const markInputs = document.querySelectorAll('.markInput:not([disabled])');
    
    console.log('Mark inputs found:', markInputs.length);
    
    // Use approach: try to fill by row index if roll matching fails
    // Create array of inputs in order
    const inputsArray = Array.from(markInputs);
    
    // Try matching by roll from table
    const rollInputMap = {};
    
    inputsArray.forEach((input, index) => {
        const row = input.closest('tr');
        // Get the roll from first td in the row (it shows roll number as text)
        const firstTd = row.querySelector('td:nth-child(2)');
        if (firstTd) {
            // Text content only, exclude hidden input values
            const text = firstTd.textContent.trim();
            const roll = parseInt(text);
            if (roll) {
                rollInputMap[roll] = input;
            }
        }
    });
    
    console.log('Roll map:', rollInputMap);
    console.log('OCR data:', extractedData);
    
    // Now fill marks
    extractedData.forEach((data, idx) => {
        if (!data.roll || data.mark === undefined || data.mark === null) return;
        
        const ocrRoll = data.roll;
        const markValue = data.mark;
        
        // Direct match
        let matchedInput = rollInputMap[ocrRoll];
        
        // Try suffix match (e.g., 2608001 -> 1)
        if (!matchedInput && ocrRoll > 1000) {
            const lastDigits = parseInt(ocrRoll.toString().slice(-2));
            if (lastDigits && rollInputMap[lastDigits]) {
                matchedInput = rollInputMap[lastDigits];
                console.log('Matched suffix:', ocrRoll, '->', lastDigits);
            }
        }
        
        // Fallback: use index order - assume OCR and table have same student order
        if (!matchedInput && idx < inputsArray.length) {
            matchedInput = inputsArray[idx];
            console.log('Using index fallback:', idx, 'for roll', ocrRoll);
        }
        
        if (matchedInput) {
            if (markValue <= totalMarks) {
                matchedInput.value = markValue;
                matchedInput.dispatchEvent(new Event('change', { bubbles: true }));
                filledCount++;
                console.log('Filled roll', ocrRoll, 'mark', markValue);
            } else {
                console.log('Mark > total:', markValue, '>', totalMarks);
            }
        }
    });
    
    return filledCount;
}