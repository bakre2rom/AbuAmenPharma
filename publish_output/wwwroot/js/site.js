// Light double-click protection for buttons.
(function () {
    const CLICK_GUARD_MS = 900;

    function now() {
        return Date.now();
    }

    function isLocked(button) {
        const lockUntil = parseInt(button.dataset.clickGuardUntil || "0", 10);
        return now() < lockUntil;
    }

    function lock(button, durationMs) {
        button.dataset.clickGuardUntil = String(now() + durationMs);
    }

    document.addEventListener("click", function (event) {
        const button = event.target.closest("button, input[type='submit']");
        if (!button) return;
        if (button.dataset.noClickGuard === "true") return;

        if (isLocked(button)) {
            event.preventDefault();
            event.stopPropagation();
            return;
        }

        lock(button, CLICK_GUARD_MS);
    }, true);
})();
