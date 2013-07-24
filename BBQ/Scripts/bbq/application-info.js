(function ($) {
    var app = "BBQ";
    // configure toastr
    toastr.options = {
        debug: false,
        positionClass: 'toast-bottom-right'
    };
    //
    // functions
    function toastr_await(clear, foo) {
        if (clear) {
            toastr.clear(foo);
        }
        else {
            foo();
        }
    }
    bbq_tmq.toastr_error = function (message, clear, title) {
        function show() {
            toastr.options.timeOut = 19000;
            toastr.error(message, ((typeof title === 'undefined') ? app : title));
        }
        toastr_await(clear, show);
    }
    bbq_tmq.toastr_warning = function (message, clear, title) {
        function show() {
            toastr.options.timeOut = 8000;
            toastr.warning(message, ((typeof title === 'undefined') ? app : title));
        }
        toastr_await(clear, show);
    }
    bbq_tmq.toastr_info = function (message, clear, title) {
        function show() {
            toastr.options.timeOut = 8000;
            toastr.info(message, ((typeof title === 'undefined') ? app : title));
        }
        toastr_await(clear, show);
    }
    bbq_tmq.toastr_success = function (message, clear, title) {
        function show() {
            toastr.options.timeOut = 8000;
            toastr.success(message, ((typeof title === 'undefined') ? app : title));
        }
        toastr_await(clear, show);
    }
    
})(jQuery);