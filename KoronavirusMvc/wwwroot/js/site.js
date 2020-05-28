//Pri svakom kliku kontrole koja ima css klasu delete zatraži potvrdu
//za razliku od  //$(".delete").click ovo se odnosi i na elemente koji će se pojaviti u budućnosti 
//dinamičkim učitavanjem
$(function () {
    $(document).on('click', '.delete', function (event) {
        if (!confirm("Obrisati zapis?")) {
            event.preventDefault();
        }
    });
});

function clearOldMessage() {
    $("#tempmessage").siblings().remove();
    $("#tempmessage").removeClass("alert-success");
    $("#tempmessage").removeClass("alert-danger");
    $("#tempmessage").html('');
}

function SetDeleteAjax(selector, url, paramname) {
    $(document).on('click', selector, function (event) {

        event.preventDefault(); //u slučaju da se radi o nekom submit buttonu, inače nije nužno        
        var paramval = $(this).data(paramname);
        var tr = $(this).parents("tr");
        var aktivan = $(tr).data('aktivan');
        if (aktivan != true) { //da spriječimo dva brza klika...
            $(tr).data('aktivan', true);
            if (confirm('Obrisati zapis?')) {
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                clearOldMessage();
                $.post(url, { id: paramval, __RequestVerificationToken: token }, function (data) {
                    if (data.successful) {
                        $(tr).remove();
                    }
                    $("#tempmessage").addClass(data.successful ? "alert-success" : "alert-danger");
                    $("#tempmessage").html(data.message);
                }).fail(function (jqXHR) {
                    alert(jqXHR.status + " : " + jqXHR.responseText);
                    $(tr).data('aktivan', false);
                });
            }
            else {
                $(tr).data('aktivan', false);
            }
        }
    });
}