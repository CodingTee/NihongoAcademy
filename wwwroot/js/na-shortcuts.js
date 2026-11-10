/* ==========================================================================
   na-shortcuts.js — the site-wide keyboard layer

   One document listener, one registry. Any script can teach the page a new
   key by calling

       naKeys.register({ keys:['s'], group:'Photos', label:'…', run: fn, when: fn });

   and the on-screen cheat sheet (press ?) is generated from that registry,
   so the sheet can never drift away from what the page actually answers to.

   Two-key chords: register({ seq:['g','h'], … }) and the engine waits for the
   second key, showing a hint of the options. Page scripts register later but
   still win over the generic site-wide layer, because `priority` sorts the
   list (page handlers default to 0, the generic layer sits below it).

   Rules the engine obeys:
     · nothing fires while you are typing in a field
     · nothing fires while an overlay owns the screen, unless the handler
       asks for it with always: true (the photo viewer needs its own arrows)
     · "when" returning false simply means "not my key" — the engine keeps
       looking down the list, so several components can claim the same key in
       different states (← moves a photo on the stage but pages the viewer)
   ========================================================================== */

(function () {
    'use strict';

    var handlers = [];
    var modals = [];          // stack of { close: fn } — topmost owns the screen
    var sheet = null;         // the cheat sheet element
    var body = null;          // where its rows are painted
    var pill = null;          // the floating "shortcuts" capsule
    var toastEl = null;
    var toastTimer = null;
    var isOpen = false;

    /* ------------------------------------------------------------- helpers */

    function field(t) {
        if (!t || !t.tagName) return false;
        if (t.isContentEditable) return true;
        var n = t.tagName;
        return n === 'INPUT' || n === 'TEXTAREA' || n === 'SELECT' || n === 'OPTION';
    }

    function norm(k) { return String(k).toLowerCase(); }

    function chip(text) {
        var k = document.createElement('kbd');
        k.textContent = text;
        return k;
    }

    /* ------------------------------------------------------------- registry */

    function register(def) {
        if (!def || !def.keys || !def.keys.length) return null;
        /* a chord reads its whole sequence out of `keys` — ['g','h'] means
           "press g, then h" */
        if (Array.isArray(def.seq) && def.seq.length) def.keys = def.seq.slice();
        def.keys = def.keys.map(norm);
        def.when = typeof def.when === 'function' ? def.when : function () { return true; };
        def.run = typeof def.run === 'function' ? def.run : function () { return true; };
        def.priority = typeof def.priority === 'number' ? def.priority : 0;
        handlers.push(def);
        /* stable sort: page scripts (priority 0) always outrank the generic
           site-wide layer (negative priorities), and same-tier handlers keep
           their registration order */
        handlers.sort(function (a, b) { return b.priority - a.priority; });
        return def;
    }

    /* an overlay hands the engine one close() per opened screen, so Esc
       always knows what the user is looking at */
    function pushModal(close, name) { modals.push({ close: close, name: name || '' }); }
    function popModal() { return modals.pop() || null; }
    function modalUp() { return modals.length > 0; }

    function closeTop() {
        var m = modals.pop();
        if (m && typeof m.close === 'function') { m.close(); return true; }
        return false;
    }

    /* the little "you pressed this" note at the bottom of the screen */
    function toast(text, key) {
        if (!toastEl) {
            toastEl = document.createElement('div');
            toastEl.className = 'na-keys-toast';
            toastEl.setAttribute('role', 'status');
            document.body.appendChild(toastEl);
        }
        while (toastEl.firstChild) toastEl.removeChild(toastEl.firstChild);
        toastEl.classList.remove('is-chord');
        if (key) toastEl.appendChild(chip(key));
        if (text) {
            var s = document.createElement('span');
            s.textContent = text;
            toastEl.appendChild(s);
        }
        toastEl.classList.remove('is-on');
        void toastEl.offsetWidth;                    // restart the animation
        toastEl.classList.add('is-on');
        clearTimeout(toastTimer);
        toastTimer = setTimeout(function () { toastEl.classList.remove('is-on'); }, 1600);
    }

    /* ----------------------------------------------------- the cheat sheet */

    var GROUP_ORDER = ['Everywhere', 'Go to — 移動'];

    function groups() {
        var order = [];
        var map = {};
        handlers.forEach(function (h) {
            // Only list what this page actually answers to right now — a key
            // whose condition is unmet (e.g. Sensei-panel keys while the panel
            // is closed) would be a lie on the sheet.
            try { if (!h.when({})) return; } catch (e) { /* show it anyway */ }
            var g = h.group || 'Page';
            if (!map[g]) { map[g] = []; order.push(g); }
            if (map[g].indexOf(h) === -1) map[g].push(h);
        });
        order.sort(function (a, b) {
            var ia = GROUP_ORDER.indexOf(a);
            var ib = GROUP_ORDER.indexOf(b);
            if (ia === -1 && ib === -1) return 0;      // keep registration order
            if (ia === -1) return 1;
            if (ib === -1) return -1;
            return ia - ib;
        });
        return order.map(function (g) { return { name: g, items: map[g] }; });
    }

    function render() {
        if (!body) return;
        while (body.firstChild) body.removeChild(body.firstChild);

        groups().forEach(function (g) {
            var sec = document.createElement('section');
            sec.className = 'na-keys-group';

            var head = document.createElement('h4');
            head.textContent = g.name;
            sec.appendChild(head);

            var list = document.createElement('ul');
            g.items.forEach(function (h) {
                var li = document.createElement('li');
                li.className = 'na-keys-row';
                li.setAttribute('data-na-keys', h.keys.join(' '));

                var keys = document.createElement('span');
                keys.className = 'na-keys-row-keys';
                h.keys.forEach(function (k, i) {
                    if (i > 0) {
                        /* a chord: "G then H" reads better than two bare chips */
                        var then = document.createElement('span');
                        then.className = 'na-keys-then';
                        then.textContent = 'then';
                        keys.appendChild(then);
                    }
                    keys.appendChild(chip(pretty(k)));
                });

                var label = document.createElement('span');
                label.className = 'na-keys-row-label';
                label.textContent = h.label;

                li.appendChild(keys);
                li.appendChild(label);
                list.appendChild(li);
            });
            sec.appendChild(list);
            body.appendChild(sec);
        });
    }

    function pretty(k) {
        if (k === 'arrowleft') return '←';
        if (k === 'arrowright') return '→';
        if (k === 'arrowup') return '↑';
        if (k === 'arrowdown') return '↓';
        if (k === ' ') return 'Space';
        if (k === 'escape') return 'Esc';
        if (k === 'enter') return '⏎';
        return k.toUpperCase();
    }

    function build() {
        sheet = document.createElement('div');
        sheet.className = 'na-keys';
        sheet.setAttribute('data-na-keys', '');
        sheet.setAttribute('role', 'dialog');
        sheet.setAttribute('aria-modal', 'true');
        sheet.setAttribute('aria-label', 'Keyboard shortcuts');
        sheet.hidden = true;
        sheet.innerHTML =
            '<div class="na-keys-scrim" data-na-keys-scrim></div>' +
            '<div class="na-keys-card">' +
                '<button class="na-keys-close" type="button" data-na-keys-close aria-label="Close the shortcuts">' +
                    '<i class="bi bi-x-lg"></i></button>' +
                '<div class="na-keys-head">' +
                    '<i class="bi bi-keyboard"></i>' +
                    '<div>' +
                        '<h2>Keyboard shortcuts <span class="na-keys-head-jp">キーボード</span></h2>' +
                        '<p>Press <kbd>G</kbd> then a letter to jump anywhere on the site · ' +
                        '<kbd>/</kbd> search · <kbd>B</kbd> music · ' +
                        '<kbd>?</kbd> this sheet · <kbd>Esc</kbd> puts everything back.</p>' +
                    '</div>' +
                '</div>' +
                '<div class="na-keys-body" data-na-keys-body></div>' +
            '</div>';
        document.body.appendChild(sheet);
        body = sheet.querySelector('[data-na-keys-body]');

        sheet.querySelector('[data-na-keys-close]').addEventListener('click', close);
        sheet.querySelector('[data-na-keys-scrim]').addEventListener('click', close);

        pill = document.createElement('button');
        pill.type = 'button';
        pill.className = 'na-keys-pill';
        pill.setAttribute('data-na-keys-pill', '');
        pill.title = 'Keyboard shortcuts — press ?';
        pill.innerHTML = '<i class="bi bi-keyboard"></i><span>Shortcuts</span>';
        pill.addEventListener('click', function () { isOpen ? close() : open(); });
        document.body.appendChild(pill);
    }

    function open() {
        if (isOpen) return;
        if (!sheet) build();
        render();
        sheet.hidden = false;
        void sheet.offsetWidth;
        sheet.classList.add('is-on');
        document.body.style.overflow = 'hidden';
        isOpen = true;
        pushModal(close, 'sheet');
        var c = sheet.querySelector('[data-na-keys-close]');
        if (c) c.focus();
    }

    function close() {
        if (!isOpen || !sheet) { isOpen = false; return; }
        sheet.classList.remove('is-on');
        document.body.style.overflow = '';
        isOpen = false;
        clearTimeout(close.timer);
        /* leave it visible for the fade, then take it out of the flow */
        close.timer = setTimeout(function () { if (!isOpen) sheet.hidden = true; }, 260);
        /* the engine owns the sheet, so it removes itself from the stack */
        for (var i = modals.length - 1; i >= 0; i--) {
            if (modals[i].name === 'sheet') { modals.splice(i, 1); break; }
        }
        var c = sheet.querySelector('[data-na-keys-close]');
        if (c) c.focus();
    }

    /* while the sheet is up, a held-down key lights its row — the sheet turns
       into a little instrument panel you can play with */
    function hit(key) {
        if (!sheet || !isOpen) return;
        var rows = sheet.querySelectorAll('[data-na-keys]');
        for (var i = 0; i < rows.length; i++) {
            var on = (' ' + rows[i].getAttribute('data-na-keys') + ' ').indexOf(' ' + key + ' ') > -1;
            rows[i].classList.toggle('is-hit', on);
        }
    }

    /* ------------------------------------------------------- two-key chords */
    var pending = null;   // { key: 'g', opts: [handlers], timer }

    function chordStarts(key) {
        return handlers.filter(function (h) {
            return h.keys.length > 1 && h.keys[0] === key && h.when({});
        });
    }

    function setPending(key, opts) {
        pending = { key: key, opts: opts };
        clearTimeout(pending.timer);
        pending.timer = setTimeout(function () { pending = null; }, 1600);
        var hint = opts.map(function (h) {
            return pretty(h.keys[1]).toUpperCase() + ' ' + shortLabel(h);
        }).join('   ·   ');
        toast(hint, key.toUpperCase());
        if (toastEl) toastEl.classList.add('is-chord');
    }

    function clearPending() {
        if (!pending) return;
        clearTimeout(pending.timer);
        pending = null;
    }

    /* the hint toast has limited room — the first clause before any " — " */
    function shortLabel(h) {
        var t = String(h.label || '');
        var dash = t.indexOf(' — ');
        return (dash > 0 ? t.slice(0, dash) : t).replace(/\s*\/.*$/, '');
    }

    /* ------------------------------------------------------------- dispatch */

    function onKey(e) {
        /* another script already answered this key — leave it alone */
        if (e.defaultPrevented) return;
        if (e.ctrlKey || e.metaKey || e.altKey) return;

        var t = e.target;
        if (field(t)) return;

        var key = norm(e.key);
        if (!key) return;

        /* Space and Enter belong to whatever a link or button is focused on */
        var tag = (t && t.tagName) || '';
        if ((key === ' ' || key === 'enter') && (tag === 'BUTTON' || tag === 'A' || tag === 'SUMMARY')) return;

        /* the sheet is open only while it sits on the modal stack */
        var sheetUp = isOpen;

        /* ---- a chord is waiting for its second key ---- */
        if (pending) {
            var done = null;
            for (var i = 0; i < pending.opts.length; i++) {
                if (pending.opts[i].keys[1] === key) { done = pending.opts[i]; break; }
            }
            clearPending();
            if (key === 'escape') return;                     // Esc just cancels
            if (done && done.when(e)) {
                done.run(e);
                e.preventDefault();
                return;
            }
            /* not a chord after all — fall through and treat this key alone */
        }

        /* ---- this key STARTS a chord (g …) ---- */
        var starts = chordStarts(key);
        if (starts.length && !modalUp()) {
            if (sheetUp) { hit(key); return; }                // the sheet is up
            setPending(key, starts);
            e.preventDefault();
            return;
        }

        for (var i = 0; i < handlers.length; i++) {
            var h = handlers[i];
            if (h.keys.indexOf(key) === -1) continue;
            if (h.always !== true && modalUp() && !sheetUp) continue;
            if (sheetUp && !h.always && h.group !== 'Everywhere') continue;
            if (!h.when(e)) continue;
            h.run(e);
            e.preventDefault();
            if (h.stop === true) e.stopPropagation();
            return;
        }
        if (sheetUp) hit(key);
    }

    /* --------------------------------------------------------------- public */

    var api = {
        register: register,
        toast: toast,
        open: open,
        close: close,
        toggle: function () { isOpen ? close() : open(); },
        isOpen: function () { return isOpen; },
        pushModal: pushModal,
        popModal: popModal,
        closeTop: closeTop,
        modalUp: modalUp,
        refresh: function () { render(); }
    };

    window.naKeys = api;

    document.addEventListener('DOMContentLoaded', function () {
        build();
        register({
            keys: ['?'],
            group: 'Everywhere',
            label: 'Open this keyboard sheet',
            always: true,
            when: function () { return !isOpen; },
            run: function () { open(); }
        });
        register({
            keys: ['escape'],
            group: 'Everywhere',
            label: 'Close the photo viewer / this sheet',
            always: true,
            run: function () { return closeTop(); }
        });
        document.addEventListener('keydown', onKey);
    });
})();
