/* ==========================================================================
   region-page-keys.js — the keys that only make sense on a region page

   j / k   walk down and up the page section by section
   f       regional cuisine · m festivals · y the let's-play game
   h       back to the top
   0       back to the very top
   t       read this region's name aloud in Japanese
   [ / ]   the previous / next region
   (g starts the site-wide "go to" chords, so the game moved to y)

   Everything goes through naKeys, so the "?" sheet lists these automatically.
   ========================================================================== */

(function () {
    'use strict';

    if (!window.naKeys) return;

    var NA = window.naKeys;

    var OFFSET = 96;          // the floating navbar would otherwise cover the heading
    var hero = document.querySelector('.na-region-hero');
    var sections = Array.prototype.slice.call(document.querySelectorAll('.na-region-section'));
    var all = [hero].concat(sections).filter(Boolean);
    var titleJa = document.querySelector('.na-region-hero-title-ja');
    var regionName = titleJa ? titleJa.textContent.trim() : '';

    function top(sec) {
        return Math.max(0, sec.getBoundingClientRect().top + window.pageYOffset - OFFSET);
    }

    function goTo(sec, key, label) {
        if (!sec) return false;
        sec.scrollIntoView({ behavior: 'smooth', block: 'start' });
        NA.toast(label || 'Scrolling', key);
        return true;
    }

    /* the sections carry English + Japanese headings, so match on either */
    function byHeading(word) {
        for (var i = 0; i < sections.length; i++) {
            var h = sections[i].querySelector('h2');
            if (h && h.textContent.toLowerCase().indexOf(word) > -1) return sections[i];
        }
        return null;
    }

    function bySelector(sel) {
        return document.querySelector(sel);
    }

    /* ---------------------------------------------------------- j / k paging */
    function page(dir) {
        /* the tolerance has to sit OUTSIDE the current position, otherwise the
           section we are already standing on passes as "the next one up" and the
           page scrolls to the very spot it is on — looking like nothing happened */
        var y = window.pageYOffset;
        var list = dir > 0 ? all : all.slice().reverse();
        for (var i = 0; i < list.length; i++) {
            var t = top(list[i]);
            if (dir > 0 ? t > y + 8 : t < y - 8) {
                window.scrollTo({ top: t, behavior: 'smooth' });
                NA.toast(list[i].className.indexOf('hero') > -1 ? 'Region top'
                                                                : (dir > 0 ? 'Next section' : 'Previous section'),
                         dir > 0 ? 'J' : 'K');
                return true;
            }
        }
        window.scrollTo({ top: dir > 0 ? document.body.scrollHeight : 0, behavior: 'smooth' });
        return true;
    }

    /* ----------------------------------------------------------------- keys */
    var jump = [
        { keys: ['f'], label: 'Regional cuisine / 郷土料理', word: 'regional cuisine' },
        { keys: ['m'], label: 'Festivals & events / 祭り',    word: 'festival' },
        { keys: ['y'], label: "Let's play / 遊び",            sel: '.na-quiz-section' },
        { keys: ['h'], label: 'Region top / 最初へ',          hero: true },
        { keys: ['0'], label: 'Back to the very top',          top: true }
    ];

    jump.forEach(function (j) {
        NA.register({
            keys: j.keys,
            group: 'Moving around — 移動',
            label: j.label,
            run: function () {
                if (j.top) {
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                    NA.toast('Top of the page', '0');
                    return true;
                }
                if (j.hero) return goTo(hero, j.keys[0], 'Top of the page');
                var sec = j.sel ? bySelector(j.sel) : byHeading(j.word);
                return goTo(sec, j.keys[0], j.label);
            }
        });
    });

    NA.register({
        keys: ['j'],
        group: 'Moving around — 移動',
        label: 'Next section / 次の項目',
        run: function () { return page(1); }
    });

    NA.register({
        keys: ['k'],
        group: 'Moving around — 移動',
        label: 'Previous section / 前の項目',
        run: function () { return page(-1); }
    });

    NA.register({
        keys: ['['],
        group: 'Moving around — 移動',
        label: 'Previous region / 前の地方',
        run: function () {
            var a = bySelector('.na-region-prevnext-prev');
            if (!a) return false;
            NA.toast('Previous region', '[');
            a.click();
            return true;
        }
    });

    NA.register({
        keys: [']'],
        group: 'Moving around — 移動',
        label: 'Next region / 次の地方',
        run: function () {
            var a = bySelector('.na-region-prevnext-next');
            if (!a) return false;
            NA.toast('Next region', ']');
            a.click();
            return true;
        }
    });

    NA.register({
        keys: ['t'],
        group: 'Moving around — 移動',
        label: 'Read the region name aloud / 地名を読む',
        run: function () {
            if (!regionName || !window.speechSynthesis) return false;
            try {
                var synth = window.speechSynthesis;
                synth.cancel();
                var u = new SpeechSynthesisUtterance(regionName);
                u.lang = 'ja-JP';
                u.rate = 0.8;
                synth.speak(u);
            } catch (err) { /* no voice pack */ }
            NA.toast(regionName, 'T');
            return true;
        }
    });
})();
