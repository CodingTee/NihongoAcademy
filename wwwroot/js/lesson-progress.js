// ===== Lesson Scroll Progress Tracking =====
// Tracks how far the user has scrolled through lesson content
// and periodically saves progress to the server.

var _lessonId = 0;
var _currentProgress = 0;
var _lastSavedProgress = 0;
var _isCompleted = false;
var _saveTimer = null;

function initLessonProgress(lessonId, initialProgress, isCompleted) {
    _lessonId = lessonId;
    _currentProgress = initialProgress;
    _lastSavedProgress = initialProgress;
    _isCompleted = isCompleted;

    if (_isCompleted) return; // already done, no need to track

    // Listen to scroll events (throttled)
    var scrollTimeout;
    window.addEventListener('scroll', function () {
        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(_onScroll, 200);
    });

    // Periodically save progress (every 8 seconds)
    _saveTimer = setInterval(function () {
        if (!_isCompleted && _currentProgress > _lastSavedProgress) {
            _saveProgress(_currentProgress);
        }
    }, 8000);

    // Save on page leave
    window.addEventListener('beforeunload', function () {
        if (!_isCompleted && _currentProgress > _lastSavedProgress) {
            var params = 'lessonId=' + _lessonId + '&percentage=' + _currentProgress;
            var token = getAntiForgeryToken();
            if (token) params += '&__RequestVerificationToken=' + encodeURIComponent(token);
            navigator.sendBeacon('/Lessons/UpdateProgress', new Blob([params], { type: 'application/x-www-form-urlencoded' }));
            _notifyProgressChanged();
        }
    });

    // Initial scroll check (in case content is short)
    setTimeout(_onScroll, 500);
}

function _calcScrollProgress() {
    var contentEl = document.querySelector('.content-html');
    if (!contentEl) return _currentProgress;

    var rect = contentEl.getBoundingClientRect();
    var contentHeight = contentEl.scrollHeight;
    var viewportHeight = window.innerHeight;

    // How far the user has scrolled past the content
    var scrolledPast = viewportHeight - rect.top;
    if (scrolledPast < 0) scrolledPast = 0;

    // Progress = how much of the content they've seen
    var scrollable = contentHeight - viewportHeight;
    if (scrollable <= 0) return 100; // content fits in one screen

    // Reading is evidence of exposure, not mastery. It contributes at most 70%.
    var pct = Math.min(70, Math.round((scrolledPast / scrollable) * 70));
    return Math.max(_currentProgress, pct); // never go backwards
}

function _updateProgressUI(pct) {
    var bar = document.getElementById('progress-bar-fill');
    var label = document.querySelector('#complete-btn')
        ? document.querySelector('#complete-btn').parentElement.querySelector('.text-muted')
        : null;
    if (bar) bar.style.width = pct + '%';
    if (label) label.textContent = pct + '%';
}

function _saveProgress(pct) {
    if (_isCompleted || pct <= _lastSavedProgress) return;

    fetch('/Lessons/UpdateProgress', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: 'lessonId=' + _lessonId + '&percentage=' + pct
    })
    .then(function (r) { return r.json(); })
    .then(function (data) {
        if (data.success) {
            _lastSavedProgress = pct;
            _currentProgress = pct;
            _notifyProgressChanged();
        }
    })
    .catch(function () { /* silent */ });
}

function _onScroll() {
    if (_isCompleted) return;
    var pct = _calcScrollProgress();
    if (pct > _currentProgress) {
        _currentProgress = pct;
        _updateProgressUI(pct);
    }

    // Completion is awarded by demonstrated practice mastery, not scrolling.
}

function _notifyProgressChanged() {
    try {
        localStorage.setItem('na-progress-updated', Date.now().toString());
        localStorage.removeItem('na-progress-updated');
    } catch (e) { /* silent */ }
}
