// script pour l'interface de gestion de sélection avec deux <select...>
// Il faut utiliser le fichier de styles css/flashButtons.css les <div> de
// classe MoveLeft et MoveRight
//
// auteur : Nicolas Chourot

$(document).ready(initUI);

function initUI() {
    DisableIcon('#MoveLeft');
    DisableIcon('#MoveRight');
    SortAllSelect();
    $('#UnselectedItems').change(function () {
        $('#UnselectedItems option:selected').each(function () {
            $("#SelectedItems option:selected").prop("selected", false);
            EnableIcon("#MoveLeft");
            DisableIcon("#MoveRight");
        });
    });

    $('#SelectedItems').change(function () {
        $('#SelectedItems option:selected').each(function () {
            $("#UnselectedItems option:selected").prop("selected", false);
            EnableIcon("#MoveRight");
            DisableIcon("#MoveLeft");
        });
    });

    $(document).on('submit', 'form', function () {
        $('#SelectedItems option').prop('selected', true);
    });

    ///////////////////////////////////////////////////////////////////
    // On the click event on the image id="MoveLeft"
    ///////////////////////////////////////////////////////////////////
    $("#MoveLeft").on('click', function () {
        $('#UnselectedItems option:selected').each(function () {
            $(this).remove();
            $(this).prop("selected", false);
            $('#SelectedItems').append($(this));
            SortSelect("SelectedItems");
            ScrollTo("#SelectedItems", $(this).offset().top);
            DisableIcon("#MoveLeft");
        });
    });

    ///////////////////////////////////////////////////////////////////
    // On the click event on the image id="MoveRight"
    ///////////////////////////////////////////////////////////////////
    $("#MoveRight").on('click', function () {
        $('#SelectedItems option:selected').each(function () {
            $(this).remove();
            $(this).prop("selected", false);
            $('#UnselectedItems').append($(this));
            SortSelect("UnselectedItems");
            ScrollTo("#UnselectedItems", $(this).offset().top);
            DisableIcon("#MoveRight");
        });
    });
}

function DisableIcon(iconId) {
    $(iconId).removeClass();
    if (iconId === "#MoveRight") {
        $(iconId).addClass("flashButton iconRightDisable");
    } else {
        $(iconId).addClass("flashButton iconLeftDisable");
    }
}

function EnableIcon(iconId) {
    $(iconId).removeClass();
    if (iconId === "#MoveRight") {
        $(iconId).addClass("flashButton iconRight");
    } else {
        $(iconId).addClass("flashButton iconLeft");
    }
}


///////////////////////////////////////////////////////////////////
// Sort text items of a listbox
///////////////////////////////////////////////////////////////////
function normalize(str) {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
}
function SortSelect(selectId) {
    $("#" + selectId).html($("#" + selectId + " option").sort(function (a, b) {
        return a.text === b.text ? 0 : normalize(a.text) < normalize(b.text) ? -1 : 1;
    }))
}

function SortAllSelect() {
    SortSelect("SelectedItems");
    SortSelect("UnselectedItems");
}

function ScrollTo(selectId, optionTop) {
    var selectObj = $(selectId);
    var selectTop = selectObj.offset().top;
    selectObj.scrollTop(selectObj.scrollTop() + (optionTop - selectTop));
}