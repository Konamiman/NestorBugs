$(document).ready(function () {
    $("#application-selector").change();

    var converter1 = Markdown.getSanitizingConverter();
    var editor1 = new Markdown.Editor(converter1, "-description");
    editor1.run();

    var converter2 = Markdown.getSanitizingConverter();
    var editor2 = new Markdown.Editor(converter2, "-environment");
    editor2.run();

    $("#editForm").confirmUnsavedChanges();

    $(":submit").click(function () {
        $("#editForm").confirmUnsavedChanges.Saving();
    });

    $(":submit").attr("disabled", "true");
});

function ToggleHelp(title) {
    $("#editor-information-" + title).toggle(200, 'swing', function () {
        if ($(this).css("display") == "none") {
            $("#editor-information-link-" + title).text("[Help]");
        } else {
            $("#editor-information-link-" + title).text("[Hide help]");
        }
    });
}

$("#application-selector").live({
    'change': function () {
        var text = $(this).find(":selected").text();
        if (text == "(Other)") {
            $("#applicationName").show();
        } else {
            $("#applicationName").hide();
        }
    },
    'keyup': function () {
        $(this).change();
    }
});


// CODE FROM:
// http://geekswithblogs.net/thomasthedeuce/archive/2011/05/20/145462.aspx
// http://stackoverflow.com/a/155812/4574
// http://stackoverflow.com/a/6360734/4574

//thomas gathings
//jquery plugin to enable confirmation of unsaved changes on common input elements.
//apply to an ancestor, such as fieldset, div, or form.

jQuery.fn.confirmUnsavedChanges = function () {
    var $obj = this;
    var saving = false;
    jQuery(window).bind('beforeunload', function (e) {
        //if local element data 'dirty' = true, show unload confirmation
        if (!saving && $obj.data("dirty")) {
            if (!e) e = window.event;
            //e.cancelBubble is supported by IE - this will kill the bubbling process.
            e.cancelBubble = true;
            e.returnValue = "You have unsaved changes that will be lost if you abandon the page.";
            //e.stopPropagation works in Firefox.
            if (e.stopPropagation) {
                e.stopPropagation();
                e.preventDefault();
            }

            return e.returnValue;
        }
    });

    $("body").attr("onunload", "confirmExit()");

    //for the local element, add data 'dirty', set to false
    $obj.data("dirty", false);
    //finds descendants by selector, scoped to this
    $(":input").change(function () {
        $obj.data("dirty", true);
        $(":submit").removeAttr("disabled");
    });
    jQuery.fn.confirmUnsavedChanges.Saving = function () {
        saving = true;
    }
    jQuery.fn.confirmUnsavedChanges.ConfirmAjaxUnload = function () {
        if (!saving && $obj.data("dirty")) {
            return confirm("Are you sure you want to navigate away from this page?\r\n\r\nYou have unsaved changes.\r\n\r\nPress OK to continue, or Cancel to stay on the current page.");
        }
    }
};