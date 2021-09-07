
_curPFStatusID = '';

function PFComboLoad() {

    var combo = $(this).data('tComboBox'); // $(this) is equivalent to $('#ComboBox')
    combo.select(0);
    _curEmployeeID = null;
}

function PFComboChanged() {
    $('#data').html('');
    var combobox = $(this).data('tComboBox'); // $(this) is equivalent to $('#ComboBox')
    _curPFStatusID = combobox.value();

    if (_curPFStatusID) {
        $("#dvPFRules").html('');
        $("#dvPFRules").load('../PFSettings/GetPFRules/?pfStatusID=' + _curPFStatusID);
    }
}

function updateSuccessPFRules(data) {
    if (data.Success == true) {
        $('#DialogSpace1').dialog('close');
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        //$("#dvEmpSalary").html('');
        //$("#dvEmpSalary").load('../Salary/GetEmployeeSalary/?empId=' + _curEmployeeID);
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}



//common part for all scripts//
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