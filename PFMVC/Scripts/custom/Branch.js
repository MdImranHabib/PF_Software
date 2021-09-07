

function updateSuccess1(data) {
    if (data.Success == true) {
        var rbnd = $('#BranchGrid').data('tGrid');
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


var branchID;
function onCommand(e) {
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Branch");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Branch/InsertBranchForm/"

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
        var branchID = d.BranchID;
        $("#DialogSpace1").dialog("option", "title", "Edit Branch");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/Branch/BranchForm/?id=" + branchID;

        $.get(viewUrl, function (data) {
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
        branchID = dataItem.BranchID;
        var viewUrl = rootPath + "/Branch/DeletePossible/?id=" + branchID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record?"),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete "+dataItem.BranchName+"?",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Branch/DeleteConfirm/?id=" + branchID;
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

                                    $("#ModalSpace").html(data.ErrorMessage);
                                    //ShowDropDownMessage(data.ErrorMessage);
                                }

                            })
                        }},{
                        text: "Cancel",
                        "class" : "btn red",
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
}