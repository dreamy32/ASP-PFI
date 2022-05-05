$.validator.addMethod(
    "GenderId",
    function (value, element) { return ($("input[name='GenderId']:checked").val() != undefined); },
    "Choix obligatoire"
);