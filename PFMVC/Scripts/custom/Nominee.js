
var CurEmpID = "";

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
            id: "save_btn",
            "class": "btn blue",
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
    });

    
    

});

function updateSuccessNomineeForm(data) {
    if (data.Success == true) {
        $('#DialogSpace1').dialog('close');
        var rbnd = $('#NomineeGrid').data('tGrid');
        rbnd.rebind();
        
        ShowDropDownMessage(data.Message);
        $("#update-message1").hide();
    }
    else {
        $("#update-message1").html(data.ErrorMessage);
        $("#update-message1").show();
    }
}



function NomineeGridOnCommand(e) {
    $(".ui-dialog-buttonpane").show();
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);

    if (e.name == "InsertCommand") {
        $("#DialogSpace1").dialog("option", "title", "Create Branch");
        var dialogDiv = $('#DialogSpace1');
        var viewUrl = rootPath + "/Employee/NomineeForm/?empId=" + CurEmpID;

        $.get(viewUrl, function (data) {

            if (data.Success == false) {
                ShowModalMessage(data.ErrorMessage);
            }
            else {
                dialogDiv.html(data);
                var $form = $("#DialogForm1");
                $form.unbind();
                $form.data("validator", null);
                $.validator.unobtrusive.parse(document);
                $form.validate($form.data("unobtrusiveValidation").options);
                dialogDiv.dialog('open');
            }
        });
        return false;
    }
    else if (e.name == "EditCommand") {
        d = grid.dataItem(row);
       
        $("#DialogSpace1").dialog("option", "title", "Edit Branch");
        var dialogDiv2 = $("#DialogSpace1");

        var viewUrl = rootPath + "/Employee/NomineeForm/?empId=" + CurEmpID+"&nomineeID="+d.NomineeID;

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
        var viewUrl = rootPath + "/Employee/DeletePossible/?empId=" + CurEmpID + "&nomineeID=" + d.NomineeID;

        $.get(viewUrl, function (data) {

            if (data.Success == true) {
                $("#ModalSpace").html("Are you sure you want to delete this record?"),
                $("#ModalSpace").dialog({
                    width: 'auto',
                    resizable: false,
                    modal: true,
                    show: "blind",
                    hide: "blind",

                    title: "Delete " + dataItem.BranchName + "?",
                    buttons: [{
                        text: "Yes", click: function () {
                            viewUrl = rootPath + "/Employee/DeleteConfirm/?empId=" + CurEmpID + "&nomineeID=" + d.NomineeID;
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
        return false;
    }
}

function NomineeGridOnRowSelect(e) {
    var dialogDiv;
    var viewUrl;
    var row = e.row;
    var grid = $(this).data("tGrid");
    var dataItem = grid.dataItem(row);
    imageFileName = dataItem.NomineeImageFileName;
    signFileName = dataItem.NomineeSignFileName;
    currentNomID = dataItem.NomineeID;
    currentEmployeeID = dataItem.EmpID;

   
    $("#dvNomPhoto").html('');
    $("#dvNomSign").html('');
    
    $("#Attachments").removeClass("hidden");
    if (imageFileName) {
        var currentTime = new Date().getTime();
        loadImage("../Picture/NomPhotos/" + imageFileName + ".jpg?lastmod=" + currentTime, 200, 220, "#dvNomPhoto");
     
    }
    else {
        loadImage("../Picture/NomPhotos/image404.jpg", 200, 220, "#dvNomPhoto");
        
    }
    if (signFileName) {
        loadImage("../Picture/NomSignature/" + imageFileName + ".jpg?lastmod=" + currentTime, 200, 80, "#dvNomSign");
    }
    else {
        loadImage("../Picture/NomSignature/image404.jpg", 200, 80, "#dvNomSign");
    }


}

function NomineeGridOnDataBinding(e) {
    CurEmpID = $("#CurEmpID").text();
    e.data = {
        empID : CurEmpID
    };


}

//=========================Photo upload=====================//

function test() {
    $("#NomImgFile").click();
}

function signtest() {
    $("#NomSignFile").click();
}

var currentEmployeeImg = "";
var currentEmployeeID = "";
var currentNomID = "";

function onSuccessSign(e) {
    if (e.response.Success == true) {
        ShowDropDownMessage(e.response.Message);
        $("#dvNomSign").html('');
        var currentTime = new Date().getTime();
        loadImage("../Picture/NomSignature/" + currentEmployeeID + "_" + currentNomID + ".jpg?lastmod=" + currentTime, 200, 100, "#dvNomSign");
        var rbnd = $('#NomineeGrid').data('tGrid');
        rbnd.rebind();
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
    });
    e.data = { nomID: currentNomID, employeeID : currentEmployeeID };


    if (!currentNomID) {
        ShowModalMessage("Please select nominee to change his/her image!")
        e.preventDefault();
        return false;
    }
}

function onSuccess(e) {
    if (e.response.Success == true) {
        ShowDropDownMessage(e.response.Message);
        $("#dvNomPhoto").html('');
        var currentTime = new Date().getTime();
        loadImage("../Picture/NomPhotos/" + currentEmployeeID + "_" + currentNomID + ".jpg?lastmod=" + currentTime, 200, 220, "#dvNomPhoto");
        var rbnd = $('#NomineeGrid').data('tGrid');
        rbnd.rebind();
    }
}

function onError(e) {
    alert("Error (" + e.operation + ") :: " + getFileInfo(e));
    e.preventDefault(); // Suppress error message
}


function loadImage(path, width, height, target) {

    $('<img src="' + path + '">').load(function () {
        $(this).width(width).height(height).appendTo(target);
    });
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

//======================================================================================//

function ShowModalMessage(_message) {
    $("#ModalInformation").html(_message),
               $("#ModalInformation").dialog({
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
                       }
                   }]
               });
}

function ShowDropDownMessage(_message) {

    $('#commonMessage').html(_message);
    $('#commonMessage').delay(400).slideDown(400).delay(3000).slideUp(400);
}