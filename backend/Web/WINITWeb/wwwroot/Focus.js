


//window.focustotable = function () {
//    window.scrollTo({
//        top: document.body.scrollHeight,
//        behavior: 'smooth'
//    });
//};

window.focustotable = function (position) {
    // Check the position parameter and scroll accordingly
    if (position === 'top') {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    } else if (position === 'bottom') {
        window.scrollTo({
            top: document.body.scrollHeight,
            behavior: 'smooth'
        });
    } else {
        console.error('Invalid position parameter. Use "top" or "bottom".');
    }
};
function saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;

    document.body.appendChild(link);
    document.body.removeChild(link);
    link.click();
}
