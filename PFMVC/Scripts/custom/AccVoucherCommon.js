
$('.next').live("keyup", function (e) {
    var n = $(".next").length;

    if (e.which == 13) { //Enter key
        e.preventDefault(); //to skip default behavior of the enter key
        var nextIndex = $('.next').index(this) + 1;

        if (nextIndex < n) {
            //InfoNotify($(this).attr('id') +" and "+ 'Credit' + currentAccountHeadIndex)
            if ($(this).hasClass('lastcell')) {
                //alert($('#LedgerID' + currentAccountHeadIndex).val());

                if ($('#LedgerID' + currentAccountHeadIndex).val()) {
                    if ($('#LedgerID' + (parseInt(currentAccountHeadIndex) + 1)).length == 0) {
                        addRow("VoucherTable");
                    }
                }
            }
            $('.next')[nextIndex].focus();
        }
        else {


            $('.next')[nextIndex - 1].blur();
        }
    }
});


var currentAccountHeadIndex = 0;
$('.accountHead').unbind('blur');
$('.accountHead').unbind('focus');


//very important for switching option DIV
$('.accountHead').live('blur', function () {
    $('#LLT td.can_be_selected').removeClass('selectedRow');
}).live('focus', function () {
    $(".optionDiv").addClass("hidden");
    currentAccountHeadIndex = $('.accountHead').index(this);
    if ($('#inputType-' + currentAccountHeadIndex).val() == "Dr.") {
        if ($('#voucherType').text() === "Payment Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden");
        }
        else if ($('#voucherType').text() === "Receipt Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv3").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else if ($('#voucherType').text() === "Contra Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv3").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else if ($('#voucherType').text() === "Journal Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden");
        }
    }
    else if ($('#inputType-' + currentAccountHeadIndex).val() == "Cr.") {
        if ($('#voucherType').text() === "Payment Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv3").removeClass("hidden");
        }
        else if ($('#voucherType').text() === "Receipt Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else if ($('#voucherType').text() === "Contra Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv3").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else if ($('#voucherType').text() === "Journal Voucher") {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden"); //optionDiv3 holding the CashAccountList
        }
        else {
            $(".optionDiv").addClass("hidden");
            $("#optionDiv").removeClass("hidden");
        }
    }
    FilterAccountHeadTable();

});

//searching ledgername and should change the index to -1 again!
$('.accountHead').live('keyup', function (e) {
    if ((e.keyCode >= 97 && e.keyCode <= 122) || (e.keyCode >= 65 && e.keyCode <= 90) || (e.keyCode >= 48 && e.keyCode <= 57) || e.keyCode == 8) {
        FilterAccountHeadTable();
        $('#LedgerName' + currentAccountHeadIndex).css({ "color": "red" });
        $('#LedgerID' + currentAccountHeadIndex).val("");
    }
});

