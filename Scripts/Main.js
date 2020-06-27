function hideShowFocus(id,focusId) {
    var x = document.getElementById(id);
    if (x.style.display === "none") {
        x.style.display = "block";
        var focusObj = document.getElementById(focusId);
        focusObj.focus();
    } else {
        x.style.display = "none";
    }
}

function hideShow(id) {
    var x = document.getElementById(id);
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}

function hideMinimalShow(id) {
    var x = document.getElementById(id);
    var _height = '32px';
    if (x.clientHeight > 20) {
        if (x.style.height == _height) {
            x.style.height = 'auto';
            x.style.borderBottom = '';
        }
        else {
            x.style.height = _height;
            x.style.borderBottom = 'black dashed 1px';
        }
    }
}


