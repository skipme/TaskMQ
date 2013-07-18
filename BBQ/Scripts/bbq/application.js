var bbq_tmq = {};
(function ($) {
    var url_host = "http://localhost:82/";
    var url_c = url_host + "tmq/c";

    var main_cmodel = null;
    var mods_cmodel = null;

    function resetModels()
    {
        main_cmodel = bbq_tmq.m_main ={
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
    }
    function url_cb_js(url)
    {
        return url + "?format=json&callback=?";
    }

    function getServiceModel(succ, err)
    {
        jsonp(function (data) {
            main_cmodel = bbq_tmq.m_main = data;
            succ(main_cmodel);
        }, err, { GetMain: true });
    }
    function getModsModel(succ, err)
    {
        jsonp(function (data) {
            mods_cmodel = bbq_tmq.m_mods = data;
            succ(mods_cmodel);
        }, err, { GetModules: true });
    }
    function jsonp(succ, err, data)
    {
        $.ajax({ url: url_cb_js(url_c), dataType: "jsonp", data: data, timeout: 10000 })
            .done(function (data) {
                mods_cmodel = bbq_tmq.m_mods = data;
                succ(mods_cmodel);
            }).fail(function () {
                console.log("error at: " + url_c);
                if (err)
                    err();
            });
    }
    function createTask(description, channel, module, parametersStr, intervalType, intervalValue)
    {
        var pobj = $.parseJSON(parametersStr);
        var t = {
            Description: description,
            ChannelName: channel,
            ModuleName: module,
            intervalType: intervalType,
            intervalValue: intervalValue,
            parameters: pobj
        };
        main_cmodel.Tasks.push(t);
    }
    resetModels();

    function stub(){}
    bbq_tmq = {
        m_main: main_cmodel,
        m_mods: mods_cmodel,
        rollbackAppC: resetModels, // reset models
        commitandReset: stub,
        commitandRestart: stub,
        syncFrom: getServiceModel,
        syncFromMods: getModsModel,

        createTask: createTask
    };
})(jQuery);