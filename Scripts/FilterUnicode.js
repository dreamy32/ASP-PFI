$(function () {
    $(":input").change(function () {
        let r = $(this).val().replace(/[^\x00-\xFF]/g, "");
        $(this).val(r);
    });
    $("textarea").change(function () {
        let r = $(this).val().replace(/[^\x00-\xFF]/g, "");
        $(this).val(r);
    });
})