function getSidebar(page) {
    var req = new XMLHttpRequest();

    req.onload = function () {
        document.getElementsByClassName("sidebar")[0].innerHTML = this.responseText.replace(">" + page + "<", "><strong>" + page + "</strong><");
    }

    var path = window.location.href.split("/");
    var sidebarPage = "";
    for (var p = path.length - 2; p >= 0; p--) {
        if (path[p].toLowerCase() == "library")
            break;
        sidebarPage += "../";
    }
    sidebarPage += "Sidebar.html";

    req.open('GET', sidebarPage);
    req.send();
}

function toggleDropdown(div) {
    var hidediv = div.nextElementSibling;

    if (hidediv.style.display == "none") {
        hidediv.style.display = "block";
        div.innerHTML = "<img src=\"/Library/Assets/vs/open.png\" />"
    }
    else {
        hidediv.style.display = "none";
        div.innerHTML = "<img src=\"/Library/Assets/vs/closed.png\" />"
    }
}