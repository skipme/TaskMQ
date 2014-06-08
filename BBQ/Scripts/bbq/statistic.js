(function ($) {
    //bbq_tmq.url_stat
    bbq_tmq.heartbeat = function (succ, err) {
        bbq_tmq.jsonp(function (data) {// success
            succ(data);
        }, err, { GetHeartbit: true }, bbq_tmq.url_stat);
    }

    bbq_tmq.throughput = function (succ, err) {
        bbq_tmq.jsonp(function (data) {// success
            succ(data);
        }, err, { GetThroughput: true }, bbq_tmq.url_stat);
    }
})(jQuery);