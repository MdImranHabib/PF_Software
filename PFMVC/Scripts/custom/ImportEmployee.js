
function onUpload(e) {
    // Array with information about the uploaded files
    var files = e.files;
    // Check the extension of each file and abort the upload if it is not .jpg
    $.each(files, function () {
        
        if (this.extension == ".xls" || this.extension == ".xlsx") {
         
        }
        else {
            alert("Only .xls or .xlsx files can be uploaded")
            e.preventDefault();
            return false;
        }
    });
    
}

function onSuccess(e) {
    alert("onsuccess");
    if (e.response.Success == true) {
        ShowDropDownMessage(e.response.Message);
        $("#dvPreImport").html('');
        
    }
}

function onError(e) {
    alert("onerror");
    $("#dvPreImport").load(e.response);
    e.preventDefault(); // Suppress error message
}

//Added by Avishek Date:May-13-2015
$(document).ready(function () {
    $("#ui-datepicker-div").hide();
    $("#Radio").attr("checked", false);
    $("#EmployeeId").autocomplete({
        source: rootPath + "/Loan/Import/AutocompleteSuggestionsForEmpId",
        minLength: 1,
        select: function (event, ui) {
            $("#EmployeeId").val(ui.item.label);
            $("#LoanId").val(ui.item.value);
            $("#Employee").val(ui.item.label);
            $("#Loan").val(ui.item.value);
            GetInstallNoAndAmount();
            return false;
        }
    });

    $("#LoanId").autocomplete({
        source: rootPath + "/Loan/Import/AutocompleteSuggestionsForLoanId",
        minLength: 1,
        select: function (event, ui) {
            $("#EmployeeId").val(ui.item.value);
            $("#LoanId").val(ui.item.label);
            $("#Employee").val(ui.item.value);
            $("#Loan").val(ui.item.label);
            GetInstallNoAndAmount();
            return false;
        }
    });

});

function GetInstallNoAndAmount() {
        var loanId = $("#Loan").val().toString();
        var empId = $("#Employee").val().toString();
    $.ajax({
        type: 'POST',
        url: rootPath + "/Loan/Import/GetAmountAndInstallmentNo",
        data:  { empId: empId, loanId: loanId },
        dataType: "json",
        async: false,
        success: function (data) {
            $("#InstallmentAmount").val(data.PaymentAmount);
            $("#InstallmentNo").val(data.InstallmentNumber);
            $("#Balance").val(data.Amount);
            $("#Interest").val(data.Interest);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(XMLHttpRequest + ": " + textStatus + ": " + errorThrown, 'Error!!!');
        }
    });
}


function Save() {
    var radio = $('input:radio[name=Radio]:checked').val();
    if (radio == "Yes") {
        var lst_vm_pfLoanPayment = {
            IdentificationNumber: $("#Employee").val(),
            LoanID: $("#Loan").val(),
            PrincipalAmount: $("#Balance").val(),
            Interest: $("#Interest").val(),
            InstallmentNumber: $("#InstallmentNo").val(),
            PaymentDate: $("#PaymentMonth").val()
        }
        $.ajax({
            type: 'POST',
            url: rootPath + "/Loan/Import/SaveForLoanSettlement",
            data: JSON.stringify(lst_vm_pfLoanPayment),
            contentType: "application/json",
            dataType: "json",
            async: false,
            success: function (data) {
                ShowDropDownMessage(data.Message);
                $("#dvPreImport").html('');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert(XMLHttpRequest + ": " + textStatus + ": " + errorThrown, 'Error!!!');
            }
        });

    }
    else {
        var lst_vm_pfLoanPayment = {
            IdentificationNumber: $("#Employee").val(),
            LoanID: $("#Loan").val(),
            InstallmentAmount: $("#InstallmentAmount").val(),
            InstallmentNumber: $("#InstallmentNo").val(),
            PaymentDate: $("#PaymentMonth").val()
        }
        $.ajax({
            type: 'POST',
            url: rootPath + "/Loan/Import/Save",
            data: JSON.stringify(lst_vm_pfLoanPayment),
            contentType: "application/json",
            dataType: "json",
            async: false,
            success: function (data) {
                ShowDropDownMessage(data.Message);
                $("#dvPreImport").html('');
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                alert(XMLHttpRequest + ": " + textStatus + ": " + errorThrown, 'Error!!!');
            }
        });
    }
}



//End
