
const customPlugin1 = function (system) {
    var elem = document.createElement("div");
    elem.innerHTML =
        "<div style=\"text-align: center; font-family: Titillium Web,sans-serif; margin: 16px;\">This text was injected via /ext/custom-plugin1.js, using the SwaggerUIOptions.Plugins method.</div>";

    document.body.insertBefore(elem, document.body.firstChild);
    return {
    };
}
