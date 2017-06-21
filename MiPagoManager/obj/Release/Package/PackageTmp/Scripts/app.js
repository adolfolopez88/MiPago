$(function () {

    $('.datepicker').datepicker({
        format: "dd/mm/yyyy",
        language: "es"
    });


    $('.multiselect').multiselect({
        nonSelectedText: 'sin seleccion...',
    });

    $('.multiselect-busqueda').multiselect({
        enableFiltering: true,
        includeSelectAllOption: true,
        nonSelectedText: 'sin seleccion...',
        selectAllText: 'seleccionar todos',
        filterPlaceholder: 'Buscar...',
        maxHeight: 400,
        dropUp: true
    });


    $('.modal-ajax').click(function (e) {
        e.preventDefault();
        var url = $(this).attr('href');
        $.get(url, function (data) {
            $(data).modal().on("hidden.bs.modal", function () {
                $(this).remove(); 
            });
        });
    });

});

