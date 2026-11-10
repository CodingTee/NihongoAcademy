/**
 * lesson-quiz.js
 * Short recap built from a single lesson's own vocabulary, so the quiz stays
 * tied to what the learner just read.
 * Self-check only: questions are graded in the browser and no practice attempts
 * are written, which keeps the mastery metrics on the practice page untouched.
 */
(function () {
    'use strict';

    var targetsEl = document.getElementById('mini-quiz-targets');
    var poolEl = document.getElementById('mini-quiz-pool');
    var root = document.getElementById('mini-quiz-root');
    if (!targetsEl || !poolEl || !root) return;

    var targets = parseJson(targetsEl);
    var pool = parseJson(poolEl);
    var requested = Number(root.getAttribute('data-question-count')) || 0;
    var kanaTypes = ['Hiragana', 'Katakana'];
    var optionRow = null;
    var feedbackEl = null;
    var nextEl = null;

    var questions = [];
    var index = 0;
    var score = 0;
    var answered = false;

    function parseJson(element) {
        try { return JSON.parse(element.textContent) || []; } catch (error) { return []; }
    }

    function shuffle(list) {
        var copy = list.slice();
        for (var i = copy.length - 1; i > 0; i--) {
            var j = Math.floor(Math.random() * (i + 1));
            var swap = copy[i];
            copy[i] = copy[j];
            copy[j] = swap;
        }
        return copy;
    }

    function isKana(item) {
        return kanaTypes.indexOf(item.PartOfSpeech) !== -1;
    }

    function promptFor(item) {
        if (isKana(item) && item.Romaji) return 'Which kana is "' + item.Romaji + '"?';
        if (item.English) return 'Which word means "' + item.English + '"?';
        if (item.Romaji) return 'Which word is read "' + item.Romaji + '"?';
        return '';
    }

    function explain(item) {
        if (isKana(item) && item.Romaji) return 'is the kana for "' + item.Romaji + '".';
        if (item.English) return 'means "' + item.English + '".';
        return 'belongs to this lesson.';
    }

    // Three wrong options, never repeating a Japanese form and never offering a
    // second word with the same meaning as the answer.
    function buildOptions(target) {
        var seen = {};
        seen[target.Japanese] = true;
        var preferred = [];
        var sameMeaning = [];

        pool.forEach(function (candidate) {
            if (!candidate.Japanese || seen[candidate.Japanese]) return;
            seen[candidate.Japanese] = true;
            var collides = target.English && candidate.English &&
                candidate.English.toLowerCase() === target.English.toLowerCase();
            (collides ? sameMeaning : preferred).push(candidate);
        });

        var wrong = shuffle(preferred).slice(0, 3);
        if (wrong.length < 3) wrong = wrong.concat(shuffle(sameMeaning).slice(0, 3 - wrong.length));
        return shuffle([target].concat(wrong));
    }

    function buildQuestions() {
        var askable = targets.filter(function (item) { return promptFor(item) !== ''; });
        var count = requested > 0 ? Math.min(requested, askable.length) : askable.length;
        return shuffle(askable).slice(0, count).map(function (target) {
            return { target: target, prompt: promptFor(target), options: buildOptions(target) };
        });
    }

    function start() {
        questions = buildQuestions();
        index = 0;
        score = 0;
        if (!questions.length) { renderEmpty(); return; }
        renderQuestion();
        updateScore();
    }

    function element(tag, className, text) {
        var node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined) node.textContent = text;
        return node;
    }

    function renderQuestion() {
        if (index >= questions.length) { renderResult(); return; }
        answered = false;
        root.textContent = '';

        var question = questions[index];
        var card = element('div', 'na-card-flat p-4');

        var head = element('div', 'd-flex justify-content-between align-items-center mb-3');
        head.appendChild(element('span', 'na-badge bg-na-primary text-white',
            'Question ' + (index + 1) + ' of ' + questions.length));
        head.appendChild(element('span', 'text-muted small', 'Choose one answer · keys 1–4'));

        var heading = element('h4', 'mb-3', question.prompt);

        optionRow = element('div', 'row g-2');
        question.options.forEach(function (option) {
            var column = element('div', 'col-6 col-md-3');
            var button = element('button', 'quiz-option w-100 mb-0', option.Japanese);
            button.type = 'button';
            button.setAttribute('lang', 'ja');
            if (isKana(question.target)) button.style.fontSize = '1.6rem';
            button.addEventListener('click', function () { checkAnswer(option, button); });
            column.appendChild(button);
            optionRow.appendChild(column);
        });

        feedbackEl = element('div', 'mt-3');

        var footer = element('div', 'd-flex justify-content-end mt-3');
        nextEl = element('button', 'btn-na-primary');
        nextEl.type = 'button';
        nextEl.style.display = 'none';
        nextEl.innerHTML = (index === questions.length - 1 ? 'See results' : 'Next question') +
            ' <i class="bi bi-arrow-right"></i>';
        nextEl.addEventListener('click', function () { index++; renderQuestion(); });
        footer.appendChild(nextEl);

        card.appendChild(head);
        card.appendChild(heading);
        card.appendChild(optionRow);
        card.appendChild(feedbackEl);
        card.appendChild(footer);
        root.appendChild(card);
        updateProgress();
    }

    function checkAnswer(option, button) {
        if (answered) return;
        answered = true;

        var question = questions[index];
        var isCorrect = option.Id === question.target.Id;
        question.chosen = option;
        question.isCorrect = isCorrect;
        if (isCorrect) score++;

        var buttons = optionRow.querySelectorAll('button');
        Array.prototype.forEach.call(buttons, function (item) { item.disabled = true; });
        button.classList.add(isCorrect ? 'correct' : 'incorrect');
        if (!isCorrect) {
            Array.prototype.forEach.call(buttons, function (item) {
                if (item.textContent === question.target.Japanese) item.classList.add('correct');
            });
        }

        feedbackEl.textContent = '';
        feedbackEl.appendChild(element('div', isCorrect ? 'text-success' : 'text-danger',
            isCorrect ? 'Correct.' : 'Not quite.'));
        var detail = element('div', 'small text-muted',
            question.target.Japanese + ' ' + explain(question.target));
        detail.setAttribute('lang', 'ja');
        feedbackEl.appendChild(detail);

        nextEl.style.display = '';
        nextEl.focus();
        updateScore();
    }

    function updateProgress() {
        var text = document.getElementById('mini-quiz-progress-text');
        var bar = document.getElementById('mini-quiz-progress');
        if (text) text.textContent = 'Question ' + (index + 1) + ' of ' + questions.length;
        // index is zero based, so report the question the learner is actually on.
        if (bar) bar.style.width = Math.round((index + 1) * 100 / questions.length) + '%';
    }

    function updateScore() {
        var chip = document.getElementById('mini-quiz-score');
        if (chip) chip.textContent = score + ' / ' + questions.length;
    }

    function renderResult() {
        // Drop the finished question's controls before rebuilding the card, otherwise
        // the next button keeps its click target and Enter re-renders the results.
        answered = false;
        nextEl = null;
        optionRow = null;
        root.textContent = '';
        var percent = Math.round(score * 100 / questions.length);
        updateScore();

        var text = document.getElementById('mini-quiz-progress-text');
        var bar = document.getElementById('mini-quiz-progress');
        if (text) text.textContent = 'Quiz complete';
        if (bar) bar.style.width = '100%';

        var card = element('div', 'na-card-flat p-4 na-quiz-result');

        var heading = element('h4', 'mb-1', 'Quiz complete');
        var scoreLine = element('div', 'na-quiz-result-score mb-2',
            score + ' / ' + questions.length);
        var percentLine = element('div', 'text-muted mb-3', percent + '% correct');
        var message = element('p', 'mb-4', resultMessage(percent));

        card.appendChild(heading);
        card.appendChild(scoreLine);
        card.appendChild(percentLine);
        card.appendChild(message);

        var recap = element('ol', 'small mb-4 ps-3');
        questions.forEach(function (question) {
            var line = element('li', 'mb-2');
            line.appendChild(element('div', 'text-muted', question.prompt));
            var answer = element('div', question.isCorrect ? 'text-success' : 'text-danger',
                question.chosen ? question.chosen.Japanese : 'No answer');
            answer.setAttribute('lang', 'ja');
            line.appendChild(answer);
            if (!question.isCorrect) {
                var correct = element('div', 'small text-muted', 'Answer: ' + question.target.Japanese);
                correct.setAttribute('lang', 'ja');
                line.appendChild(correct);
            }
            recap.appendChild(line);
        });
        card.appendChild(recap);

        var actions = element('div', 'd-flex flex-wrap gap-2');
        var retry = element('button', 'btn-na-primary btn-sm', 'Try again');
        retry.type = 'button';
        retry.addEventListener('click', start);
        actions.appendChild(retry);

        var practice = document.querySelector('a[href*="/Practice/LessonPractice"]');
        if (practice) {
            var practiceLink = element('a', 'btn-na-outline btn-sm', 'Practice This Lesson');
            practiceLink.href = practice.getAttribute('href');
            actions.appendChild(practiceLink);
        }
        var lesson = document.querySelector('a[href*="/Lessons/Details"]');
        if (lesson) {
            var lessonLink = element('a', 'na-link-soft align-self-center', 'Back to Lesson');
            lessonLink.href = lesson.getAttribute('href');
            actions.appendChild(lessonLink);
        }
        card.appendChild(actions);

        root.appendChild(card);
    }

    function resultMessage(percent) {
        if (percent === 100) return 'Perfect score. This lesson looks solid, so the mastery check is the next step.';
        if (percent >= 80) return 'Nice work. Read the misses below, then take the mastery check.';
        if (percent >= 60) return 'Good start. Reread the lesson and run the quiz again before the mastery check.';
        return 'Go back to the lesson once more, then come back to this quiz.';
    }

    function renderEmpty() {
        root.textContent = '';
        var card = element('div', 'na-card-flat p-4 text-center');
        card.innerHTML = '<i class="bi bi-inbox" style="font-size:2rem;"></i>';
        card.appendChild(element('p', 'text-muted mt-2 mb-0',
            'This lesson does not have enough items for a quiz yet. Use Practice This Lesson instead.'));
        root.appendChild(card);
        var text = document.getElementById('mini-quiz-progress-text');
        var bar = document.getElementById('mini-quiz-progress');
        if (text) text.textContent = 'Not available yet';
        if (bar) bar.style.width = '0%';
    }

    document.addEventListener('keydown', function (event) {
        if (!root.hasChildNodes()) return;
        var tag = (document.activeElement && document.activeElement.tagName) || '';
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

        if (!answered && optionRow) {
            var pick = optionRow.querySelectorAll('button')[Number(event.key) - 1];
            if (pick && event.key >= '1' && event.key <= '4') {
                event.preventDefault();
                pick.click();
                return;
            }
        }
        if (answered && event.key === 'Enter' && nextEl && nextEl.style.display !== 'none' &&
            nextEl.isConnected) {
            event.preventDefault();
            nextEl.click();
        }
    });

    start();
})();
