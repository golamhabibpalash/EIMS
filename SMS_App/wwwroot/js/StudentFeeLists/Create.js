$('#warningMsg').fadeOut(6000);

$('#AcademicSessionId').change(function () {
    let id = $('#AcademicSessionId option:selected').val();

    $.ajax({
        url: "/Services/GetClassListBySessionId/" + id,
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