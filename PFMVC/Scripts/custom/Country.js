


function updateSuccess1(data) {
    if (data.Success == true) {
        var rbnd = $('#CountryGrid').data('tGrid');
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


var countryID;
function onCommand(e) {
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Country");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Country/CountryForm/"

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
        countryID = d.CountryID;
        $("#DialogSpace1").dialog("option", "title", "Edit Country");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/Country/CountryForm/?id=" + countryID;

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
        countryID = dataItem.CountryID;
        var viewUrl = rootPath + "/Country/DeletePossible/?id=" + countryID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete "+dataItem.Country,
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Country/DeleteConfirm/?id=" + countryID;
                            $.post(viewUrl, function (data) {
                                
                                if (data.Success == true) {
                                    $("#ModalSpace").html('');
                                    $("#ModalSpace").dialog('close');
                                    ShowDropDownMessage(data.Message);
                                    grid.ajaxRequest();
                                }
                                else {

                                    $("#ModalSpace").html("Something Problem while deleting!!!");
                                }

                            })
                        }},{
                        text:"Cancel", "class": "btn red", click: function () {
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