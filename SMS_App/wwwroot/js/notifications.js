// In-app notification centre (header bell).
// Progressive enhancement: everything degrades to the full /Notifications page if JS
// or the SignalR connection is unavailable. Config is supplied on window.EIMS_NOTIF by
// the layout; if the bell markup is absent (non-admin layouts) this script does nothing.
(function () {
    "use strict";

    var cfg = window.EIMS_NOTIF;
    if (!cfg) { return; }

    var badge = document.getElementById("notif-badge");
    var countLabel = document.getElementById("notif-count-label");
    var list = document.getElementById("notif-list");
    if (!badge || !list) { return; }

    var EMPTY_HTML =
        '<li class="text-center text-muted p-3" data-notif-empty="1"><p class="mb-0">No new notifications</p></li>';

    function setCount(n) {
        n = n || 0;
        badge.textContent = " " + n + " ";
        if (countLabel) { countLabel.textContent = n; }
        badge.style.display = n > 0 ? "" : "none";
    }

    function escapeHtml(s) {
        return String(s == null ? "" : s)
            .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
    }

    function itemHtml(n) {
        var href = cfg.openUrlBase + encodeURIComponent(n.id);
        var unreadClass = n.isRead ? "" : " fw-bold";
        return '<li data-notif-id="' + n.id + '">' +
            '<a href="' + href + '" class="d-block px-3 py-2' + unreadClass + '">' +
            '<span class="d-block">' + escapeHtml(n.title) + '</span>' +
            '<small class="text-muted d-block">' + escapeHtml(n.message) + '</small>' +
            '<small class="text-muted">' + escapeHtml(n.createdAtText) + '</small>' +
            '</a></li>';
    }

    function renderList(items) {
        if (!items || items.length === 0) {
            list.innerHTML = EMPTY_HTML;
            return;
        }
        list.innerHTML = items.map(itemHtml).join("");
    }

    function prepend(n) {
        var empty = list.querySelector("[data-notif-empty]");
        if (empty) { empty.remove(); }
        list.insertAdjacentHTML("afterbegin", itemHtml(n));
    }

    function loadRecent() {
        fetch(cfg.recentUrl, { headers: { "X-Requested-With": "XMLHttpRequest" }, credentials: "same-origin" })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (data) {
                if (!data) { return; }
                setCount(data.unreadCount);
                renderList(data.items);
            })
            .catch(function () { /* leave the server-rendered badge as-is */ });
    }

    function markAllRead() {
        var body = new URLSearchParams();
        if (cfg.token) { body.append("__RequestVerificationToken", cfg.token); }
        fetch(cfg.markAllUrl, {
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest", "Content-Type": "application/x-www-form-urlencoded" },
            body: body.toString(),
            credentials: "same-origin"
        }).then(function () {
            setCount(0);
            list.querySelectorAll("a.fw-bold").forEach(function (a) { a.classList.remove("fw-bold"); });
        }).catch(function () { /* ignore - page reload will reconcile */ });
    }

    // Wire the "mark all read" control if the layout rendered one.
    var markAllBtn = document.getElementById("notif-mark-all");
    if (markAllBtn) {
        markAllBtn.addEventListener("click", function (e) { e.preventDefault(); markAllRead(); });
    }

    // Initial population from the server.
    loadRecent();

    // Real-time: connect to the hub if the SignalR client loaded.
    if (window.signalR) {
        var connection = new signalR.HubConnectionBuilder()
            .withUrl(cfg.hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on("receiveNotification", function (n) {
            if (typeof n.unreadCount === "number") {
                setCount(n.unreadCount);
            } else {
                setCount((parseInt(badge.textContent, 10) || 0) + 1);
            }
            prepend(n);
        });

        connection.start().catch(function () {
            // Hub unreachable (e.g. websockets blocked): the badge still updates on
            // navigation via loadRecent(), so this is a soft failure.
        });
    }
})();
