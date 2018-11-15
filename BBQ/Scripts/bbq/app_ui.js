﻿(function ($) {

    function reviewMainModel() {

    }
    var bbqmvc = angular.module('bbq', ['ngAnimate']);

    bbqmvc.run(function () {
        // Do post-load initialization stuff here
    });
    bbqmvc.controller('bbqCtrl', function bbqCtrl($scope, $location) {

        setTimeout(function () { $("div[ng-app='bbq']").fadeIn(); }, 500);

        $scope.intervals = [
            { t: "withoutInterval", l: 'Immediately', s: true, sv: false },
            { t: "intervalMilliseconds", l: 'Milliseconds', s: true, sv: true },
            { t: "intervalSeconds", l: 'Seconds', s: true, sv: true },
            { t: "DayTime", l: 'Specific Datetime', s: false, sv: false },
            { t: "isolatedThread", l: 'Isolated Thread', s: false, sv: false }];

        $scope.newtask = null;
        $scope.ref_task = null;
        $scope.edit_task = null;
        $scope.edit_task_index = -1;
        $scope.edit_assembly = null;
        $scope.ref_assembly = null;

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
            $scope.edit_assembly = {};
        }
        function resetNewForms() {
            resetTaskForms();
        }
        $scope.textp = /^(\w+\s*[\u005B\u005D]*)+$/;// 'word word' or 'word' or 'word word word ' !only one space character!

        $scope.m_main = null;
        $scope.m_mods = null;
        $scope.m_assemblys = null;
        $scope.m_extra = null;

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
                        var restartReq = false;
                        data.forEach(function (asm) {
                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] span[app_role="status"]').text(asm.state);

                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] span[app_role="desc"] a i').attr("class",
                                asm.revisionTag == asm.revisionSourceTag ? "fas fa-check-circle" : "fas fa-level-up-alt");

                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] span[app_role="desc-loaded"] a i').attr("class",
                               asm.loaded ? "fas fa-check-circle" : "fas fa-exclamation-triangle");

                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] td[app_role="fetch"] a').css("display",
                                asm.allowedFetch ? "block" : "none");

                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] td[app_role="build"] a').css("display",
                                asm.allowedBuild ? "block" : "none");

                            $('div#assemblys tr[assembly_sel="' + asm.Name + '"] td[app_role="update"] a').css("display",
                                asm.allowedUpdate ? "block" : "none");

                            if (asm.loadedRevision != asm.revisionTag)// loaded not actual assembly build
                                restartReq = true;
                        });
                        if (restartReq) {
                            $scope.triggers.wRestart = true;
                            $scope.$apply();
                        }
                        actx.ok();
                    },
                    function () { actx.ok(); }
                    );
            }
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
        function soft_copy(src, dst)
        {
            for (var x in src)
            {
                dst[x] = src[x];
            }
        }
        $scope.assembly_edit = function (asm) {
            if (!bbq_tmq.check_synced()) {
                alert('the state is not synced...');
                return;
            }
            $scope.ref_assembly = asm;

            $scope.newtask.parametersStr = angular.toJson(asm.BSParameters, true);
            //$scope.edit_assembly = angular.copy(asm);
            soft_copy(asm, $scope.edit_assembly);
            
            $('div#checkBSresults').text('');
            $('div#modal-edit-assembly').modal('show');
        }
        $scope.assembly_represent = function (bsName) {
            var reprobj = null;
            for (var i = 0; i < $scope.m_extra.BuildServerTypes.length; i++) {
                if ($scope.m_extra.BuildServerTypes[i].Name === bsName) {
                    reprobj = $scope.m_extra.BuildServerTypes[i].ParametersModel;
                    break;
                }
            }
            if (reprobj !== null)
                $scope.newtask.parametersStr = angular.toJson(reprobj, true);
        }
        $scope.assembly_edit_cpy = function () {
            if (!$scope.editAssemblyForm.$valid) { return; }
            var obj = null;
            try {
                obj = angular.fromJson($scope.newtask.parametersStr);
            } catch (e) {
                bbq_tmq.toastr_warning(" check json syntax! " + e.message);
                return;
            }

            angular.copy($scope.edit_assembly, $scope.ref_assembly);
            $scope.ref_assembly.BSParameters = obj;
            if (bbq_tmq.m_assemblys.Assemblys.indexOf($scope.ref_assembly) < 0)
                bbq_tmq.m_assemblys.Assemblys.push($scope.ref_assembly);
            bbq_tmq.assmPartChanged();
            $scope.triggers.wRestart = true;

            //animateChangeElement($("#t-tasks tr.animchange")[$scope.edit_task_index]);
            $('div#modal-edit-assembly').modal('hide');
        }
        $scope.newassembly = function () {
            //check sync state
            if (!bbq_tmq.check_synced()) {
                bbq_tmq.toastr_warning('the state is not synced...');
                return;
            }

            var asm = { Name: "", BuildServerType: $scope.m_extra.BuildServerTypes[0].Name, BSParameters: {} };

            $scope.assembly_edit(asm);
            $scope.assembly_represent(asm.BuildServerType);
        }
        $scope.assembly_checkParameters = function (bsName, params) {
            var obj = null;
            try {
                obj = angular.fromJson(params);
            } catch (e) {
                bbq_tmq.toastr_warning(" check json syntax! " + e.message);
                return;
            }
            $('div#checkBSresults').html('<i class="fas fa-sync-alt"></i>')
            bbq_tmq.assemblies.CheckBS(bsName, params, function (msg) {
                $('div#checkBSresults').text(msg);
            }, function (msg) {
                $('div#checkBSresults').text(msg);
            });
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
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromMods(function (d) {
                    $scope.m_mods = d;
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromAssemblys(function (d) {
                    $scope.m_assemblys = d;
                    actx.ok();
                }, function () { actx.ok(); })
            }, function (actx) {
                bbq_tmq.syncFromExtras(function (d) {
                    $scope.m_extra = d;
                    actx.ok();
                }, function () { actx.ok(); })
            }
            ).ondone(function () {
                $scope.triggers.Info = !bbq_tmq.check_synced();
                if (!$scope.triggers.Info) {

                    bbq_tmq.toastr_info(" Configuration synced ", true);

                    updateHeartbeat();
                    updateAssemblies();

                    $scope.$apply();

                    updateChannelToMTypeMap();
                }
                else {
                    bbq_tmq.toastr_error(" Error in configuration sync.", true);
                    bbq_tmq.rollbackAppC();

                    $scope.$apply();
                }

            });
        }
        function updateChannelToMTypeMap() {
            bbq_tmq.getChannelToMTypeMap(function (data) {
                $("div#servp td.chMtypeName").text("* not assigned");
                for (var chn in data) {
                    $("div#servp td.chMtypeName[channel='" + chn + "']").text(data[chn]);
                }

            }, function () {
                bbq_tmq.toastr_error(" Error in retrieve channel MType map.");
            });

        }

        $scope.sync = function () {
            var address = $("#txt-configurationhost").val();
            if (address.indexOf("http://") < 0 && address.indexOf("http://") < 0) {
                bbq_tmq.toastr_error("url string must start with protocol section");
            }
            else {
                if (address[address.length - 1] != '/') {
                    address = address + '/'; // fix malformed errors
                    bbq_tmq.toastr_warning("url used: " + address);
                }
                bbq_tmq.setHostAddress(address);

                resetTriggers();
                resetNewForms();
                ResyncAll();
            }
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
                bbq_tmq.toastr_warning('the state is not synced...');
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
                    if (data.ConfigCommitID) {
                        bbq_tmq.toastr_success(" main configuration upload id: " + data.ConfigCommitID);
                        actx.ok();
                    } else {
                        aactx.ok();// model don't need commit
                    }
                }, function (msg) { actx.error("main configuration upload error"); })
            },
            function (actx) {
                bbq_tmq.syncToMods(function (data) {
                    if (data.ConfigCommitID) {
                        bbq_tmq.toastr_success(" modules configuration upload id: " + data.ConfigCommitID);
                        actx.ok();
                    } else {
                        actx.ok();// model don't need commit
                    }
                }, function (msg) { actx.error("module configuration upload error"); })
            })
      .ondone(function () {
          bbq_tmq.CommitAndReset(function (data) {

              refModels();
              $scope.triggers.Info = true;
              $scope.triggers.wReset = false;
              tryKillHeartbeat();
              tryKillAssembliesUpdate();

              $scope.$apply();

              bbq_tmq.toastr_success(" configuration commit ok ", true);
          }, function (msg) { bbq_tmq.toastr_error(" Configuration commit error: " + msg); })
      }, function (msg) {
          bbq_tmq.toastr_error(" Configuration commit error: " + msg);
      });

        }
        $scope.commit_restart = function () {
            aftermath(// restart operation can be allowed by server by maintenance reason
                function (actx) {
                    bbq_tmq.syncToMain(function (data) {
                        if (data.ConfigCommitID) {
                            bbq_tmq.toastr_success(" main configuration upload id: " + data.ConfigCommitID);
                            actx.ok();
                        } else {
                            actx.ok();// model don't need commit
                        }
                    }, function (msg) { actx.error("main configuration upload error"); })
                },
                function (actx) {
                    bbq_tmq.syncToMods(function (data) {
                        if (data.ConfigCommitID) {
                            bbq_tmq.toastr_success(" module configuration upload id: " + data.ConfigCommitID);
                            actx.ok();
                        } else {
                            actx.ok();// model don't need commit
                        }
                    }, function (msg) { actx.error("module configuration upload error"); })
                },
                function (actx) {
                    bbq_tmq.syncToAssemblies(function (data) {
                        if (data.ConfigCommitID) {
                            bbq_tmq.toastr_success(" assembly configuration upload id: " + data.ConfigCommitID);
                            actx.ok();
                        } else {
                            actx.ok();// model don't need commit
                        }
                    }, function (msg) { actx.error("assembly configuration upload error"); })
                })
          .ondone(function () {
              bbq_tmq.CommitAndRestart(function (data) {

                  refModels();
                  $scope.triggers.Info = true;
                  //$scope.triggers.wReset = false;
                  $scope.triggers.wRestart = false;
                  tryKillHeartbeat();
                  tryKillAssembliesUpdate();

                  $scope.$apply();

                  bbq_tmq.toastr_success(" configuration commit/restart service pending ok ", true);
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

        function refModels() {
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
                //case 17: return "without";
                //case 18: return "ms";
                //case 19: return "sec";
                //case 20: return "daytime";
                //case 21: return "isolated";

                case "withoutInterval": return "without";
                case "intervalMilliseconds": return "ms";
                case "intervalSeconds": return "sec";
                case "DayTime": return "daytime";
                case "isolatedThread": return "isolated";
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