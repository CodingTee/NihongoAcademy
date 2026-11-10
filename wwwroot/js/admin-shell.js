/* ===== Admin workbench shell =====
   - Sidebar links & pane forms navigate in-pane (no full page loads).
   - Draggable divider between sidebar and content pane (width persisted).
   - Ctrl/Cmd+B collapses the sidebar to an icon rail (state persisted).
   Only active on Admin* pages (the shell markup is rendered by _Layout). */
(function () {
    'use strict';

    var shell = document.getElementById('naAdminShell');
    var sidebar = document.getElementById('naAdminSidebar');
    var pane = document.getElementById('naAdminPane');
    if (!shell || !sidebar || !pane) return;

    var W_KEY = 'na-admin-sidebar-w';
    var C_KEY = 'na-admin-sidebar-collapsed';
    var MIN_W = 180, MAX_W = 400, DEFAULT_W = 248;
    var root = document.documentElement;

    function store(k, v) { try { localStorage.setItem(k, v); } catch (e) {} }
    function read(k) { try { return localStorage.getItem(k); } catch (e) { return null; } }

    /* ---------- width ---------- */
    function applyWidth(w) { sidebar.style.width = w + 'px'; }
    function savedWidth() {
        var w = parseInt(read(W_KEY) || '', 10);
        return (w >= MIN_W && w <= MAX_W) ? w : DEFAULT_W;
    }
    if (!root.classList.contains('na-admin-collapsed')) applyWidth(savedWidth());

    /* ---------- collapse (icon rail) ---------- */
    function isCollapsed() { return root.classList.contains('na-admin-collapsed'); }
    function setCollapsed(v) {
        root.classList.toggle('na-admin-collapsed', v);
        store(C_KEY, v ? '1' : '0');
        if (v) {
            sidebar.style.width = '';
        } else {
            applyWidth(savedWidth());
        }
        var btn = document.getElementById('naAdminSidebarToggle');
        if (btn) {
            btn.setAttribute('aria-expanded', v ? 'false' : 'true');
            btn.title = v ? 'Expand sidebar (Ctrl+B)' : 'Collapse sidebar (Ctrl+B)';
            var ic = btn.querySelector('i');
            if (ic) ic.className = v ? 'bi bi-chevron-double-right' : 'bi bi-chevron-double-left';
        }
    }
    if (read(C_KEY) === '1') setCollapsed(true);

    var toggle = document.getElementById('naAdminSidebarToggle');
    if (toggle) toggle.addEventListener('click', function () { setCollapsed(!isCollapsed()); });

    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && (e.key === 'b' || e.key === 'B')) {
            e.preventDefault();
            setCollapsed(!isCollapsed());
        }
    });

    /* ---------- drag resizer ----------
       Pointer Events + setPointerCapture: the pointerup is always delivered to the
       resizer no matter where the user releases (even outside the window), so the
       "resizing" state can never get stuck and lock the content pane. */
    var resizer = document.getElementById('naAdminResizer');
    if (resizer) {
        var dragFinish = null;
        var startDrag = function (ev) {
            if (dragFinish) return; /* already dragging */
            if (isCollapsed()) setCollapsed(false);
            var startX = ev.clientX;
            var startW = sidebar.getBoundingClientRect().width;
            document.body.classList.add('na-admin-resizing');
            var move = function (cx) {
                var raw = startW + (cx - startX);
                /* pushed past the shortest usable width -> snap shut into the icon rail */
                if (raw < MIN_W - 20) { dragFinish(true); return; }
                applyWidth(Math.min(MAX_W, Math.max(MIN_W, raw)));
            };
            var onMove = function (e) { move(e.clientX); };
            var finish = function (snapCollapsed) {
                if (!dragFinish) return;
                dragFinish = null;
                resizer.removeEventListener('pointermove', onMove);
                resizer.removeEventListener('pointerup', onUp);
                resizer.removeEventListener('pointercancel', onCancel);
                window.removeEventListener('blur', onUp);
                document.body.classList.remove('na-admin-resizing');
                if (snapCollapsed) {
                    setCollapsed(true);
                } else if (!isCollapsed()) {
                    store(W_KEY, String(Math.round(sidebar.getBoundingClientRect().width)));
                }
            };
            var onUp = function () { finish(false); };
            var onCancel = function () { finish(false); };
            dragFinish = finish;
            resizer.addEventListener('pointermove', onMove);
            resizer.addEventListener('pointerup', onUp);
            resizer.addEventListener('pointercancel', onCancel);
            window.addEventListener('blur', onUp); /* Alt-Tab / focus loss mid-drag */
        };
        resizer.addEventListener('pointerdown', function (e) {
            if (e.pointerType === 'mouse' && e.button !== 0) return;
            e.preventDefault();
            try { resizer.setPointerCapture(e.pointerId); } catch (_) { }
            startDrag(e);
        });
        /* capture lost for any reason -> end the drag */
        resizer.addEventListener('lostpointercapture', function () {
            if (dragFinish) dragFinish(false);
        });
        /* safety net: if the class ever leaks without an active drag, clear it next frame */
        document.addEventListener('pointerup', function () {
            if (dragFinish) return;
            requestAnimationFrame(function () {
                document.body.classList.remove('na-admin-resizing');
            });
        });
    }

    /* ---------- in-pane navigation ---------- */
    var loading = false;
    var progress = document.getElementById('naAdminProgress');

    /* Page cache: a visited URL renders instantly on return (back/forward, re-click).
       Prefetch on hover/focus warms the cache before the user commits to a click. */
    var pageCache = new Map();
    var CACHE_MAX = 40;
    var prefetching = new Set();

    function cacheGet(url) { return pageCache.get(url); }
    function cachePut(url, html) {
        if (pageCache.size >= CACHE_MAX) pageCache.delete(pageCache.keys().next().value);
        pageCache.set(url, html);
    }

    function fetchPage(url) {
        return fetch(url, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { return r.ok ? r.text() : null; });
    }

    function prefetch(url) {
        if (!url || url === currentTarget() || pageCache.has(url) || prefetching.has(url)) return;
        if (url.indexOf(location.origin) === 0) url = url.slice(location.origin.length);
        prefetching.add(url);
        var run = function () {
            fetchPage(url).then(function (html) {
                prefetching.delete(url);
                if (html) cachePut(url, html);
            }).catch(function () { prefetching.delete(url); });
        };
        if (window.requestIdleCallback) requestIdleCallback(run, { timeout: 1500 });
        else setTimeout(run, 150);
    }

    shell.addEventListener('mouseover', function (e) {
        var a = e.target && e.target.closest ? e.target.closest('a[href]') : null;
        if (!a || !shell.contains(a)) return;
        var abs = new URL(a.href, location.href);
        if (abs.origin !== location.origin) return;
        prefetch(abs.pathname + abs.search);
    });
    shell.addEventListener('focusin', function (e) {
        var a = e.target && e.target.closest ? e.target.closest('a[href]') : null;
        if (!a || !shell.contains(a)) return;
        var abs = new URL(a.href, location.href);
        if (abs.origin !== location.origin) return;
        prefetch(abs.pathname + abs.search);
    });

    function setLoading(v) {
        loading = v;
        shell.classList.toggle('na-admin-loading', v);
        if (progress) progress.classList.toggle('active', v);
    }

    function currentTarget() { return location.pathname + location.search; }

    function markActive(target) {
        target = target || currentTarget();
        var qIdx = target.indexOf('?');
        var path = qIdx >= 0 ? target.slice(0, qIdx) : target;
        sidebar.querySelectorAll('a.nav-link').forEach(function (a) {
            var href = a.getAttribute('href') || '';
            // exact path+query match, or a plain-path link also matches the same path with any query
            var match = href === target || (href.indexOf('?') < 0 && href === path);
            if (match) a.classList.add('active');
            else a.classList.remove('active');
        });
    }

    /* re-execute inline scripts injected with innerHTML (they never run on their own) */
    function runScripts(container) {
        container.querySelectorAll('script').forEach(function (old) {
            var s = document.createElement('script');
            for (var i = 0; i < old.attributes.length; i++) {
                s.setAttribute(old.attributes[i].name, old.attributes[i].value);
            }
            s.textContent = old.textContent;
            old.parentNode.replaceChild(s, old);
        });
    }

    function reinitWidgets() {
        if (!window.bootstrap) return;
        pane.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
            try { bootstrap.Tooltip.getOrCreateInstance(el); } catch (e) {}
        });
        pane.querySelectorAll('[data-bs-toggle="popover"]').forEach(function (el) {
            try { bootstrap.Popover.getOrCreateInstance(el); } catch (e) {}
        });
    }

    /* Pull the .na-admin-pane element out of a full page response. */
    function extractPane(html) {
        var doc = new DOMParser().parseFromString(html, 'text/html');
        return doc.querySelector('.na-admin-pane');
    }

    function swapIn(newPane) {
        pane.innerHTML = newPane.innerHTML;
        runScripts(pane);
        reinitWidgets();
        markActive();
        window.scrollTo(0, 0);
        var t = newPane.ownerDocument.querySelector('title');
        if (t) document.title = t.textContent;
    }

    function loadIntoPane(url, push) {
        // cache hit -> render instantly, no request, no progress flash
        var cached = cacheGet(url);
        if (cached) {
            var cachedPane = extractPane(cached);
            if (cachedPane) {
                swapIn(cachedPane);
                markActive(url);
                if (push) history.pushState({ naAdmin: true }, '', url);
                return;
            }
        }
        if (loading) return;
        setLoading(true);
        fetchPage(url)
            .then(function (html) {
                setLoading(false);
                if (!html) throw new Error('HTTP error');
                cachePut(url, html);
                var newPane = extractPane(html);
                if (!newPane) { window.location.href = url; return; }   // non-admin target (e.g. logout) -> full nav
                swapIn(newPane);
                markActive(url);
                if (push) history.pushState({ naAdmin: true }, '', url);
            })
            .catch(function () {
                setLoading(false);
                window.location.href = url;
            });
    }

    function submitForm(form) {
        var action = form.getAttribute('action') || (location.pathname + location.search);
        var method = (form.getAttribute('method') || 'GET').toUpperCase();

        if (method === 'GET') {
            var params = new URLSearchParams();
            new FormData(form).forEach(function (v, k) { if (v) params.append(k, v); });
            var qs = params.toString();
            var sep = action.indexOf('?') >= 0 ? '&' : '?';
            loadIntoPane(qs ? action + sep + qs : action, true);
            return;
        }

        if (loading) return;
        setLoading(true);
        fetch(action, {
            method: method,
            body: new FormData(form),
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).then(function (r) {
            if (!r.ok) throw new Error('HTTP ' + r.status);
            return r.text().then(function (html) { return { html: html, url: r.url }; });
        }).then(function (res) {
            setLoading(false);
            var newPane = extractPane(res.html);
            if (!newPane) { window.location.href = res.url; return; }
            swapIn(newPane);
            markActive(new URL(res.url).pathname + new URL(res.url).search);
            if (res.url && res.url !== location.href) history.pushState({ naAdmin: true }, '', res.url);
        }).catch(function () {
            setLoading(false);
            window.location.href = action;
        });
    }

    /* Which links navigate in-pane: everything inside the workbench shell, plus the
       "Admin" pill and the Admin entry of the account dropdown in the top navbar
       (both live outside the shell but still target admin pages). */
    function isAdminNavLink(a) {
        if (shell.contains(a)) return true;
        if (a.classList.contains('na-admin-pill')) return true;
        if (a.classList.contains('dropdown-item')) {
            var h = a.getAttribute('href') || '';
            return h === '/Admin' || h.indexOf('/Admin') === 0;
        }
        return false;
    }

    /* links inside the shell (sidebar + pane) stay in-pane */
    document.addEventListener('click', function (e) {
        if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        var a = e.target && e.target.closest ? e.target.closest('a[href]') : null;
        if (!a || !isAdminNavLink(a)) return;
        if (a.target === '_blank' || a.hasAttribute('download') || a.hasAttribute('data-na-fullnav')) return;
        var href = a.getAttribute('href') || '';
        if (!href || href.charAt(0) === '#' || /^(mailto:|tel:|javascript:)/i.test(href)) return;
        var abs = new URL(a.href, location.href);
        if (abs.origin !== location.origin) return;
        var target = abs.pathname + abs.search;
        if (target === currentTarget()) { e.preventDefault(); return; }   // already here: no pointless full reload
        e.preventDefault();
        loadIntoPane(target, true);
    });

    /* forms inside the pane stay in-pane */
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!pane.contains(form) || form.hasAttribute('data-na-fullnav')) return;
        e.preventDefault();
        submitForm(form);
    });

    /* Inline onchange="this.form.submit()" calls the NATIVE method, which fires no
       submit event — route it through a cancelable event so the pane can intercept. */
    var nativeSubmit = HTMLFormElement.prototype.submit;
    HTMLFormElement.prototype.submit = function () {
        if (pane.contains(this) && !this.hasAttribute('data-na-fullnav')) {
            var ev = new Event('submit', { bubbles: true, cancelable: true });
            if (!this.dispatchEvent(ev)) return;   // intercepted -> handled in-pane
        }
        return nativeSubmit.call(this);
    };

    /* browser back / forward */
    window.addEventListener('popstate', function () {
        loadIntoPane(currentTarget(), false);
    });

    markActive();
})();
