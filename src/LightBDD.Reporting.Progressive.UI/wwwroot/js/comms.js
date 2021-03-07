﻿window.comms = (function () {
    function onInitialized() {
        if (window.parent !== window) {
            window.parent.postMessage({ m: "initialized" }, "*");
        }
    }

    function onUnknownEvent(data) {
        console.warn("Unrecognized event: ", data);
    }

    function handleEvent(e) {
        var data = e.data;

        if (data.m === 'import') {
            DotNet.invokeMethodAsync('LightBDD.Reporting.Progressive.UI', 'InteropService_Import', data.t,data.v,data.d);
        }
        else
        {
            onUnknownEvent(data);
        }
    }

    return {
        onInitialized: onInitialized,
        handleEvent:handleEvent
    };
})();

if (window.addEventListener) {
    window.addEventListener("message", comms.handleEvent, false);
} else {
    window.attachEvent("onmessage", comms.handleEvent);
}