window.onload = function() {
    var configObject = JSON.parse(document.getElementById('config').value);
    var oauthConfigObject = JSON.parse(document.getElementById('oath2').value);

    // Apply mandatory parameters
    configObject.dom_id = "#swagger-ui";
    configObject.presets = [SwaggerUIBundle.presets.apis, SwaggerUIStandalonePreset];
    configObject.layout = "StandaloneLayout";

    if (!configObject.hasOwnProperty("oauth2RedirectUrl"))
        configObject.oauth2RedirectUrl = window.location + "oauth2-redirect.html"; // use the built-in default

    // Build a system
    const ui = SwaggerUIBundle(configObject);

    // Apply OAuth config
    ui.initOAuth(oauthConfigObject);
};