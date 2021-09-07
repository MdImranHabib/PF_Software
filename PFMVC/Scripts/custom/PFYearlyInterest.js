

var rowid = 0;

function PFYearlyInterestGridOnCommand(e) {

    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Rule");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/PFSettings/YearlyInterest/YearlyInterestForm/";

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
        var conMonth = d.ConMonth;
        var conYear = d.ConYear;

   
        $("#DialogSpace1").dialog("option", "title", "Interest Rate");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/PFSettings/YearlyInterest/YearlyInterestForm/?conMonth=" + conMonth + "&conYear=" + conYear;

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
        var viewUrl = rootPath + "/PFSettings/YearlyInterest/PFRuleDeletePossible/?rowid=" + _rowid;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to de-activate this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete " + dataItem.DepartmentName,
                    buttons: [{
                        text: "Yes",  click: function () {
                            viewUrl = rootPath + "/PFSettings/YearlyInterest/PFRuleDeleteConfirm/?rowid=" + _rowid;
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
                        text: "Cancel", "class": "btn red", click: function () {
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


function updateSuccessYearlyInterestForm(data) {
    if (data.Success == true) {
        var rbnd = $('#PFYearlyInterestGrid').data('tGrid');
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