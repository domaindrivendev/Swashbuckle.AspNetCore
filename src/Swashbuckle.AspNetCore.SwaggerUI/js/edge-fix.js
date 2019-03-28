if (window.navigator.userAgent.indexOf("Edge") > -1) {
    console.log("Removing native Edge fetch in favor of swagger-ui's polyfill")
    window.fetch = undefined;
} 