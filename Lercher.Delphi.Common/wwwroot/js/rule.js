/// <reference path="d.ts/angular.d.ts"/>
// angular 1.4.5
// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
var mod = angular.module("delphiApp", ['dx']);
mod.controller("rule", function ($scope, $http, $location) {
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.oracle = {
        owner: null,
        language: null,
        rule: null
    };
    $scope.rule = {};
    // --- REST service calls
    var url = { rule: "api/rule/" };
    $scope.set = function (property, val) {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    };
    $scope.crii = function (id) { return "default.html#/CRITERIA?CRIID=" + id + "#pk"; };
    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path = $location.path().replace("/", "") || "TRREADY45";
        var hash = $location.hash();
        var search = $location.search();
        $scope.oracle.language = search.language || "EN";
        $scope.oracle.rule = search.rule || "";
        $scope.errors = [];
        $scope.closederrors = 0;
        if ($scope.oracle.owner !== path) {
            $scope.oracle.owner = path;
        }
        show_rule($scope.oracle.rule, $scope.oracle.language);
    });
    function notice(s) {
        $scope.errors.push(s);
    }
    function show_rule(rule, language) {
        notice("loading rule " + rule + "/" + language + " ...");
        $scope.workflow = {};
        $http.get(url.rule + $scope.oracle.owner + "/" + rule, { params: { language: language } }).error(error)
            .success(function (data) { $scope.rule = denormalize(data); $scope.closederrors++; });
        function denormalize(rule) {
            rule.rule = rule.rule[0];
            console.log(rule);
            return rule;
        }
    }
    function error(data) {
        notice(data.ExceptionMessage || data.Message || data);
    }
});
