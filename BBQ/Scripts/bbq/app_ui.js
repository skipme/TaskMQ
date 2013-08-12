(function ($) {

    function reviewMainModel() {

    }
    var bbqmvc = angular.module('bbq', []);
    bbqmvc.run(function () {
        // Do post-load initialization stuff here
        $("select.customselect").selectpicker({ style: 'btn-primary', menuStyle: 'dropdown-inverse' });
    });
    bbqmvc.controller('bbqCtrl', function bbqCtrl($scope, $location) {

        $scope.intervals = [
            { t: 17, l: 'without', s: true, sv: false },
            { t: 18, l: 'ms', s: true, sv: true },
            { t: 19, l: 'sec', s: true, sv: true },
            { t: 20, l: 'daytime', s: false, sv: false },
            { t: 21, l: 'isolated', s: false, sv: false }];

        $scope.newtask = null;
        $scope.ref_task = null;
        $scope.edit_task = null;
        function resetNewTaskForm() {
            $scope.newtask = {
                description: '', channel: '', module: '',
                parametersStr: '', intervalType: $scope.intervals[0].t,
                intervalValue: 0,
                itpxy: $scope.intervals[0],
                mpxy: null,
                mpxyz: null
            };
        }
        function resetTaskForms() {
            resetNewTaskForm();
            $scope.edit_task = { ChannelName: '' };
        }
        function resetNewForms() {
            resetTaskForms();
        }
        $scope.textp = /^(\w+\s*[\u005B\u005D]*)+$/;// 'word word' or 'word' or 'word word word ' !only one space character!

        $scope.m_main = null;
        $scope.m_mods = null;

        $scope.triggers = null;
        function resetTriggers() {
            $scope.triggers = { Info: false, wReset: false, wRestart: false };
        }
        function aftermath() {
            var aftermath_context = {
                num: arguments.length,
                sequence: [],
                ondonef: null,
                onerrorf: null,
                errorHit: false,
                ondone: function (a, e) { this.ondonef = a; this.onerrorf = e; this.go(); },
                ok: function () {
                    this.num--;
                    if (this.num <= 0 && !this.errorHit) {
                        this.ondonef();
                    }
                },
                error: function (msg) {
                    this.num--;
                    if (this.onerrorf && !this.errorHit) {
                        this.errorHit = true;
                        this.onerrorf(msg);
                    }
                },
                go: function () {
                    for (var i = 0; i < this.sequence.length; i++) {
                        this.sequence[i](this);
                    }
                }
            };
            for (var i = 0; i < arguments.length; i++) {
                aftermath_context.sequence.push(arguments[i]);
            }
            return aftermath_context;
        }
        function ResyncAll() {
            aftermath(function (actx) {
                bbq_tmq.syncFrom(function (d) {
                    $scope.m_main = d;
                    //$scope.newtask.channel = d.Channels[0].Name;
                    $scope.$apply();

                    //bbq_tmq.toastr_success(" Synced main conf ");
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromMods(function (d) {
                    $scope.m_mods = d;
                    //$scope.newtask.module = d.Modules[0].Name;

                    $scope.$apply();

                    //bbq_tmq.toastr_success(" Synced modules conf ");
                    actx.ok();
                }, function () { actx.ok(); })
            }
            ).ondone(function () {
                $scope.triggers.Info = !bbq_tmq.check_synced();
                if (!$scope.triggers.Info)
                    bbq_tmq.toastr_info(" Configuration synced ", true);
                else {
                    bbq_tmq.toastr_error(" Erorr in configuration sync.", true);
                    bbq_tmq.rollbackAppC();
                }
                $scope.$apply();
            });
        }
        $scope.sync = function () {
            resetTriggers();
            resetNewForms();
            ResyncAll();
        }
        $scope.$watch('m_main', function () {
            return true;
        });
        $scope.update = function () {

        };
        //  task dialogs:
        $scope.task_represent = function (mpxy) {
            $scope.newtask.parametersStr = angular.toJson(mpxy.ParametersModel, true);
        }
        $scope.show_newtask = function () {
            //check sync state
            if (!bbq_tmq.check_synced()) {
                alert('the state is not synced...');
                return;
            }
            $scope.newtask.mpxy = $scope.m_mods.Modules[0];
            $scope.newtask.module = $scope.m_mods.Modules[0].Name;

            $('div#modal-new-task').modal('show');
        }
        $scope.newtask_add = function () {

            //validation
            if (!$scope.newTaskForm.$valid || $scope.m_main === null || $scope.m_mods === null
                || $scope.m_main.Channels.length === 0
                || $scope.m_mods.Modules.length === 0) { return; }
            //
            bbq_tmq.createTask($scope.newtask);
            $('div#modal-new-task').modal('hide');

            bbq_tmq.toastr_info(" Task created: " + $scope.newtask.description);
            resetNewTaskForm();
            $scope.triggers.wReset = true;
        }

        $scope.task_edit = function (model_e) {
            //check sync state
            if (!bbq_tmq.check_synced()) {
                alert('the state is not synced...');
                return;
            }
            $scope.ref_task = model_e;

            $scope.intervals.forEach(function (e) {
                if (e.t == model_e.intervalType)
                    $scope.newtask.itpxy = e;
            });
            $scope.newtask.mpxy = null;
            $scope.m_mods.Modules.forEach(function (e) {
                if (e.Name == model_e.ModuleName)
                    $scope.newtask.mpxyz = e;
            });

            $scope.newtask.parametersStr = angular.toJson(model_e.parameters, true);

            angular.copy(model_e, $scope.edit_task);

            $('div#modal-edit-task').modal('show');
        }

        $scope.task_edit_cpy = function () {
            if (!$scope.editTaskForm.$valid) { return; }
            var obj = null;
            try {
                obj = angular.fromJson($scope.newtask.parametersStr);
            } catch (e) {
                bbq_tmq.toastr_warning(" check json syntax! " + e.message);
                return;
            }

            angular.copy($scope.edit_task, $scope.ref_task);
            $scope.ref_task.parameters = obj;

            bbq_tmq.mainPartChanged();
            $scope.triggers.wReset = true;

            $('div#modal-edit-task').modal('hide');
        }
        // ~ task dialogs

        $scope.rollback_sync = function () {
            ResyncAll();
        }
        $scope.commit_reset = function () {
            aftermath(
                function (actx) {
                    bbq_tmq.syncToMain(function (data) {
                        bbq_tmq.toastr_success(" main configuration upload id: " + data.ConfigCommitID);
                        actx.ok();
                    }, function (msg) { actx.error("main configuration upload error"); })
                },
                function (actx) {
                    bbq_tmq.syncToMods(function (data) {
                        bbq_tmq.toastr_success(" module configuration upload id: " + data.ConfigCommitID);
                        actx.ok();
                    }, function (msg) { actx.error("module configuration upload error"); })
                })
          .ondone(function () {
              bbq_tmq.CommitAndReset(function (data) {
                 
                  refModels();
                  $scope.triggers.Info = true;
                  $scope.triggers.wReset = false;
                  
                  $scope.$apply();

                  bbq_tmq.toastr_success(" configuration commit ok ", true);
              }, function (msg) { bbq_tmq.toastr_error(" Configuration commit error: " + msg); })
          }, function (msg) {
              bbq_tmq.toastr_error(" Configuration commit error: " + msg);
          });
        }
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        
        resetTriggers();
        resetNewForms();
        $scope.triggers.Info = true;
        //ResyncAll();

        function refModels()
        {
            $scope.m_main = bbq_tmq.m_main;
            $scope.m_mods = bbq_tmq.m_mods;
        }
    });// ~controller

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
})(jQuery);