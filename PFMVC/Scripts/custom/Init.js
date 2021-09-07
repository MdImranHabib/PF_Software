//$('.dateTimePicker').datepicker({ dateFormat: "dd/M/yy", changeYear: true, changeMonth: true });
//$('.dateTimePickerNoFuture').datepicker({ dateFormat: "dd/M/yy", changeYear: true, changeMonth: true, maxDate: "0D" });

$(".dateTimePicker").unbind();
$(".dateTimePickerNoFuture").unbind();
$(".dateTimePickerNoFuture").live("focus", function () {
    $('.dateTimePickerNoFuture').datepicker({
        dateFormat: "dd/M/yy", changeYear: true, changeMonth: true, maxDate: "0D"
    });
});

$(".dateTimePicker").live("focus", function () {
    $('.dateTimePicker').datepicker({
        dateFormat: "dd/M/yy", changeYear: true, changeMonth: true
    });
});


//=============================//common part for all scripts//====================================//

$(".NumOnly").live("keypress", function (e) {
    //if the letter is not digit then display error and don't type anything
    if (e.which != 8 && e.which != 0 && e.which != 13 && (e.which < 48 || e.which > 57) && e.which != 46) {
        $(this).notify("Please enter number only", {autoHideDelay: 2000});
        return false;
    }
});

function AccFormatMoney(value) {
    return accounting.formatMoney(value, "");
}

$(document).ready(function () {

    $("input[type=text]").focus().select();

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
            "class":"Next",
            click: function () {
                $("update-message1").html(''); //make sure there is nothing on the message before we continue                         
                $("#DialogForm1").submit();
            }
        },
        {
            text: "Cancel",
            click: function () {
                $(this).dialog("close");
            }
        }]
    })//.live("keyup", function (e) {
        //if (e.which == 13) {
          //  $('#save_btn').click();
        //}
    //});


    $(document).ajaxSend(function (event, request, settings) {
        $('#loading-indicator').show();
    });

    $(document).ajaxComplete(function (event, request, settings) {
        $('#loading-indicator').hide();
    });

    //To show cross icon inside text box
    function tog(v) { return v ? 'addClass' : 'removeClass'; }
    $(document).on('input', '.clearable', function () {
        $(this)[tog(this.value)]('x');
    }).on('mousemove', '.x', function (e) {
        $(this)[tog(this.offsetWidth - 18 < e.clientX - this.getBoundingClientRect().left)]('onX');
    }).on('click', '.onX', function () {
        $(this).removeClass('x onX').val('');
    });

    $('input[type=text]').live('focus', function () {
        if (!($(this).hasClass('clearable'))) {
            $(this).addClass('clearable');
        }
    });

    $('.Next').live("keyup", function (e) {
        var n = $(".Next").length;

        if (e.which == 13) { //Enter key
            e.preventDefault(); //to skip default behavior of the enter key
            var nextIndex = $('.Next').index(this) + 1;
            if (nextIndex < n) {
                $('.Next')[nextIndex].focus();
            }
            else {
                $('.Next')[nextIndex - 1].blur();
            }
        }
    });

    var viewUrl = rootPath + "/NotificationFactory/GetNotificationPF";
    //var viewUrl = "/NotificationFactory/GetNotificationAccounting";
    $.get(viewUrl, function (data) {
        $("#dvNotification").html("");
        $("#dvNotification").html(data);
        
    })
});

function ShowModalMessage(_message) {
    $("#ModalInformation").html(_message),
               $("#ModalInformation").dialog({
                   width: '50%',
                   resizable: false,
                   modal: true,
                   show: "highlight",
                   hide: "highlight",
                   title: "Message",
                   buttons: [{
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

function SuccessNotify(message) {
    $.notify(message, { className: "success", position: "bottom right", clickToHide: true });
}

function ErrorNotify(message) {
    $.notify(message, { className: "error", position: "bottom right", clickToHide: true});
}

function WarnNotify(message) {
    $.notify(message, { className: "Warn", position: "bottom right", clickToHide: true });
}

function InfoNotify(message) {
    $.notify(message, { className: "Info", position: "bottom right", clickToHide: true });
}

function ExportToExcel(tableID) {
    if (tableID) {
        window.open('data:application/vnd.ms-excel,' + encodeURIComponent('<table>' + $('#' + tableID + '').html() + '</table>'));
    }
    else {
        ErrorNotify('Data source undefined!!!');
    }
}

function FancyBox(viewURL) {

    $.fancybox.open(
          {
              'title': 'Report Window',
              'type': 'iframe',
              //fitToView: false,
              //width: '90%',
              //height: '90%',
              //autoSize: false,
              'transitionIn': 'elastic',
              'transitionOut': 'elastic',
              'speedIn': 1000,
              'speedOut': 700,
              autoSize: true,
              closeClick: false,
              'href': viewURL
          }
      );

}



//====================End of common=========================================//
