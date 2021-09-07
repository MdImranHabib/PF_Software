


function updateSuccess1(data) {
    if (data.Success == true) {
        var rbnd = $('#DesignationGrid').data('tGrid');
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


var ID;
function onCommand(e) {

    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Designation");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Designation/DesignationForm/"

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
        ID = d.DesignationID;
        $("#DialogSpace1").dialog("option", "title", "Edit Designation");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/Designation/DesignationForm/?id=" + ID;

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
        ID = dataItem.DesignationID;
        var viewUrl = rootPath + "/Designation/DeletePossible/?id=" + ID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete "+dataItem.Designation+"?",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Designation/DeleteConfirm/?id=" + ID;
                            $.post(viewUrl, function (data) {
                                $(".ui-dialog-buttonpane").show();
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
                        }},{
                        text: "Cancel", "class":"btn red",click: function () {
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