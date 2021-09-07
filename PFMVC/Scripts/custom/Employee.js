
$(document).ready(function () {
    inp = $("#branch_name option:selected").text();
    if (inp == "All Branch") {
        $("#branch_name").attr('disabled', false);
    }
    else $("#branch_name").attr('disabled', true);

    FilterTable2();
    $("#branch_name").change(function () {
        // var selectedBranch = $("#branch_name option:selected").text();
        FilterTable2();

    });
});

function FilterTable2() {
    index = -1;
    inp = $("#branch_name option:selected").text();
    if (inp == "All Branch") {
        inp = "";
    }
    $("#data:visible tr:not(:has(>th))").each(function () {
        if (~$(this).text().toLowerCase().indexOf(inp.toLowerCase())) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
    $('#Hedding').show();
};

function getBrachShortNamebyBranchID() {

    var branchId = $("#branch-name").val();
    $.ajax({
        url: rootPath + "/Employee/GetList",
        type: 'post',
        dataType: 'json',
        data: { BranchID: branchId },
        success: function (result) {
            $("#shortName").val(result.BranchLocation + "-");
            
        },
        error: function (result) {
            alert("Error Ocured");
        }
    });
}

function test() {
    $("#EmpImgFile").click();
}

function signtest() {
    $("#EmpSignFile").click();
}

var currentEmployeeImg = "";
var currentEmployeeID = "";


function onSuccessSign(e) {
    if (e.response.Success == true) {
        ShowDropDownMessage(e.response.Message);
        $("#dvEmpSign").html('');
        var currentTime = new Date().getTime();
        loadImage("../Picture/Signature/" + currentEmployeeID + ".jpg?lastmod=" + currentTime, 200, 100, "#dvEmpSign");
        $('#EmployeeGrid tr.t-state-selected .empIMG').text(currentEmployeeID);
    }
}


function onUpload(e) {
    // Array with information about the uploaded files
    var files = e.files;
    // Check the extension of each file and abort the upload if it is not .jpg
    $.each(files, function () {
        if (this.extension != ".jpg") {
            alert("Only .jpg files can be uploaded")
            e.preventDefault();
            return false;
        }
        if (this.size > 200000) {
            alert("OOops! file size too big! Max 200 KB allowed!")
            e.preventDefault();
            return false;
        }
    });
    e.data = { employeeID: currentEmployeeID };


    if (!currentEmployeeID) {
        ShowModalMessage("Please select employee to change his/her image!")
        e.preventDefault();
        return false;
    }
}

function onSuccess(e) {
    if (e.response.Success == true) {
        ShowDropDownMessage(e.response.Message);
        $("#dvEmpPhoto").html('');
        var currentTime = new Date().getTime();
        loadImage("../Picture/Photos/" + currentEmployeeID + ".jpg?lastmod=" + currentTime, 200, 220, "#dvEmpPhoto");
        $('#EmployeeGrid tr.t-state-selected .empIMG').text(currentEmployeeID);
    }
}

function onError(e) {
    alert("Error (" + e.operation + ") :: " + getFileInfo(e));
    e.preventDefault(); // Suppress error message
}

function getFileInfo(e) {
    return $.map(e.files, function (file) {
        var info = file.name;

        // File size is not available in all browsers
        if (file.size > 0) {
            info += " (" + Math.ceil(file.size / 1024) + " KB)";
        }
        return info;
    }).join(", ");
}

function EmployeeGridOnRowSelect(e) {
    var dialogDiv;
    var viewUrl;
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);
    empImg = $('#EmployeeGrid tr.t-state-selected .empIMG').text();//dataItem.EmpImg;
    employeeID = dataItem.EmpID;
    currentEmployeeImg = dataItem.EmpImg;
    currentEmployeeID = dataItem.EmpID;

    $("#dvEmpDetail").html('');
    $("#dvEmpPhoto").html('');
    $("#dvEmpSign").html('');
    $("#dvEmpDetail").load('../Employee/GetEmployeeByID/?empId=' + employeeID);
    $("#Attachments").removeClass("hidden");
    var currentTime = new Date().getTime();

    if (empImg) {
        loadImage("../Picture/Photos/" + empImg + ".jpg?lastmod=" + currentTime, 200, 220, "#dvEmpPhoto");
        loadImage("../Picture/Signature/" + empImg + ".jpg?lastmod=" + currentTime, 200, 80, "#dvEmpSign");
    }
    else {
        loadImage("../Picture/Photos/null.jpg?lastmod=" + currentTime, 200, 220, "#dvEmpPhoto");
        loadImage("../Picture/Signature/null.jpg?lastmod=" + currentTime, 200, 80, "#dvEmpSign");
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
        debugger;
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
        var empID = dataItem.EmpID;

        var viewUrl = rootPath + "/Employee/DeletePossible/?id=" + empID;

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
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Employee/DeleteConfirm/?id=" + empID;
                            $.post(viewUrl, function (data) {

                                if (data.Success == true) {


                                    $("#ModalSpace").dialog('close');
                                    $("#ModalSpace").html('');
                                    ShowDropDownMessage(data.Message);
                                    //clear the related info
                                    $("#dvEmpDetail").html('');
                                    $("#dvEmpPhoto").html('');
                                    $("#dvEmpSign").html('');
                                    $("#Attachments").addClass("hidden");

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
