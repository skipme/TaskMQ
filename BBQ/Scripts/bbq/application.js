var bbq_tmq = {};
(function ($) {
    var url_host = "http://localhost:82/";
    var url_c = url_host + "tmq/c";
    var url_c_pxy = "/bbq/" + "PxySet";

    var main_cmodel = null;
    var mods_cmodel = null;

    var main_cmodel_id = null;

    var main_synced = false;
    var mods_synced = false;

    uuid = (function () {
        // Private array of chars to use
        var CHARS = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz'.split('');

        return function (len, radix) {
            var chars = CHARS, uuid = [], rnd = Math.random;
            radix = radix || chars.length;

            if (len) {
                // Compact form
                for (var i = 0; i < len; i++) uuid[i] = chars[0 | rnd() * radix];
            } else {
                // rfc4122, version 4 form
                var r;

                // rfc4122 requires these characters
                uuid[8] = uuid[13] = uuid[18] = uuid[23] = '-';
                uuid[14] = '4';

                // Fill in random data.  At i==19 set the high bits of clock sequence as
                // per rfc4122, sec. 4.1.5
                for (var i = 0; i < 36; i++) {
                    if (!uuid[i]) {
                        r = 0 | rnd() * 16;
                        uuid[i] = chars[(i == 19) ? (r & 0x3) | 0x8 : r & 0xf];
                    }
                }
            }

            return uuid.join('');
        };
    })();

    function resetModels() {
        main_cmodel = bbq_tmq.m_main = {
            "Connections": [
            ],
            "Channels": [
            ],
            "Tasks": [
            ]
        };
        mods_cmodel = bbq_tmq.m_mods = {
            "Modules": [
            ],
        };
        main_cmodel_id = uuid();
    }
    function url_cb_js(url) {
        return url + "?format=json&callback=?";
    }

    function getServiceModel(succ, err) {
        main_synced = false;
        jsonp(function (data) {
            main_synced = true;
            main_cmodel = bbq_tmq.m_main = data;
            succ(main_cmodel);
        }, err, { MainPart: true });
    }
    function getModsModel(succ, err) {
        mods_synced = false;
        jsonp(function (data) {
            mods_synced = true;
            mods_cmodel = bbq_tmq.m_mods = data;
            succ(mods_cmodel);
        }, err, { ModulesPart: true });
    }
    function jsonp(succ, err, data) {
        $.ajax({ url: url_cb_js(url_c), dataType: "json", data: data, timeout: 10000 })
            .done(function (data) {
                succ(data);
            }).fail(function () {
                //console.log("error at: " + url_c);
                bbq_tmq.toastr_error(" jsonp unavailable: " + url_c);
                if (err)
                    err();
            });
    }
    function json_proxy(succ, err, data) {
        $.ajax({ url: url_c_pxy, dataType: "json", data: data, timeout: 10000, type: 'POST' })
           .done(function (data) {
               var cresp = angular.fromJson(data);
               succ(cresp);
           }).fail(function () {
               //console.log("error at: " + url_c);
               bbq_tmq.toastr_error(" json proxy unavailable: " + url_c);
               if (err)
                   err();
           });
    }
    // obj {description, channel, module, parametersStr, intervalType, intervalValue}
    function createTask(obj) {
        var pobj = $.parseJSON(obj.parametersStr);
        var t = {
            Description: obj.description,
            ChannelName: obj.channel,
            ModuleName: obj.module,
            intervalType: obj.intervalType,
            intervalValue: obj.intervalValue,
            parameters: pobj
        };
        main_cmodel.Tasks.push(t);
        main_cmodel_id = uuid();
    }
    // ==============
    function setServiceModelAndReset(succ, err) {
        json_proxy(function (data) {
            if (data.Result == 'OK') {
                succ(data);
            } else {
                if (err)
                    err();
            }
        }, err, { data: angular.toJson({ MainPart: true, Body: angular.toJson(main_cmodel, false), ConfigId: main_cmodel_id }) });
    }
    // =========
    resetModels();

    function stub() { }
    bbq_tmq = {
        check_synced: function () { return main_synced && mods_synced; },
        m_main: main_cmodel,
        m_mods: mods_cmodel,
        rollbackAppC: resetModels, // reset models
        commitandReset: stub,
        commitandRestart: stub,
        syncFrom: getServiceModel,
        syncFromMods: getModsModel,

        createTask: createTask,
        syncToAndReset: setServiceModelAndReset
    };
    //
    jQuery.cachedScript = function (url, options) {
        // allow user to set any option except for dataType, cache, and url
        options = $.extend(options || {}, {
            dataType: "script",
            cache: true,
            url: url
        });
        // Use $.ajax() since it is more flexible than $.getScript
        // Return the jqXHR object so we can chain callbacks
        return jQuery.ajax(options);
    };
    // Usage
    $.cachedScript("Scripts/bbq/application-info.js").done(function (script, textStatus) {
        console.log(textStatus);
    });

})(jQuery);

