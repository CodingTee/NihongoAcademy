/* ==========================================================================
   region-showcase.js — "Must-see highlights" interactive photo viewer

   Drives the three-column stage: the photo in the middle, the caption
   panel on the left (which follows whichever photo is on stage) and the
   contents list on the right (each row jumps straight to that photo).
   Shares one full-screen viewer with the gallery tiles further down.
   ========================================================================== */

(function () {
    'use strict';

    var AUTO_MS = 5200;
    /* a 1×1 transparent GIF, so an empty src never falls back to the page URL */
    var PLACEHOLDER = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7';

    /* true while the full-screen viewer owns the screen — the keyboard engine
       uses it to keep the arrow keys with the viewer instead of the stage */
    var lbOpen = false;
    var stageKeysWired = false;

    function pad(n) { return n < 10 ? '0' + n : String(n); }

    /* Chrome needs a user gesture before it will speak, and a keypress counts
       as one — so "read this aloud" goes through the same door as the clicks. */
    function speak(text) {
        if (!text || !window.speechSynthesis) return;
        try {
            var synth = window.speechSynthesis;
            synth.cancel();
            var u = new SpeechSynthesisUtterance(text);
            u.lang = 'ja-JP';
            u.rate = 0.85;
            synth.speak(u);
        } catch (err) { /* no Japanese voice pack — stay quiet */ }
    }

    function $(el, sel) { return el ? el.querySelector(sel) : null; }
    function $$(el, sel) { return el ? Array.prototype.slice.call(el.querySelectorAll(sel)) : []; }
    function all(sel) { return Array.prototype.slice.call(document.querySelectorAll(sel)); }
    function txt(el, v) { if (el) el.textContent = v || ''; }

    /* shared by the stage viewer and the gallery tiles — keep it on the
       outer scope so every entry point below can reach it. */
    function span(cls, text) {
        var s = document.createElement('span');
        s.className = cls;
        s.textContent = text;
        return s;
    }

    /* paint the caption line under a full-screen photo */
    function fillCaption(host, m) {
        if (!host) return;
        host.textContent = '';
        if (m.kind) host.appendChild(span('na-sh-lb-kind', m.kind));
        if (m.title) host.appendChild(span('na-sh-lb-title', m.title));
        if (m.titleja) host.appendChild(span('na-sh-lb-ja', m.titleja));
        if (m.note) host.appendChild(span('na-sh-lb-note', m.note));
    }

    function showLightbox(lb, src, alt, meta) {
        if (!lb) return;
        var img = lb.querySelector('img');
        // never assign "" — an empty src falls back to the page URL
        if (img) { img.src = src || PLACEHOLDER; img.alt = alt || ''; }
        fillCaption(lb.querySelector('[data-sh-lb-caption]'), meta || {});
        lb.classList.add('is-open');
        document.body.style.overflow = 'hidden';
        lbOpen = true;                  // the keyboard engine now knows we are locked
        lockViewer();
    }

    /* The viewer can be opened from three places (the stage, the dish cards,
       the festival cards). Whichever it is, the engine has to hear about it —
       otherwise Esc would have nothing to close and the arrow keys would keep
       paging the photo stage behind the overlay. */
    var viewerModalPushed = false;
    var viewerCloser = null;              // filled in once the viewer exists
    function lockViewer() {
        if (viewerModalPushed || !window.naKeys) return;
        window.naKeys.pushModal(function () { if (lbOpen && viewerCloser) viewerCloser(); }, 'viewer');
        viewerModalPushed = true;
    }
    function unlockViewer() {
        if (!viewerModalPushed || !window.naKeys) return;
        window.naKeys.popModal();
        viewerModalPushed = false;
    }

    function metaOf(el) {
        return {
            kind: el.getAttribute('data-sh-kind'),
            title: el.getAttribute('data-sh-title'),
            titleja: el.getAttribute('data-sh-titleja'),
            note: el.getAttribute('data-sh-note')
        };
    }

    document.addEventListener('DOMContentLoaded', function () {
        var wraps = Array.prototype.slice.call(document.querySelectorAll('[data-na-showcase]'));
        for (var i = 0; i < wraps.length; i++) init(wraps[i]);
        initGlightbox();
        initTiles();
        initSayButtons();
    });

    /* ------------------------------------------------------------------
       "Fun & curious" — read a Japanese headword aloud.
       Chrome needs a user gesture, which a click is; if the machine has
       no Japanese voice pack the button simply stays quiet.
       ------------------------------------------------------------------ */
    function initSayButtons() {
        var synth = window.speechSynthesis;
        if (!synth) return;

        all('[data-na-say]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var text = btn.getAttribute('data-na-say') || '';
                if (!text) return;
                try {
                    synth.cancel();
                    var u = new SpeechSynthesisUtterance(text);
                    u.lang = 'ja-JP';
                    u.rate = 0.85;
                    btn.classList.add('is-speaking');
                    synth.speak(u);
                    u.onend = function () { btn.classList.remove('is-speaking'); };
                } catch (err) { /* no speech output available — ignore */ }
            });
        });
    }

    /* ------------------------------------------------------------------ */
    function init(wrap) {
        var viewport = $(wrap, '[data-sh-viewport]');
        if (!viewport) return;

        var slides = $$(wrap, '[data-sh-slide]');
        if (!slides.length) return;
        var total = slides.length;

        var indexEl = $(wrap, '[data-sh-index]');
        var totalEl = $(wrap, '[data-sh-total]');
        var barEl = $(wrap, '[data-sh-bar]');
        var prevBtn = $(wrap, '[data-sh-prev]');
        var nextBtn = $(wrap, '[data-sh-next]');
        var autoBtn = $(wrap, '[data-sh-autoplay]');

        /* NOTE: the caption uses its own data-sh-now-* hooks — the slides above
           carry plain data-sh-kind / data-sh-title etc., so a plain lookup
           would grab the slide instead of the caption text. */
        var now = {
            panel: $(wrap, '[data-sh-now]'),
            kind: $(wrap, '[data-sh-now-kind]'),
            title: $(wrap, '[data-sh-now-title]'),
            titleja: $(wrap, '[data-sh-now-titleja]'),
            note: $(wrap, '[data-sh-now-note]')
        };

        var gotoBtns = $$(wrap, '[data-sh-goto]');

        var cur = 0;
        var timer = null;
        var onScreen = true;      // kept true until we know the stage is off screen
        var reduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        /* ------------------------------------------------ captions */
        function readNow(i) {
            var sl = slides[i];
            if (!sl) return;
            txt(now.kind, sl.getAttribute('data-sh-kind'));
            txt(now.title, sl.getAttribute('data-sh-title'));
            txt(now.titleja, sl.getAttribute('data-sh-titleja'));
            txt(now.note, sl.getAttribute('data-sh-note'));
        }

        function paint(i) {
            for (var s = 0; s < total; s++) {
                var on = s === i;
                slides[s].classList.toggle('is-active', on);
                slides[s].setAttribute('aria-hidden', on ? 'false' : 'true');
            }

            txt(indexEl, pad(i + 1));
            txt(totalEl, pad(total));
            if (barEl) barEl.style.width = ((i + 1) / total * 100) + '%';

            for (var g = 0; g < gotoBtns.length; g++) {
                gotoBtns[g].classList.toggle('is-active', g === i);
            }

            readNow(i);

            if (now.panel && !reduced) {
                now.panel.classList.remove('is-fading');
                void now.panel.offsetWidth;   /* restart the fade */
                now.panel.classList.add('is-fading');
            }
        }

        function go(i) {
            cur = (i + total) % total;
            paint(cur);
        }

        function step(d) { stopAuto(); go(cur + d); }

        /* ------------------------------------------------ controls */
        if (prevBtn) prevBtn.addEventListener('click', function () { step(-1); });
        if (nextBtn) nextBtn.addEventListener('click', function () { step(1); });

        gotoBtns.forEach(function (b) {
            b.addEventListener('click', function () {
                stopAuto();
                go(parseInt(b.getAttribute('data-sh-goto'), 10) || 0);
            });
        });

        /* ------------------------------------------------ autoplay */
        function startAuto() {
            if (timer || total < 2) return;
            timer = setInterval(function () { go(cur + 1); }, AUTO_MS);
            if (autoBtn) { autoBtn.setAttribute('aria-pressed', 'true'); autoBtn.classList.remove('is-paused'); }
        }

        function stopAuto() {
            if (timer) { clearInterval(timer); timer = null; }
            if (autoBtn) { autoBtn.setAttribute('aria-pressed', 'false'); autoBtn.classList.add('is-paused'); }
        }

        if (autoBtn) {
            autoBtn.addEventListener('click', function () {
                if (timer) stopAuto(); else startAuto();
            });
        }

        /* ------------------------------------------------ paging gestures */
        var drag = null;

        viewport.addEventListener('pointerdown', function (e) {
            drag = { x: e.clientX, y: e.clientY, t: Date.now() };
        });

        viewport.addEventListener('pointerup', function (e) {
            if (!drag) return;
            var dx = e.clientX - drag.x;
            var dy = e.clientY - drag.y;
            drag = null;
            if (Math.abs(dx) > 45 && Math.abs(dx) > Math.abs(dy)) step(dx < 0 ? 1 : -1);
        });

        viewport.addEventListener('pointercancel', function () { drag = null; });

        viewport.addEventListener('wheel', function (e) {
            if (Math.abs(e.deltaX) <= Math.abs(e.deltaY)) return;  /* vertical scroll belongs to the page */
            e.preventDefault();
            step(e.deltaX > 0 ? 1 : -1);
        }, { passive: false });

        viewport.addEventListener('click', function (e) {
            if (e.target.closest && e.target.closest('[data-sh-prev], [data-sh-next]')) return;
            openLbFromSlides(cur);
        });

        /* ------------------------------------------------ keyboard
           The stage no longer listens for keys itself: it teaches naKeys a
           handful of them, and naKeys prints the "?" cheat sheet from the
           same list — so the sheet can never promise a key the stage does
           not answer to. */
        if (!stageKeysWired) {
            stageKeysWired = true;
            var NA = window.naKeys;
            if (NA) {
                var NAtooltip = function () {
                    return slides[cur] ? slides[cur].getAttribute('data-sh-title') || '' : '';
                };
                function live() { return onScreen; }

                NA.register({
                    keys: ['arrowleft'],
                    group: 'Photos — 写真',
                    label: 'Previous photo / 前の写真',
                    when: live,
                    run: function () { stopAuto(); step(-1); NA.toast(NAtooltip(), '←'); }
                });
                NA.register({
                    keys: ['arrowright'],
                    group: 'Photos — 写真',
                    label: 'Next photo / 次の写真',
                    when: live,
                    run: function () { stopAuto(); step(1); NA.toast(NAtooltip(), '→'); }
                });
                NA.register({
                    keys: [' '],
                    group: 'Photos — 写真',
                    label: 'Auto-play on / off',
                    when: live,
                    run: function () {
                        if (timer) { stopAuto(); NA.toast('Auto-play off', 'Space'); }
                        else { startAuto(); NA.toast('Auto-play on', 'Space'); }
                    }
                });
                NA.register({
                    keys: ['1', '2', '3', '4', '5', '6', '7', '8', '9'],
                    group: 'Photos — 写真',
                    label: 'Jump to photo 1–9',
                    when: live,
                    run: function (e) {
                        var n = parseInt(e.key, 10);
                        if (!n || n > total) return;
                        stopAuto(); go(n - 1);
                        NA.toast(NAtooltip(), String(n));
                    }
                });
                NA.register({
                    keys: ['s'],
                    group: 'Photos — 写真',
                    label: 'Read this photo aloud / 読む',
                    when: live,
                    run: function () {
                        var sl = slides[cur];
                        if (!sl) return;
                        var t = sl.getAttribute('data-sh-titleja') || sl.getAttribute('data-sh-title') || '';
                        speak(t);
                        NA.toast(t, 'S');
                    }
                });
                /* Enter opens the viewer — but once the game is on screen Enter
                   belongs to the game, so this steps aside */
                NA.register({
                    keys: ['enter'],
                    group: 'Photos — 写真',
                    label: 'Enlarge the photo / 拡大',
                    when: function () { return onScreen && !window.naRegionQuizOnScreen; },
                    run: function () {
                        openLbFromSlides(cur);
                        NA.toast(NAtooltip(), '⏎');
                    }
                });
            }
        }

        /* ------------------------------------------------ entrance */
        if (window.IntersectionObserver && !reduced) {
            var io = new IntersectionObserver(function (entries) {
                entries.forEach(function (en) {
                    if (en.isIntersecting) {
                        en.target.classList.add('is-visible');
                        startAuto();
                    }
                    /* the arrow keys stay on this stage only while it is in
                       view — otherwise they wouldfire somewhere off-screen */
                    onScreen = en.isIntersecting;
                });
            }, { threshold: .25 });
            io.observe(wrap);
        } else {
            startAuto();
        }

        /* ------------------------------------------------ go */
        paint(0);
    }

    /* ================================================================
       the shared full-screen viewer
       ================================================================ */
    function initGlightbox() {
        var lb = document.querySelector('[data-sh-lightbox]');
        if (!lb) return;

        var lbImg = lb.querySelector('img');
        var lbCap = lb.querySelector('[data-sh-lb-caption]');
        var closeBtn = lb.querySelector('[data-sh-lb-close]');
        var prevBtn = lb.querySelector('[data-sh-lb-prev]');
        var nextBtn = lb.querySelector('[data-sh-lb-next]');

        var lastFocus = null;
        var show = false;

        /* repaint the opened viewer without reopening it */
        function paintLb(src, alt, meta) {
            if (lbImg) { lbImg.src = src || PLACEHOLDER; lbImg.alt = alt || ''; }
            fillCaption(lbCap, meta || {});
        }

        viewerCloser = close;

        function open() {
            lastFocus = document.activeElement;
            lb.classList.add('is-open');
            document.body.style.overflow = 'hidden';
            show = true;
            lbOpen = true;
            lockViewer();               /* the engine now owns Esc + the arrows */
            if (closeBtn) closeBtn.focus();
        }

        function close() {
            lb.classList.remove('is-open');
            document.body.style.overflow = '';
            show = false;
            lbOpen = false;
            unlockViewer();
            if (lastFocus && lastFocus.focus) lastFocus.focus();
        }

        if (closeBtn) closeBtn.addEventListener('click', close);
        if (prevBtn) prevBtn.addEventListener('click', function () { step(-1); });
        if (nextBtn) nextBtn.addEventListener('click', function () { step(1); });

        lb.addEventListener('click', function (e) {
            if (e.target === lb || (e.target.classList && e.target.classList.contains('na-sh-lb-caption'))) close();
        });

        /* the viewer's own keys live in the shared registry (always: true, so
           they keep working while the screen is locked to this overlay) */
        var NA = window.naKeys;
        if (NA) {
            NA.register({
                keys: ['arrowleft'],
                group: 'Photos — 写真',
                label: 'Previous photo / 前の写真',
                always: true,
                when: function () { return lbOpen; },
                run: function () { step(-1); }
            });
            NA.register({
                keys: ['arrowright'],
                group: 'Photos — 写真',
                label: 'Next photo / 次の写真',
                always: true,
                when: function () { return lbOpen; },
                run: function () { step(1); }
            });
            NA.register({
                keys: ['escape', ' '],
                group: 'Photos — 写真',
                label: 'Close the viewer / 閉じる',
                always: true,
                when: function () { return lbOpen; },
                run: function () { close(); }
            });
        }

        function step(d) {
            var idx = currentIndex();
            if (idx < 0) return;
            var next = (idx + d + sources.length) % sources.length;
            var s = sources[next];
            paintLb(s.src, s.alt, s);
        }

        /* the viewer walks every photo on the page, in document order */
        var sources = collectSources();

        function collectSources() {
            var list = [];
            all('[data-na-showcase]').forEach(function (wrap) {
                $$(wrap, '[data-sh-slide]').forEach(function (sl) {
                    list.push({
                        src: attr(sl.querySelector('img'), 'src'),
                        alt: attr(sl.querySelector('img'), 'alt'),
                        kind: sl.getAttribute('data-sh-kind'),
                        title: sl.getAttribute('data-sh-title'),
                        titleja: sl.getAttribute('data-sh-titleja'),
                        note: sl.getAttribute('data-sh-note')
                    });
                });
            });
            all('.na-sh-tile').forEach(function (t) {
                list.push({
                    src: t.getAttribute('data-sh-src') || attr(t.querySelector('img'), 'src'),
                    alt: t.getAttribute('data-sh-alt') || attr(t.querySelector('img'), 'alt'),
                    kind: t.getAttribute('data-sh-kind'),
                    title: t.getAttribute('data-sh-title'),
                    titleja: t.getAttribute('data-sh-titleja'),
                    note: t.getAttribute('data-sh-note')
                });
            });
            var seenSrc = {};
            function push(s) {
                if (!s.src || seenSrc[s.src]) return;
                seenSrc[s.src] = 1;
                list.push(s);
            }

            /* cuisine + festival cards carry the same shape of data */
            all('[data-sh-card]').forEach(function (t) {
                push({
                    src: t.getAttribute('data-sh-src') || attr(t.querySelector('img'), 'src'),
                    alt: t.getAttribute('data-sh-alt') || attr(t.querySelector('img'), 'alt'),
                    kind: t.getAttribute('data-sh-kind'),
                    title: t.getAttribute('data-sh-title'),
                    titleja: t.getAttribute('data-sh-titleja'),
                    note: t.getAttribute('data-sh-note')
                });
            });
            /* the stage holds the same photos as the gallery and the cards —
               de-duplicate so paging never revisits a photo twice */
            return list;
        }

        function currentIndex() {
            for (var i = 0; i < sources.length; i++) {
                var im = sources[i].src.replace(/^.*\//, '');
                if (lbImg && lbImg.src.indexOf(im) > -1) return i;
            }
            return sources.length ? 0 : -1;
        }
    }

    function attr(el, name) { return el ? el.getAttribute(name) || '' : ''; }

    /* the stage hands the photo it is showing to the viewer */
    function openLbFromSlides(i) {
        var lb = document.querySelector('[data-sh-lightbox]');
        if (!lb) return;
        var slide = document.querySelectorAll('[data-sh-slide]')[i];
        if (!slide) return;
        var img = slide.querySelector('img');
        showLightbox(lb, attr(img, 'src'), attr(img, 'alt'), metaOf(slide));
    }

    /* gallery tiles and the cuisine / festival cards open the very same viewer */
    function initTiles() {
        all('.na-sh-tile, [data-sh-card]').forEach(function (tile) {
            function open() {
                var img = tile.querySelector('img');
                showLightbox(
                    document.querySelector('[data-sh-lightbox]'),
                    tile.getAttribute('data-sh-src') || attr(img, 'src'),
                    tile.getAttribute('data-sh-alt') || attr(img, 'alt'),
                    metaOf(tile)
                );
            }

            tile.addEventListener('click', open);

            /* the dish cards are <article> wrappers (a <button> cannot flex its
               children), so hand them the keyboard path as well */
            if (tile.tagName !== 'BUTTON' && tile.tagName !== 'A') {
                tile.setAttribute('tabindex', '0');
                tile.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter' || e.key === ' ' || e.key === 'Spacebar') {
                        e.preventDefault();
                        open();
                    }
                });
            }
        });
    }
})();
