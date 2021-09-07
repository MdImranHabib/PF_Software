



var currentEmployeeImg = "";
var currentEmployeeID = "";



function EmployeeGridOnRowSelect(e) {
    var dialogDiv;
    var viewUrl;
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);
    empImg = dataItem.EmpImg;
    employeeID = dataItem.EmpID;
    currentEmployeeImg = dataItem.EmpImg;
    currentEmployeeID = dataItem.EmpID;

    $("#dvEmpDetail").html('');
    $("#dvEmpPhoto").html('');
    $("#dvEmpDetail").load('GetEmployeeByID/?empId=' + employeeID);

    $('#BSalaryGrid').data('tGrid').ajaxRequest();

    if (empImg) {
        loadImage("../Picture/Photos/" + empImg + ".jpg", 100, 110, "#dvEmpPhoto");
    }
    else {
        loadImage("../Picture/Photos/image404.jpg", 100, 110, "#dvEmpPhoto");
    }

}


function loadImage(path, width, height, target) {

    $('<img src="' + path + '">').load(function () {
        $(this).width(width).height(height).appendTo(target);
    });
}

function EmployeeGridOnCommand(e) {

    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Employee");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Employee/EmployeeForm/"

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
        ID = d.EmpID;
        $("#DialogSpace1").dialog("option", "title", "Edit Employee");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/Employee/EmployeeForm/?id=" + ID;

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
        ID = dataItem.DepartmentID;
        var viewUrl = rootPath + "/Employee/DeletePossible/?id=" + ID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record? "),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete " + dataItem.DepartmentName,
                    buttons: [{
                        text: "Yes",  click: function () {
                            viewUrl = rootPath + "/Employee/DeleteConfirm/?id=" + ID;
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

function BSalaryGridOnCommand(e) {
    return false;
}

var index = 0;
function BSalaryGridOnDataBinding(e) {
    index = 0;
    e.data = $.extend(e.data, { empID: currentEmployeeID });
    var grid = $('#BSalaryGrid').data('tGrid')
    index = (grid.currentPage - 1) * grid.pageSize;
}

function AddSalary() {
    var viewUrl = rootPath + "/Salary/SalaryForm/?empId=" + currentEmployeeID;

    $( "#DialogSpace1" ).dialog( "option", "title", "Employee contribution information" );
    var dialogDiv2 = $("#DialogSpace1");

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

function updateSuccess1(data) {
    if (data.Success == true) {
        var rbnd = $('#EmployeeGrid').data('tGrid');
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

function ShowDropDownMessage(_message) {
    $('#commonMessage').html(_message);
    $('#commonMessage').delay(400).slideDown(400).delay(3000).slideUp(400);
}


function ShowModalMessage(_message) {
    $("#ModalSpace").html(_message),
               $("#ModalSpace").dialog({
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
                       } //cncl btn 
                   }]//btn 
               }); //modal dialog 
}