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

var mod = angular.module("delphiApp", ['ngAnimate', 'ngSanitize', 'ui.bootstrap']);
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

    $scope.set = (property: string, val: string) => {
        var search = $location.search();
        search[property] = val;
        $location.search(search);
        return false;
    }

    $scope.class = (s) => 's' + s === $scope.highlight ? "highlight " : "";

    $scope.$on('$locationChangeSuccess', function () {
        $log.log("$locationChangeSuccess -> " + $location.url());
        var search: any = $location.search();
        $scope.highlight = search.hl || "(none)";
        $scope.oracle.language = search.language || "EN";
        $scope.oracle.workflow = search.workflow || "WFALLGOOD";
        var owner: string = search.owner || "TRREADY45";
        $scope.errors = [];
        $scope.closederrors = 0;
        if ($scope.oracle.owner !== owner) {
            $scope.oracle.owner = owner;
            show("languages");
            show("workflows");
        }
        show_workflow($scope.oracle.workflow, $scope.oracle.language);
    });

    $scope.showdlg = step => {
        $uibModal.open({
            templateUrl: 'events.html',
            controller: 'eventsController',
            resolve: { step, oracle: $scope.oracle }
        }).result.catch(function () {
            // ignore "Possibly unhandled rejection: backdrop click"
        });
    };

    $scope.showcqaction = () => {
        $uibModal.open({
            templateUrl: 'cqaction.html'
        }).result.catch(function () {
            // ignore "Possibly unhandled rejection: backdrop click"
        });
    };

    function show(s: string) {
        notice("loading " + s + " ...");
        $scope[s] = [];
        $http({ url: url.values + $scope.oracle.owner + "/" + s })
            .then(response => { $scope[s] = response.data; $scope.closederrors++; })
            .catch(error);
    }

    function show_workflow(wf: string, language: string) {
        notice("loading workflow " + wf + "/" + language + " ...");
        $scope.workflow = {};
        $http({ url: url.workflowfo + $scope.oracle.owner + "/" + wf, params: { language } })
            .then(response => { $scope.workflow = denormalize(response.data); $scope.closederrors++; })
            .catch(error);

        function denormalize(wf) {
            wf.consequencerules = objectify(wf.consequencerules);
            wf.workflow = wf.workflow[0];
            for (var s = 0; s < wf.steps.length; s++) {
                var step = wf.steps[s];
                step.consequences = [];
                for (var c = 0; c < wf.consequences.length; c++) {
                    var cq = wf.consequences[c];
                    if (cq.order === step.order) {
                        cq.rules = findrules(wf.consequencerules, step.order, cq.consequenceorder);
                        step.consequences.push(cq);
                    }
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
            delete wf.consequencerules;
            $log.log(wf);
            return wf;
        }

        function objectify(consequencerules) {
            var r = {};
            for (var i in consequencerules) {
                var cr = consequencerules[i];
                var p = "p" + cr.id;
                r[p] = r[p] || {};
                var val = cr.textvalue || cr.value;
                r[p][cr.code] = val;
                r[p].rule = r[p].rule || cr.rule; // it's currently the same in every line, but it needn't.
            }
            return r;
        }

        function findrules(consequencerules, step, cq) {
            var r = [];
            for (var i in consequencerules) {
                var cr = consequencerules[i];
                if (cr.WSTORDER === step && cr.WSCORDER === cq)
                    r.push(cr.rule);
            }
            return r;
        }

    }

    function notice(s: string) {
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
        .then(response => $scope.eventdefinition = response.data)
        .catch(error);

    function notice(s: string) {
        $scope.errors.push(s);
    }

    function error(r) {
        notice(r.data.ExceptionMessage || r.data.Message || r.data);
    }
});