


function MISGridOnCommand(e) {
    debugger;
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create MIS");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/AccountMapping/MISForm/"

        $.get(viewUrl, function (data) {
            debugger;
                debugger;
                dialogDiv.html(data);
                var $form = $("#DialogForm1");

                $form.unbind();
                $form.data("validator", null);

                $.validator.unobtrusive.parse(document);

                $form.validate($form.data("unobtrusiveValidation").options);
                dialogDiv.dialog('open');

                $.post(viewUrl, function (data) {
                    alert(data.Message);
                    alert(data.Success);
                    if (data.Message == "First") {
                        return;
                    }
                    if (data.Success == true) {
                        debugger;
                        
                        $("#ModalSpace").dialog('close');
                        $("#ModalSpace").html('');
                        ShowDropDownMessage(data.Message);
                        grid.ajaxRequest();
                        $("#loading-indicator").hide();
                    } else {
                        $("#ModalSpace").dialog('close');
                        $("#ModalSpace").html('');
                        ShowDropDownMessage(data.Message);
                        grid.ajaxRequest();
                        $("#loading-indicator").hide();
                    }
                })
           
        });
        return false;
    }
    else if (e.name == "EditCommand") {
        d = grid.dataItem(row);
        ID = d.id;
        $("#DialogSpace1").dialog("option", "title", "Edit MIS");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/AccountMapping/MISForm/?id=" + ID;

        $.get(viewUrl, function (data) {
            dialogDiv2.html(data);
            $(".ui-dialog-buttonpane").show();
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            $form.validate($form.data("unobtrusiveValidation").options);
            //$(this).dialog("close");
            dialogDiv2.dialog('open');            
        });
        return false;
    }
    else if (e.name == "DeleteCommand") {
        var id = dataItem.id;
        var viewUrl = rootPath + "/AccountMapping/DeletePossible/?id=" + id;

        $.get(viewUrl, function (data) {
            debugger;
            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",
                    title: "Delete",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/AccountMapping/DeleteConfirm/?id=" + id;
                            $.post(viewUrl, function (data) {
                                if (data.Success == true) {
                                    $("#ModalSpace").dialog('close');
                                    $("#ModalSpace").html('');
                                    ShowDropDownMessage(data.Message);
                                    grid.ajaxRequest();
                                }
                                else {
                                    $("#ModalSpace").html("Some Problem occure while deleting!!!");
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