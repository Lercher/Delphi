/// <reference path="d.ts/angular.d.ts"/>
// angular 1.4.5
// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
var mod = angular.module("delphiApp", ['dx']);
mod.controller("delphi", function ($scope, $http) {
    $scope.filter = { limit: 15 };
    $scope.show = { tables: show_tables, tabledata: show_tabledata };
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    var url = { tables: "api/tables/", tabledata: "api/table/", tablemetadata: "api/tablemetadata/" };
    $scope.oracle = { owner: "TRREADY45" };
    $scope.error = null;
    $scope.tables = [];
    $scope.gridSettings = {};
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
    $scope.navigateto = function (r, fk, direction) {
        // alert("Show the one " + fk.destination + " where " + fk.matchcolumn[0].dest + " = " + r[fk.matchcolumn[0].src] + " ... by " + fk.key + " with " + fk.matchcolumn.length + " match(es))");
        var body = { key: fk.key, pk: buildPK(r), direction: direction };
        show_tabledata($scope.oracle.table, body);
    };
    $scope.open = function (r) { return $scope.oracle.values = [r]; };
    $scope.gridSettings = getGridSettings("oracle.values");
    function buildPK(r) {
        var pks = $scope.oracle.pk;
        var pk = [];
        for (var i = 0; i < pks.length; i++) {
            var v = r[pks[i]];
            pk.push({ key: pks[i], value: v });
        }
        return pk;
    }
    function show_tabledata(t, body) {
        if (body) {
            $scope.error = "following a link, loading data for " + t + " ...";
            $http.post(url.tabledata + $scope.oracle.owner + "/" + t, body).error(error)
                .success(function (data) { $scope.oracle = data; $scope.error = null; });
        }
        else {
            $scope.error = "loading data for " + t + " ...";
            $http.get(url.tabledata + $scope.oracle.owner + "/" + t).error(error)
                .success(function (data) { $scope.oracle = data; $scope.error = null; });
        }
    }
    function show_tables() {
        $scope.error = "loading tables ...";
        $scope.oracle.table = null;
        $scope.tables = [];
        $http.get(url.tables + $scope.oracle.owner).error(error)
            .success(function (data) { $scope.tables = data; $scope.error = null; });
    }
    function error(data) {
        $scope.error = data.ExceptionMessage || data.Message || data;
    }
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
