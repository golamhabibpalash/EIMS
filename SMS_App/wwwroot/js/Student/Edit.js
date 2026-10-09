
$(".fadeOutMsg").fadeOut(10000);

$('#AcademicSessionId').change(function () {
    let id = $('#AcademicSessionId option:selected').val();

    $.ajax({
        url: "/Students/GetClassList/" + id,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#AcademicClassId').empty();
            var o = '<option disabled selected>Select Class Name</option>';
            $('#AcademicClassId').append(o);
            $.each(data, function (i, obj) {
                console.log(obj.name);
                var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $('#AcademicClassId').append(op);
            });
        },
        error: function (err) {
            console.log(err);
        }
    });
});

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

            if (data != null && data.length > 0) {
                var o = '<option disabled selected>Select Section Name</option>';
                var o2 = '<option value="">No Section</option>';
                $('#AcademicSectionId').append(o);
                $('#AcademicSectionId').append(o2);
                $.each(data, function (i, obj) {
                    var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                    $('#AcademicSectionId').append(op);
                });
            }
            else {
                var o = '<option value="">No Section</option>';
                $('#AcademicSectionId').append(o);
            }
        },
        error: function (err) {
            console.log(err);
        }
    });
});

$('#PresentDivisionId').change(function () {
    let id = $('#PresentDivisiontId option:selected').val();

    $.ajax({
        url: "/Students/GetDistrictList/" + id,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#PresentDistrictId').empty();
            var o = '<option disabled selected>Select District Name</option>';
            $('#PresentDistrictId').append(o);
            $.each(data, function (i, obj) {
                console.log(obj.name);
                var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $('#PresentDistrictId').append(op);
            });
        },
        error: function (err) {
            console.log(err);
        }
    });
});

$('#PresentDistrictId').change(function () {
    let id = $('#PresentDistrictId option:selected').val();

    $.ajax({
        url: "/Students/GetUpazilaList/" + id,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#PresentUpazilaId').empty();
            var o = '<option disabled selected>Select Upazila Name</option>';
            $('#PresentUpazilaId').append(o);
            $.each(data, function (i, obj) {
                console.log(obj.name);
                var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $('#PresentUpazilaId').append(op);
            });
        },
        error: function (err) {
            console.log(err);
        }
    });
});


$('#PermanentDivisionId').change(function () {

    let id = $('#PermanentDivisionId option:selected').val();

    $.ajax({
        url: "/Students/GetDistrictList/" + id,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#PermanentDistrictId').empty();
            var o = '<option disabled selected>Select District Name</option>';
            $('#PermanentDistrictId').append(o);
            $.each(data, function (i, obj) {
                console.log(obj.name);
                var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $('#PermanentDistrictId').append(op);
            });
        },
        error: function (err) {
            console.log(err);
        }
    });
});

$('#PermanentDistrictId').change(function () {
    let id = $('#PermanentDistrictId option:selected').val();

    $.ajax({
        url: "/Students/GetUpazilaList/" + id,
        dataType: "JSON",
        type: "POST",
        cache: false,
        success: function (data) {
            $('#PermanentUpazilaId').empty();
            var o = '<option disabled selected>Select Upazila Name</option>';
            $('#PermanentUpazilaId').append(o);
            $.each(data, function (i, obj) {
                console.log(obj.name);
                var op = '<option value="' + obj.id + '">' + obj.name + '</option>';
                $('#PermanentUpazilaId').append(op);
            });
        },
        error: function (err) {
            console.log(err);
        }
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
    $("#previousImageId").hide();
    readURL(this);
});

//Same as present Addresss
function loadAddress() {
    var preAddressArea = $('#PresentAddressArea').val();
    var preAddressPO = $('#PresentAddressPO').val();

    var preDivVal = $('#PresentDivisiontId').val();

    var preDisText = $('#PresentDistrictId option:selected').text();
    var preDisVal = $('#PresentDistrictId').val();
    var preDis = '<option value="' + preDisVal + '">' + preDisText + '</option>';

    var preUpVal = $('#PresentUpazilaId').val();
    var preUpText = $('#PresentUpazilaId option:selected').text();
    var preUp = '<option value="' + preUpVal + '">' + preUpText + '</option>';

    $('#PermanentAddressArea').val(preAddressArea);
    $('#PermanentAddressPO').val(preAddressPO);

    $('#PermanentDivisiontId').val(preDivVal);

    $('#PermanentDistrictId').html(preDis);
    $('#PermanentUpazilaId').html(preUp);

}
function emptyAddress() {
    $('#PermanentAddressArea').val("");
    $('#PermanentAddressPO').val("");
    $('#PermanentDivisiontId').change();
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
    }
});
