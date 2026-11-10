// ===== NihongoAcademy JavaScript =====

document.addEventListener("DOMContentLoaded", function () {
    initSakuraPetals();
    initHeroCarousel();
});

// ===== Hero carousel (interactive dots) =====
function initHeroCarousel() {
    var carousel = document.querySelector("[data-hero-carousel]");
    if (!carousel) return;

    var slides = carousel.querySelectorAll(".na-hero-v2-slide");
    var dots = carousel.querySelectorAll(".na-hero-v2-dot");
    var track = carousel.querySelector(".na-hero-v2-slides");
    if (slides.length <= 1 || slides.length !== dots.length || !track) return;

    var current = 0;
    var interval = null;
    var delay = 5000;

    function show(index) {
        if (index < 0) index = slides.length - 1;
        if (index >= slides.length) index = 0;
        current = index;

        // Slide the track horizontally for a real carousel feel.
        track.style.transform = "translateX(-" + (current * 100) + "%)";

        // Let CSS know which slide is active (e.g. hide background on slides 2/3).
        carousel.setAttribute("data-active-slide", current.toString());

        slides.forEach(function (slide, i) {
            slide.classList.toggle("is-active", i === current);
        });
        dots.forEach(function (dot, i) {
            dot.classList.toggle("is-active", i === current);
        });
    }

    // Ensure initial state is explicit.
    show(0);

    function next() { show(current + 1); }
    function prev() { show(current - 1); }

    function start() {
        stop();
        interval = setInterval(next, delay);
    }

    function stop() {
        if (interval) {
            clearInterval(interval);
            interval = null;
        }
    }

    dots.forEach(function (dot, i) {
        dot.addEventListener("click", function () {
            show(i);
            start();
        });
    });

    // Touch / drag swipe so it feels like a real sliding carousel.
    var startX = 0, deltaX = 0, dragging = false;
    carousel.addEventListener("touchstart", function (e) {
        startX = e.touches[0].clientX;
        dragging = true;
        stop();
    }, { passive: true });
    carousel.addEventListener("touchmove", function (e) {
        if (!dragging) return;
        deltaX = e.touches[0].clientX - startX;
        track.style.transition = "none";
        track.style.transform = "translateX(calc(-" + (current * 100) + "% + " + deltaX + "px))";
    }, { passive: true });
    carousel.addEventListener("touchend", function () {
        if (!dragging) return;
        dragging = false;
        track.style.transition = "";
        if (Math.abs(deltaX) > 50) {
            if (deltaX < 0) show(current + 1);
            else show(current - 1);
        } else {
            show(current);
        }
        deltaX = 0;
        start();
    });

    // Auto-play continuously so the hero is always moving.
    start();
}

// ===== Japanese pronunciation (Web Speech API) =====
var jaVoice = null;

function pickJapaneseVoice() {
    if (!("speechSynthesis" in window)) return;
    var voices = window.speechSynthesis.getVoices();
    var japaneseVoices = voices.filter(function (v) { return v.lang && v.lang.toLowerCase().indexOf("ja") === 0; });
    japaneseVoices.sort(function (a, b) {
        function score(voice) {
            var value = voice.lang.toLowerCase() === "ja-jp" ? 4 : 0;
            if (voice.localService) value += 2;
            if (/natural|nanami|haruka|sayaka|ayumi|kyoko|otoya/i.test(voice.name)) value += 3;
            return value;
        }
        return score(b) - score(a);
    });
    jaVoice = japaneseVoices[0] || null;
}

if ("speechSynthesis" in window) {
    pickJapaneseVoice();
    window.speechSynthesis.addEventListener("voiceschanged", pickJapaneseVoice);
}

// Keep a reference to the active utterance; Chrome garbage-collects unreferenced
// utterances mid-speech, which kills the audio and never fires onend.
var jaUtterance = null;
var jaFinish = null; // settles the most recent speakJapanese promise

