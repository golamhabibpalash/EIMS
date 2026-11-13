
var width = Math.max(window.screen.width, window.innerWidth);
function openNav() {
    if (width < 450) {
        document.getElementById("myNav").style.display = "block";
        document.getElementById("myNav").style.width = "82%";
    }
    else {
        document.getElementById("myNav").style.display = "block";
        document.getElementById("myNav").style.width = "15%";
    }
}

function closeNav() {
        document.getElementById("myNav").style.width = "0%"; 
}



