
function GeneratePFReportRDLC(fileType) {
    var d = new Date();
    var x = document.querySelector('input[name="ReportOptionPF"]:checked').value;
    
    var empID = $('#SelectedEmpID').val();

    if (x == "CheckPrint") {
        if (x == "CheckPrint" && empID == "") {
            ShowModalMessage("Pl's Fill The Employee Id");
            return false;
        }
    }
    else {
        var fromDate = $('#txtFromDate').val();
        var toDate = $('#txtToDate').val();

        //debugger;

        if (Date.parse(fromDate) > Date.parse(toDate)) {
            ShowModalMessage("To Date must be Greater than From Date");
            return false;
        }

        if ((fromDate == "" || toDate == "")) {
            ShowModalMessage("Pl's Fill To Date & From Date");
            return false;
        }

        if (x == "PFMemberContribution" && $("#txtEmpID") == "") {
            ShowModalMessage("Pl's Fill The Employee Id");
            return false;
        }

        if (x == "PFClosedMemberlist") {
            if ((fromDate == "" || toDate == "")) {
                ShowModalMessage("Pl's Fill To Date & From Date");
                return false;
            }

        }
    }
    var viewURL = '../../Report/ReportPF/Report/?id=' + fileType + '&reportOptions=' + x + "&empId=" + empID + "&fromDate=" + fromDate + "&toDate=" + toDate;
    //window.location = viewURL;
    //window.open(viewURL, 'window name', 'window settings');
    FancyBox(viewURL);
    return false;
}