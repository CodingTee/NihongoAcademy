/* ==========================================================================
   site-keys.js — the keys every page of the academy answers to

   Navigation is a two-key chord, GitHub/Gmail style: press G, let go, then
   the letter of where you want to be. A hint toast lists the options while
   the chord is waiting, and Esc cancels it.

       G H  Home            G L  Lessons        G P  Practice
       G Q  Quizzes         G F  Flashcards     G C  Community
       G E  Explore Japan   G D  Dashboard      G R  Leaderboard
       G U  My profile

   Single keys that work everywhere:

       /    Focus this page's search box (if it has one)
       I    Notifications — open the bell menu
       E    Mark all notifications read (while the bell menu is open)
       J/K  Next / previous — walks the card list on list pages
            (Lessons, Community, Flashcards …), otherwise pages the scroll

   List pages opt in with data-na-list on their container, and may name the
   link inside each card with data-na-list-item (e.g. "a.na-post-link").

   Everything registers through naKeys with a NEGATIVE priority, so a page's
   own keys (Lesson Study, Flashcard review, region keys …) always win.
   ========================================================================== */

(function () {
    'use strict';

    if (!window.naKeys) return;
    var NA = window.naKeys;

    /* ------------------------------------------------- G chords: go to … */
    var NAV = [
        { seq: ['g', 'h'], path: '/',                      label: 'Home — ホーム' },
        { seq: ['g', 'l'], path: '/Lessons',               label: 'Lessons — レッスン' },
        { seq: ['g', 'p'], path: '/Practice',              label: 'Practice — 練習' },
        { seq: ['g', 'q'], path: '/Quizzes',               label: 'Quizzes — クイズ' },
        { seq: ['g', 'f'], path: '/Flashcards',            label: 'Flashcards — 単語帳' },
        { seq: ['g', 'c'], path: '/Community',             label: 'Community — コミュニティ' },
        { seq: ['g', 'e'], path: '/ExploreJapan',          label: 'Explore Japan — 日本探索' },
        { seq: ['g', 'd'], path: '/Dashboard',             label: 'Dashboard — 学習状況' },
        { seq: ['g', 'r'], path: '/Community/Leaderboard', label: 'Leaderboard — ランキング' }
    ];

    NAV.forEach(function (n) {
        NA.register({
            seq: n.seq,
            group: 'Go to — 移動',
            label: n.label,
            run: function () {
                NA.toast(n.label.split(' — ')[0], n.seq[0].toUpperCase() + n.seq[1].toUpperCase());
                window.location.href = n.path;
            }
        });
    });

    /* G U — profile lives behind the avatar menu, so reuse its real link */
    NA.register({
        seq: ['g', 'u'],
        group: 'Go to — 移動',
        label: 'My profile — マイページ',
        run: function () {
            var a = document.querySelector('a.dropdown-item[href*="Account/Manage"]');
            if (a && a.href) {
                NA.toast('My profile', 'GU');
                window.location.href = a.href;
            } else {
                NA.toast('Open the avatar menu (top right) for your profile', 'GU');
            }
        }
    });


    /* ------------------------------------------------- / — page search */
    NA.register({
        keys: ['/'],
        group: 'Everywhere',
        label: 'Search this page',
        priority: -5,
        run: function () {
            var box = document.querySelector('.na-control-search-input, input[name="search"], input[type="search"]');
            if (box) {
                box.focus();
                if (box.select) box.select();
                NA.toast('Search', '/');
            } else {
                NA.toast('No search on this page', '/');
            }
        }
    });

    /* ------------------------------------------------- I / E — notifications */
    NA.register({
        keys: ['i'],
        group: 'Everywhere',
        label: 'Notifications — open the bell menu',
        priority: -5,
        run: function () {
            var toggle = document.getElementById('notifDropdown');
            if (toggle) {
                toggle.click();
                NA.toast('Notifications', 'I');
            } else {
                NA.toast('No notifications here', 'I');
            }
        }
    });

    NA.register({
        keys: ['e'],
        group: 'Everywhere',
        label: 'Mark all notifications read',
        priority: -5,
        when: function () {
            var btn = document.getElementById('naMarkAllReadBtn');
            if (!btn) return false;
            var menu = btn.closest('.dropdown-menu');
            return !!menu && menu.classList.contains('show');
        },
        run: function () {
            var btn = document.getElementById('naMarkAllReadBtn');
            if (btn) {
                btn.click();
                NA.toast('All caught up', 'E');
            }
        }
    });

    /* ------------------------------------------------- J / K — list walking */
    var list = document.querySelector('[data-na-list]');
    if (list) {
        var items = [];

        function collect() {
            var sel = list.getAttribute('data-na-list-item');
            var nodes = sel
                ? Array.prototype.slice.call(list.querySelectorAll(sel))
                : Array.prototype.slice.call(list.children)
                      .map(function (child) { return child.querySelector('a[href]'); });
            items = nodes.filter(function (n) {
                return n && n.offsetParent !== null && (n.getAttribute('href') || '').charAt(0) !== '#';
            });
        }
        collect();

        if (items.length) {
            var idx = -1;

            function focusItem(i) {
                idx = (i + items.length) % items.length;
                items.forEach(function (n) { n.removeAttribute('tabindex'); });
                var n = items[idx];
                n.setAttribute('tabindex', '-1');
                n.focus({ preventScroll: true });
                n.scrollIntoView({ behavior: 'smooth', block: 'center' });
                NA.toast((idx + 1) + ' of ' + items.length, '');
            }

            NA.register({
                keys: ['j'],
                group: 'Moving around — 移動',
                label: 'Next item — 次の項目',
                priority: -10,
                when: function () { return items.length > 0; },
                run: function () { focusItem(idx + 1); return true; }
            });

            NA.register({
                keys: ['k'],
                group: 'Moving around — 移動',
                label: 'Previous item — 前の項目',
                priority: -10,
                when: function () { return items.length > 0; },
                run: function () { focusItem(idx - 1); return true; }
            });
        }
    }

    /* ------------------------------------------------- J / K — page paging */
    function pageBy(dir) {
        window.scrollBy({ top: dir * window.innerHeight * 0.85, behavior: 'smooth' });
        NA.toast(dir > 0 ? 'Scrolling down' : 'Scrolling up', dir > 0 ? 'J' : 'K');
    }

    NA.register({
        keys: ['j'],
        group: 'Moving around — 移動',
        label: 'Scroll down — 下にスクロール',
        priority: -20,
        run: function () { pageBy(1); return true; }
    });

    NA.register({
        keys: ['k'],
        group: 'Moving around — 移動',
        label: 'Scroll up — 上にスクロール',
        priority: -20,
        run: function () { pageBy(-1); return true; }
    });
})();
