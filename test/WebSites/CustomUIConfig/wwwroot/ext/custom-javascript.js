var callback = function() {
    var elem = document.createElement("div");
    elem.innerHTML =
        "<div style=\"text-align: center; font-family: Titillium Web,sans-serif; margin: 16px;\">This text was injected via /ext/custom-javascript.js, using the SwaggerUIOptions.InjectJavascript method.</div>";

    document.body.insertBefore(elem, document.body.firstChild);
};

if (document.readyState === "complete" || (document.readyState !== "loading" && !document.documentElement.doScroll)) {
    callback();
} else {
    document.addEventListener("DOMContentLoaded", callback);
}