
function alertt(data) {
    if (data.Success == true) {
        ShowDropDownMessage(data.Message);
        $("#button" + data.LoanID).disabled = true;
        $("#button" + data.LoanID).text("- Paid -");
        $("#button" + data.LoanID).removeClass("btn-warning").addClass("btn-default");
    }
    else {
        ShowModalMessage(data.ErrorMessage);
    }
}

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