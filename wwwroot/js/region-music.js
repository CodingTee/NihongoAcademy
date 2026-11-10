/* ==========================================================================
   region-music.js — the background music of the Regions part

   Rules we have to live with:
   · Sound autoplay is blocked until the visitor has touched the page, so the
     first attempt is made with sound and, if the browser refuses, we silently
     fall back to a MUTED start (muted autoplay is always allowed). The pill
     then shows as not-live and one real click unlocks the sound, because a
     click IS a gesture.
   · The choice is remembered in localStorage, so coming back to a region
     doesn't surprise anyone who turned it off once.
   · Walking from one region to the next keeps the SAME performance going:
     the playhead is parked in sessionStorage on the way out and picked up
     again on the next region page, so `]` doesn't restart the song from 0.
     (A full page load is unavoidable — Regions are server-rendered pages,
     not one long document — so the position is what has to travel.)
   · The track pauses with the tab and resumes with it, and it never plays
     under a modal (lightbox / cheat sheet) — nobody wants music over a photo.
   · `B` flips it, just like every other key on the region page.
   ========================================================================== */

(function () {
    'use strict';

    var KEY = 'na-region-music';
    var POS = 'na-region-music-pos';        /* the playhead, for the next region */
    var audio = document.getElementById('na-region-music');
    var btn = document.getElementById('na-region-music-btn');
    if (!audio || !btn) return;

    audio.loop = true;
    audio.volume = 0.42;

    /* '1' = on, anything else = off; on by default on the regions */
    var want = localStorage.getItem(KEY);
    if (want !== '0' && want !== '1') want = '1';
    var mutedFallback = false;

    /* ---------- continuity across the region pages ----------
       Metadata has to be in before currentTime can be set, so the seek is
       queued on `loadedmetadata`; a stale position (older than a few hours)
       or an off switch both mean "start over" instead of "resume". */
    var resumeAt = 0;
    try {
        resumeAt = parseFloat(sessionStorage.getItem(POS)) || 0;
        var savedAt = parseFloat(sessionStorage.getItem(POS + '-at')) || 0;
        if (Date.now() - savedAt > 3 * 60 * 60 * 1000) resumeAt = 0;
    } catch (e) { resumeAt = 0; }

    function applyResume() {
        if (!(resumeAt > 0) || want !== '1') { resumeAt = 0; return; }
        var d = audio.duration;
        try { audio.currentTime = (isFinite(d) && d > 0) ? (resumeAt % d) : resumeAt; } catch (e) { }
        resumeAt = 0;
    }
    audio.addEventListener('loadedmetadata', applyResume);
    if (audio.readyState >= 1) applyResume();

    function savePos() {
        try {
            sessionStorage.setItem(POS, String(audio.currentTime || 0));
            sessionStorage.setItem(POS + '-at', String(Date.now()));
        } catch (e) { }
    }
    window.addEventListener('pagehide', savePos);
    window.addEventListener('beforeunload', savePos);
    /* a slow page might never fire pagehide — so park it as we go too */
    var lastPark = 0;
    audio.addEventListener('timeupdate', function () {
        var now = Date.now();
        if (now - lastPark > 4000) { lastPark = now; savePos(); }
    });

    /* ---------- the pill ---------- */
    var label = btn.querySelector('.na-music-label');

    function paint() {
        var audible = want === '1' && !audio.muted && !audio.paused;
        btn.classList.toggle('is-on', audible);
        btn.setAttribute('aria-pressed', audible ? 'true' : 'false');
        btn.title = audible
            ? 'Turn the region music off  (B)'
            : 'Turn the region music on  (B)';
        if (label) label.textContent = audible ? 'Music on' : 'Music off';
    }

    /* ---------- playback ---------- */
    /* Sound may only START inside a real user gesture. A programmatic call —
       page load, or Ask Sensei answering "open music" from a setTimeout long
       after the click that sent it — is refused by every modern browser and
       falls back to muted. pendingSound remembers that debt: the next genuine
       gesture (click, key, touch) retries WITH sound and the pill flips to
       "Music on". Without this, "open music" from Sensei would hum silently
       forever while insisting the music is on. */
    var pendingSound = false;

    function tryPlay() {
        if (want !== '1') { audio.pause(); return; }
        audio.muted = false;
        var p = audio.play();
        if (p && typeof p.catch === 'function') {
            p.then(function () { pendingSound = false; });
            p.catch(function () {
                /* refused — hum along muted until the next real gesture */
                mutedFallback = true;
                pendingSound = true;
                audio.muted = true;
                var q = audio.play();
                if (q && typeof q.catch === 'function') q.catch(function () { });
                paint();
            });
        }
        paint();
    }

    function toggle() {
        want = (want === '1') ? '0' : '1';
        try { localStorage.setItem(KEY, want); } catch (e) { }
        if (want === '1') {
            mutedFallback = false;
            tryPlay();
        } else {
            // An explicit "off" cancels any sound the browser still owes us.
            pendingSound = false;
            audio.pause();
            paint();
        }
    }

    /* ---------- wiring ---------- */
    btn.addEventListener('click', function (e) {
        e.preventDefault();
        toggle();
    });

    ['play', 'pause', 'volumechange', 'loadedmetadata'].forEach(function (ev) {
        audio.addEventListener(ev, paint);
    });

    /* The gesture that unlocks sound after a refused play(). Unlike the old
       fire-once listener this stays armed for the whole page life — it costs
       nothing unless pendingSound is set, and it covers every way sound can
       end up owed: page load, Sensei's "open music", a return to a hidden tab. */
    function onGesture() {
        if (!pendingSound) return;
        pendingSound = false;
        mutedFallback = false;
        tryPlay();
    }

    ['pointerdown', 'keydown', 'touchstart'].forEach(function (ev) {
        window.addEventListener(ev, onGesture, { passive: true });
    });

    /* be a good citizen: stop with the tab, pick up where we left off */
    document.addEventListener('visibilitychange', function () {
        if (document.hidden) {
            if (!audio.paused) { audio.pause(); audio.dataset.wasPlaying = '1'; }
        } else if (want === '1' && audio.dataset.wasPlaying === '1') {
            delete audio.dataset.wasPlaying;
            tryPlay();
        }
    });

    /* ---------- keyboard ---------- */
    if (window.naKeys && typeof window.naKeys.register === 'function') {
        window.naKeys.register({
            keys: ['b'],
            group: 'Everywhere',
            label: 'Region music on / off',
            run: function () {
                toggle();
                if (window.naKeys && window.naKeys.toast) {
                    // Toast what is AUDIBLE, not what was asked for — a refused
                    // play() leaves the pill (and this) at "Music off" until a
                    // gesture lands, and claiming otherwise is a lie.
                    var audible = want === '1' && !audio.muted && !audio.paused;
                    window.naKeys.toast(audible ? 'Music on' : 'Music off', 'B');
                }
                return true;
            }
        });
        window.naKeys.refresh && window.naKeys.refresh();
    }

    /* ---------- Sensei hook ----------
       The AI tutor ("Ask Sensei") needs to control the music when the learner
       asks — "close the music". This is the same seam the keyboard uses, so
       the pill, localStorage and the playhead all stay consistent. */
    window.naRegionMusic = {
        isOn: function () { return want === '1' && !audio.muted && !audio.paused; },
        // True when the learner asked for music but the browser is holding the
        // sound until the next real gesture — Ask Sensei checks this so it can
        // say "one click and it starts" instead of "it's on!".
        needsGesture: function () { return pendingSound; },
        set: function (on) {
            var target = on ? '1' : '0';
            if (want !== target) {
                toggle();
            } else if (on && (audio.paused || mutedFallback)) {
                /* Already marked "on" but nothing is audible — the browser
                   refused the earlier autoplay, or the tab was hidden. A
                   request from Ask Sensei is a real user gesture now, so
                   retry the playback instead of silently doing nothing. */
                mutedFallback = false;
                tryPlay();
            }
        },
        toggle: function () { toggle(); }
    };

    /* start — sound first, muted if the browser says no */
    tryPlay();
})();
