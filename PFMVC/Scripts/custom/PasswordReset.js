
$(document).ready(function () {
    $('#password_reset').dialog({
        autoOpen: false,
        width: 'auto',
        resizable: false,
        modal: true,
        show: "Highlight",
        hide: "Highlight",
        buttons: [{
            text: "Save",
            id: "save_btn_pass_reset",
            "class": "btn blue",
            click: function () {
                $("update-message1").html(''); //make sure there is nothing on the message before we continue                         
                $("#DialogForm-pr").submit();
            }
        },
        {
            text: "Cancel",
            click: function () {
                $(this).dialog("close");
            }
        }]
    }).live("keyup", function (e) {

        if (e.keyCode === 13) {
            $('#save_btn_pass_reset').click();
        }
    });
})

function password_change(context) {

    $("#password_reset").dialog("option", "title", "Changing password for: " + context.UserName);
    var dialogDiv2 = $("#password_reset");

    var viewUrl = rootPath + "/UserManagement/ResetPassword/?userID=" + context.UserID + "&UserName=" + context.UserName;

    $.get(viewUrl, function (data) {
        dialogDiv2.html(data);
        $(".ui-dialog-buttonpane").show();
        var $form = $("#DialogForm-pr");

        $form.unbind();
        $form.data("validator", null);

        $.validator.unobtrusive.parse(document);

        $form.validate($form.data("unobtrusiveValidation").options);

        dialogDiv2.dialog('open');
    });
}

function updateSuccess_pr(data) {
    if (data.Success == true) {
        ShowDropdownMessage(data.Message);
        $("#update-message-pr").hide();
        $('#password_reset').dialog('close');
    }
    else {
        $("#update-message-pr").html(data.ErrorMessage);
        $("#update-message-pr").show();
    }
}

function ShowDropdownMessage(_message) {
    $('#commonMessage').html(_message);
    $('#commonMessage').delay(400).slideDown(400).delay(3000).slideUp(400);
}