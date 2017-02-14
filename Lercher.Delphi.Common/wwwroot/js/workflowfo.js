/// <reference path="d.ts/angular.d.ts"/>
// angular 1.6.1
// VS2015 - important 
// Check Tools/Options/Typescript/Project/General - Automatically compile typescript files which are not part of a project
// and look for "Output(s) generated successfully." in the status bar after saving this file
var mod = angular.module("delphiApp", ['dx', 'ngAnimate', 'ngSanitize', 'ui.bootstrap']);
mod.controller("workflowfo", function ($scope, $http, $location, $uibModal, $log) {
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
    $scope.set = function (property, val) {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    };
    $scope.class = function (s) { return 's' + s === $scope.highlight ? "highlight " : ""; };
    $scope.$on('$locationChangeSuccess', function () {
        $log.log("$locationChangeSuccess -> " + $location.url());
        var path = $location.path().replace("/", "") || "TRREADY45";
        var hash = $location.hash();
        var search = $location.search();
        $scope.highlight = search.hl || "(none)";
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
    $scope.showdlg = function (step) {
        $uibModal.open({
            templateUrl: 'events.html',
            controller: 'eventsController',
            resolve: { step: step, oracle: $scope.oracle }
        }).result.catch(function () {
            // ignore. 
            // "Possibly unhandled rejection: backdrop click"
        });
        ;
    };
    function show(s) {
        notice("loading " + s + " ...");
        $scope[s] = [];
        $http({ url: url.values + $scope.oracle.owner + "/" + s })
            .then(function (response) { $scope[s] = response.data; $scope.closederrors++; })
            .catch(error);
    }
    function show_workflow(wf, language) {
        notice("loading workflow " + wf + "/" + language + " ...");
        $scope.workflow = {};
        $http({ url: url.workflowfo + $scope.oracle.owner + "/" + wf, params: { language: language } })
            .then(function (response) { $scope.workflow = denormalize(response.data); $scope.closederrors++; })
            .catch(error);
        function denormalize(wf) {
            wf.workflow = wf.workflow[0];
            for (var s = 0; s < wf.steps.length; s++) {
                var step = wf.steps[s];
                step.consequences = [];
                for (var c = 0; c < wf.consequences.length; c++) {
                    var cq = wf.consequences[c];
                    if (cq.order === step.order)
                        step.consequences.push(cq);
                }
                step.jumps = [];
                for (var j = 0; j < wf.jumps.length; j++) {
                    var jump = wf.jumps[j];
                    if (jump.order === step.order)
                        step.jumps.push(jump);
                }
                step.dependencies = [];
                for (var d = 0; d < wf.dependencies.length; d++) {
                    var dep = wf.dependencies[d];
                    if (dep.order === step.order)
                        step.dependencies.push(dep);
                }
            }
            delete wf.consequences;
            delete wf.jumps;
            delete wf.dependencies;
            $log.log(wf);
            return wf;
        }
    }
    function notice(s) {
        $scope.errors.push(s);
    }
    function error(r) {
        notice(r.data.ExceptionMessage || r.data.Message || r.data);
    }
});
mod.controller("eventsController", function ($scope, $log, $http, $uibModalInstance, step, oracle) {
    // TODO: load 
    $scope.step = step;
    $scope.oracle = oracle;
    $scope.eventdefinition = {};
    $log.log(step);
    var url = { values: "api/values/", event: "api/event/" };
    $http({ url: url.event + $scope.oracle.owner + "/AVDOSS", params: { language: $scope.oracle.language, code: "GLOBAL" } })
        .then(function (response) { return $scope.eventdefinition = response.data; })
        .catch(error);
    function notice(s) {
        $scope.errors.push(s);
    }
    function error(r) {
        notice(r.data.ExceptionMessage || r.data.Message || r.data);
    }
});
