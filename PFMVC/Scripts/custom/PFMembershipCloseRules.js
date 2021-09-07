

var rowid = 0;

function PFMCRulesGridOnCommand(e) {

    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Rule");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/PFSettings/MembershipClosed/MembershipClosingRulesForm/?rowid=" + 0;

        $.get(viewUrl, function (data) {
            dialogDiv.html(data);
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            $form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv.dialog('open');
        });
        return false;
    }
    else if (e.name == "EditCommand") {
        
        d = grid.dataItem(row);
        _rowid = d.ROWID;
        _status = d.status;
           

        if (_status == "Deactivated") {
            ShowModalMessage("Deactivated rule cannot be edited...");
            return false;
        }
        $("#DialogSpace1").dialog("option", "title", "Edit Rule");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/PFSettings/MembershipClosed/MembershipClosingRulesForm/?rowid=" + _rowid;

        $.get(viewUrl, function (data) {
            if (data.Success == false) {

                ShowModalMessage(data.ErrorMessage);
                return false;
            }
            dialogDiv2.html(data);
            $(".ui-dialog-buttonpane").show();
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            $form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv2.dialog('open');
        });
        return false;
    }
    else if (e.name == "DeleteCommand") {
        _rowid = dataItem.ROWID;
        var viewUrl = rootPath + "/PFSettings/MembershipClosed/PFMembershipClosingRuleDeletePossible/?rowid=" + _rowid;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html(data.Message),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete " + dataItem.DepartmentName,
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/PFSettings/MembershipClosed/PFMembershipClosingRuleDeleteConfirm/?rowid=" + _rowid;
                            $.post(viewUrl, function (data) {

                                if (data.Success == true) {
                                    //$(".ui-dialog-buttonpane").hide();
                                    //$("#ModalSpace").html("Deleted successfully!!");

                                    $("#ModalSpace").dialog('close');
                                    $("#ModalSpace").html('');
                                    ShowDropDownMessage(data.Message);
                                    grid.ajaxRequest();
                                }
                                else {

                                    $("#ModalSpace").html("Something Problem while deleting!!!");
                                }

                            })
                        }
                    }, {
                        text: "Cancel", click: function () {
                            $(this).dialog("close");
                        }
                    }]
                });
            }
            else {
                ShowModalMessage(data.ErrorMessage);
            }
        });
        return false;
    }
}


function updateSuccessPFMCRulesForm(data) {
    if (data.Success == true) {
        var rbnd = $('#PFRulesGrid').data('tGrid');
        rbnd.rebind();
        $('#DialogSpace1').dialog('close');
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}