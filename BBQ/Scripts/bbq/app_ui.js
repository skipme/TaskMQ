//(function ($) {
    $(document).ready(
        function () {

    });

    function reviewMainModel()
    {

    }
    var bbqmvc = angular.module('bbq', []);
    bbqmvc.controller('bbqCtrl', function bbqCtrl($scope, $location) {
        $scope.m_main = null;
        $scope.m_mods = null;

        $scope.triggers = { Info: false, wReset: false, wRestart: false };

        bbq_tmq.syncFrom(function (d) {
            $scope.m_main = d;
            $scope.$apply();
        }, function () { });

        bbq_tmq.syncFromMods(function (d) {
            $scope.m_mods = d;
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