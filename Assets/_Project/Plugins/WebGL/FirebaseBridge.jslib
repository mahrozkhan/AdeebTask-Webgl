mergeInto(LibraryManager.library, {
    JS_FirebaseInit: function(configJsonPtr) {
        var config = JSON.parse(UTF8ToString(configJsonPtr));
        if (!window.firebase.apps.length) {
            window.firebase.initializeApp(config);
        }
        window.firebaseDB = window.firebase.firestore();
    },
    JS_FirebaseSaveProject: function(jsonPtr, gameObjectNamePtr, callbackMethodPtr) {
        var json = UTF8ToString(jsonPtr);
        var goName = UTF8ToString(gameObjectNamePtr);
        var cbMethod = UTF8ToString(callbackMethodPtr);
        var data = JSON.parse(json);

        window.firebaseDB.collection("projects").doc(data.projectId).set(data)
            .then(function() {
                SendMessage(goName, cbMethod, "ok");
            })
            .catch(function(error) {
                SendMessage(goName, cbMethod, "error:" + error.message);
            });
    },
    JS_FirebaseLoadProjectList: function(gameObjectNamePtr, callbackMethodPtr) {
        var goName = UTF8ToString(gameObjectNamePtr);
        var cbMethod = UTF8ToString(callbackMethodPtr);

        window.firebaseDB.collection("projects").get()
            .then(function(snapshot) {
                var projects = [];
                snapshot.forEach(function(doc) {
                    projects.push(doc.data());
                });
                SendMessage(goName, cbMethod, JSON.stringify(projects));
            })
            .catch(function(error) {
                SendMessage(goName, cbMethod, "error:" + error.message);
            });
    },
    JS_FirebaseDeleteProject: function(projectIdPtr, gameObjectNamePtr, callbackMethodPtr) {
        var projectId = UTF8ToString(projectIdPtr);
        var goName = UTF8ToString(gameObjectNamePtr);
        var cbMethod = UTF8ToString(callbackMethodPtr);

        window.firebaseDB.collection("projects").doc(projectId).delete()
            .then(function() {
                SendMessage(goName, cbMethod, "ok");
            })
            .catch(function(error) {
                SendMessage(goName, cbMethod, "error:" + error.message);
            });
    },
    JS_FirebaseLoadAssetCatalogue: function(gameObjectNamePtr, callbackMethodPtr) {
        var goName = UTF8ToString(gameObjectNamePtr);
        var cbMethod = UTF8ToString(callbackMethodPtr);

        window.firebaseDB.collection("config").doc("asset_catalogue").get()
            .then(function(doc) {
                if (doc.exists) {
                    SendMessage(goName, cbMethod, JSON.stringify(doc.data()));
                } else {
                    SendMessage(goName, cbMethod, "error:asset_catalogue document not found");
                }
            })
            .catch(function(error) {
                SendMessage(goName, cbMethod, "error:" + error.message);
            });
    }
});
