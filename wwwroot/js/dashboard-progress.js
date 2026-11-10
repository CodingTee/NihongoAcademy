(function () {
    var data = window.naDashboardProgress || {};

    /* ===== Study Rhythm Calendar ===== */
    function initCalendar() {
        var grid = document.getElementById("naCalGrid");
        var monthEl = document.getElementById("naCalMonth");
        if (!grid || !monthEl) return;
        var months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var days = {};
        (data.activeDays || []).forEach(function (day) { days[day] = true; });
        function iso(date) {
            return date.getFullYear() + "-" + String(date.getMonth() + 1).padStart(2, "0") + "-" + String(date.getDate()).padStart(2, "0");
        }
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        var todayKey = iso(today);
        var view = new Date(today.getFullYear(), today.getMonth(), 1);
        function render() {
            var year = view.getFullYear();
            var month = view.getMonth();
            var first = new Date(year, month, 1);
            var lead = first.getDay();
            var length = new Date(year, month + 1, 0).getDate();
            var studied = 0;
            var html = "";
            for (var i = 0; i < lead; i++) html += '<div class="na-day na-day--empty"></div>';
            for (var day = 1; day <= length; day++) {
                var key = iso(new Date(year, month, day));
                var on = days[key] === true;
                if (on) studied++;
                html += '<div class="na-day' + (on ? ' na-day--on' : '') + (key === todayKey ? ' na-day--today' : '') + '"><span class="na-day-num">' + day + '</span></div>';
            }
            var tail = (7 - ((lead + length) % 7)) % 7;
            for (var t = 0; t < tail; t++) html += '<div class="na-day na-day--out"></div>';
            monthEl.textContent = months[month] + " " + year + (studied ? " · " + studied + " studied" : "");
            grid.innerHTML = html;
        }
        document.getElementById("naCalPrev")?.addEventListener("click", function () { view.setMonth(view.getMonth() - 1); render(); });
        document.getElementById("naCalNext")?.addEventListener("click", function () { view.setMonth(view.getMonth() + 1); render(); });
        render();
    }

    /* ===== Quiz Trend Bars (replaces chart.js line chart) ===== */
    function initScoreBars() {
        var wrap = document.getElementById("scoreBars");
        if (!wrap) return;
        var labels = data.quizLabels && data.quizLabels.length ? data.quizLabels : ["No data"];
        var scores = data.quizScores && data.quizScores.length ? data.quizScores : [0];
        var max = Math.max.apply(null, scores.concat([100]));
        var html = '<div class="na-bars-row">';
        for (var i = 0; i < scores.length; i++) {
            var pct = max ? Math.round((scores[i] / max) * 100) : 0;
            html += '<div class="na-bar-col" title="' + labels[i] + ': ' + scores[i] + '%">' +
                '<div class="na-bar-track"><div class="na-bar-fill" style="height:' + pct + '%"></div></div>' +
                '<span class="na-bar-cap">' + scores[i] + '%</span></div>';
        }
        html += '</div>';
        wrap.innerHTML = html;
    }

    /* ===== 5-Skill Bars (replaces chart.js radar chart) ===== */
    function initRadarBars() {
        var wrap = document.getElementById("radarBars");
        if (!wrap) return;
        var labels = data.radarLabels || ["Vocabulary", "Grammar", "Listening", "Reading", "Writing"];
        var values = data.radarValues || [20, 20, 15, 15, 15];
        var html = "";
        for (var i = 0; i < labels.length; i++) {
            var v = values[i] || 0;
            html += '<div class="na-skill-row"><span class="na-skill-name">' + labels[i] + '</span>' +
                '<div class="na-progress"><div class="na-progress-fill" style="width:' + v + '%"></div></div>' +
                '<span class="na-skill-pct">' + v + '%</span></div>';
        }
        wrap.innerHTML = html;
    }

    /* ===== Animated Count-Up for Metric Cards ===== */
    function initCountUp() {
        var metrics = document.querySelectorAll("[data-count]");
        if (!metrics.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var el = entry.target;
                if (el.dataset.counted) return;
                el.dataset.counted = "1";

                var target = parseInt(el.dataset.count, 10);
                var unit = el.querySelector(".na-metric-unit");
                var unitText = unit ? unit.textContent : "";
                var duration = 1000;
                var start = performance.now();

                function tick(now) {
                    var elapsed = now - start;
                    var progress = Math.min(elapsed / duration, 1);
                    var eased = 1 - Math.pow(1 - progress, 4);
                    var current = Math.round(eased * target);
                    el.textContent = current;
                    if (unitText) {
                        var span = document.createElement("span");
                        span.className = "na-metric-unit";
                        span.textContent = unitText;
                        el.appendChild(span);
                    }
                    if (progress < 1) requestAnimationFrame(tick);
                }
                requestAnimationFrame(tick);
                observer.unobserve(el);
            });
        }, { threshold: 0.3 });

        metrics.forEach(function (el) { observer.observe(el); });
    }

    /* ===== JLPT Progress Bar Animation ===== */
    function initJlptBars() {
        var bars = document.querySelectorAll(".na-progress-animated");
        if (!bars.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var bar = entry.target;
                if (bar.dataset.animated) return;
                bar.dataset.animated = "1";
                var width = bar.dataset.width;
                setTimeout(function () {
                    bar.style.width = width;
                }, 200);
                observer.unobserve(bar);
            });
        }, { threshold: 0.3 });

        bars.forEach(function (bar) { observer.observe(bar); });
    }

    /* ===== Metric Card Entrance Animation ===== */
    function initMetricEntrance() {
        var cards = document.querySelectorAll(".na-metric-animated");
        if (!cards.length) return;

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("na-metric-visible");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });

        cards.forEach(function (card, i) {
            card.style.transitionDelay = (i * 100) + "ms";
            observer.observe(card);
        });
    }

    /* ===== Init All ===== */
    document.addEventListener("DOMContentLoaded", function () {
        initCalendar();
        initScoreBars();
        initRadarBars();
        initCountUp();
        initJlptBars();
        initMetricEntrance();
    });
})();
