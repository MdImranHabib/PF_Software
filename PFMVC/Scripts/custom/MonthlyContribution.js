
var selectedDate = "";

$(function () {
    $('#txtDate').datepicker({
        changeMonth: true,
        changeYear: true,
        showButtonPanel: true,
        dateFormat: 'MM yy',
        onClose: function (dateText, inst) {
            var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
            var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
            $(this).datepicker('setDate', new Date(year, month, 1));
            //alert(01+"/"+(parseInt(month)+1)+"/"+year);
            selectedDate = 01 + "/" + (parseInt(month) + 1) + "/" + year;
        }
    });
});


function MonthlyContributionInformation() {
    if (selectedDate) {
        $("#Partial1").load('../Salary/MonthlyContributionInformation/?dtMonth=' + selectedDate);
    }
    else {
        ShowModalMessage("Please select date!");
    }
}

function updateSuccessSalaryProcess(data) {
    if (data.Success == true) {
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        $("#Partial1").load('../Salary/MonthlyContributionInformation/?dtMonth=' + selectedDate);
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}










//=============================//common part for all scripts//====================================//
$(document).ready(function () {

    $('#DialogSpace1').dialog({
        autoOpen: false,
        width: 'auto',
        resizable: false,
        modal: true,
        show: "Highlight",
        hide: "Highlight",
        buttons: [{
            text: "Save",
            "class": "btn blue",
            id: "save_btn",
            click: function () {
                $("update-message1").html(''); //make sure there is nothing on the message before we continue                         
                $("#DialogForm1").submit();
            }
        },
        {
            text: "Cancel",
            "class": "btn red",
            click: function () {
                $(this).dialog("close");
            }
        }]
    }).live("keyup", function (e) {

        if (e.keyCode === 13) {
            $('#save_btn').click();
        }
    });;

});

function ShowModalMessage(_message) {
    $("#ModalInformation").html(_message),
               $("#ModalInformation").dialog({
                   width: 'auto',
                   resizable: false,
                   modal: true,
                   show: "highlight",
                   hide: "highlight",
                   title: "Confirmation",
                   buttons: [{
                       "class": "btn red",
                       text: "OK",
                       click: function () {
                           $(this).dialog("close");
                       }
                   }]
               });
}

function ShowDropDownMessage(_message) {

    $('#commonMessage').html(_message);
    $('#commonMessage').delay(400).slideDown(400).delay(3000).slideUp(400);
}

//====================End of common=========================================//