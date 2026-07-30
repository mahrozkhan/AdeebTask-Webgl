mergeInto(LibraryManager.library, {
    JS_SaveToLocalStorage: function(keyPtr, jsonPtr) {
        var key = UTF8ToString(keyPtr);
        var json = UTF8ToString(jsonPtr);
        try { localStorage.setItem(key, json); } 
        catch (e) { console.error("localStorage save failed:", e); }
    },
    JS_LoadFromLocalStorage: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        var json = localStorage.getItem(key);
        if (json === null) json = "";
        var bufferSize = lengthBytesUTF8(json) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(json, buffer, bufferSize);
        return buffer;
    },
    JS_DeleteFromLocalStorage: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        localStorage.removeItem(key);
    },
    JS_GetAllProjectKeys: function(prefixPtr) {
        var prefix = UTF8ToString(prefixPtr);
        var keys = [];
        for (var i = 0; i < localStorage.length; i++) {
            var k = localStorage.key(i);
            if (k.startsWith(prefix)) keys.push(k);
        }
        var result = JSON.stringify(keys);
        var bufferSize = lengthBytesUTF8(result) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(result, buffer, bufferSize);
        return buffer;
    }
});
