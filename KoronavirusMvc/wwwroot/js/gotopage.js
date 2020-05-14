$(function () {
    $(".pagebox").click(function () {
        $(this).select();
    });

    $(".pagebox").bind('keyup', function (event) {
        var keycode = event.keyCode ? event.keyCode : event.which;
        var pagebox = $(this);
        if (keycode == 13) {
            if (validRange(pagebox.val(), pagebox.data('min'), pagebox.data('max'))) {
                var link = pagebox.data('url');
                link = link.replace('-1', pagebox.val());
                window.location = link;
            }
        }
        else if (keycode == 27) {
            pagebox.val(pagebox.data('current'));
        }
    });
});

function validRange(str, min, max) {
    var intRegex = /^\d+$/;
    if (intRegex.test(str)) {
        var num = parseInt(str);
        return num >= min && num <= max;
    }
    else {
        return false;
    }
}