function FilterAccountHeadTable() {
    //InfoNotify("Key Press");
    index = -1;
    inp = $('#LedgerName' + currentAccountHeadIndex).val();

    //InfoNotify(inp);
    //This should ignore first row with th inside
    $(".LLT:visible tr:not(:has(>th))").each(function () {
        if (~$(this).text().toLowerCase().indexOf(inp.toLowerCase())) {
            // Show the row (in case it was previously hidden)
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}


function save() {
    $("#DialogForm1").submit();
}

function SwitchOptionDiv() {
    debugger;

    if ($("#optionDiv2").hasClass("hidden")) {
        $(".optionDiv").addClass("hidden");
        $("#optionDiv2").removeClass("hidden");
    }
    else {
        $("#optionDiv2").addClass("hidden");
        $("#optionDiv").removeClass("hidden");
    }
}



$(".accountHead").unbind('keydown');
var index = -1;
//38 up, 40down
$(".accountHead").live("keydown", (function (e) {
    if (e.keyCode === 40) {
        //InfoNotify("IN>" + index + " and length>" + $('#LLT tr:visible td.can_be_selected').length);
        index = (index + 1 >= $('.LLT:visible tr:visible td.can_be_selected').length) ? 0 : index + 1;
        //InfoNotify(index);
        $('.LLT:visible tr:visible td.can_be_selected').removeClass('selectedRow');
        $('.LLT:visible tr:visible td.can_be_selected:eq(' + index + ')').addClass('selectedRow');

        var s = $('.LLT:visible tr:visible td.can_be_selected:eq(' + index + ')').position();
        $("div").scrollTop(s.top);


        return false;
    }
    else if (e.keyCode === 38) {
        //InfoNotify(index);
        index = (index <= 0) ? $('#LLT tr:visible td.can_be_selected').length - 1 : index - 1;
        $('.LLT:visible tr:visible td.can_be_selected').removeClass('selectedRow');
        $('.LLT:visible tr:visible td.can_be_selected:eq(' + index + ')').addClass('selectedRow');

        var s = $('.LLT:visible tr:visible td.can_be_selected:eq(' + index + ')').position();
        $("div").scrollTop(s.top);

        return false;
    }
    else if (e.keyCode == 13) {
        //SuccessNotify(currentAccountHeadIndex +"and index>"+index);
        if (currentAccountHeadIndex > -1 && index > -1) {
            //InfoNotify("after enter index>" + index);
            var ledgerName = $('.LLT:visible tr:visible:eq(' + index + ') td:eq(' + 0 + ')');
            var ledgerID = $('.LLT:visible tr:visible:eq(' + index + ') td:eq(' + 1 + ')');
            //InfoNotify("led id>"+ledgerID.text());
            $('#LedgerName' + currentAccountHeadIndex).val(ledgerName.text());
            $('#LedgerID' + currentAccountHeadIndex).val(ledgerID.text());
            $('#LedgerName' + currentAccountHeadIndex).css({ "color": "black" });
            index = -1;

            $('.LLT:visible td.can_be_selected').removeClass('selectedRow');
        }
    }
}));





$('.debit').live('change', function () {
    var d = 0;

    $('.debit').each(function () {
        if ($(this).val()) {
            d = (parseFloat(d) + parseFloat($(this).val().replace(/,/g, ''))).toFixed(2);
        }
    })
    if (!d) { d = 0 };
    $('#result_debit').text(d + "/=");

});
$('.credit').live('change', function () {
    var c = 0;
    $('.credit').each(function () {
        if ($(this).val()) {
            c = parseFloat( c ) + parseFloat( $( this ).val().replace( /,/g, '' ) );
        }
    })

    if (!c) { c = 0 };

    $('#result_credit').text(c + "/=");
});

function calculate(){
    var d = 0;
    $('.debit').each(function () {
        if ($(this).val()) {
            d = (parseFloat(d) + parseFloat($(this).val().replace(/,/g, ''))).toFixed(2);
        }
    })
    if ( !d ) { d = 0 };
    $('#result_debit').text(d + "/=");

    var c = 0;
    $('.credit').each(function () {
        if ($(this).val()) {
            c = parseFloat( c ) + parseFloat( $( this ).val().replace( /,/g, '' ) );
        }
    })

    if (!c) { c = 0 };

    $('#result_credit').text(c + "/=");
}


var inputTypeIndex
$('.inputType').live('focus', function () {
    inputTypeIndex = $('.inputType').index(this);
    //InfoNotify(inputTypeIndex);
    $('#inputType-' + inputTypeIndex).notify("Spacebar to switch Cr./Dr.", { className: "info", position: "left", autoHideDelay: 2000 });
})

$( '.inputType' ).live( 'keydown', function ( e ){
    //InfoNotify("keyup code:" + e.keyCode);
    if (e.keyCode == 32) {
        //InfoNotify("id = " + $(e.target).attr("id"));
        //if ($('#voucherType').text() == "Journal Voucher") {
        //    return; // we will not change input type if it is journal voucher! hope you understand...
        //}
        //at last fixed journal voucher no more accepted
        SwitchInputType(inputTypeIndex);
    }
})

//$('.inputType').live('click', function (e) {
//    //InfoNotify("click id = " + $(e.target).attr("id"));
//    InfoNotify("click id = " + e.which);
//    SwitchInputType(inputTypeIndex);
//})

function SwitchInputType(n) {
    //InfoNotify("inputTypeIndex: " + n);

    if ($('#inputType-' + n).val() == "Dr.") {
        //InfoNotify('in dr.');
        $('#inputType-' + n).val("Cr.");
        $('#inputType-' + n).removeClass('btn-info').addClass('btn-danger');
        $('#LedgerName' + n).val('');
        $('#LedgerID' + n).val('');
        $('#Debit' + n).attr('readonly', 'readonly');
        $('#Debit' + n).val('').removeClass('next lastcell').addClass('hidden');
        $('#Credit' + n).val('').addClass('next lastcell').removeClass('hidden');
        $('#Credit' + n).attr('readonly', false);
    }
    else {
        $('#inputType-' + n).val("Dr.").removeClass('btn-danger').addClass('btn-info');
        $('#inputType-' + n);
        $('#LedgerName' + n).val('');
        $('#LedgerID' + n).val('');
        $('#Credit' + n).attr('readonly', 'readonly');
        $('#Credit' + n).val('').removeClass('next lastcell').addClass('hidden');
        $('#Debit' + n).val('').addClass('next lastcell').removeClass('hidden');
        $('#Debit' + n).attr('readonly', false);
    }
}

function getreport(voucherID, type) {
   
    var viewURL = '../../Accounting/Base/VoucherReport/?voucherID=' + voucherID+"&type="+type;
    //window.open(viewURL, 'window name', 'window settings')
    FancyBox(viewURL);
}