function speakJapanese(text, options) {
    if (!text) return Promise.resolve(false);

    if (!("speechSynthesis" in window)) {
        var unsupportedMessage = document.getElementById("na-speech-status");
        if (unsupportedMessage) unsupportedMessage.textContent = "Japanese audio is unavailable in this browser.";
        return Promise.resolve(false);
    }

    options = options || {};
    var synth = window.speechSynthesis;

    // A new call supersedes whatever is queued, so its promise cannot hang.
    if (jaFinish) { jaFinish(false); jaFinish = null; }
    synth.cancel();

    var utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = "ja-JP";
    if (jaVoice) utterance.voice = jaVoice;
    utterance.rate = Math.max(0.5, Math.min(1.2, Number(options.rate) || 0.82));
    utterance.pitch = 1;
    utterance.volume = 1;
    return new Promise(function (resolve) {
        var settled = false;
        var finish = function (ok) {
            if (settled) return;
            settled = true;
            if (jaFinish === finish) jaFinish = null;
            resolve(ok);
        };
        utterance.onend = function () { finish(true); };
        utterance.onerror = function () { finish(false); };
        jaFinish = finish;
        jaUtterance = utterance;
        // Chrome drops utterances queued in the same tick as cancel(); a short
        // delay keeps click-to-speak reliable.
        setTimeout(function () {
            if (settled) return;
            if (synth.paused) synth.resume();
            synth.speak(utterance);
        }, 60);
    });
}
window.speakJapanese = speakJapanese;

// Speaker buttons: <button class="na-speak-btn" data-speak="こんにちは">
// Kana chart cells: click any .kana-cell to hear its syllable
document.addEventListener("click", function (e) {
    var btn = e.target.closest(".na-speak-btn");
    if (btn) {
        e.preventDefault();
        e.stopPropagation();
        btn.classList.add("is-speaking");
        btn.setAttribute("aria-busy", "true");
        speakJapanese(btn.getAttribute("data-speak"), { rate: btn.getAttribute("data-rate") })
            .catch(function () {
                var message = document.getElementById("na-speech-status");
                if (message) message.textContent = "Japanese audio is unavailable in this browser.";
            })
            .finally(function () {
                btn.classList.remove("is-speaking");
                btn.removeAttribute("aria-busy");
            });
        return;
    }

    var cell = e.target.closest(".kana-cell");
    if (cell) {
        var romaji = cell.querySelector(".kana-romaji");
        var text = cell.textContent;
        if (romaji) text = text.replace(romaji.textContent, "");
        speakJapanese(text.trim()).catch(function () { });
    }
});

// ===== Sakura falling petals decoration =====
function initSakuraPetals() {
    var container = document.querySelector(".sakura-decoration");
    if (!container) return;

    for (var i = 0; i < 24; i++) {
        var petal = document.createElement("div");
        petal.className = "sakura-petal";
        // Cycle through the four petal images so the falling petals vary.
        var petalImg = 1 + Math.floor(Math.random() * 4);
        petal.style.backgroundImage = "url('/images/petal" + petalImg + ".png')";

        // Random size (34-60px) for a natural mix - no two petals look the same.
        var size = Math.floor(Math.random() * 26 + 34);
        petal.style.setProperty("--size", size + "px");

        // Keep the petal fully inside the viewport (allow for its own size):
        // a petal at the very right edge gets clipped by overflow:hidden and
        // looks stuck half-cut, so clamp the left edge with its width.
        var maxX = Math.max(0, window.innerWidth - (size + 12));
        petal.style.left = Math.floor(Math.random() * maxX) + "px";

        // Per-petal motion variety so they don't sway/fall in lockstep:
        //  - random horizontal drift amount and direction (left or right)
        //  - random spin direction (clockwise / counter-clockwise)
        //  - random fall duration and a NEGATIVE delay so they are already
        //    scattered mid-air when the page loads (no synchronized start).
        var swayAmp = Math.floor(Math.random() * 60 + 20);
        var swayDir = Math.random() < 0.5 ? 1 : -1;
        petal.style.setProperty("--sway", (swayAmp * swayDir) + "px");
        petal.style.setProperty("--spin", Math.random() < 0.5 ? "1" : "-1");
        petal.style.setProperty("--dur", (Math.random() * 7 + 9) + "s");   // 9-16s
        petal.style.animationDelay = "-" + (Math.random() * 14) + "s";      // already mid-fall
        petal.style.opacity = Math.random() * 0.35 + 0.4;
        container.appendChild(petal);
    }
}

// ===== Flashcard logic =====
var flashcardData = [];
var currentCardIndex = 0;
var knownCount = 0;
var unknownCount = 0;

function initFlashcards(data) {
    flashcardData = data;
    currentCardIndex = 0;
    knownCount = 0;
    unknownCount = 0;
    showCard(0);
    updateCounter();
}

