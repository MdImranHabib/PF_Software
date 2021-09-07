
var curUserName = "";

$(document).ready(function () {

    //$("#tabs-1").niceScroll({"autohidemode":false});

    var gridHidCol = $("#UserGrid").data("tGrid");
    //gridHidCol.hideColumn("FullName");
    gridHidCol.hideColumn("Email");
    gridHidCol.hideColumn("Phone");

});



function UGOnRowSelect(e) {
    var dialogDiv;
    var viewUrl;
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);
    currentUserName = dataItem.UserName;
    curUserName = dataItem.UserName;

    $("#RoleModuleDiv").html('');
    $("#UserRolesDiv").html('');
    $("#UserRolesDiv").load('/UserManagement/UserManagement/GetRoleList/?userName=' + dataItem.UserName);
}

function URGOnCommand(e) {
    //var roleName = e.row.cells[1].innerHTML; //$(this).closest('td').siblings(':first-child').next().html();//
    //alert("tanvir"+roleName);
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertRole") {
        ShowModalMessage("Adding new role disabled");
        return false;
        $("#DialogSpace1").dialog("option", "title", "Create New Role");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/UserManagement/AddRole/?id=0";

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
    }

    return false;

}

function UGOnCommand(e) {
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {

        $("#DialogSpace1").dialog("option", "title", "Create New User");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/UserManagement/UserManagement/Register/";

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
    }

    else if (e.name == "EditCommand") {
        d = grid.dataItem(row);
        var usrId = d.UserId;
        $("#DialogSpace1").dialog("option", "title", "Edit UserInfo");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/UserManagement/UserManagement/GetUserInfo/?id=" + usrId;

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
    }

    else if (e.name == "DeleteCommand") {
        var userID = dataItem.UserId;
        var userName = dataItem.UserName;

        var viewUrl = rootPath + "/UserManagement/UserManagement/UserDeletePossible/?userID=" + userID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record?"),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "User Name: " + dataItem.UserFullName,
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/UserManagement/UserDeleteConfirm/?username=" + userName;
                            $.post(viewUrl, function (data) {

                                if (data.Success == true) {
                                    //$(".ui-dialog-buttonpane").hide();
                                    //$("#ModalSpace").html("Deleted successfully!!");
                                    $("#ModalSpace").dialog('close');
                                    $("#ModalSpace").html('');
                                    ShowModalMessage("Deleted successfully!!");
                                    grid.ajaxRequest();
                                }
                                else {

                                    $("#ModalSpace").html("Something Problem while deleting!!!");
                                }

                            })
                        }
                    }, {
                        text: "Cancel",
                        "class": "btn red",
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
    }
    return false;
}



function updateSuccessUI(data) {

    if (data.Success == true) {

        var rbnd = $('#UserGrid').data('tGrid');
        rbnd.rebind();
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
        $('#DialogSpace1').dialog('close');
        $("#update-message-rm").html(''); //role module
        $("#update-message-ur").html(''); //user role
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}

function updateSuccessRM(data) {
    if (data.Success == true) {
        ShowDropDownMessage(data.Message);
        $("#update-message-rm").html('');
        $('#DialogSpace1').dialog('close');
    }
    else {
        $("#update-message-rm").html(data.ErrorMessage);
        $("#update-message-rm").show();
    }
}

function updateSuccessUR(data) {

    if (data.Success == true) {
        ShowDropDownMessage(data.Message);
        $("#update-message-ur").hide();

    }
    else {
        $("#update-message-ur").html(data.ErrorMessage);
        $("#update-message-ur").show();
        $("#update-message-ur").addClass("alert alert-danger");
    }
}

function updateSuccessUR_add(data) {
    $(".validation-summary-errors").hide();
    if (data.Success == true) {
        ShowDropDownMessage(data.Message);
        $("#update-message-ur").hide();
        $('#DialogSpace1').dialog('close');
        $("#UserRolesDiv").load('UserManagement/GetRoleList/?userName=' + curUserName);
    }
    else {
        $("#update-message-ur").html(data.ErrorMessage);
        $("#update-message-ur").show();
    }
}





$(".GetModule").live('click', function () {
    var roleName = $(this).closest('td').siblings(':first-child').next().html();//e.row.cells[1].innerHTML;
    //$("#RoleModuleDiv").html('');
    //$("#RoleModuleDiv").load('UserManagement/GetModuleList/?roleName=' + roleName);

    $("#DialogSpace1").dialog("option", "title", "Access Module");
    var dialogDiv2 = $("#DialogSpace1");

    var viewUrl = rootPath + "/UserManagement/UserManagement/GetModuleList/?roleName=" + roleName;

    $.get(viewUrl, function (data) {
        dialogDiv2.html(data);
        $(".ui-dialog-buttonpane").show();
        dialogDiv2.dialog('open');
    });

})