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

    $scope.set = (property: string, val: string) => {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    }

    $scope.$on('$locationChangeSuccess', function () {
        console.log("$locationChangeSuccess -> " + $location.url());
        var search: any = $location.search();
        $scope.oracle.language = search.language || "EN";
        $scope.oracle.target = search.target || "AVDOSS";
        var owner: string = search.owner || "TRREADY45";
        $scope.errors = [];
        $scope.closederrors = 0;
        if ($scope.oracle.owner !== owner) {
            $scope.oracle.owner = owner;
            show("languages");
            show("targets");
        }
        show_workflow($scope.oracle.target, $scope.oracle.language);
    });

    function notice(s: string) {
        $scope.errors.push(s);
    }

    function show(s: string) {
        notice("loading " + s + " ...");
        $scope[s] = [];
        $http({ url: url.values + $scope.oracle.owner + "/" + s })
            .then(response => { $scope[s] = response.data; $scope.closederrors++; })
            .catch(error);
    }

    function show_workflow(target: string, language: string) {
        notice("loading workflow " + target + "/" + language + " ...");
        $scope.workflow = {};
        $http({url: url.workflow + $scope.oracle.owner + "/" + target, params: { language } })
            .then(response => { $scope.workflow = denormalize(response.data); $scope.closederrors++; })
            .catch(error);

        function denormalize(wf) {
            // convert the array steps to an associative array:
            var ar = wf.steps;
            wf.steps = {}
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
        };

    }

    function error(r) {
        notice(r.data.ExceptionMessage || r.data.Message || r.data);
    }
});

