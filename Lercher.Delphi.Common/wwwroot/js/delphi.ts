/// <reference path="d.ts/angular.d.ts"/>
// angular 1.6.1

declare var $: any;

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
    $scope.filter = { limit: 15, detailsfilter: "" };
    $scope.show = {
        // only these function modify the $location
        tables: show_tables,
        tabledata: show_tabledata,
        follow: show_follow,
        restrict: show_restrict
    };
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.oracle = { owner: "TRREADY45" };
    $scope.error = null;
    $scope.tables = [];
    $scope.gridSettings = {};
    $scope.location = $location;
    $scope.displaygrid = true;
    $scope.displaymetadata = false;

    $scope.descriptionOf = c => {
        var ar = $scope.oracle.columns;
        for (var i = 0; i < ar.length; i++) {
            if (ar[i].NAME === c)
                return ar[i].DESCRIPTION;
        }
        return null;
    };
    $scope.linksof = c => {
        var r = [];
        var l = $scope.oracle.links
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
    $scope.isLink = (c, v) => v && ($scope.linksof(c) !== []);
    $scope.gridSettings = getGridSettings("oracle.values");

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path : string = $location.path();
        var hash: string = $location.hash();
        var search: any = $location.search();
        if (search.owner) {
            $scope.oracle.owner = search.owner;
            delete search.owner;
        }
        if (path && hash === "pk") {
            var body: LINK = {
                key: "", 
                pk: objectToKvArray(search),
                direction: "pk"
            };
            request_tabledata($scope.oracle.owner, path, body);
            // TODO - single item display is not a database query yet
        } else if (path && hash && hash.indexOf(".") > 0) {
            var fkdir = hash.split(".", 2);
            var body: LINK = {
                key: fkdir[0],
                pk: objectToKvArray(search),
                direction: fkdir[1]
            }
            request_tabledata($scope.oracle.owner, path, body);
        } else if (path) {
            request_tabledata($scope.oracle.owner, path);
        } else if (!path) {
            // OK this is the start URL
        } else {
            $scope.error = "URL error: unrecognizable URL path " + $location.url();
        }
    });

    function show_restrict(r) {
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
        show_tabledata($scope.oracle.table, body);
    }

    function show_tabledata(tablename: string, body: LINK = undefined) {
        $location.path(tablename); // table as PATH
        if (body) {
            var hash = body.key ? body.key + '.' + body.direction : body.direction;
            $location.hash(hash); // foreign key constraint name + . + forward/back or pk as HASH
            $location.search(kvArrayToObject(body.pk)); // the primary keys as SEARCH
        } else {
            $location.hash(null);
            $location.search({});
        }
    }

    interface LINK { key: string, pk: Array<KV>, direction: string };
    interface KV { key: string, value: any };

    function buildPK(r : Object) : Array<KV> {
        var pks = $scope.oracle.pk;
        var pk : Array<KV> = [];
        for (var i = 0; i < pks.length; i++) {
            var v = r[pks[i]];
            pk.push({ key: pks[i], value: v });
        }
        return pk;
    }

    function kvArrayToObject(kv: Array<KV>) : Object {
        var r = {};
        for (var i = 0; i < kv.length; i++) {
            r[kv[i].key] = kv[i].value;
        }
        return r;
    }

    function objectToKvArray(o: Object): Array<KV> {
        var r: Array<KV> = [];
        for (var k in o) {
            if (o.hasOwnProperty(k))
                r.push({ key: k, value: o[k] });
        }
        return r;
    }

    // --- REST service calls
    var url = { tables: "api/tables/", tabledata: "api/table/", tablemetadata: "api/tablemetadata/" };

    function request_tabledata(owner: string, tablename: string, body: LINK = undefined) {
        if (body) {
            $scope.error = "following a link, loading data for " + tablename + " ...";
            $http({ method: "POST", url: url.tabledata + owner + "/" + tablename, data: body })
                .then(response => { $scope.oracle = response.data; $scope.error = null; })
                .catch(error);
        } else {
            $scope.error = "loading data for " + tablename + " ...";
            $http({ url: url.tabledata + owner + "/" + tablename })
                .then(response => { $scope.oracle = response.data; $scope.error = null; })
                .catch(error);
        }
    }

    function show_tables() {
        $scope.error = "loading tables ...";
        $scope.oracle.table = null;
        $scope.tables = [];
        $http({ url: url.tables + $scope.oracle.owner })
            .then(response => { $scope.tables = response.data; $scope.error = null })
            .catch(error);
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
            selection: { mode: 'single' }
        }
    }
});

mod.directive("outgoingLinks", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "outgoingLinks.html"
    }
});

mod.directive("incomingLinks", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "incomingLinks.html"
    }
});

mod.directive("listOfTables", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "listOfTables.html"
    }
});

mod.directive("singleitemData", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "singleitemData.html"
    }
});

mod.directive("singleitemDetailtables", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "singleitemDetailtables.html"
    }
});

mod.directive("multipleitemDxgrid", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "multipleitemDxgrid.html"
    }
});

mod.directive("multipleitemTable", function () {
    return {
        restrict: "E",
        replace: true,
        templateUrl: "multipleitemTable.html"
    }
});

