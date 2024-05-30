
function ConfirmAction(message) {
    if (!confirm(message)) {
        event.preventDefault();
    }
}

function Confirm(element) {
    let table = document.querySelector('#tabl');
    var status = table.rows[element.closest('tr').rowIndex].cells[2].innerHTML;
    ;
    if (status.trim() == "Выполняется") {
        if (!confirm("Вы подтверждаете выполнение заказа?")) {
            event.preventDefault();
        }
    }
}

function ConfirmUnpinFromTable(tableId,message,errorMessage) {

    var countRows = document.getElementById(tableId).rows.length - 1;
    if (countRows == 1) {
        event.preventDefault();
        alert(errorMessage);
    }
    else
    {
        if (!confirm(message)) {
            event.preventDefault();
        }
    }
}

function CheckBoxes(errorMessage) {
    var boxes = document.getElementsByClassName('check_box');
    var checked = false;
    for (i = 0; i < boxes.length; i++) {
        if (boxes[i].checked) {
            checked = true;
            break;
        }
    }
    if (!checked) {
        alert(errorMessage);
        event.preventDefault();
    }
}

function SetDate() {

    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1;
    var min_yyyy = today.getFullYear();
    var max_yyyy = today.getFullYear() + 10;
    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }

    today = min_yyyy + '-' + mm + '-' + dd;
    max_date = max_yyyy + '-' + mm + '-' + dd;
    document.getElementById("datefield").setAttribute("min", today);
    document.getElementById("datefield").setAttribute("max", max_date);
}

function SetMaxDate()
{
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1;
    var min_yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }

    today = min_yyyy + '-' + mm + '-' + dd;
    document.getElementById("datefield").setAttribute("max", today);
}

function SetStartDate()
{
    var today = new Date();
    var dd = today.getDate() - 1;
    var mm = today.getMonth() + 1;
    var min_yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }

    today = min_yyyy + '-' + mm + '-' + dd;
    document.getElementById("startField").setAttribute("max", today);
}

function CheckDifferenceBetweenDates()
{
    var startInput = document.getElementById('startField').value;
    var endInput = document.getElementById('datefield').value;

    var startDate = new Date(startInput);
    var endDate = new Date(endInput);

    if (!isNaN(endDate) && !isNaN(startDate)) {
        if (endDate - startDate <= 0) {
            alert('Дата конца интервала должна быть больше даты начала');
            event.preventDefault();
        }
    }
}

function searchBookingInSelectBox() {
    var input = document.getElementById('numberBooking');
    var selectBox = document.getElementById('selectBox');

    var i = 0;
    for (var option of selectBox.options) {
        if (option.value.trim() === input.value.trim()) {
            selectBox.selectedIndex = i;
            break;
        } 
        i++;
    }
}

function selectedFile()
{
    var photo = document.getElementById("photo");
    if (!photo.value)
    {
        alert('Необходимо выбрать изображение');
        event.preventDefault();
    }

}

function SelectedFileWithRadio()
{
    var photo = document.getElementById('photo');
    var radios = document.getElementsByName('radioForPhoto');
    for (var i = 0; i < radios.length; i++) {
        if (radios[i].checked)
        {
            if (!photo.value)
            {
                alert('Необходимо выбрать изображение');
                event.preventDefault();
                break;
            }
        }
    }
}

function CheckDifferenceBetweenStartEndDates() {
    var startInput = document.getElementById('datefield').value;
    var endInput = document.getElementById('endfield').value;

    var startDate = new Date(startInput);
    var endDate = new Date(endInput);

    if (!isNaN(endDate) && !isNaN(startDate)) {
        if (endDate - startDate < 0) {
            alert('Дата конца интервала должна быть больше или равно дате начала');
            event.preventDefault();
        }
    }
}


function CheckDifferenceBetween2Numbers(start, end)
{
    var startNumber = parseFloat(document.getElementById(start).value);
    var endNumber = parseFloat(document.getElementById(end).value);

    if (endNumber < startNumber)
    {
        alert('Стартовое числовое значение должно быть меньше или равно конечному числовому значению');
        event.preventDefault();
    }

}