(function ($) {
    bbq_tmq.assemblies = {};

    bbq_tmq.assemblies.takeAdvanceInfo = function (succ, err) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
            succ(data);
        }, err, { Statuses: true });
    }

    bbq_tmq.assemblies.fetchAssemblySource = function (name) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
            bbq_tmq.toastr_info("successfully enqueued Source Fetch for " + name);
        }, function () { }, { Fetch: true, Name: name });
    }
    bbq_tmq.assemblies.buildAssemblySource = function (name) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
            bbq_tmq.toastr_info("successfully enqueued Source Building for " + name);
        }, function () { }, { Build: true, Name: name });
    }
    bbq_tmq.assemblies.UpdatePackage = function (name) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
            bbq_tmq.toastr_info("successfully enqueued Update Package for " + name);
        }, function () { }, { Package: true, Name: name });
    }
    bbq_tmq.assemblies.CheckBS = function (bsname, bsparams, resp) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
        //bbq_tmq.json_proxy(function (data) {
            if (data.CheckResult) {
                bbq_tmq.toastr_info(" successfully checked: " + bsname);
                resp(" successfully checked");
            }
            else {
                bbq_tmq.toastr_error(" error occured for: " + bsname + " remark: " + data.Remark);
                resp(data.Remark);
            }
        }, function () {

            }, { CheckBS: true, BSName: bsname, BSParameters: bsparams });
        //}, { data: { CheckBS: true, BSName: bsname, BSParameters: bsparams }, urlprefix: "tmq/assemblies" });
    }
    bbq_tmq.assemblies.updateAllAssemblyPackages = null;
    bbq_tmq.assemblies.uploadAssemplyPackage = null;

})(jQuery);