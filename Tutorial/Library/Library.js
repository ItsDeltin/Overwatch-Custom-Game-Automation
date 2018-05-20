function getSidebar(page) {
    var req = new XMLHttpRequest();

    req.onload = function () {
        document.getElementsByClassName("sidebar")[0].innerHTML = this.responseText.replace(">" + page + "<", "><strong>" + page + "</strong><");
    }

    var path = window.location.href.split("/");
    var sidebarPage = "";
    for (var p = path.length - 2; p >= 0; p--) {
        if (path[p] == "Library")
            break;
        sidebarPage += "../";
    }
    sidebarPage += "Sidebar.html";

    req.open('GET', sidebarPage);
    req.send();
}