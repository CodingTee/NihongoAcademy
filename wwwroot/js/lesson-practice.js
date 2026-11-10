/**
 * lesson-practice.js
 * Lesson-specific practice: Flashcards, Recognition, Listening, Mini Quiz
 * Data is injected via #lesson-vocab-data script tag
 */
(function () {
    'use strict';

    // ── Load vocab data ──
    const dataEl = document.getElementById('lesson-vocab-data');
    if (!dataEl) return;

    // Injected payloads come straight from the server; a single malformed one must
    // not abort this IIFE and take every interaction on the page down with it.
    const parseInjected = (element, fallback) => {
        if (!element) return fallback;
        try {
            return JSON.parse(element.textContent || '');
        } catch (error) {
            console.warn('lesson-practice: unreadable injected data', error);
            return fallback;
        }
    };

    const vocabData = parseInjected(dataEl, []);
    if (!Array.isArray(vocabData) || vocabData.length === 0) return;
    const reviewData = parseInjected(document.getElementById('lesson-review-vocab-data'), []);
    const reviewVocab = Array.isArray(reviewData) ? reviewData : [];
    const allPracticeData = [...vocabData, ...reviewVocab];
    const lessonId = Number(document.getElementById('lesson-practice-id')?.value || 0);

    function updateMasteryStatus(data) {
        const status = document.getElementById('mastery-status');
        if (!status || !data) return;
        if (data.mastered) {
            status.textContent = 'Mastered';
            status.classList.add('text-na-accent');
        } else if (data.awaitingDelayedReview) {
            status.textContent = data.reviewDueDate && new Date(data.reviewDueDate) > new Date()
                ? 'Review pending'
                : 'Complete delayed review';
            status.classList.add('text-na-primary');
        }

        if (data.results) {
            const metMap = data.typeMet || {};
            Object.entries(data.results).forEach(([mode, metric]) => {
                const card = document.querySelector(`[data-mastery-mode="${mode}"]`);
                if (!card) return;
                const accuracy = card.querySelector('.mastery-accuracy');
                const coverage = card.querySelector('.mastery-coverage');
                const attempts = card.querySelector('.mastery-attempts');
                const bar = card.querySelector('[data-mastery-bar]');
                const isProduction = mode === 'production';
                const met = metMap[mode] === true;
                if (accuracy) accuracy.textContent = isProduction ? (met ? 'Done' : 'Pending') : `${metric.accuracy}%`;
                if (coverage) coverage.textContent = metric.covered;
                if (attempts) attempts.textContent = metric.count;
                if (bar) {
                    bar.style.width = (isProduction ? (met ? 100 : 0) : metric.accuracy) + '%';
                    bar.className = 'progress-bar ' + (met ? 'bg-na-accent' : 'bg-na-primary');
                }
                card.classList.toggle('border-na-accent', met);
                const muted = card.querySelector('.small.text-muted');
                let badge = card.querySelector('[data-mastery-badge]');
                if (met && !badge && muted) {
                    badge = document.createElement('span');
                    badge.className = 'text-success ms-1';
                    badge.setAttribute('data-mastery-badge', mode);
                    badge.textContent = '✓';
                    muted.appendChild(badge);
                } else if (!met && badge) {
                    badge.remove();
                }
            });
        }
        if (data.firstPassProgress != null) {
            const label = document.getElementById('first-pass-progress-label');
            const bar = document.getElementById('first-pass-progress-bar');
            if (bar) bar.style.width = data.firstPassProgress + '%';
            if (label) label.textContent = data.mastered ? 'Stable mastery achieved' : data.firstPassProgress + '%';
        }
        if (data.activitiesTotal != null) {
            const actLine = document.getElementById('activities-status');
            if (actLine) {
                const done = data.activitiesComplete;
                const total = data.activitiesTotal;
                const allDone = total > 0 && done === total;
                actLine.innerHTML = `Activities: <strong class="${allDone ? 'text-na-accent' : ''}">${done}/${total}</strong> complete · required to unlock`;
            }
        }
    }

    function recordAttempt(vocabularyId, exerciseType, answer) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        return fetch('/Practice/RecordLessonAttempt', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
            body: new URLSearchParams({ lessonId, vocabularyId, exerciseType, answer })
        }).then(r => r.ok ? r.json() : Promise.reject(new Error('Unable to save lesson attempt'))).then(data => {
            updateMasteryStatus(data);
            return data;
        });
    }

    // ── Tab switching ──
    const tabs = document.querySelectorAll('.na-tabs-scroll .na-tab');
    const panels = document.querySelectorAll('.na-practice-tab');
    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('na-tab--active'));
            tab.classList.add('na-tab--active');
            panels.forEach(p => p.style.display = 'none');
            const target = tab.dataset.tab;
            const panel = document.getElementById('tab-' + target);
            if (panel) panel.style.display = 'block';
            // The listening prompt is audio-only, so it starts when the learner
            // opens that tab. Nothing speaks while the page is loading.
            if (target === 'listening') playListenPrompt();
        });
    });

    // ── Speech helper ──
    function speak(text) {
        if (typeof window.speakJapanese === 'function') return window.speakJapanese(text);
    }

    function recordActivity(activityId, answer) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        return fetch('/Practice/RecordActivityAttempt', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
            body: new URLSearchParams({ lessonId, activityId, answer })
        }).then(r => r.ok ? r.json() : Promise.reject(new Error('Unable to save activity')))
            .then(data => { updateMasteryStatus(data); return data; });
    }

    document.querySelectorAll('.activity-card').forEach(card => {
        const activityId = Number(card.dataset.activityId || 0);
        const feedback = card.querySelector('.activity-feedback');

        card.querySelectorAll('.activity-answer').forEach(button => {
            button.addEventListener('click', () => {
                card.querySelectorAll('.activity-answer').forEach(x => { x.disabled = true; });
                recordActivity(activityId, button.dataset.answer || '').then(data => {
                    button.classList.add(data.isCorrect ? 'correct' : 'incorrect');
                    if (feedback) feedback.innerHTML = data.isCorrect
                        ? `<span class="text-success">Correct. ${data.explanation || ''}</span>`
                        : `<span class="text-danger">Answer: <strong>${data.correctAnswer || ''}</strong></span><div class="small mt-1">${data.explanation || ''}</div>`;
                }).catch(() => {
                    card.querySelectorAll('.activity-answer').forEach(x => { x.disabled = false; });
                    if (feedback) feedback.textContent = 'Could not save this answer. Please try again.';
                });
            });
        });

        card.querySelector('.activity-submit')?.addEventListener('click', () => {
            const input = card.querySelector('.activity-input');
            const answer = input?.value.trim() || '';
            if (!answer) return;
            recordActivity(activityId, answer).then(data => {
                if (input) input.disabled = true;
                if (feedback) feedback.innerHTML = data.isCorrect
                    ? `<span class="text-success">Correct. ${data.explanation || ''}</span>`
                    : `<span class="text-danger">Model answer: <strong>${data.correctAnswer || ''}</strong></span><div class="small mt-1">${data.explanation || ''}</div>`;
            }).catch(() => { if (feedback) feedback.textContent = 'Could not save this answer. Please try again.'; });
        });
    });

    // ── Shuffle ──
    function shuffle(arr) {
        const a = [...arr];
        for (let i = a.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [a[i], a[j]] = [a[j], a[i]];
        }
        return a;
    }

    function makePracticeOrder() {
        const targets = shuffle(vocabData);
        if (!reviewVocab.length) return targets;
        const reviewCount = Math.max(1, Math.ceil(targets.length * 3 / 7));
        return shuffle([...targets, ...shuffle(reviewVocab).slice(0, reviewCount)]);
    }

    function showPracticeKind(elementId, item) {
        const el = document.getElementById(elementId);
        if (!el) return;
        el.textContent = item.IsReview ? 'Earlier lesson review' : 'Current lesson target';
        el.className = item.IsReview ? 'small text-na-primary' : 'small text-muted';
    }

    // ══════════════════════════════
    //  FLASHCARDS
    // ══════════════════════════════
    let fcIndex = 0;
    let fcFlipped = false;
    let fcFinished = false;
    const fcJp = document.getElementById('fc-jp');
    const fcRomaji = document.getElementById('fc-romaji');
    const fcEn = document.getElementById('fc-en');
    const fcPos = document.getElementById('fc-pos');
    const fcEx = document.getElementById('fc-ex');
    const fcReading = document.getElementById('fc-reading');
    const fcCounter = document.getElementById('fc-counter');
    const fcCard = document.getElementById('lesson-flashcard-inner');

    function showFlashcard(idx) {
        if (idx < 0 || idx >= vocabData.length) return;
        fcIndex = idx;
        fcFlipped = false;
        if (fcCard) fcCard.classList.remove('flipped');
        const v = vocabData[idx];
        if (fcJp) fcJp.textContent = v.Japanese;
        if (fcRomaji) fcRomaji.textContent = v.Romaji;
        if (fcEn) fcEn.textContent = v.English;
        if (fcPos) fcPos.textContent = v.PartOfSpeech || '';
        if (fcEx) fcEx.innerHTML = v.ExampleJapanese ? `${v.ExampleJapanese}<br/><small class="text-na-primary">${v.ExampleReading && v.ExampleReading !== v.ExampleJapanese ? v.ExampleReading + '<br/>' : ''}</small><small>${v.ExampleEnglish || ''}</small>` : '';
        if (fcCounter) fcCounter.textContent = `${idx + 1} / ${vocabData.length}`;
    }

    // Flip on click
    const fcContainer = document.getElementById('lesson-flashcard');
    if (fcContainer) {
        fcContainer.addEventListener('click', () => {
            if (fcFinished) return;
            fcFlipped = !fcFlipped;
            if (fcCard) fcCard.classList.toggle('flipped', fcFlipped);
        });
    }

    // Nav
    document.getElementById('fc-prev')?.addEventListener('click', () => { if (!fcFinished) showFlashcard(fcIndex - 1); });
    document.getElementById('fc-next')?.addEventListener('click', () => { if (!fcFinished) showFlashcard(fcIndex + 1); });

    function recordFlashcardReview(vocabularyId, quality) {
        fetch('/Flashcards/RecordReview', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: 'vocabularyId=' + vocabularyId + '&quality=' + quality
        }).catch(() => { /* recording is best-effort */ });
    }

    function finishFlashcards() {
        fcFinished = true;
        const card = document.getElementById('lesson-flashcard');
        const nav = document.getElementById('fc-nav');
        const rating = document.getElementById('fc-rating');
        if (card) card.style.display = 'none';
        if (nav) nav.style.display = 'none';
        if (rating) rating.style.display = 'none';
        const complete = document.getElementById('fc-complete');
        if (complete) complete.style.display = 'block';
    }

    function restartFlashcards() {
        fcFinished = false;
        const card = document.getElementById('lesson-flashcard');
        const nav = document.getElementById('fc-nav');
        const rating = document.getElementById('fc-rating');
        const complete = document.getElementById('fc-complete');
        if (card) card.style.display = '';
        if (nav) nav.style.display = '';
        if (rating) rating.style.display = '';
        if (complete) complete.style.display = 'none';
        fcIndex = 0;
        showFlashcard(0);
    }

    document.getElementById('fc-restart')?.addEventListener('click', restartFlashcards);

    // Keyboard nav
    document.addEventListener('keydown', e => {
        if (fcFinished) return;
        if (e.key === 'ArrowLeft') showFlashcard(fcIndex - 1);
        if (e.key === 'ArrowRight') showFlashcard(fcIndex + 1);
        if (e.key === ' ' && document.getElementById('tab-flashcards')?.style.display !== 'none') {
            e.preventDefault();
            fcFlipped = !fcFlipped;
            if (fcCard) fcCard.classList.toggle('flipped', fcFlipped);
        }
    });

    // Rating: record SRS quality, then advance. The last card ends the session
    // instead of silently doing nothing past the end of the list.
    document.querySelectorAll('.fc-rate').forEach(btn => {
        btn.addEventListener('click', () => {
            if (fcFinished) return;
            const v = vocabData[fcIndex];
            if (v) recordFlashcardReview(v.Id, parseInt(btn.dataset.quality, 10));
            if (fcIndex + 1 < vocabData.length) {
                showFlashcard(fcIndex + 1);
            } else {
                finishFlashcards();
            }
        });
    });


    // ══════════════════════════════
    //  RECOGNITION (MCQ)
    // ══════════════════════════════
    let recIndex = 0;
    const recOrder = makePracticeOrder();
    const recTarget = document.getElementById('rec-target');
    const recOptions = document.getElementById('rec-options');

    function showRecQuestion() {
        if (recIndex >= recOrder.length) recIndex = 0;
        const correct = recOrder[recIndex];
        if (recTarget) recTarget.textContent = correct.Romaji || correct.English;
        showPracticeKind('rec-kind', correct);

        const options = [correct];
        const pool = allPracticeData.filter(v => v.Id !== correct.Id);
        const shuffledPool = shuffle(pool);
        for (let i = 0; i < 3 && i < shuffledPool.length; i++) options.push(shuffledPool[i]);
        const shuffledOpts = shuffle(options);

        if (recOptions) {
            recOptions.innerHTML = '';
            shuffledOpts.forEach(opt => {
                const col = document.createElement('div');
                col.className = 'col-6 col-md-3';
                const btn = document.createElement('button');
                btn.className = 'quiz-option w-100';
                btn.textContent = opt.Japanese;
                btn.onclick = function () {
                    // Disable all
                    recOptions.querySelectorAll('button').forEach(b => b.disabled = true);
                    recordAttempt(correct.Id, 'recognition', opt.Japanese).then(result => {
                        const isCorrect = result?.isCorrect === true;
                        btn.classList.add(isCorrect ? 'correct' : 'incorrect');
                        if (!isCorrect) {
                            [...recOptions.querySelectorAll('button')]
                                .find(b => b.textContent === result?.correctAnswer)?.classList.add('correct');
                        }
                    }).catch(() => {
                        recOptions.querySelectorAll('button').forEach(b => b.disabled = false);
                        window.alert('This answer could not be saved. Check your connection and try again.');
                    });
                };
                col.appendChild(btn);
                recOptions.appendChild(col);
            });
        }
    }

    document.getElementById('rec-next-btn')?.addEventListener('click', () => {
        recIndex++;
        showRecQuestion();
    });
    showRecQuestion();

    // ══════════════════════════════
    //  LISTENING (MCQ from audio)
    // ══════════════════════════════
    let listenIndex = 0;
    const listenOrder = makePracticeOrder();
    const listenOptions = document.getElementById('listen-options');
    const listenPlayBtn = document.getElementById('listen-play-btn');

    // Reading the prompt out loud is a separate step from drawing it: the page
    // renders the question quietly and only speaks on a learner action.
    function playListenPrompt() {
        const item = listenOrder[listenIndex];
        if (!item) return;
        speak(item.PartOfSpeech === 'Kanji' && item.ExampleJapanese ? item.ExampleJapanese : item.Japanese);
    }

    function showListenQuestion() {
        if (listenIndex >= listenOrder.length) listenIndex = 0;
        const correct = listenOrder[listenIndex];
        showPracticeKind('listen-kind', correct);

        const options = [correct];
        const pool = allPracticeData.filter(v => v.Id !== correct.Id);
        const shuffledPool = shuffle(pool);
        for (let i = 0; i < 3 && i < shuffledPool.length; i++) options.push(shuffledPool[i]);
        const shuffledOpts = shuffle(options);

        if (listenOptions) {
            listenOptions.innerHTML = '';
            shuffledOpts.forEach(opt => {
                const col = document.createElement('div');
                col.className = 'col-6 col-md-3';
                const btn = document.createElement('button');
                btn.className = 'quiz-option w-100';
                btn.textContent = opt.Japanese;
                btn.onclick = function () {
                    listenOptions.querySelectorAll('button').forEach(b => b.disabled = true);
                    recordAttempt(correct.Id, 'listening', opt.Japanese).then(result => {
                        const isCorrect = result?.isCorrect === true;
                        btn.classList.add(isCorrect ? 'correct' : 'incorrect');
                        if (!isCorrect) {
                            [...listenOptions.querySelectorAll('button')]
                                .find(b => b.textContent === result?.correctAnswer)?.classList.add('correct');
                        }
                    }).catch(() => {
                        listenOptions.querySelectorAll('button').forEach(b => b.disabled = false);
                        window.alert('This answer could not be saved. Check your connection and try again.');
                    });
                };
                col.appendChild(btn);
                listenOptions.appendChild(col);
            });
        }
    }

    listenPlayBtn?.addEventListener('click', playListenPrompt);

    document.getElementById('listen-next-btn')?.addEventListener('click', () => {
        listenIndex++;
        showListenQuestion();
        playListenPrompt();
    });
    showListenQuestion();

    // ══════════════════════════════
    //  ACTIVE RECALL (no choices)
    // ══════════════════════════════
    let recallIndex = 0;
    const recallOrder = makePracticeOrder();
    const recallTarget = document.getElementById('recall-target');
    const recallInput = document.getElementById('recall-input');
    const recallFeedback = document.getElementById('recall-feedback');

    function showRecallQuestion() {
        if (recallIndex >= recallOrder.length) recallIndex = 0;
        const current = recallOrder[recallIndex];
        if (recallTarget) recallTarget.textContent = current.Romaji;
        showPracticeKind('recall-kind', current);
        if (recallInput) { recallInput.value = ''; recallInput.disabled = false; recallInput.focus(); }
        if (recallFeedback) recallFeedback.textContent = '';
    }

    document.getElementById('recall-check')?.addEventListener('click', () => {
        const current = recallOrder[recallIndex];
        const answer = recallInput?.value.trim() || '';
        if (recallInput) recallInput.disabled = true;
        recordAttempt(current.Id, 'recall', answer).then(result => {
            if (recallFeedback) {
                recallFeedback.innerHTML = result?.isCorrect
                    ? '<span class="text-success">Correct!</span>'
                    : `<span class="text-danger">Answer: <strong>${result?.correctAnswer || current.Japanese}</strong></span>`;
            }
            setTimeout(() => { recallIndex++; showRecallQuestion(); }, 1100);
        }).catch(() => {
            if (recallInput) recallInput.disabled = false;
            if (recallFeedback) recallFeedback.innerHTML = '<span class="text-danger">This answer could not be saved. Check your connection and try again.</span>';
        });
    });
    recallInput?.addEventListener('keydown', e => { if (e.key === 'Enter') document.getElementById('recall-check')?.click(); });
    showRecallQuestion();

    document.getElementById('production-complete')?.addEventListener('click', event => {
        const text = document.getElementById('production-input')?.value.trim() || '';
        const checks = [...document.querySelectorAll('.production-check')];
        const complete = text.length >= 8 && checks.length > 0 && checks.every(x => x.checked);
        const feedback = document.getElementById('production-feedback');
        if (!complete) {
            if (feedback) feedback.innerHTML = '<span class="text-danger">Write a complete response and confirm all three checks.</span>';
            return;
        }
        const button = event.currentTarget;
        button.disabled = true;
        recordAttempt(vocabData[0].Id, 'production', text).then(() => {
            if (feedback) feedback.innerHTML = '<span class="text-success">Output task recorded. Keep this response and try it again later without support.</span>';
        }).catch(() => {
            button.disabled = false;
            if (feedback) feedback.innerHTML = '<span class="text-danger">This response could not be saved. Check your connection and try again.</span>';
        });
    });

})();
