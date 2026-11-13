
function showCreateForm() {
    document.getElementById("questionCreateDivId").style.display = "block";
    document.getElementById("myPage").style.opacity = "0.5";    
}

function hideCreateForm() {
    document.getElementById("questionCreateDivId").style.display = "none";
    document.getElementById("myPage").style.opacity = "1";    
}

$('#QCreateVM_AcademicClassId').change(function () {
    $('#qDetailsDivId').empty();
    let classId = $('#QCreateVM_AcademicClassId option:selected').val();
    $.ajax({
        url: '/AcademicSubjects/GetSubjectsByClassId?classId=' + classId,
        method: 'Post',
        type: 'JSON',
        success: function (data) {
            $('#QCreateVM_AcademicSubjectId').empty();
            var op = '<option disabled selected> Select Subject </option>';
            $('#QCreateVM_AcademicSubjectId').append(op);
            $.each(data, function (i, obj) {
                var op = "<option value='" + obj.id + "'>" + obj.subjectName + "</option>";
                $('#QCreateVM_AcademicSubjectId').append(op);
            });
        },
        error: function () { }
    });
});

$('#QCreateVM_AcademicSubjectId').change(function () {
    let subjectId = $('#QCreateVM_AcademicSubjectId option:selected').val();
    $.ajax({
        url: '/Chapters/GetChapterBySubject?SubjectId=' + subjectId,
        method: 'Post',
        type: 'JSON',
        success: function (data) {
            $('#QCreateVM_ChapterId').empty();
            var op = '<option disabled selected> Select Chapter </option>';
            $('#QCreateVM_ChapterId').append(op);
            $.each(data, function (i, obj) {
                var op = "<option value='" + obj.id + "'>" + obj.chapterName + "</option>";
                $('#QCreateVM_ChapterId').append(op);
            });

        },
        error: function () { }
    });

    $.ajax({
        url: '/AcademicSubjects/GetSubjectDetailsBySubjectId?SubjectId=' + subjectId,
        method: 'Post',
        type: 'JSON',
        success: function (data) {
            $('#qDetailsDivId').empty();
            let j = 1;
            for (var i = 0; i < data.questionFormat.numberOfQuestion; i++) {
                
                let lbl = '<label asp-for="QCreateVM.QuestionDetails[' + i + '].QuestionText" class="form-label col-md-2 col-2 text-md-end col-form-label bg-lightgray">Q. (' + j + ')</label>';
                let inputDiv = '<div class="col-md-10 col-10"><input name="QCreateVM.QuestionDetails[' + i + '].QuestionText" class="form-control" required /></div>';
                let myDiv = '<div class="form-group row pb-2">' + lbl + inputDiv + '</div>';
                $('#qDetailsDivId').append(myDiv);
                j++;
            }
        },
        error: function () { }
    });
});


$("#questionCreateForm").validate({
    errorClass: 'errors',
    rules: {
        'QCreateVM.ChapterId': { required: true, minlength: 1 },
        'QCreateVM.Uddipok': { required: true, minlength: 10 },
        'QCreateVM.QuestionDetails[0].QuestionText': { required: true, minlength: 5 },
        'QCreateVM.QuestionDetails[1].QuestionText': { required: true, minlength: 5 },
        'QCreateVM.QuestionDetails[2].QuestionText': { required: true, minlength: 5 },
        'QCreateVM.QuestionDetails[3].QuestionText': { required: true, minlength: 5 }
    },
    messages: {
        'QCreateVM.ChapterId': "Please Select Chapter",
        'QCreateVM.Uddipok': { required: "Uddipak is missing", minlength: "You have to write clearly something" },
        'QCreateVM.QuestionDetails[0].QuestionText': { required: "Question 1 is missing", minlength: "You have to ask clearly something" },
        'QCreateVM.QuestionDetails[1].QuestionText': { required: "Question 2 is missing", minlength: "You have to ask clearly something" },
        'QCreateVM.QuestionDetails[2].QuestionText': { required: "Question 3 is missing", minlength: "You have to ask clearly something" },
        'QCreateVM.QuestionDetails[3].QuestionText': { required: "Question 4 is missing", minlength: "You have to ask clearly something" },
    },

    submitHandler: function (form) {
        //var image = document.getElementById("QCreateVM_Image");
        //var files = image.files;
        //var formData = new FormData();
        //for (var i = 0; i !== files.length; i++) {
        //    formData.append("files", files[i]);
        //}
        var formData = new FormData($('#questionCreateForm').get(0));
        $.ajax({
            url: "/QuestionBanks/CreateQuestion",
            type: "post",
            data: formData,
            processData: false,
            contentType: false,
            success: function (data, status) {
                //let totalTr = $('#tableBody tr').length + 1;
                //let slTd = '<td>' + totalTr + '</td>';
                //let nameTd = '<td> <input id="ChapterName_' + data.id + '" type="text" class="form-control-plaintext" name="ChapterName" value="' + data.chapterName + '" /></td>';

                //let classTd = '<td>' + data.academicSubject.academicClass.name + '</td>';
                //let subjectTd = '<td>' + data.academicSubject.subjectName + '</td>';
                //let btnGroup = '<div class="btn-group"><span class="btn btn-sm btn-success" id="edit_' + data.id + '" onclick="editChapter(' + data.id + ')">Edit</span><span class="btn btn-sm btn-warning" id="update_' + data.id + '" style="display:none" onclick="updateChapter(' + data.id + ')">Update</span><span class="btn btn-sm btn-danger" id="delete_' + data.id + '">Delete</span></div>';
                //let actionTd = '<td>' + btnGroup + '</td>';

                //let tr = '<tr>' + slTd + nameTd + classTd + subjectTd + actionTd + '</tr>';

                //$('#tableBody').append(tr);

                $('#modalCloseButton').trigger("click");
                alertify.success(data.msg);
            }
        });
    }
});