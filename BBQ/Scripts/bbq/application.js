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
        //$.ajax({
        //    url: url_cb_js(url_c),
        //    contentType: "",
        //    data: null,
        //    type: 'GET',
        //    success: function (d) {
        //        main_cmodel = bbq_tmq.m_main = $.parseJSON(d);
        //        succ(bbq_tmq.m_main);
        //    },
        //    error: err
        //});
        $.getJSON(url_cb_js(url_c), {GetMain: true }, function (data) {
            main_cmodel = bbq_tmq.m_main = data;
            succ(main_cmodel);
        });
    }
    function getModsModel(succ, err) {
        $.getJSON(url_cb_js(url_c), {GetModules: true}, function (data) {
            mods_cmodel = bbq_tmq.m_mods = data;
            succ(mods_cmodel);
        });
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
        syncFromMods: getModsModel
    };
})(jQuery);