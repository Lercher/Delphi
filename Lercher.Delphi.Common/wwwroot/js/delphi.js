/// <reference path="d.ts/angular.d.ts"/>
// angular 1.6.1
// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
// VS2017 - important 
// Check Tools/Options/Text Editor/JavaScript|TypeScript/Project/Compile on Save
// - Automatically compile typescript files which are not part of a project
// - Use System code generation for modules which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
var mod = angular.module("delphiApp", ['dx']);
mod.controller("delphi", function ($scope, $http, $location) {
    $scope.filter = {
        limit: 15,
        mincount: 0,
        tables: "",
        detailsfilter: ""
    };
    $scope.show = {
        // only these function modify the $location
        tables: show_tables,
        tabledata: show_tabledata,
        follow: show_follow,
        restrict: show_restrict
    };
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.oracle = { owner: "TRunknown" };
    $scope.error = null;
    $scope.tables = [];
    $scope.gridSettings = {};
    $scope.location = $location;
    $scope.displaygrid = true;
    $scope.displaymetadata = false;
    $scope.descriptionOf = function (c) {
        var ar = $scope.oracle.columns;
        for (var i = 0; i < ar.length; i++) {
            if (ar[i].NAME === c)
                return ar[i].DESCRIPTION;
        }
        return null;
    };
    $scope.linksof = function (c) {
        var r = [];
        var l = $scope.oracle.links;
        for (var key in l) {
            if (l.hasOwnProperty(key)) {
                var v = l[key];
                for (var i = 0; i < v.matchcolumn.length; i++) {
                    if (v.matchcolumn[i].src === c) {
                        r.push(v);
                    }
                }
            }
        }
        return r;
    };
    $scope.isLink = function (c, v) { return v && ($scope.linksof(c) !== []); };
    $scope.gridSettings = getGridSettings("oracle.values");
    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path = $location.path();
        var hash = $location.hash();
        var search = $location.search();
        if (search.owner) {
            $scope.oracle.owner = search.owner;
            delete search.owner;
        }
        if (path && hash === "pk") {
            var body = {
                key: "",
                pk: objectToKvArray(search),
                direction: "pk"
            };
            request_tabledata($scope.oracle.owner, path, body);
            // TODO - single item display is not a database query yet
        }
        else if (path && hash && hash.indexOf(".") > 0) {
            var fkdir = hash.split(".", 2);
            var body = {
                key: fkdir[0],
                pk: objectToKvArray(search),
                direction: fkdir[1]
            };
            request_tabledata($scope.oracle.owner, path, body);
        }
        else if (path) {
            request_tabledata($scope.oracle.owner, path);
        }
        else if (!path) {
            // OK this is the start URL
        }
        else {
            $scope.error = "URL error: unrecognizable URL path " + $location.url();
        }
    });
    function show_restrict(r) {
        $scope.selectedRow = null;
        $scope.oracle.values = [r];
        var pk = buildPK(r);
        $location.path($scope.oracle.table);
        $location.search(kvArrayToObject(pk));
        $location.hash("pk");
        console.log($location.url());
    }
    function show_follow(r, fk, direction) {
        // alert("Show the one " + fk.destination + " where " + fk.matchcolumn[0].dest + " = " + r[fk.matchcolumn[0].src] + " ... by " + fk.key + " with " + fk.matchcolumn.length + " match(es))");
        var body = { key: fk.key, pk: buildPK(r), direction: direction };
        show_tabledata($scope.oracle.owner, $scope.oracle.table, body);
    }
    function show_tabledata(owner, tablename, body) {
        if (body === void 0) { body = undefined; }
        $location.path(tablename); // table as PATH
        if (body) {
            var hash = body.key ? body.key + '.' + body.direction : body.direction;
            $location.hash(hash); // foreign key constraint name + . + forward/back or pk as HASH
            var se = kvArrayToObject(body.pk);
            se['owner'] = owner;
            $location.search(se); // the primary keys as SEARCH
        }
        else {
            $location.hash(null);
            $location.search({ owner: owner });
        }
    }
    ;
    ;
    function buildPK(r) {
        var pks = $scope.oracle.pk;
        var pk = [];
        for (var i = 0; i < pks.length; i++) {
            var v = r[pks[i]];
            pk.push({ key: pks[i], value: v });
        }
        return pk;
    }
    function kvArrayToObject(kv) {
        var r = {};
        for (var i = 0; i < kv.length; i++) {
            r[kv[i].key] = kv[i].value;
        }
        return r;
    }
    function objectToKvArray(o) {
        var r = [];
        for (var k in o) {
            if (o.hasOwnProperty(k))
                r.push({ key: k, value: o[k] });
        }
        return r;
    }
    // --- REST service calls
    var url = { tables: "api/tables/", tabledata: "api/table/", tablemetadata: "api/tablemetadata/" };
    function request_tabledata(owner, tablename, body) {
        if (body === void 0) { body = undefined; }
        if (body) {
            $scope.error = "following a link, loading data for " + tablename + " ...";
            $http({ method: "POST", url: url.tabledata + owner + "/" + tablename, data: body })
                .then(function (response) { $scope.oracle = response.data; $scope.error = null; })["catch"](error);
        }
        else {
            $scope.error = "loading data for " + tablename + " ...";
            $http({ url: url.tabledata + owner + "/" + tablename })
                .then(function (response) { $scope.oracle = response.data; $scope.error = null; })["catch"](error);
        }
    }
    function show_tables() {
        $scope.error = "loading tables ...";
        $scope.oracle.table = null;
        $scope.tables = [];
        $http({ url: url.tables + $scope.oracle.owner })
            .then(function (response) { $scope.tables = response.data; $scope.error = null; })["catch"](error);
    }
    function error(r) {
        $scope.error = r.data.ExceptionMessage || r.data.Message || r.data;
    }
    // for the DX grid only
    function getGridSettings(dsref) {
        return {
            bindingOptions: { dataSource: dsref },
            columnChooser: { enabled: true },
            allowColumnReordering: true,
            sorting: { mode: 'multiple' },
            groupPanel: { visible: true, emptyPanelText: 'Drag a column header here to group records' },
            pager: { visible: true },
            paging: { pageSize: 15 },
            filterRow: { visible: true },
            searchPanel: { visible: true },
            selection: { mode: 'single' },
            rowAlternationEnabled: true,
            onSelectionChanged: function (selectedItems) {
                return $scope.selectedRow = selectedItems.selectedRowsData[0];
            }
        };
    }
});
mod.directive("outgoingLinks", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "outgoingLinks.html"
    };
});
mod.directive("incomingLinks", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "incomingLinks.html"
    };
});
mod.directive("listOfTables", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "listOfTables.html"
    };
});
mod.directive("singleitemData", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "singleitemData.html"
    };
});
mod.directive("singleitemDetailtables", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "singleitemDetailtables.html"
    };
});
mod.directive("multipleitemDxgrid", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "multipleitemDxgrid.html"
    };
});
mod.directive("multipleitemTable", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "multipleitemTable.html"
    };
});
//# sourceMappingURL=delphi.js.map