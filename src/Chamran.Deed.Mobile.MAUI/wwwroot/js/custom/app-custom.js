(function ($) {
    $(document).on('click', '.menu-item', function () {
        $(".drawer-overlay").click();
    })
})(jQuery);


function SetButtonTitle(id, title) {
    $(`#` + id).text(title);
}


function MakeInputReadOnly(id) {
    $(id).prop('readonly', true);
}

function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

function moveToNext(element) {
    var next = element.nextElementSibling;
    if (next && next.tagName === "INPUT") {
        next.focus();
    }
}

function clickEvent(first,last){
			if(first.value.length){
				document.getElementById(last).focus();
			}
		}        