/* ==========================================================================
   region-quiz.js — "Let's play / 〇〇で遊ぼう"

   Eight rounds of regional Japanese, one set per region: that region's own
   food, sights and festivals. Each round hides the meaning behind one
   Japanese prompt and offers four English answers.

   The question list is rendered by the view, so everything the game needs
   (question, answers, teaching note, photo) arrives in one JSON blob, plus
   a small meta blob (stamp, ranks, tips) for the end-of-run card.
   ========================================================================== */
(function () {
    'use strict';

    var Q = window.naQuizQuestions || [];
    var META = window.naQuizMeta || {};
    var root = document.querySelector('[data-na-quiz]');
    if (!root || !Q.length) return;

    var el = {
        round:   root.querySelector('[data-q-round]'),
        score:   root.querySelector('[data-q-score]'),
        streak:  root.querySelector('[data-q-streak]'),
        time:    root.querySelector('[data-q-time]'),
        dots:    root.querySelector('[data-q-dots]'),
        timebar: root.querySelector('[data-q-timebar]'),
        timefill:root.querySelector('[data-q-timefill]'),
        pic:     root.querySelector('[data-q-pic]'),
        kind:    root.querySelector('[data-q-kind]'),
        ja:      root.querySelector('[data-q-ja]'),
        reading: root.querySelector('[data-q-reading]'),
        romaji:  root.querySelector('[data-q-romaji]'),
        say:     root.querySelector('[data-q-say]'),
        options: root.querySelector('[data-q-options]'),
        feedback:root.querySelector('[data-q-feedback]'),
        start:   root.querySelector('[data-q-start]'),
        restart: root.querySelector('[data-q-restart]'),
        again:   root.querySelector('[data-q-again]'),
        over:    root.querySelector('[data-q-over]'),
        stamp:   root.querySelector('[data-q-stamp]'),
        rank:    root.querySelector('[data-q-rank]'),
        final:   root.querySelector('[data-q-final]'),
        tip:     root.querySelector('[data-q-tip]'),
        best:    root.querySelector('[data-q-best]'),
        scoreCell: root.querySelector('[data-q-score-cell]')
    };

    var TOTAL = Q.length;
    var LIMIT = 15;                       // seconds per question
    // One high score per region — the key comes from the view's meta blob.
    var BEST_KEY = 'na-quiz-best-' + (META.key || 'region');
    var KEYS = ['A', 'B', 'C', 'D', 'E'];

    var order = [];                    // permutation of the question list
    var idx = 0;
    var score = 0;
    var streak = 0;
    var clock = 0;
    var ticker = null;
    var locked = false;
    var running = false;

    /* ------------------------------------------------------------------ data */

    function shuffle(a) {
        for (var i = a.length - 1; i > 0; i--) {
            var j = Math.floor(Math.random() * (i + 1));
            var t = a[i]; a[i] = a[j]; a[j] = t;
        }
        return a;
    }

    function order1() { var r = []; for (var i = 0; i < TOTAL; i++) r.push(i); return shuffle(r); }

    function best() {
        var v = 0;
        try { v = parseInt(window.localStorage.getItem(BEST_KEY) || '0', 10) || 0; } catch (e) { v = 0; }
        return v;
    }

    function saveBest(v) { try { window.localStorage.setItem(BEST_KEY, String(v)); } catch (e) { /* private mode */ } }

    function esc(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function say(text, btn) {
        if (!window.speechSynthesis) return;
        window.speechSynthesis.cancel();
        var u = new SpeechSynthesisUtterance(text);
        u.lang = 'ja-JP';
        u.rate = 0.85;
        if (btn) { btn.classList.add('is-speaking'); }
        u.onend = function () { if (btn) btn.classList.remove('is-speaking'); };
        window.speechSynthesis.speak(u);
    }

    /* ------------------------------------------------------------- painting */

    function hud() {
        el.round.innerHTML = (idx + 1) + '<i>/' + TOTAL + '</i>';
        el.score.textContent = String(score);
        el.streak.textContent = String(streak);
        el.scoreCell.classList.toggle('is-hot', streak >= 3);
        var b = best();
        el.best.textContent = b ? 'Best ' + b : 'First run';
    }

    function buildDots() {
        el.dots.innerHTML = '';
        for (var i = 0; i < TOTAL; i++) {
            var d = document.createElement('span');
            d.className = 'na-quiz-dot';
            el.dots.appendChild(d);
        }
    }

    function markDot(cls) {
        var d = el.dots.children[idx];
        if (d) d.className = 'na-quiz-dot ' + cls;
    }

    function paintClock() {
        el.time.textContent = String(Math.max(clock, 0));
        var pct = Math.max(clock, 0) / LIMIT;
        el.timefill.style.transform = 'scaleX(' + pct + ')';
        el.timebar.classList.toggle('is-low', pct <= 0.34);
    }

    /* ------------------------------------------------------------- question */

    function renderQuestion() {
        var q = Q[order[idx]];
        locked = false;

        el.kind.textContent = q.kind;
        el.ja.textContent = q.ja;
        el.reading.textContent = q.reading;
        el.romaji.textContent = q.romaji;

        el.pic.innerHTML = q.pic
            ? '<img src="' + esc(q.pic) + '" alt="" loading="lazy" decoding="async" />'
            : '<i class="bi bi-emoji-smile"></i>';

        el.say.onclick = function () { say(q.say, el.say); };

        el.feedback.className = 'na-quiz-feedback is-idle';
        el.feedback.textContent = running
            ? 'Which of these is 「' + q.ja + '」? The timer is running.'
            : 'Press Start to play — the clock only runs once the game begins.';

        var pool = q.opts.map(function (t, i) { return { text: t, ok: i === q.answer }; });
        shuffle(pool);

        el.options.innerHTML = '';
        pool.forEach(function (o, i) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'na-quiz-opt';
            b.disabled = !running;
            b.innerHTML = '<span class="na-quiz-opt-key">' + KEYS[i] + '</span>' +
                          '<span class="na-quiz-opt-text"></span>';
            b.querySelector('.na-quiz-opt-text').textContent = o.text;
            b.addEventListener('click', function () { answer(o, b); });
            el.options.appendChild(b);
        });

        hud();
        startClock();
    }

    function startClock() {
        clock = LIMIT;
        paintClock();
        clearInterval(ticker);
        // The preview round shown before the first Start must not burn the
        // clock down, and it must never auto-answer on the player's behalf.
        if (!running) { clock = LIMIT; paintClock(); return; }
        ticker = setInterval(function () {
            clock -= 1;
            if (clock <= 0) { paintClock(); clearInterval(ticker); answer(null, null); return; }
            paintClock();
        }, 1000);
    }

    /* -------------------------------------------------------------- answer */

    function answer(pick, btn) {
        if (locked || !running) return;
        locked = true;
        clearInterval(ticker);

        var q = Q[order[idx]];
        var buttons = el.options.querySelectorAll('.na-quiz-opt');
        var texts = [];
        for (var i = 0; i < buttons.length; i++) {
            var b = buttons[i];
            b.disabled = true;
            var label = b.querySelector('.na-quiz-opt-text').textContent;
            texts.push(label);
            if (label === q.opts[q.answer]) b.classList.add('is-right');
            else if (pick && label === pick.text) b.classList.add('is-wrong');
        }

        var correct = !!pick && pick.ok;
        var truth = q.opts[q.answer];

        if (correct) {
            streak += 1;
            score += 100 + (streak > 1 ? 40 * Math.min(streak, 5) : 0);
            el.feedback.className = 'na-quiz-feedback is-right';
            el.feedback.innerHTML = '✓ <b>' + esc(q.ja) + '</b> — ' + esc(truth) + '. ' + esc(q.note);
            markDot('is-right');
        } else {
            streak = 0;
            el.feedback.className = 'na-quiz-feedback is-wrong';
            el.feedback.innerHTML = '✗ Not quite — <b>' + esc(q.ja) + '</b> ('
                + esc(q.reading) + ') means <b>' + esc(truth) + '</b>. ' + esc(q.note);
            markDot('is-wrong');
        }

        hud();
        el.start.disabled = false;
        el.start.innerHTML = '<i class="bi bi-arrow-right"></i> Next';
        el.start.setAttribute('data-q-next', '1');
        el.say.onclick = function () { say(q.say, el.say); };
    }

    /* ---------------------------------------------------------------- flow */

    function next() {
        idx += 1;
        if (idx >= TOTAL) { finish(); return; }
        // safety net: never advance into a round the player cannot answer
        running = true;
        renderQuestion();
    }

    function begin() {
        idx = 0;
        score = 0;
        streak = 0;
        clock = LIMIT;
        running = true;
        el.over.hidden = true;
        el.say.classList.remove('is-speaking');
        buildDots();
        el.start.disabled = false;
        el.start.removeAttribute('data-q-next');
        el.start.innerHTML = '<i class="bi bi-play-fill"></i> Start';
        el.restart.hidden = true;
        order = order1();                  // every game is a different run
        renderQuestion();
    }

    /* Per-region end-of-run card: a stamp, four rank names and four tips, all
       authored alongside the questions in Services/RegionQuizzes.cs. */
    function grade() {
        var p = score / Math.max(TOTAL * 100, 1);
        var stamps = META.stamp || '遊';
        var ranks = META.ranks || ['完走', 'なかなか', '見習い', 'beginner'];
        var tips = META.tips || [
            'A perfect run. Press play again and see if you can beat it.',
            'Almost there. Read the note on the rounds you missed.',
            'Good start. The kanji stick after two rounds.',
            'No matter — the words stick after two rounds. Press play again.'
        ];

        if (p >= 1) return { rank: ranks[0], tip: tips[0], stamp: stamps, bad: false };
        if (p >= 0.85) return { rank: ranks[1], tip: tips[1], stamp: stamps, bad: false };
        if (p >= 0.5) return { rank: ranks[2], tip: tips[2], stamp: stamps, bad: false };
        return { rank: ranks[3], tip: tips[3], stamp: stamps, bad: true };
    }

    function finish() {
        clearInterval(ticker);
        running = false;
        var g = grade();
        el.stamp.textContent = g.stamp;
        el.stamp.classList.toggle('is-bad', g.bad);
        el.rank.textContent = g.rank;
        el.final.innerHTML = score + '<small>SCORE</small>';
        el.tip.textContent = g.tip;
        el.over.hidden = false;

        if (score > best()) saveBest(score);
        el.scoreCell.classList.remove('is-hot');

        el.start.disabled = true;
        el.start.innerHTML = '<i class="bi bi-play-fill"></i> Start';
        el.restart.hidden = false;
        try { el.again.focus(); } catch (e) { /* nothing */ }
    }

    /* -------------------------------------------------------------- wiring */

    el.start.addEventListener('click', function () {
        if (el.start.hasAttribute('data-q-next')) { next(); return; }
        begin();
    });
    el.restart.addEventListener('click', begin);
    el.again.addEventListener('click', begin);

    el.over.addEventListener('click', function (e) {
        if (e.target === el.over) el.over.hidden = true;
    });

    /* ------------------------------------------------------------- keyboard
       A–D answer the round, Enter starts it / moves on, R plays again. These
       are registered with the shared engine so that the "?" cheat sheet,
       the stage keys and the viewer all agree on who owns which key — and so
       the game falls back to its own listener if the engine is missing. */
    /* the photo stage asks this flag before it claims a key, so while the game
       owns the screen the arrows and Enter belong to the game alone */
    var onScreen = true;
    window.naRegionQuizOnScreen = onScreen;
    if (window.IntersectionObserver) {
        var qio = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) {
                onScreen = en.isIntersecting;
                window.naRegionQuizOnScreen = onScreen;
            });
        }, { threshold: .2 });
        qio.observe(root);
    }

    function wireKeys(NA) {
        function live() { return onScreen; }

        if (NA) {
            NA.register({
                keys: ['a', 'b', 'c', 'd'],
                group: "Let's play — 遊び",
                label: 'Answer A–D / 答える',
                when: live,
                run: function (e) {
                    var i = ['a', 'b', 'c', 'd'].indexOf(String(e.key).toLowerCase());
                    if (i < 0) return false;
                    var b = el.options.querySelectorAll('.na-quiz-opt')[i];
                    if (!b || b.disabled) return false;
                    b.click();
                    return true;
                }
            });
            NA.register({
                keys: ['enter'],
                group: "Let's play — 遊び",
                label: 'Start the run / go to the next round',
                when: live,
                run: function () {
                    if (el.start.disabled) return false;
                    el.start.click();
                    return true;
                }
            });
            NA.register({
                keys: ['r'],
                group: "Let's play — 遊び",
                label: 'Play again / もう一遍',
                when: function () { return onScreen && !el.over.hidden; },
                run: function () { begin(); NA.toast('Play again', 'R'); return true; }
            });
        }

        /* fallback when the shared engine is not on the page */
        root.addEventListener('keydown', function (e) {
            if (!NA) {
                if (!running) return;
                var i = ['a', 'b', 'c', 'd'].indexOf(String(e.key).toLowerCase());
                if (i > -1) {
                    var b = el.options.querySelectorAll('.na-quiz-opt')[i];
                    if (b && !b.disabled) { b.click(); e.preventDefault(); }
                }
                if (e.key === 'Enter' && !el.start.disabled) { el.start.click(); e.preventDefault(); }
            }
        });
    }

    wireKeys(window.naKeys);

    // warm-up: show the first question so the block never looks empty
    order = order1();
    buildDots();
    hud();
    renderQuestion();
    el.options.querySelectorAll('.na-quiz-opt').forEach(function (b) { b.disabled = true; });
})();
