// ===== NihongoAcademy JavaScript =====

document.addEventListener("DOMContentLoaded", function () {
    initSakuraPetals();
});

// ===== Japanese pronunciation (Web Speech API) =====
var jaVoice = null;

function pickJapaneseVoice() {
    if (!("speechSynthesis" in window)) return;
    var voices = window.speechSynthesis.getVoices();
    jaVoice = voices.find(function (v) { return v.lang === "ja-JP"; }) ||
              voices.find(function (v) { return v.lang && v.lang.indexOf("ja") === 0; }) ||
              null;
}

if ("speechSynthesis" in window) {
    pickJapaneseVoice();
    // Some browsers load the voice list asynchronously
    window.speechSynthesis.onvoiceschanged = pickJapaneseVoice;
}

function speakJapanese(text) {
    if (!text) return;

    if (!("speechSynthesis" in window)) {
        alert("您的浏览器不支持语音合成，请使用 Edge 或 Chrome 浏览器。");
        return;
    }

    window.speechSynthesis.cancel();
    var utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = "ja-JP";
    if (jaVoice) utterance.voice = jaVoice;
    utterance.rate = 0.85;   // slightly slower for learners
    window.speechSynthesis.speak(utterance);
}

// Speaker buttons: <button class="na-speak-btn" data-speak="こんにちは">
// Kana chart cells: click any .kana-cell to hear its syllable
document.addEventListener("click", function (e) {
    var btn = e.target.closest(".na-speak-btn");
    if (btn) {
        e.preventDefault();
        e.stopPropagation();
        speakJapanese(btn.getAttribute("data-speak"));
        return;
    }

    var cell = e.target.closest(".kana-cell");
    if (cell) {
        var romaji = cell.querySelector(".kana-romaji");
        var text = cell.textContent;
        if (romaji) text = text.replace(romaji.textContent, "");
        speakJapanese(text.trim());
    }
});

// ===== Sakura falling petals decoration =====
function initSakuraPetals() {
    var container = document.querySelector(".sakura-decoration");
    if (!container) return;

    for (var i = 0; i < 15; i++) {
        var petal = document.createElement("div");
        petal.className = "sakura-petal";
        petal.style.left = Math.random() * 100 + "%";
        petal.style.animationDuration = (Math.random() * 8 + 8) + "s";
        petal.style.animationDelay = Math.random() * 5 + "s";
        petal.style.opacity = Math.random() * 0.3 + 0.1;
        petal.style.transform = "scale(" + (Math.random() * 0.8 + 0.5) + ")";
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
            example.textContent = data.exampleJapanese ? data.exampleJapanese + " — " + (data.exampleEnglish || "") : "";
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
    .then(function(response) { return response.json(); })
    .then(function(data) {
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
            '<p class="text-muted">完了！</p>' +
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

    startQuizTimer();
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
            submitQuiz();
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

function submitQuiz() {
    if (quizTimerInterval) clearInterval(quizTimerInterval);

    var unanswered = quizAnswers.filter(function (a) { return a === 0; }).length;
    if (unanswered > 0) {
        if (!confirm(unanswered + " question(s) unanswered. Submit anyway?")) {
            startQuizTimer();
            return;
        }
    }

    var form = document.getElementById("quiz-form");
    if (!form) return;

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
    .then(function (response) { return response.json(); })
    .then(function (data) {
        if (data.success) {
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
        }
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
