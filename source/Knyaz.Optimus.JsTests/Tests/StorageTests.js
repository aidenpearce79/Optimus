Test("StorageTests", {
    "SetItem": {
        run: function () {
            var length = sessionStorage.length;
            sessionStorage.setItem("myITem", "myValue");
            Assert.AreEqual("myValue", sessionStorage.getItem("myITem"));
            Assert.AreEqual("myValue", sessionStorage["myITem"]);
            Assert.AreEqual(length + 1, sessionStorage.length);
        }
    },
    "SetItemTwice": {
        run: function () {
            var length = sessionStorage.length;
            sessionStorage.setItem("myITem2", "myValue");
            sessionStorage.setItem("myITem2", "myValue2");
            Assert.AreEqual("myValue2", sessionStorage.getItem("myITem2"));
            Assert.AreEqual(length+1, sessionStorage.length);
        }
    },
    "RemoveItem": {
        run: function () {
            var length = sessionStorage.length;
            sessionStorage.setItem("myITem3", "myValue");
            sessionStorage.setItem("myITem4", "myValue2");
            sessionStorage.removeItem("myITem3");
            Assert.AreEqual(length+1, sessionStorage.length);
        }
    },
    "Clear": {
        run: function () {
            sessionStorage.setItem("myITem3", "myValue");
            sessionStorage.setItem("myITem4", "myValue2");
            sessionStorage.clear();
            Assert.AreEqual(0, sessionStorage.length);
        }
    },
    "GetAbsentItem": {
        run: function () {
            Assert.AreEqual(null, sessionStorage.getItem("absent"));
        }
    },
    "GetKey":{
        run:function () {
            sessionStorage.clear();
            sessionStorage.setItem("uu","aa");
            Assert.AreEqual("uu", sessionStorage.key(0));
        }
    },
    "SetByIndexer":{
        run:function () {
            sessionStorage["setByIndexer"] = "ok";
            Assert.AreEqual("ok", sessionStorage.getItem("setByIndexer"),"get using getItem method");
            Assert.AreEqual("ok", sessionStorage["setByIndexer"], "get using indexer");
        }
    },
    "SessionStorage":{
        run:function () {
            Assert.IsNotNull(window.sessionStorage, "window.sessionStorage");
            Assert.IsNotNull(sessionStorage, "sessionStorage");
        }
    },
    "LocalStorage":{
        run:function () {
            Assert.IsNotNull(window.sessionStorage, "window.localStorage");
            Assert.IsNotNull(sessionStorage, "localStorage");
        }
    },
    "StoreBoolKey":{
        run:function(){
            window.localStorage[true]="Hello";
            Assert.AreEqual("Hello", window.localStorage[true]);
            Assert.AreEqual("Hello", window.localStorage["true"]);
        }
    },
    "StoreBoolItem":{
        run:function(){
            window.localStorage["A"]=true;
            Assert.AreEqual("true", window.localStorage["A"]);
            Assert.AreEqual(false, window.localStorage["A"] === true);
        }
    },
    "StoreNumberItem":{
        run:function(){
            window.localStorage["A"]=10;
            Assert.AreEqual("10", window.localStorage["A"]);
            Assert.AreEqual(false, window.localStorage["A"] === 10);
            Assert.AreEqual(true, window.localStorage["A"] === "10");
        }
    },
    "StoreObjectItem":{
        run:function(){
            window.localStorage["A"]={x:'hi'};
            Assert.AreEqual("[object Object]", window.localStorage["A"]);
        }
    }
});