function showCard(index) {
    currentCardIndex = index;
    var card = document.querySelector(".flashcard");
    if (!card) return;

    card.classList.remove("flipped");

    var front = document.querySelector(".flashcard-front");
    var back = document.querySelector(".flashcard-back");
    var data = flashcardData[index];

    if (front && data) {
        front.querySelector(".flashcard-jp").textContent = data.japanese;
        front.querySelector(".flashcard-romaji").textContent = data.romaji;
        front.querySelector(".flashcard-type").textContent = data.partOfSpeech || "";
    }

    if (back && data) {
        back.querySelector(".flashcard-english").textContent = data.english;
        back.querySelector(".flashcard-type").textContent = data.partOfSpeech || "";
        var example = back.querySelector(".flashcard-example");
        if (example) {
            example.textContent = data.exampleJapanese
                ? data.exampleJapanese + (data.exampleReading && data.exampleReading !== data.exampleJapanese ? "（" + data.exampleReading + "）" : "") + " — " + (data.exampleEnglish || "")
                : "";
        }
    }

    var counter = document.getElementById("card-counter");
    if (counter) {
        counter.textContent = (index + 1) + " / " + flashcardData.length;
    }

    // Reset SRS interval display
    var intervalEl = document.getElementById("srs-interval");
    if (intervalEl) intervalEl.textContent = "—";

    var prevBtn = document.getElementById("prev-card");
    var nextBtn = document.getElementById("next-card");
    if (prevBtn) prevBtn.disabled = index === 0;
    if (nextBtn) nextBtn.disabled = index === flashcardData.length - 1;
}

function flipCard() {
    var card = document.querySelector(".flashcard");
    if (card) card.classList.toggle("flipped");
}

function speakFlashcardFront() {
    var data = flashcardData[currentCardIndex];
    if (data) speakJapanese(data.japanese);
}

function speakFlashcardExample() {
    var data = flashcardData[currentCardIndex];
    if (data && data.exampleJapanese) speakJapanese(data.exampleJapanese);
}

function nextCard() {
    if (currentCardIndex < flashcardData.length - 1) {
        showCard(currentCardIndex + 1);
    }
}

function prevCard() {
    if (currentCardIndex > 0) {
        showCard(currentCardIndex - 1);
    }
}

function markSRS(quality) {
    if (currentCardIndex >= flashcardData.length) return;
    var card = flashcardData[currentCardIndex];

    // Quality 1 = Again (unknown), 2+ = known to varying degrees
    if (quality < 2) {
        unknownCount++;
    } else {
        knownCount++;
    }
    updateCounter();

    // Send SRS review to server
    fetch("/Flashcards/RecordReview", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: "vocabularyId=" + card.id + "&quality=" + quality
    })
    .then(function (response) {
        if (!response.ok) throw new Error("HTTP " + response.status);
        return response.json();
    })
    .then(function (data) {
        if (data.success) {
            var intervalEl = document.getElementById("srs-interval");
            if (intervalEl) {
                if (data.interval === 0) intervalEl.textContent = "10 min";
                else if (data.interval === 1) intervalEl.textContent = "1 day";
                else intervalEl.textContent = data.interval + " days";
            }
        }
    })
    .catch(function() { /* silently fail — SRS is enhancement, not critical */ });

    // Notify other tabs that progress changed
    try {
        localStorage.setItem("na-progress-updated", Date.now().toString());
        localStorage.removeItem("na-progress-updated");
    } catch (e) { /* silent */ }

    // Anki-style: re-queue "Again" cards to the end of the current session so the
    // learner sees them again before finishing. Non-Again ratings are done for good.
    if (quality === 1) {
        flashcardData.push(card);
    }

    if (currentCardIndex < flashcardData.length - 1) {
        nextCard();
    } else {
        showResults();
    }
}

// Legacy support — maps to SRS quality levels
function markKnown() { markSRS(3); }
function markUnknown() { markSRS(1); }

function updateCounter() {
    var known = document.getElementById("known-count");
    var unknown = document.getElementById("unknown-count");
    if (known) known.textContent = knownCount;
    if (unknown) unknown.textContent = unknownCount;
}

