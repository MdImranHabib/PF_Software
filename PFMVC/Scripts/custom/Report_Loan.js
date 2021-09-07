

function GenerateLoanReportRDLC(fileType) {


    var d = new Date();

    var x = document.querySelector('input[name="ReportOptionLoan"]:checked').value;

    var fromDate = $('#txtFromDateLoan').val();
    var toDate = $('#txtToDateLoan').val();
    var empID = $('#SelectedEmpID').val();
    var loanID = $('#txtLoanID').val();

    if (Date.parse(fromDate) > Date.parse(toDate)) {
        ShowModalMessage("To Date must be Greater than From Date");
    }

    //==For payment detail load id is required
    if (x == "PaymentDetail") {

        if (loanID) {

        }
        else {
            ShowModalMessage("Please select loand ID to get payment detail...");
            return false;
        }
    }

    if (x == "LoanListWithEmpId") {
        x == "PaymentDetail";
    }
    //===============================

    var viewURL = rootPath + '../../Report/ReportLoan/Report/?id=' + fileType + '&reportOptions=' + x + "&empId=" + empID + "&fromDate=" + fromDate + "&toDate=" + toDate+"&loanID="+loanID;
    //window.location = viewURL;
    //window.open(viewURL, 'window name', 'window settings')
    FancyBox(viewURL);
}