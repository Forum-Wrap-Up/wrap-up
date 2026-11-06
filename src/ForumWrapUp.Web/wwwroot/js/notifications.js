const initialized = new WeakSet();

export function initializeNotifications(root = document) {
    const notifications = root.matches?.("[data-notification]")
        ? [root]
        : root.querySelectorAll("[data-notification]");

    for (const notification of notifications) {
        if (initialized.has(notification)) continue;
        initialized.add(notification);

        let region = document.querySelector(".notification-region");
        if (!region) {
            region = document.createElement("div");
            region.className = "notification-region";
            region.setAttribute("role", "region");
            region.setAttribute("aria-label", "Notifiche");
            document.body.append(region);
        }
        region.append(notification);

        const close = notification.querySelector("[data-notification-close]");
        close.hidden = false;
        const duration = Number(notification.dataset.dismissAfter);
        const autoDismiss = Number.isFinite(duration) && duration > 0;
        const listeners = new AbortController();
        let remaining = duration;
        let startedAt = 0;
        let timer = null;
        let dismissed = false;

        function pause() {
            if (timer === null) return;
            window.clearTimeout(timer);
            remaining = Math.max(0, remaining - (performance.now() - startedAt));
            timer = null;
        }

        function dismiss() {
            if (dismissed) return;
            dismissed = true;
            pause();
            listeners.abort();

            if (notification.contains(document.activeElement)) {
                document.querySelector("#contenuto")?.focus({ preventScroll: true });
            }

            const remove = () => {
                notification.remove();
                if (!region.childElementCount) region.remove();
            };

            if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
                remove();
                return;
            }

            notification.setAttribute("aria-hidden", "true");
            notification.classList.add("notification--leaving");
            window.setTimeout(remove, 180);
        }

        function resume() {
            if (dismissed || !autoDismiss || timer !== null || document.hidden
                || notification.matches(":hover, :focus-within")) return;
            if (remaining <= 0) {
                dismiss();
                return;
            }
            startedAt = performance.now();
            timer = window.setTimeout(dismiss, remaining);
        }

        const options = { signal: listeners.signal };
        close.addEventListener("click", dismiss, options);
        notification.addEventListener("pointerenter", pause, options);
        notification.addEventListener("pointerleave", resume, options);
        notification.addEventListener("focusin", pause, options);
        notification.addEventListener("focusout", () => window.setTimeout(resume, 0), options);
        document.addEventListener("visibilitychange", () => {
            if (document.hidden) pause();
            else resume();
        }, options);
        resume();
    }
}

initializeNotifications();