function showResults() {
    var total = flashcardData.length;
    var pct = total > 0 ? Math.round((knownCount / total) * 100) : 0;

    var resultsDiv = document.getElementById("flashcard-results");
    var studyDiv = document.getElementById("flashcard-study");

    if (resultsDiv && studyDiv) {
        studyDiv.style.display = "none";
        resultsDiv.style.display = "block";
        resultsDiv.innerHTML =
            '<div class="na-card text-center">' +
            '<h3>Session Complete!</h3>' +
            '<p class="text-muted">All done!</p>' +
            '<div class="row mt-4">' +
            '<div class="col-4"><div class="na-stat"><div class="stat-number text-success">' + knownCount + '</div><div class="stat-label">Known</div></div></div>' +
            '<div class="col-4"><div class="na-stat"><div class="stat-number text-danger">' + unknownCount + '</div><div class="stat-label">Review</div></div></div>' +
            '<div class="col-4"><div class="na-stat"><div class="stat-number">' + pct + '%</div><div class="stat-label">Score</div></div></div>' +
            '</div>' +
            '<button class="btn-na-primary mt-4" onclick="restartFlashcards()">Study Again</button>' +
            '</div>';
    }
}

function restartFlashcards() {
    var resultsDiv = document.getElementById("flashcard-results");
    var studyDiv = document.getElementById("flashcard-study");
    if (resultsDiv) resultsDiv.style.display = "none";
    if (studyDiv) studyDiv.style.display = "block";
    knownCount = 0;
    unknownCount = 0;
    showCard(0);
    updateCounter();
}

// ===== Quiz logic =====
var quizData = [];
var quizAnswers = [];
var currentQuestion = 0;
var quizStartTime = 0;
var quizTimerInterval = null;
var quizTimeLimit = 0;

function initQuiz(questions, timeLimitMinutes) {
    quizData = questions;
    quizTimeLimit = timeLimitMinutes * 60;
    quizAnswers = new Array(questions.length).fill(0);
    currentQuestion = 0;
    quizStartTime = Date.now();

    // A quiz without a time limit should not run a countdown that is already expired.
    if (quizTimeLimit > 0) {
        startQuizTimer();
    }
    showQuestion(0);
    updateQuizProgress();
}

function startQuizTimer() {
    if (quizTimerInterval) clearInterval(quizTimerInterval);

    quizTimerInterval = setInterval(function () {
        var elapsed = Math.floor((Date.now() - quizStartTime) / 1000);
        var remaining = quizTimeLimit - elapsed;

        if (remaining <= 0) {
            clearInterval(quizTimerInterval);
            // Time is up, so there is nothing left to confirm: asking would restart
            // the timer and then ask again every second, locking the page.
            submitQuiz({ auto: true });
            return;
        }

        var minutes = Math.floor(remaining / 60);
        var seconds = remaining % 60;
        var display = minutes + ":" + (seconds < 10 ? "0" : "") + seconds;

        var timerEl = document.getElementById("quiz-timer");
        if (timerEl) {
            timerEl.textContent = display;
            if (remaining < 60) {
                timerEl.classList.add("warning");
            }
        }

        var hiddenTime = document.getElementById("time-taken");
        if (hiddenTime) {
            hiddenTime.value = elapsed;
        }
    }, 1000);
}

function showQuestion(index) {
    currentQuestion = index;

    document.querySelectorAll(".quiz-question-card").forEach(function (card) {
        card.classList.remove("active");
    });

    var card = document.querySelector('.quiz-question-card[data-index="' + index + '"]');
    if (card) card.classList.add("active");

    var q = quizData[index];
    if (!q) return;

    var options = document.querySelectorAll('.quiz-question-card[data-index="' + index + '"] .quiz-option');
    options.forEach(function (opt, i) {
        opt.classList.remove("selected", "correct", "incorrect");
        if (quizAnswers[index] === (i + 1)) {
            opt.classList.add("selected");
        }
    });

    updateQuizProgress();
    updateQuizNavButtons();
}

function selectAnswer(questionIndex, optionNumber) {
    quizAnswers[questionIndex] = optionNumber;

    var options = document.querySelectorAll('.quiz-question-card[data-index="' + questionIndex + '"] .quiz-option');
    options.forEach(function (opt, i) {
        opt.classList.remove("selected");
        if ((i + 1) === optionNumber) {
            opt.classList.add("selected");
        }
    });

    updateQuizProgress();
}

