// SmartSociety — global UI system: theme toggle, toast notifications, animations.
// Frontend-only. Does not touch any controller/TempData/model logic.

(function () {
    "use strict";

    var reducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* ------------------------------------------------------------------ *
     * Theme (light/dark)
     * ------------------------------------------------------------------ */
    var THEME_KEY = 'ss-theme';

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        document.querySelectorAll('.theme-toggle').forEach(function (btn) {
            btn.setAttribute('aria-label', theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode');
        });
    }

    function initTheme() {
        var saved = localStorage.getItem(THEME_KEY);
        var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        applyTheme(saved || (prefersDark ? 'dark' : 'light'));

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.theme-toggle');
            if (!btn) return;
            var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            localStorage.setItem(THEME_KEY, next);
        });
    }

    /* ------------------------------------------------------------------ *
     * Toast notifications
     * ------------------------------------------------------------------ */
    var toastStack;

    function getToastStack() {
        if (!toastStack) {
            toastStack = document.querySelector('.toast-stack');
            if (!toastStack) {
                toastStack = document.createElement('div');
                toastStack.className = 'toast-stack';
                toastStack.setAttribute('role', 'status');
                toastStack.setAttribute('aria-live', 'polite');
                document.body.appendChild(toastStack);
            }
        }
        return toastStack;
    }

    var TOAST_ICONS = {
        success: 'bi-check-circle-fill',
        error: 'bi-x-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        info: 'bi-info-circle-fill'
    };
    var TOAST_TITLES = {
        success: 'Success',
        error: 'Something went wrong',
        warning: 'Heads up',
        info: 'Notice'
    };

    function showToast(tone, message, duration) {
        if (!message) return;
        tone = TOAST_ICONS[tone] ? tone : 'info';
        duration = duration || 5000;

        var stack = getToastStack();
        var item = document.createElement('div');
        item.className = 'toast-item tone-' + tone;
        item.innerHTML =
            '<span class="toast-icon"><i class="bi ' + TOAST_ICONS[tone] + '"></i></span>' +
            '<span class="toast-body">' +
                '<span class="toast-title">' + TOAST_TITLES[tone] + '</span>' +
                '<span class="toast-msg"></span>' +
            '</span>' +
            '<button type="button" class="toast-close" aria-label="Dismiss"><i class="bi bi-x-lg"></i></button>' +
            (reducedMotion ? '' : '<span class="toast-progress" style="animation-duration:' + duration + 'ms"></span>');
        item.querySelector('.toast-msg').textContent = message;

        stack.appendChild(item);

        var timer = setTimeout(function () { dismiss(); }, duration);

        function dismiss() {
            clearTimeout(timer);
            item.classList.add('leaving');
            setTimeout(function () { item.remove(); }, reducedMotion ? 0 : 260);
        }

        item.querySelector('.toast-close').addEventListener('click', dismiss);
    }

    // Auto-convert any server-rendered TempData alert boxes into toasts,
    // then hide the original inline alert so it isn't shown twice.
    function initAutoToasts() {
        var map = { 'alert-success': 'success', 'alert-danger': 'error', 'alert-warning': 'warning', 'alert-info': 'info' };
        document.querySelectorAll('.alert').forEach(function (el) {
            if (el.classList.contains('js-toasted') || el.closest('.toast-stack')) return;
            var tone = null;
            for (var cls in map) { if (el.classList.contains(cls)) { tone = map[cls]; break; } }
            if (!tone) return;
            var text = el.textContent.trim();
            if (!text) return;
            showToast(tone, text);
            el.classList.add('js-toasted');
        });
    }

    /* ------------------------------------------------------------------ *
     * Number counter animation for stat/KPI cards
     * ------------------------------------------------------------------ */
    function initCounters() {
        if (reducedMotion) return;
        document.querySelectorAll('.stat-card strong, [data-counter]').forEach(function (el) {
            var raw = el.textContent.trim();
            var match = raw.match(/^(\D*)(\d[\d,]*)(\D*)$/);
            if (!match) return;
            var prefix = match[1], target = parseInt(match[2].replace(/,/g, ''), 10), suffix = match[3];
            if (isNaN(target) || target <= 0) return;

            var duration = 900, start = null;
            function step(ts) {
                if (!start) start = ts;
                var progress = Math.min((ts - start) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3);
                var value = Math.floor(eased * target);
                el.textContent = prefix + value.toLocaleString() + suffix;
                if (progress < 1) requestAnimationFrame(step);
                else el.textContent = prefix + target.toLocaleString() + suffix;
            }
            requestAnimationFrame(step);
        });
    }

    /* ------------------------------------------------------------------ *
     * Scroll-reveal for landing-page sections
     * ------------------------------------------------------------------ */
    function initScrollReveal() {
        var targets = document.querySelectorAll('.scroll-reveal');
        if (!targets.length) return;
        if (reducedMotion || !('IntersectionObserver' in window)) {
            targets.forEach(function (el) { el.classList.add('is-visible'); });
            return;
        }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });
        targets.forEach(function (el) { observer.observe(el); });
    }

    /* ------------------------------------------------------------------ *
     * Sidebar (mobile) — kept here alongside the rest of the UI logic
     * ------------------------------------------------------------------ */
    function initSidebar() {
        var sidebar = document.getElementById('appSidebar');
        var overlay = document.getElementById('sidebarOverlay');
        var toggle = document.getElementById('sidebarToggle');
        function closeSidebar() {
            sidebar && sidebar.classList.remove('open');
            overlay && overlay.classList.remove('show');
        }
        toggle && toggle.addEventListener('click', function () {
            sidebar && sidebar.classList.toggle('open');
            overlay && overlay.classList.toggle('show');
        });
        overlay && overlay.addEventListener('click', closeSidebar);
    }

    document.addEventListener('DOMContentLoaded', function () {
        initTheme();
        initSidebar();
        initAutoToasts();
        initCounters();
        initScrollReveal();
    });

    // Expose for pages that want to trigger a toast from their own script block.
    window.ssToast = showToast;
})();
