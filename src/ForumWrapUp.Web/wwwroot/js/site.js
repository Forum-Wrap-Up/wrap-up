(() => {
    const dialog = document.querySelector("#login-dialog");
    const trigger = document.querySelector("[data-login-trigger]");
    const form = document.querySelector("[data-login-form]");

    if (!form) return;

    const password = form.querySelector("#Password");
    const toggle = form.querySelector("[data-password-toggle]");
    const errors = form.querySelector("[data-login-errors]");

    toggle.hidden = false;
    toggle.addEventListener("click", () => {
        const reveal = password.type === "password";
        password.type = reveal ? "text" : "password";
        toggle.textContent = reveal ? "Nascondi" : "Mostra";
        toggle.setAttribute("aria-label", reveal ? "Nascondi la password" : "Mostra la password");
        toggle.setAttribute("aria-pressed", String(reveal));
    });

    if (!dialog || !trigger || typeof dialog.showModal !== "function") return;

    trigger.setAttribute("aria-haspopup", "dialog");
    trigger.setAttribute("aria-controls", "login-dialog");
    trigger.addEventListener("click", (event) => {
        if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || event.button !== 0) return;
        event.preventDefault();
        form.querySelector("[name=ReturnUrl]").value = window.location.pathname + window.location.search;
        dialog.showModal();
        document.documentElement.classList.add("login-is-open");
        form.querySelector("#Mid").focus();
    });

    dialog.querySelector("[data-login-close]").addEventListener("click", () => dialog.close());
    dialog.addEventListener("close", () => {
        document.documentElement.classList.remove("login-is-open");
        password.type = "password";
        password.value = "";
        toggle.textContent = "Mostra";
        toggle.setAttribute("aria-label", "Mostra la password");
        toggle.setAttribute("aria-pressed", "false");
        trigger.focus();
    });

    const submit = form.querySelector("[type=submit]");
    let pending = false;
    const timeoutMs = Number(form.dataset.loginTimeoutMs) || 20000;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (pending) return;

        pending = true;
        submit.disabled = true;
        submit.textContent = "Accesso in corso…";
        form.setAttribute("aria-busy", "true");
        errors.replaceChildren();
        errors.classList.remove("validation-summary-errors");
        errors.classList.add("validation-summary-valid");
        form.querySelectorAll("[data-valmsg-for]").forEach((message) => { message.textContent = ""; });
        form.querySelectorAll("[aria-invalid]").forEach((input) => input.removeAttribute("aria-invalid"));

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                credentials: "same-origin",
                headers: { "X-Login-Dialog": "true", "Accept": "application/json" },
                signal: AbortSignal.timeout(timeoutMs)
            });

            if (!response.headers.get("content-type")?.includes("application/json")) {
                throw new Error(response.status === 400
                    ? "La sessione della pagina è scaduta. Ricarica la pagina e riprova."
                    : "Non riusciamo a completare l'accesso. Riprova tra poco.");
            }

            const result = await response.json();

            if (response.ok && result.redirectUrl) {
                window.location.assign(result.redirectUrl);
                return;
            }

            const messages = Object.values(result.errors ?? {}).flat();
            errors.textContent = messages.join(" ") || "Non riusciamo a completare l'accesso. Riprova tra poco.";

            for (const [field, messages] of Object.entries(result.errors ?? {})) {
                const input = form.elements.namedItem(field);
                const message = [...form.querySelectorAll("[data-valmsg-for]")]
                    .find((element) => element.dataset.valmsgFor === field);
                if (input instanceof HTMLInputElement) input.setAttribute("aria-invalid", "true");
                if (message) message.textContent = messages.join(" ");
            }

            if (response.status === 401) password.value = "";
        } catch (error) {
            errors.textContent = error instanceof Error && error.name === "Error"
                ? error.message
                : "Connessione interrotta. Controlla la rete e riprova.";
        } finally {
            pending = false;
            submit.disabled = false;
            submit.textContent = "Accedi";
            form.removeAttribute("aria-busy");
        }

        errors.classList.remove("validation-summary-valid");
        errors.classList.add("validation-summary-errors");
        errors.focus();
    });
})();
