(function ($) {
    //bbq_tmq.url_stat
    bbq_tmq.heartbeat = function (succ, err) {
        bbq_tmq.jsonp(bbq_tmq.url_stat, function (data) {// success
            succ(data);
        }, err, { GetHeartbit: true });
    }

    bbq_tmq.throughput = function (succ, err) {
        bbq_tmq.jsonp(bbq_tmq.url_stat, function (data) {// success
            succ(data);
        }, err, { GetThroughput: true });
    }
})(jQuery);