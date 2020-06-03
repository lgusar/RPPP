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
    $('#tempmessage').siblings().remove();
    $('#tempmessage').removeClass("alert-success");
    $('#tempmessage').removeClass("alert-danger");
    $('#tempmessage').html('');
}

function SetDeleteSimptom(selector, url, sifraSimptoma, sifraPregleda) {
    $(document).on('click', selector, function (event) {
        event.preventDefault();
        var sifraSimptomaval = $(this).data(sifraSimptoma);
        var sifraPregledaval = $(this).data(sifraPregleda);
        var tr = $(this).parents("tr");
        var aktivan = $(tr).data("aktivan");
        if (aktivan !== true) {
            $(tr).data("aktivan", true);

            if (confirm("Ukloniti zapis?")) {
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                clearOldMessage();
                $.post(url, { SifraPregleda: sifraPregledaval, SifraSimptoma: sifraSimptomaval, __RequestVerificationToken: token }, function (data) {
                    if (data.successful) {
                        $(tr).remove();
                    }
                    $('#tempmessage').addClass(data.successful ? "alert-success" : "alert-danger");
                    $('#tempmessage').html(data.message);

                }).fail(function (jqXHR) {
                    alert(jqXHR.status + " : " + jqXHR.responseText);
                    $(tr).data("aktivan", false);
                })
            }
            else {
                $(tr).data("aktivan", false);
            }
        }
    });
}

function SetDeleteTerapija(selector, url, sifraTerapije, sifraPregleda) {
    $(document).on('click', selector, function (event) {
        event.preventDefault();
        var sifraTerapijeval = $(this).data(sifraTerapije);
        var sifraPregledaval = $(this).data(sifraPregleda);
        var tr = $(this).parents("tr");
        var aktivan = $(tr).data("aktivan");
        if (aktivan !== true) {
            $(tr).data("aktivan", true);

            if (confirm("Ukloniti zapis?")) {
                var token = $('input[name="__RequestVerificationToken"]').first().val();
                clearOldMessage();
                $.post(url, { SifraPregleda: sifraPregledaval, SifraTerapije: sifraTerapijeval, __RequestVerificationToken: token }, function (data) {
                    if (data.successful) {
                        $(tr).remove();
                    }
                    $('#tempmessage').addClass(data.successful ? "alert-success" : "alert-danger");
                    $('#tempmessage').html(data.message);

                }).fail(function (jqXHR) {
                    alert(jqXHR.status + " : " + jqXHR.responseText);
                    $(tr).data("aktivan", false);
                })
            }
            else {
                $(tr).data("aktivan", false);
            }
        }
    });
}