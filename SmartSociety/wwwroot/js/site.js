// ==========================================================================
// SmartSociety — Global UI behaviors (theme, toasts, reveal, counters)
// UI-only. Does not touch any form submission / validation / routing.
// ==========================================================================
(function () {
    "use strict";


    /* ------------------------------------------------------------------ */
    /* 2) TOAST NOTIFICATIONS — auto-converts existing Bootstrap .alert   */
    /*    blocks (rendered from TempData) into premium toasts.            */
    /*    No Razor view or controller changes required.                   */
    /* ------------------------------------------------------------------ */
    function ensureToastStack() {
        var stack = document.querySelector(".toast-stack");
        if (!stack) {
            stack = document.createElement("div");
            stack.className = "toast-stack";
            document.body.appendChild(stack);
        }
        return stack;
    }

    var TOAST_ICONS = {
        success: "bi-check-circle-fill",
        danger: "bi-x-circle-fill",
        warning: "bi-exclamation-triangle-fill",
        info: "bi-info-circle-fill"
    };

    function showToast(message, type) {
        type = type || "info";
        var stack = ensureToastStack();
        var toast = document.createElement("div");
        toast.className = "app-toast toast-" + type;
        toast.innerHTML =
            '<span class="toast-icon"><i class="bi ' + (TOAST_ICONS[type] || TOAST_ICONS.info) + '"></i></span>' +
            '<span class="toast-body"></span>' +
            '<button type="button" class="toast-close" aria-label="Close"><i class="bi bi-x-lg"></i></button>';
        toast.querySelector(".toast-body").textContent = message;
        stack.appendChild(toast);

        function remove() {
            toast.classList.add("hide");
            setTimeout(function () { toast.remove(); }, 260);
        }
        toast.querySelector(".toast-close").addEventListener("click", remove);
        setTimeout(remove, 5000);
    }
    window.SmartSocietyToast = showToast;

    // Map existing Bootstrap alert classes -> toast type, then hide the
    // original inline alert so the message isn't shown twice.
    var ALERT_TYPE_MAP = {
        "alert-success": "success",
        "alert-danger": "danger",
        "alert-warning": "warning",
        "alert-info": "info"
    };

    function migrateInlineAlertsToToasts() {
        var alerts = document.querySelectorAll(".page-content .alert, .guest-content .alert");
        alerts.forEach(function (alertEl) {
            var type = "info";
            for (var cls in ALERT_TYPE_MAP) {
                if (alertEl.classList.contains(cls)) { type = ALERT_TYPE_MAP[cls]; break; }
            }
            var text = alertEl.textContent.trim();
            if (text) {
                showToast(text, type);
            }
            alertEl.style.display = "none";
        });
    }

    /* ------------------------------------------------------------------ */
    /* 3) SCROLL REVEAL — elements with class="reveal"                    */
    /* ------------------------------------------------------------------ */
    function initScrollReveal() {
        var items = document.querySelectorAll(".reveal");
        if (!items.length) return;

        if (!("IntersectionObserver" in window)) {
            items.forEach(function (el) { el.classList.add("in"); });
            return;
        }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("in");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });
        items.forEach(function (el) { observer.observe(el); });
    }

    /* ------------------------------------------------------------------ */
    /* 4) NUMBER COUNTER ANIMATION — any element with [data-counter]      */
    /*    e.g. <strong data-counter="128">128</strong>                    */
    /* ------------------------------------------------------------------ */
    function animateCounter(el) {
        var target = parseFloat(el.getAttribute("data-counter"));
        if (isNaN(target)) return;
        var duration = 900;
        var start = 0;
        var startTime = null;

        function step(ts) {
            if (!startTime) startTime = ts;
            var progress = Math.min((ts - startTime) / duration, 1);
            var eased = 1 - Math.pow(1 - progress, 3);
            var value = Math.round(start + (target - start) * eased);
            el.textContent = value.toLocaleString();
            if (progress < 1) requestAnimationFrame(step);
            else el.textContent = target.toLocaleString();
        }
        requestAnimationFrame(step);
    }

    function initCounters() {
        var counters = document.querySelectorAll("[data-counter]");
        if (!counters.length) return;

        if (!("IntersectionObserver" in window)) {
            counters.forEach(animateCounter);
            return;
        }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animateCounter(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.4 });
        counters.forEach(function (el) { observer.observe(el); });
    }

    /* ------------------------------------------------------------------ */
    /* 5) PASSWORD SHOW/HIDE TOGGLE — .password-toggle buttons            */
    /*    (auth pages use .password-wrap > .form-control + button)        */
    /* ------------------------------------------------------------------ */
    function initPasswordToggles() {
        document.querySelectorAll(".password-toggle").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var wrap = btn.closest(".password-wrap");
                if (!wrap) return;
                var input = wrap.querySelector("input");
                if (!input) return;
                var isHidden = input.type === "password";
                input.type = isHidden ? "text" : "password";
                var icon = btn.querySelector("i");
                if (icon) {
                    icon.className = isHidden ? "bi bi-eye-slash" : "bi bi-eye";
                }
            });
        });
    }

    /* ------------------------------------------------------------------ */
    /* Boot                                                                */
    /* ------------------------------------------------------------------ */
    document.addEventListener("DOMContentLoaded", function () {
        migrateInlineAlertsToToasts();
        initScrollReveal();
        initCounters();
        initPasswordToggles();
    });
})();z