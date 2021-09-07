



_curEmployeeID = '';



function GetEmployee() {

    _curEmployeeID = $("#SelectedEmpID").val();
    var fromDate = $("#txtFromDate").val();
    var toDate = $("#txtToDate").val();

    var viewUrl = rootPath + "/Employee/EmpIdValidation/?id=" + $('#searchTerm').val();

    if (_curEmployeeID) {

        $.get(viewUrl, function (data) {
            if (data.Success == true) {

                $("#dvMessage").text('');
                $("#dvMessage").removeClass("alert alert-danger");

                $("#dvEmpDetail").html('');
                $("#dvEmpDetail").load('/Employee/GetEmployeeByID/?empId=' + _curEmployeeID);

                var URL = rootPath + "/PFSettings/MemberPFStatus/SelectPFStatus/?empId=" + _curEmployeeID + "&fromDate=" + fromDate + "&toDate=" + toDate;
                $.get(URL, function (data) {
                    $("#dvEmpPFMonthlyStatus").html('');
                    $("#dvEmpPFMonthlyStatus").html(data);
                });
            }
            else {
                $("#dvMessage").text(data.ErrorMessage);
                $("#dvMessage").addClass("alert alert-danger");
            }
        });
    }
    else {
        alert('Select a memberID to continue...');
    }
}

$('#searchTerm').keyup(function (e) {

    if (e.keyCode == 13) {
        GetEmployee();
        $('#GetEmployee').focus();
    }
});