(function () {
    function navigateTo(url) {
        if (!url) return;
        window.location.href = url;
    }

    function initInvoicePrint() {
        var body = document.body;
        if (!body) return;

        var defaultCloseUrl = body.getAttribute("data-close-url") || "";
        var printBtn = document.querySelector(".js-print-invoice");
        var closeBtn = document.querySelector(".js-close-invoice");
        var closeUrl = (closeBtn && closeBtn.getAttribute("data-close-url")) || defaultCloseUrl;

        if (printBtn) {
            printBtn.addEventListener("click", function () {
                window.print();
            });
        }

        if (closeBtn) {
            closeBtn.addEventListener("click", function () {
                navigateTo(closeUrl);
            });
        }

        window.onafterprint = function () {
            navigateTo(closeUrl);
        };
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initInvoicePrint);
    } else {
        initInvoicePrint();
    }
})();
