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

    $scope.class = (s) => 's'+s === $scope.highlight ? "highlight " : ""; 

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var path: string = $location.path().replace("/", "") || "TRREADY45";
        var hash: string = $location.hash();
        var search: any = $location.search();
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
            console.log(wf);
            return wf;
        }
    }

    function error(data) {
        notice(data.ExceptionMessage || data.Message || data);
    }
});

