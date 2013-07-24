//(function ($) {
$(document).ready(
    function () {
        $("select.customselect").selectpicker({ style: 'btn-primary', menuStyle: 'dropdown-inverse' });
    });

function reviewMainModel() {

}
var bbqmvc = angular.module('bbq', []);
bbqmvc.controller('bbqCtrl', function bbqCtrl($scope, $location) {

    $scope.intervals = [
        { t: 17, l: 'without', s: true, sv: false },
        { t: 18, l: 'ms', s: true, sv: true },
        { t: 19, l: 'sec', s: true, sv: true },
        { t: 20, l: 'daytime', s: false, sv: false },
        { t: 21, l: 'isolated', s: false, sv: false }];

    $scope.newtask = null;
    function resetNewTask() {
        $scope.newtask = { description: '', channel: '', module: '', parametersStr: '', intervalType: $scope.intervals[0].t, intervalValue: 0, itpxy: $scope.intervals[0], mpxy: null };
    }
    function resetNewForms() {
        resetNewTask();
    }
    $scope.textp = /^(\w+\s{0,1})+$/;// 'word word' or 'word' or 'word word word ' !only one space character!

    $scope.m_main = null;
    $scope.m_mods = null;

    $scope.triggers = null;

    function ResyncAll() {

        $scope.triggers = { Info: false, wReset: false, wRestart: false };

        bbq_tmq.syncFrom(function (d) {
            $scope.m_main = d;
            $scope.newtask.channel = d.Channels[0].Name;
            $scope.$apply();

            bbq_tmq.toastr_success(" Synced main conf ");
        }, function () { });

        bbq_tmq.syncFromMods(function (d) {
            $scope.m_mods = d;
            $scope.newtask.module = d.Modules[0].Name;
            $scope.newtask.mpxy = d.Modules[0];
            $scope.$apply();

            bbq_tmq.toastr_success(" Synced modules conf ");
        }, function () { });
    }

    $scope.$watch('m_main', function () {
        return true;
    });
    $scope.update = function () {

    };
    // new task dialog:
    $scope.newtask_represent = function () {
        $scope.newtask.parametersStr = angular.toJson($scope.newtask.mpxy.ParametersModel, true);
    }
    $scope.newtask_add = function () {
        // check sync state
        if (!bbq_tmq.check_synced()) {
            alert('the state is not synced...');
            return;
        }
        //validation
        if (!$scope.newTaskForm.$valid || $scope.m_main === null || $scope.m_mods === null
            || $scope.m_main.Channels.length === 0
            || $scope.m_mods.Modules.length === 0) { return; }
        //
        bbq_tmq.createTask($scope.newtask);
        $('div#modal-new-task').modal('hide');

        bbq_tmq.toastr_info(" Task created: " + $scope.newtask.description);
        resetNewTask();
        $scope.triggers.wReset = true;
    }
    // ~new task dialog

    $scope.rollback_sync = function () {
        ResyncAll();
    }
    $scope.commit_reset = function () {
        bbq_tmq.syncToAndReset(function (data) {
            bbq_tmq.toastr_success(" main configuration upload id: " + data.ConfigCommitID);
        }, function () { });
    }

    resetNewForms();
    ResyncAll();
});
bbqmvc.filter('long', function () {
    return function (json) {
        if (!json) { return '-'; }
        var str = JSON.stringify(json);
        return str.length > 60 ? str.substr(0, 60) + '...' : str;
    };
});
bbqmvc.filter('intervt', function () {
    return function (num) {
        switch (num) {
            case 17: return "without";
            case 18: return "ms";
            case 19: return "sec";
            case 20: return "daytime";
            case 21: return "isolated";
            default:
                break;
        }
    };
});
//})(jQuery);