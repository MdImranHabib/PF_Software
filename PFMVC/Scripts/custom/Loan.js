
var _curEmpID = '';
var _curLoanID = '';


function GetEmployee() {

    _curLoanID = "";
    _curEmpID = $("#SelectedID").val();
    _CurEmpIdentificationNumber = $("#txtEmpID").val();

    if (_curEmpID) {

        var viewUrl = rootPath + "/Employee/EmpIdValidation/?id=" + _CurEmpIdentificationNumber;

        $.get(viewUrl, function (data) {
                if (data.Success == true) {

                    $("#dvEmpDetail").html("");
                    $("#dvLoanHistory").html("");
                    $("#dvLoanPaymentDetail").html("");

                    $("#dvMessage").text('');
                    $("#dvMessage").removeClass("alert alert-danger");

                    var URL = rootPath + "/Employee/GetEmployeeByID/?empId=" + _curEmpID;
                    $.get(URL, function (data) {
                        $("#dvEmpDetail").html('');
                        $("#dvEmpDetail").html(data);
                    });

                    var URL = rootPath + "/Loan/Loan/LoanHistory/?empId=" + _curEmpID;
                    $.get(URL, function (data) {
                        $("#dvLoanHistory").html('');
                        $("#dvLoanHistory").html(data);
                        var yearl = $("#_year").val();
                        if (yearl < 2) {
                            $("#Addloanbtn").hide();  
                        }
                    });
                }
                else {
                    $("#dvMessage").text(data.ErrorMessage);
                    $("#dvMessage").addClass("alert alert-danger");
                    $("#dvEmpDetail").html("");
                    $("#dvLoanHistory").html("");
                    $("#dvLoanPaymentDetail").html("");
                }
            });
        }
    else {
        ShowModalMessage("Select employee to continue...");
    }
}

function AddLoan() {
    if (_curEmpID) {
        $("#DialogSpace1").dialog("option", "title", "Add Loan");
        
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Loan/Loan/LoanForm/?empId=" + _curEmpID;

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
    else {
        ShowModalMessage("Select employee to continue...");
    }
}

function EditLoan() {
    if (_curLoanID) {
        $("#DialogSpace1").dialog("option", "title", "Edit Loan:"+_curLoanID);
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Loan/Loan/LoanForm/?empId=" + _curEmpID+"&pfLoanID="+_curLoanID;

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
    else {
        ShowModalMessage("Select any loan record to continue...");
    }
}

function updateSuccessLoanForm(data) {
    if (data.Success == true) {
        $('#DialogSpace1').dialog('close');
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        
        //refresh loan history grid
        var URL = rootPath + "/Loan/Loan/LoanHistory/?empId=" + _curEmpID;
        $.get(URL, function (data) {
            $("#dvLoanHistory").html('');
            $("#dvLoanHistory").html(data);
        });

        //refresh payment detail grid
        var URL = rootPath + "/Loan/Loan/LoanPaymentDetail/?empId=" + _curEmpID + "&pfLoanID=" + _curLoanID;
        $.get(URL, function (data) {
            $("#dvLoanPaymentDetail").html('');
            $("#dvLoanPaymentDetail").html(data);
        });
        
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}




function DeleteLoan() {
    if (_curLoanID) {

        var viewUrl = rootPath + "/Loan/Loan/DeleteLoanPossible/?empId=" + _curEmpID + "&pfLoanID=" + _curLoanID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html(data.Message),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete Loan?",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Loan/Loan/DeleteLoanConfirm/?empId=" + _curEmpID + "&pfLoanID=" + _curLoanID;
                            $.post(viewUrl, function (data) {

                                if (data.Success == true) {
                                    //$(".ui-dialog-buttonpane").hide();
                                    //$("#ModalSpace").html("Deleted successfully!!");
                                    $("#ModalSpace").dialog('close');
                                    $("#ModalSpace").html('');
                                    ShowDropDownMessage(data.Message);

                                    //refresh partial view
                                    var URL = rootPath + "/Loan/Loan/LoanHistory/?empId=" + _curEmpID;
                                    $.get(URL, function (data) {
                                        $("#dvLoanHistory").html('');
                                        $("#dvLoanHistory").html(data);
                                    });

                                }
                                else {

                                    $("#ModalSpace").html(data.ErrorMessage);
                                }

                            })
                        }
                    }, {
                        text: "Cancel",
                        click: function () {
                            $(this).dialog("close");
                        }
                    }]
                });
            }
            else {
                ShowModalMessage(data.ErrorMessage);
            } //else
        }); //getData
        return false;
    }
    else {
        ShowModalMessage("Please select a loan record to edit/delete this!");
    }

}


$('#txtEmpID').keyup(function (e) {
    
    if (e.keyCode == 13) {
        GetEmployee();
        $('#GetEmployee').focus();
    }
});

