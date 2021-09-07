




_curEmployeeID = '';



function GetEmployee() {

    _curEmployeeID = $("#SelectedEmpID").val();

    var viewUrl = rootPath + "/Employee/EmpIdValidation/?id=" + $('#searchTerm').val();

    if (_curEmployeeID) {

        $.get(viewUrl, function (data) {
            if (data.Success == true) {

                $("#dvEmpDetail").html('');
                $("#dvEmpDetail").load('../Employee/GetEmployeeByID/?empId=' + _curEmployeeID);

                $("#dvEmpSalary").html('');
                $("#dvEmpSalary").load('../Salary/GetSalaryHistory/?empId=' + _curEmployeeID);
            }
            else {
                $("#dvMessage").text(data.ErrorMessage);
                $("#dvMessage").addClass("alert alert-danger");
                $("#dvEmpDetail").html('');
                $("#dvEmpSalary").html('');
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


//function GetSalaryHistory() {
//    if (_curEmployeeID) {
//        $("#DialogSpace1").dialog("option", "title", "Salary History");
//        var dialogDiv = $('#DialogSpace1');
//        var viewUrl = rootPath + "/Salary/GetSalaryHistory/?empId=" + _curEmployeeID;

//        $.get(viewUrl, function (data) {
//            dialogDiv.html(data);
//            var $form = $("#DialogForm1");
//            dialogDiv.dialog('open');
//        });
//    }
//}

function UpdateSalary() {
    if (_curEmployeeID) {
        $("#DialogSpace1").dialog("option", "title", "Update Contribution");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Salary/SalaryForm/?empId=" + _curEmployeeID + "&rowID=0";

        $.get(viewUrl, function (data) {
            dialogDiv.html(data);
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            $form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv.dialog('open');
        });
    }
}

function updateSuccessSalaryForm(data) {
    if (data.Success == true) {
        $('#DialogSpace1').dialog('close');
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        $("#dvEmpSalary").html('');
        $("#dvEmpSalary").load('../Salary/GetSalaryHistory/?empId=' + _curEmployeeID);
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}

function updateSuccessPFStatus(data) {
    if (data.Success == true) {
        $('#DialogSpace1').dialog('close');
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        $("#dvEmpPFStatus").html('');
        $("#dvEmpPFStatus").load('../Salary/GetPFStatus/?empId=' + _curEmployeeID);
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}

function EditPFStatus() {

    if (_curEmployeeID) {
        $("#DialogSpace1").dialog("option", "title", "Edit PF Status & Contribution");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Salary/EditPFStatus/?empId=" + _curEmployeeID;

        $.get(viewUrl, function (data) {
            dialogDiv.html(data);
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            $form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv.dialog('open');
        });
    }
}


function CheckAmortization() {
    if (_curEmployeeID) {
        $("#DialogSpace1").dialog("option", "title", "Check Amortization");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Salary/CheckAmortization/?empId=" + _curEmployeeID;

        $.get(viewUrl, function (data) {
            dialogDiv.html(data);
            var $form = $("#DialogForm1");
            dialogDiv.dialog('open');
        });
    }
}
