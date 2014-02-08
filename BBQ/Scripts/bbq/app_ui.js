(function ($) {

    function reviewMainModel() {

    }
    var bbqmvc = angular.module('bbq', ['ngAnimate']);
   
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
        $scope.edit_task_index = -1;

        function resetNewTaskForm() {
            $scope.newtask = {
                description: '', channel: '', module: '',
                parametersStr: '{}', intervalType: $scope.intervals[0].t,
                intervalValue: 0,
                itpxy: $scope.intervals[0],
                mpxy: null,
                mpxyz: null
            };
        }
        function resetTaskForms() {
            resetNewTaskForm();
            $scope.edit_task = { ChannelName: '', parametersStr: '' };
        }
        function resetNewForms() {
            resetTaskForms();
        }
        $scope.textp = /^(\w+\s*[\u005B\u005D]*)+$/;// 'word word' or 'word' or 'word word word ' !only one space character!

        $scope.m_main = null;
        $scope.m_mods = null;
        $scope.m_assemblys = null;
        
        // * statistic
        var heartbeatInterval = null;
        $scope.stat_channels = [{ name: 'EmailC', heartbeat: [12, 14, 155, 144, 33, 55, 66, 77, 88, 33, 44, 55, 66, 33, 22], throughput: [12, 14, 155, 144] }];
        function updateHeartbeat() {

            aftermath(function (actx) {
                bbq_tmq.heartbeat(function (resp) {
                    resp.Channels.forEach(function (c) {

                        var ref = null;
                        for (var i = 0; i < $scope.stat_channels.length; i++) {
                            if ($scope.stat_channels[i].name == c.Name) {
                                ref = $scope.stat_channels[i];
                                break;
                            }
                        }
                        if (ref === null) {
                            ref = { name: c.Name, heartbeat: [], lastLeftHB: null, throughput: [] };
                            $scope.stat_channels.push(ref);
                        }

                        if (ref.lastLeftHB != c.Left) {
                            ref.lastLeftHB = c.Left;
                            ref.heartbeat.push(c.Count);
                            if (ref.heartbeat.length > 30)
                                ref.heartbeat.splice(0, 1);

                            $('span.channel-sparkline-heartbeat[statsel="' + c.Name + '"]').sparkline(ref.heartbeat, {
                                width: 70, //type: 'line',
                                lineColor: '#34495e',
                                fillColor: '#1abc9c', tooltipSuffix: ' messages'
                            });
                        }
                    });

                    actx.ok();

                }, function () {
                    actx.ok();
                });
            }, function (actx) {
                bbq_tmq.throughput(function (resp) {
                    resp.Channels.forEach(function (c) {

                        var ref = null;
                        for (var i = 0; i < $scope.stat_channels.length; i++) {
                            if ($scope.stat_channels[i].name == c.Name) {
                                ref = $scope.stat_channels[i];
                                break;
                            }
                        }
                        if (ref === null) {
                            ref = { name: c.Name, throughput: [], lastLeftTP: null, heartbeat: [] };
                            $scope.stat_channels.push(ref);
                        }

                        if (ref.lastLeftTP != c.Left) {
                            ref.lastLeftTP = c.Left;
                            ref.throughput.push(c.Count);
                            if (ref.throughput.length > 30)
                                ref.throughput.splice(0, 1);

                            $('span.channel-sparkline-throughput[statsel="' + c.Name + '"]').sparkline(ref.throughput, {
                                width: 70, //type: 'line',
                                lineColor: '#34495e',
                                fillColor: '#1abc9c', tooltipSuffix: ' messages/min'
                            });
                        }

                    });
                    actx.ok();

                }, function () {
                    actx.ok();
                });
            }).ondone(function () {
                tryKillHeartbeat();
                heartbeatInterval = setTimeout(updateHeartbeat, 1000 * 5);
            });

            
        }
        function tryKillHeartbeat() {
            if (heartbeatInterval !== null)
                clearTimeout(heartbeatInterval);
        }
        //~statistic
        // assemblies
        var assembliesInterval = null;
        function updateAssemblies() {

            aftermath(function (actx) {
                bbq_tmq.assemblies.takeAdvanceInfo(
                    function (data) {
                        data.forEach(function (asm) {
                            $('div#assemblys span[assembly_sel="' + asm.Name + '"][app_role="status"]').text(asm.state);
                            
                            $('div#assemblys span[assembly_sel="' + asm.Name + '"][app_role="desc"] a i').attr("class", 
                                asm.revisionTag == asm.revisionSourceTag ? "icon-ok" : "icon-chevron-up");

                            $('div#assemblys span[assembly_sel="' + asm.Name + '"][app_role="desc-loaded"] a i').attr("class",
                               asm.loaded ? "icon-ok" : "icon-warning-sign");
                            //$('div#assemblys span[assembly_sel="' + asm.Name + '"][app_role="desc"] a i').text(
                            //    asm.revisionTag + " / " + asm.revisionSourceTag);
                        }); 
                        actx.ok();
                    },
                    function () { actx.ok(); }
                    );
            }
            //, function (actx) {
            //}
            ).ondone(function () {
                tryKillAssembliesUpdate();
                assembliesInterval = setTimeout(updateAssemblies, 1000 * 15);
            });
        }
        function tryKillAssembliesUpdate() {
            if (assembliesInterval !== null)
                clearTimeout(assembliesInterval);
        }
        $scope.package_assembly = function (asm) {
            bbq_tmq.assemblies.UpdatePackage(asm.Name);
        }
        $scope.fetch_assembly = function (asm) {
            bbq_tmq.assemblies.fetchAssemblySource(asm.Name);
        }
        $scope.build_assembly = function (asm) {
            bbq_tmq.assemblies.buildAssemblySource(asm.Name);
        }
        $scope.assembly_info = function (asm) {
            bbq_tmq.assemblies.takeAdvanceInfo(
                   function (data) {
                       for (var i = 0; i < data.length; i++) {
                           var lasm = data[i];
                           if (asm.Name == lasm.Name) {
                               $scope.mi_cassembly = lasm;
                               $scope.$apply();
                               $('div#modal-info-assembly').modal('show');
                               break;
                           }
                       }
                   },
                   function () { bbq_tmq.toastr_warning("cant get assembly info"); }
                   );
        }
        
        // ~assemblies
        $scope.triggers = null;
        function resetTriggers() {
            $scope.triggers = { Info: false, wReset: false, wRestart: false };
        }
        //////////////////
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
        //////////////////////
        function ResyncAll() {
            aftermath(function (actx) {
                bbq_tmq.syncFrom(function (d) {
                    $scope.m_main = d;
                    //$scope.newtask.channel = d.Channels[0].Name;
                    //$scope.$apply();

                    //bbq_tmq.toastr_success(" Synced main conf ");
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromMods(function (d) {
                    $scope.m_mods = d;
                    //$scope.newtask.module = d.Modules[0].Name;

                    //$scope.$apply();

                    //bbq_tmq.toastr_success(" Synced modules conf ");
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromAssemblys(function (d) {
                    $scope.m_assemblys = d;

                    //$scope.$apply();

                    //bbq_tmq.toastr_success(" Synced assemblys conf ");
                    actx.ok();
                }, function () { actx.ok(); })
            }
            ).ondone(function () {
                //$scope.$apply();

                $scope.triggers.Info = !bbq_tmq.check_synced();
                if (!$scope.triggers.Info) {
                    bbq_tmq.toastr_info(" Configuration synced ", true);
                    updateHeartbeat();
                    updateAssemblies();
                }
                else {
                    bbq_tmq.toastr_error(" Error in configuration sync.", true);
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

            resetNewTaskForm();

            $('div#modal-new-task').modal('show');
        }
        $scope.newtask_add = function () {

            //validation
            if (!$scope.newTaskForm.$valid || $scope.m_main === null || $scope.m_mods === null
                || $scope.m_main.Channels.length === 0
                || $scope.m_mods.Modules.length === 0) { bbq_tmq.toastr_info(" Not enough channels/modules on platform "); return; }
            //
            bbq_tmq.createTask($scope.newtask);
            $('div#modal-new-task').modal('hide');

            bbq_tmq.toastr_info(" Task created: " + $scope.newtask.description);
            resetNewTaskForm();
            $scope.triggers.wReset = true;
        }

        $scope.task_edit = function (model_e, r_index) {
            
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

            $scope.edit_task_index = r_index;
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

            animateChangeElement($("#t-tasks tr.animchange")[$scope.edit_task_index]);
            $('div#modal-edit-task').modal('hide');
        }
        function animateChangeElement(elt) {
            if (typeof elt === "undefined" || elt === null || elt.classList.contains("rchange"))
                return;
            elt.classList.add("rchange");
            setTimeout(function () { elt.classList.remove("rchange"); }, 1000);
        }
        $scope.task_edit_del = function () {

            $scope.m_main.Tasks.splice(
                $scope.m_main.Tasks.indexOf($scope.ref_task), 1);
            
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
            $scope.m_assemblys = bbq_tmq.m_assemblys;
        }
    });// ~controller

    bbqmvc.filter('long', function () {
        return function (json) {
            if (!json) { return '-'; }
            var str = JSON.stringify(json);
            return str.length > 60 ? str.substr(0, 60) + '...' : str;
        };
    });
    bbqmvc.filter('jsonshort', function () {
        return function (json) {
            if (!json) { return '-'; }
            var str = JSON.stringify(json);
            return str.length > 30 ? str.substr(0, 30) + '...' : str;
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
    bbqmvc.directive('animateOnChange', function ($animate) {
        return function (scope, elem, attr) {
            var elemrow = elem[0].parentElement;
            scope.$watch(attr.animateOnChange, function (nv, ov) {
                if (elemrow.classList.contains("rchange"))
                    return;
                elemrow.classList.add("rchange");
                setTimeout(function () { elemrow.classList.remove("rchange"); }, 1000);
            })
        }
    })
})(jQuery);