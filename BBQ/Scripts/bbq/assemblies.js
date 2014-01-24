(function ($) {
    bbq_tmq.assemblies = {};

    bbq_tmq.assemblies.takeAdvanceInfo = function (succ, err) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
            succ(data);
        }, err, { Statuses: true });
    }

    bbq_tmq.assemblies.fetchAssemblySource = function (name) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
        }, function () { }, { Fetch: true, Name: name });
    }
    bbq_tmq.assemblies.buildAssemblySource = function (name) {
        bbq_tmq.jsonp(bbq_tmq.url_assemblies, function (data) {// success
        }, function () { }, { Build: true, Name: name });
    }
    bbq_tmq.assemblies.updateAllAssemblyPackages = null;
    bbq_tmq.assemblies.uploadAssemplyPackage = null;

})(jQuery);