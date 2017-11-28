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

var mod = angular.module("delphiApp", []);
mod.controller("columns", function ($scope, $http, $location) {
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.oracle = {
        owner: null,
    };
    $scope.searchfor = "";
    $scope.list = [];

    // --- REST service calls
    var url = { columns: "api/column/" };

    $scope.set = (property: string, val: string) => {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    }

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var search: any = $location.search();
        var owner: string = search.owner || "TRunknown";
        var q: string = search.q || null;
        $scope.oracle.owner = owner;
        $scope.searchfor = q;
        if (q)
            show(owner, q);
    });

    function notice(s: string) {
        $scope.error = s;
    }

    function show(owner: string, q: string) {
        notice("loading " + q + " ...");
        $scope.list = [];
        $http({ url: url.columns + $scope.oracle.owner + "/" + q })
            .then(response => { $scope.list = response.data; $scope.searchfor = q; $scope.oracle.owner = owner; notice(null); })
            .catch(error);
    }

    function error(r) {
        notice(r.data.ExceptionMessage || r.data.Message || r.data);
    }
});