function updateQuizProgress() {
    var answered = quizAnswers.filter(function (a) { return a > 0; }).length;
    var total = quizData.length;
    var pct = total > 0 ? (answered / total) * 100 : 0;

    var fill = document.querySelector(".quiz-progress-fill");
    if (fill) fill.style.width = pct + "%";

    var counter = document.getElementById("quiz-progress-text");
    if (counter) counter.textContent = answered + " / " + total + " answered";
}

function updateQuizNavButtons() {
    var prevBtn = document.getElementById("prev-question");
    var nextBtn = document.getElementById("next-question");
    var submitBtn = document.getElementById("submit-quiz");

    if (prevBtn) prevBtn.style.display = currentQuestion === 0 ? "none" : "inline-block";

    if (currentQuestion === quizData.length - 1) {
        if (nextBtn) nextBtn.style.display = "none";
        if (submitBtn) submitBtn.style.display = "inline-block";
    } else {
        if (nextBtn) nextBtn.style.display = "inline-block";
        if (submitBtn) submitBtn.style.display = "none";
    }
}

function nextQuestion() {
    if (currentQuestion < quizData.length - 1) {
        showQuestion(currentQuestion + 1);
    }
}

function prevQuestion() {
    if (currentQuestion > 0) {
        showQuestion(currentQuestion - 1);
    }
}

var quizSubmissionStarted = false;

function submitQuiz(options) {
    if (quizSubmissionStarted) return;
    if (quizTimerInterval) clearInterval(quizTimerInterval);

    options = options || {};
    var unanswered = quizAnswers.filter(function (a) { return a === 0; }).length;
    // Confirming is only meaningful when the learner submits deliberately. An expired
    // timer must never open confirm(), because cancelling it would restart the timer
    // with zero seconds left and re-enter this branch forever.
    if (!options.auto && unanswered > 0) {
        if (!confirm(unanswered + " question(s) unanswered. Submit anyway?")) {
            startQuizTimer();
            return;
        }
    }

    var form = document.getElementById("quiz-form");
    if (!form) return;

    quizSubmissionStarted = true;
    var submitButton = document.getElementById("submit-quiz");
    if (submitButton) {
        submitButton.disabled = true;
        submitButton.innerHTML = '<i class="bi bi-hourglass-split"></i> Submitting…';
    }

    var answersInput = document.getElementById("answers-data");
    if (answersInput) {
        answersInput.value = quizAnswers.join(",");
    }

    var elapsed = Math.floor((Date.now() - quizStartTime) / 1000);
    var timeInput = document.getElementById("time-taken");
    if (timeInput) timeInput.value = elapsed;

    form.submit();
}

// ===== Lesson progress =====
function getAntiForgeryToken() {
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    // Fallback to the global hidden form in _Layout.cshtml
    if (!token) token = document.querySelector('#na-global-antiforgery input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

function markLessonComplete(lessonId) {
    if (!confirm("Mark this lesson as complete?")) return;

    fetch("/Lessons/MarkComplete", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            "RequestVerificationToken": getAntiForgeryToken()
        },
        body: "lessonId=" + lessonId
    })
    .then(function (response) {
        if (!response.ok) throw new Error("HTTP " + response.status);
        return response.json();
    })
    .then(function (data) {
        // Mastery lessons reject this shortcut with requiresMastery, so surface the
        // server's own message instead of failing silently.
        if (!data.success) {
            alert(data.message || "This lesson has to be finished through its mastery check.");
            return;
        }

        var btn = document.getElementById("complete-btn");
            if (btn) {
                btn.innerHTML = '<i class="bi bi-check-circle-fill"></i> Completed';
                btn.classList.remove("btn-na-primary");
                btn.classList.add("btn-na-outline");
                btn.disabled = true;
            }
            var progress = document.getElementById("progress-bar-fill");
            if (progress) progress.style.width = "100%";

            // Notify other tabs (Home page) that progress changed
            try {
                localStorage.setItem("na-progress-updated", Date.now().toString());
                localStorage.removeItem("na-progress-updated");
            } catch (e) { /* silent */ }
    })
    .catch(function () {
        alert("An error occurred. Please try again.");
    });
}

// Listen for cross-tab progress notifications via localStorage
if (typeof window !== "undefined") {
    window.addEventListener("storage", function (e) {
        if (e.key === "na-progress-updated") {
            // Another tab updated progress — trigger a poll if on Home page
            if (typeof pollProgress === "function") {
                pollProgress();
            }
        }
    });
}
