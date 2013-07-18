//(function ($) {
    $(document).ready(
        function () {
            $("select.customselect").selectpicker({ style: 'btn-primary', menuStyle: 'dropdown-inverse' });
    });

    function reviewMainModel()
    {

    }
    var bbqmvc = angular.module('bbq', []);
    bbqmvc.controller('bbqCtrl', function bbqCtrl($scope, $location) {

        $scope.intervals = [{ t: 17, l: 'without' }, { t: 18, l: 'ms' }, { t: 19, l: 'sec' }, { t: 20, l: 'daytime' }, { t: 21, l: 'isolated' }];

        $scope.newtask = { description: '', channel: '', module: '', parametersStr: '', intervalType: $scope.intervals[0].t, intervalValue: 0 };
        $scope.textp = /^(\w+\s{0,1})+$/;// 'word word' or 'word' or 'word word word ' !only one space character!
        
        $scope.m_main = null;
        $scope.m_mods = null;

        $scope.triggers = { Info: false, wReset: false, wRestart: false };

        bbq_tmq.syncFrom(function (d) {
            $scope.m_main = d;
            $scope.newtask.channel = d.Channels[0].Name;
            $scope.$apply();
        }, function () { });

        bbq_tmq.syncFromMods(function (d) {
            $scope.m_mods = d;
            $scope.newtask.module = d.Modules[0].Name;
            $scope.$apply();
        }, function () { });

        $scope.$watch('m_main', function () {
            return true;
        });
        $scope.update = function () {
          
        };
    });
    bbqmvc.filter('long', function () {
        return function (json) {
            if (!json) { return '-'; }
            str = JSON.stringify(json);
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