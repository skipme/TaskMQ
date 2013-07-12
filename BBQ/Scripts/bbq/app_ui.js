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

       
        bbq_tmq.syncFrom(function (d) {
            $scope.m_main = d;
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

//})(jQuery);