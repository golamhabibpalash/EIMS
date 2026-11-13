$(document).ready(function () {
    $(".fadeOutMsg").fadeOut(10000);


$('#AcademicClassId').change(function () {
    let id = $('#AcademicClassId option:selected').val();
    let sessionId = $('#AcademicSessionId option:selected').val();

    $.ajax({
        url: "/api/academicsections/getbyclasswithsessionId?classId=" + id + "&sessionId=" + sessionId,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#AcademicSectionId').empty();

            if (data != null || data != '') {
                var o = '<option disabled selected>Select Section Name</option>';
                var o2 = '<option value="">No Section</option>';
                $('#AcademicSectionId').append(o);
                $('#AcademicSectionId').append(o2);
                $.each(data, function (i, obj) {
                    console.log(obj.name);
                    var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                    $('#AcademicSectionId').append(op);
                });
            }
            else {
                var o = '<option disabled selected>Section Not Found</option>';
                $('#AcademicSectionId').append(o);
            }
        },
        error: function (err) {
            console.log(err);
        }
    });
});


//Code for PresentDistrict Load
 $('#PresentDivisionId').change(function () {
    let divId = $('#PresentDivisionId option:selected').val();
    let disName = '#PresentDistrictId';
    GetDistrictByDivisionId(divId, disName);
});

//Code for PresentUpazila Load
$('#PresentDistrictId').change(function () {
    let disId = $('#PresentDistrictId option:selected').val();
    let upName = '#PresentUpazilaId';
    GetUpazilaByDistrictId(disId, upName);
});

//Code for PermanentDistrict Load
$('#PermanentDivisionId').change(function () {
    let divId = $('#PermanentDivisionId option:selected').val();
    let disName = '#PermanentDistrictId';
    GetDistrictByDivisionId(divId, disName);
});

//Code for PresentUpazila Load
$('#PermanentDistrictId').change(function () {
    let disId = $('#PermanentDistrictId option:selected').val();
    let upName = '#PermanentUpazilaId';
    GetUpazilaByDistrictId(disId, upName);
});

});

//Image load to view
function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#sPhotoId').attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]); // convert to base64 string
    }
}
$("#imgInp").change(function () {
    $("#photoDiv").show();
    readURL(this);
});

//Same as present Addresss
function loadAddress() {
    var preAddressArea = $('#PresentAddressArea').val();
    var preAddressPO = $('#PresentAddressPO').val();

    var preDivVal = $('#PresentDivisionId').val();
    var preDivText = $('#PresentDivisionId option:selected').text();
    var preDiv = '<option value="' + preDivVal + '">' + preDivText + '</option>';

    var preDisText = $('#PresentDistrictId option:selected').text();
    var preDisVal = $('#PresentDistrictId').val();
    var preDis = '<option value="' + preDisVal + '">' + preDisText + '</option>';

    var preUpVal = $('#PresentUpazilaId').val();
    var preUpText = $('#PresentUpazilaId option:selected').text();
    var preUp = '<option value="' + preUpVal + '">' + preUpText + '</option>';

    $('#PermanentAddressArea').val(preAddressArea);
    $('#PermanentAddressPO').val(preAddressPO);

    $('#PermanentDivisionId').html(preDiv);
    $('#PermanentDistrictId').html(preDis);
    $('#PermanentUpazilaId').html(preUp);

}
function emptyAddress() {
    $('#PermanentAddressArea').val("");
    $('#PermanentAddressPO').val("");
    $('#PermanentDivisionId').change();
    $('#PermanentDistrictId').empty();
    $('#PermanentAddressPSId').html('<option disabled selected>Select District First</option>');
}

$('#sameAddress').change(function () {
    let isChecked = $('#sameAddress').is(":checked");
    if (isChecked == true) {
        loadAddress();
    }
    else {
        emptyAddress();
        GetDivisionList('#PermanentDivisionId')
    }
});

function GetDivisionList(divName) {
    $.ajax({
        url: "/api/divisions/allDivisions",
        dataType: 'JSON',
        type: 'POST',
        cache: false,
        success: function (data) {
            $(divName).empty();
            var op = '<option disabled selected>Select division Name</option>';
            $(divName).append(op);
            $.each(data, function (i, obj) {
                var ob = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $(divName).append(ob);
            });
        },
        error: function (err) {

        }
    });
}
function GetDistrictByDivisionId(divId, disName) {
    $.ajax({
        url: "/api/districts/bydivision?divId=" + divId,
        dataType: 'JSON',
        type: 'POST',
        cache: false,
        success: function (data) {
            $(disName).empty();
            var op = '<option disabled selected>Select district Name</option>';
            $(disName).append(op);
            $.each(data, function (i, obj) {
                var ob = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $(disName).append(ob);
            });
        },
        error: function (err) {

        }
    });
}
function GetUpazilaByDistrictId(disId, upName) {
    $.ajax({
        url: "/api/upazilas/byDistrict?id=" + disId,
        dataType: 'JSON',
        type: 'POST',
        cache: false,
        success: function (data) {
            $(upName).empty();
            var option = '<option disabled selected>Select Upazila Name</option>';
            $(upName).append(option);
            $.each(data, function (i, obj) {
                var ob = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $(upName).append(ob);
            });
        },
        error: function (err) {

        }
    });
}

function classRollChnaged() {
    var roll = $("#ClassRoll").val();   
    var sessionId = $('#AcademicSessionId option:selected').val();
    var classId = $('#AcademicClassId option:selected').val();
    if (!sessionId) {
        alertify.warning('Please Select Session');
        return;
    }
    if (!classId) {
        alertify.warning('Please Select Class');
        return;
    }
    console.log('@Url.Action("CheckRollExist", "Students")');
    $.ajax({
        url: '/Students/CheckRollExist', 
        type: 'GET',
        data: {
            roll: roll,
            sessionId: sessionId,
            classId: classId
        },
        success: function (response) {
            if (response === true) {
                alertify.error('Sorry! This roll number is already exist');
            } else {
                alertify.success('Yes! This roll number is available');
            }
        },
        error: function () {
            console.error(error);
            alertify.error('Something went wrong!');
        }
    });
}