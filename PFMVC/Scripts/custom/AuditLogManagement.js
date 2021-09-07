
var curUserName = "";

$(document).ready(function () {

    var gridHidCol = $("#UserGrid").data("tGrid");
    //gridHidCol.hideColumn("FullName");


});



//function UGOnRowSelect(e) {
//    var dialogDiv;
//    var viewUrl;
//    var row = e.row;
//    var grid = $(this).data("tGrid");
//    var dataItem = grid.dataItem(row);
//    currentUserName = dataItem.UserName;
//    curUserName = dataItem.UserName;

//    $("#RoleModuleDiv").html('');
//    $("#UserRolesDiv").html('');
//    $("#UserRolesDiv").load('/UserManagement/UserManagement/GetRoleList/?userName=' + dataItem.UserName);
//}

//function URGOnCommand(e) {
//    //var roleName = e.row.cells[1].innerHTML; //$(this).closest('td').siblings(':first-child').next().html();//
//    //alert("tanvir"+roleName);
//    $(".ui-dialog-buttonpane").show();
//    var row = e.row;
//    var grid = $(this).data("tGrid");
//    var dataItem = grid.dataItem(row);

//    if (e.name == "InsertRole") {
//        ShowModalMessage("Adding new role disabled");
//        return false;
//        $("#DialogSpace1").dialog("option", "title", "Create New Role");
//        var dialogDiv2 = $("#DialogSpace1");

//        var viewUrl = rootPath + "/UserManagement/AddRole/?id=0";

//        $.get(viewUrl, function (data) {
//            dialogDiv2.html(data);
//            $(".ui-dialog-buttonpane").show();
//            var $form = $("#DialogForm1");

//            $form.unbind();
//            $form.data("validator", null);

//            $.validator.unobtrusive.parse(document);

//            $form.validate($form.data("unobtrusiveValidation").options);

//            dialogDiv2.dialog('open');
//        });
//    }

//    return false;

//}

function UGOnCommand(e) {
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {

        $("#DialogSpace1").dialog("option", "title", "Create New Audit");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/UserManagement/AuditLog/AddNewAudit/";

        $.get(viewUrl, function (data) {
            dialogDiv2.html(data);
            $(".ui-dialog-buttonpane").show();
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            //$form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv2.dialog('open');
        });
    }
   

    else if (e.name == "EditCommand") {
        d = grid.dataItem(row);
        var logID = d.LogID;
        $("#DialogSpace1").dialog("option", "title", "Edit Audit Lock");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/UserManagement/AuditLog/GetAuditLockInfo/?id=" + logID;

        $.get(viewUrl, function (data) {
            dialogDiv2.html(data);
            $(".ui-dialog-buttonpane").show();
            var $form = $("#DialogForm1");

            $form.unbind();
            $form.data("validator", null);

            $.validator.unobtrusive.parse(document);

            //$form.validate($form.data("unobtrusiveValidation").options);

            dialogDiv2.dialog('open');
        });
    }

    else if (e.name == "DeleteCommand") {
        var logID = dataItem.LogID;
        var viewUrl = rootPath + "/UserManagement/AuditLog/AuditLogDeletePossible/?id=" + logID;

        $.get(viewUrl, function (data) {
            //debugger;
            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto', resizable: false,
                    modal: true, show: "blind",
                    hide: "blind", title: "Delete",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/UserManagement/AuditLog/DeleteConfirm/?id=" + logID;
                                //"/AccountMapping/DeleteConfirm/?id=" + id;
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


function updateSuccessUI(data) {

    if (data.Success == true) {

        var rbnd = $('#AuditLogGrid').data('tGrid');
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

//function updateSuccessRM(data) {
//    if (data.Success == true) {
//        ShowDropDownMessage(data.Message);
//        $("#update-message-rm").html('');
//        $('#DialogSpace1').dialog('close');
//    }
//    else {
//        $("#update-message-rm").html(data.ErrorMessage);
//        $("#update-message-rm").show();
//    }
//}

//function updateSuccessUR(data) {

//    if (data.Success == true) {
//        ShowDropDownMessage(data.Message);
//        $("#update-message-ur").hide();

//    }
//    else {
//        $("#update-message-ur").html(data.ErrorMessage);
//        $("#update-message-ur").show();
//        $("#update-message-ur").addClass("alert alert-danger");
//    }
//}

//function updateSuccessUR_add(data) {
//    $(".validation-summary-errors").hide();
//    if (data.Success == true) {
//        ShowDropDownMessage(data.Message);
//        $("#update-message-ur").hide();
//        $('#DialogSpace1').dialog('close');
//        $("#UserRolesDiv").load('UserManagement/GetRoleList/?userName=' + curUserName);
//    }
//    else {
//        $("#update-message-ur").html(data.ErrorMessage);
//        $("#update-message-ur").show();
//    }
//}



//$(".GetModule").live('click', function () {
//    var roleName = $(this).closest('td').siblings(':first-child').next().html();//e.row.cells[1].innerHTML;
//    //$("#RoleModuleDiv").html('');
//    //$("#RoleModuleDiv").load('UserManagement/GetModuleList/?roleName=' + roleName);

//    $("#DialogSpace1").dialog("option", "title", "Access Module");
//    var dialogDiv2 = $("#DialogSpace1");

//    var viewUrl = rootPath + "/UserManagement/UserManagement/GetModuleList/?roleName=" + roleName;

//    $.get(viewUrl, function (data) {
//        dialogDiv2.html(data);
//        $(".ui-dialog-buttonpane").show();
//        dialogDiv2.dialog('open');
//    });

//})