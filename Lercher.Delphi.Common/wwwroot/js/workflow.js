/// <reference path="d.ts/angular.d.ts"/>
// angular 1.4.5
// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
var mod = angular.module("delphiApp", ['dx']);
mod.controller("workflow", function ($scope, $http, $location) {
    $scope.isObject = angular.isObject; // has to be a scope function to use it in a ng-switch directive
    $scope.languages = [];
    $scope.targets = [];
    $scope.oracle = {
        owner: null,
        language: null,
        target: null
    };
    $scope.workflow = {};
    // --- REST service calls
    var url = { values: "api/values/", workflow: "api/workflow/" };
    $scope.set = function (property, val) {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    };
    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path = $location.path().replace("/", "") || "TRREADY45";
        var hash = $location.hash();
        var search = $location.search();
        $scope.oracle.language = search.language || "EN";
        $scope.oracle.target = search.target || "DOSSIER";
        $scope.errors = [];
        $scope.closederrors = 0;
        if ($scope.oracle.owner !== path) {
            $scope.oracle.owner = path;
            show("languages");
            show("targets");
        }
        show_workflow($scope.oracle.target, $scope.oracle.language);
    });
    function notice(s) {
        $scope.errors.push(s);
    }
    function show(s) {
        notice("loading " + s + " ...");
        $scope[s] = [];
        $http.get(url.values + $scope.oracle.owner + "/" + s).error(error)
            .success(function (data) { $scope[s] = data; $scope.closederrors++; });
    }
    function show_workflow(target, language) {
        notice("loading workflow " + target + "/" + language + " ...");
        $scope.workflow = {};
        $http.get(url.workflow + $scope.oracle.owner + "/" + target, { params: { language: language } }).error(error)
            .success(function (data) { $scope.workflow = denormalize(data); $scope.closederrors++; });
        function denormalize(wf) {
            // convert the array steps to an associative array:
            var ar = wf.steps;
            wf.steps = {};
            for (var s = 0; s < ar.length; s++)
                wf.steps[ar[s].step] = ar[s];
            // assign a steps array to each phase:
            for (var p = 0; p < wf.phases.length; p++) {
                var phase = wf.phases[p];
                phase.steps = stepsof(wf, phase);
            }
            return wf;
        }
        function stepsof(wf, p) {
            var result = [];
            function lookup(s) {
                var uk = { step: s, internal: 0, label: "(unknown)" };
                return wf.steps[s] || uk;
            }
            var rel = wf.relations;
            for (var r = 0; r < rel.length; r++)
                if (p.phase === rel[r].phase)
                    result.push(lookup(rel[r].step));
            return result;
        }
        ;
    }
    function error(data) {
        notice(data.ExceptionMessage || data.Message || data);
    }
});
