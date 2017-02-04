/// <reference path="d.ts/angular.d.ts"/>
// angular 1.4.5

declare var $: any;

// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file

var mod = angular.module("delphiApp", ['dx']);
mod.controller("workflowfo", function ($scope, $http, $location) {
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.languages = [];
    $scope.workflows = [];
    $scope.oracle = {
        owner: null,
        language: null,
        target: null
    };
    $scope.workflow = {};

    // --- REST service calls
    var url = { values: "api/values/", workflowfo: "api/workflowfo/" };

    $scope.set = (property: string, val: string) => {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    }

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path: string = $location.path().replace("/", "") || "TRREADY45";
        var hash: string = $location.hash();
        var search: any = $location.search();
        $scope.oracle.language = search.language || "EN";
        $scope.oracle.workflow = search.workflow || "WFALLGOOD";
        $scope.errors = [];
        $scope.closederrors = 0;
        if ($scope.oracle.owner !== path) {
            $scope.oracle.owner = path;
            show("languages");
            show("workflows");
        }
        show_workflow($scope.oracle.workflow, $scope.oracle.language);
    });

    function notice(s: string) {
        $scope.errors.push(s);
    }

    function show(s: string) {
        notice("loading " + s + " ...");
        $scope[s] = [];
        $http.get(url.values + $scope.oracle.owner + "/" + s).error(error)
            .success(data => { $scope[s] = data; $scope.closederrors++; });
    }

    function show_workflow(wf: string, language: string) {
        notice("loading workflow " + wf + "/" + language + " ...");
        $scope.workflow = {};
        $http.get(url.workflowfo + $scope.oracle.owner + "/" + wf, { params: { language } }).error(error)
            .success(data => { $scope.workflow = denormalize(data); $scope.closederrors++; });

        function denormalize(wf) {
            wf.workflow = wf.workflow[0];
            return wf;
        }
    }

    function error(data) {
        notice(data.ExceptionMessage || data.Message || data);
    }
});

