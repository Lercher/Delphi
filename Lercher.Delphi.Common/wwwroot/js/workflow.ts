﻿/// <reference path="d.ts/angular.d.ts"/>
// angular 1.4.5

declare var $: any;

// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file

var mod = angular.module("delphiApp", ['dx']);
mod.controller("workflow", function ($scope, $http, $location) {
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive

    $scope.set = (property: string, val: string) => {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    }

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path: string = $location.path();
        var hash: string = $location.hash();
        var search = $location.search();
        $scope.oracle = {
            owner: path.replace("/", "") || "TRREADY45",
            language: search.language || "EN",
            target: search.target || "DOSSIER"
        };
        $scope.languages = [];
        $scope.targets = [];
        $scope.errors = [];
        show("languages");
        show("targets");
    });

    function notice(s: string) {
        $scope.errors.push(s);
    }

    // --- REST service calls
    var url = { values: "api/values/" };

    function show(s : string) {
        notice("loading " + s + " ...");
        $scope.languages = [];
        $http.get(url.values + $scope.oracle.owner + "/" + s).error(error)
            .success(data => { $scope[s] = data; });
    }

    function error(data) {
        notice(data.ExceptionMessage || data.Message || data);
    }

    // startup code
});

