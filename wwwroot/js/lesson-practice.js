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
    const vocabData = JSON.parse(dataEl.textContent);
    if (!vocabData || !vocabData.length) return;
    const lessonId = Number(document.getElementById('lesson-practice-id')?.value || 0);

    function updateMasteryStatus(data) {
        const status = document.getElementById('mastery-status');
        if (!status || !data) return;
        if (data.mastered) {
            status.textContent = 'Mastered';
            status.classList.add('text-success');
        } else if (data.awaitingDelayedReview) {
            status.textContent = data.reviewDueDate && new Date(data.reviewDueDate) > new Date()
                ? 'Review pending'
                : 'Complete delayed review';
            status.classList.add('text-na-primary');
        }
    }

    function recordAttempt(vocabularyId, exerciseType, answer) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        return fetch('/Practice/RecordLessonAttempt', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
            body: new URLSearchParams({ lessonId, vocabularyId, exerciseType, answer })
        }).then(r => r.ok ? r.json() : null).then(data => {
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
        });
    });

    // ── Speech helper ──
    function speak(text) {
        if (!window.speechSynthesis) return;
        const u = new SpeechSynthesisUtterance(text);
        u.lang = 'ja-JP';
        u.rate = 0.85;
        speechSynthesis.cancel();
        speechSynthesis.speak(u);
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

    // ══════════════════════════════
    //  FLASHCARDS
    // ══════════════════════════════
    let fcIndex = 0;
    let fcFlipped = false;
    const fcJp = document.getElementById('fc-jp');
    const fcRomaji = document.getElementById('fc-romaji');
    const fcEn = document.getElementById('fc-en');
    const fcPos = document.getElementById('fc-pos');
    const fcEx = document.getElementById('fc-ex');
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
        if (fcEx) fcEx.innerHTML = v.ExampleJapanese ? `${v.ExampleJapanese}<br/><small>${v.ExampleEnglish || ''}</small>` : '';
        if (fcCounter) fcCounter.textContent = `${idx + 1} / ${vocabData.length}`;
    }

    // Flip on click
    const fcContainer = document.getElementById('lesson-flashcard');
    if (fcContainer) {
        fcContainer.addEventListener('click', () => {
            fcFlipped = !fcFlipped;
            if (fcCard) fcCard.classList.toggle('flipped', fcFlipped);
        });
    }

    // Nav
    document.getElementById('fc-prev')?.addEventListener('click', () => showFlashcard(fcIndex - 1));
    document.getElementById('fc-next')?.addEventListener('click', () => showFlashcard(fcIndex + 1));

    // Keyboard nav
    document.addEventListener('keydown', e => {
        if (e.key === 'ArrowLeft') showFlashcard(fcIndex - 1);
        if (e.key === 'ArrowRight') showFlashcard(fcIndex + 1);
        if (e.key === ' ' && document.getElementById('tab-flashcards')?.style.display !== 'none') {
            e.preventDefault();
            fcFlipped = !fcFlipped;
            if (fcCard) fcCard.classList.toggle('flipped', fcFlipped);
        }
    });

    // Rating
    document.querySelectorAll('.fc-rate').forEach(btn => {
        btn.addEventListener('click', () => {
            // For MVP, just advance to next card
            showFlashcard(fcIndex + 1);
        });
    });

    // ══════════════════════════════
    //  RECOGNITION (MCQ)
    // ══════════════════════════════
    let recIndex = 0;
    const recOrder = shuffle(vocabData.map((_, i) => i));
    const recTarget = document.getElementById('rec-target');
    const recOptions = document.getElementById('rec-options');

    function showRecQuestion() {
        if (recIndex >= recOrder.length) recIndex = 0;
        const vIdx = recOrder[recIndex];
        const correct = vocabData[vIdx];
        if (recTarget) recTarget.textContent = correct.Romaji || correct.English;

        const options = [correct];
        const pool = vocabData.filter(v => v.Id !== correct.Id);
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
                    const isCorrect = opt.Id === correct.Id;
                    if (isCorrect) {
                        btn.classList.add('correct');
                    } else {
                        btn.classList.add('incorrect');
                    }
                    recordAttempt(correct.Id, 'recognition', opt.Japanese);
                    // Disable all
                    recOptions.querySelectorAll('button').forEach(b => b.disabled = true);
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
    const listenOrder = shuffle(vocabData.map((_, i) => i));
    const listenOptions = document.getElementById('listen-options');
    const listenPlayBtn = document.getElementById('listen-play-btn');

    function showListenQuestion() {
        if (listenIndex >= listenOrder.length) listenIndex = 0;
        const vIdx = listenOrder[listenIndex];
        const correct = vocabData[vIdx];

        // Play audio
        const listeningText = correct.PartOfSpeech === 'Kanji' && correct.ExampleJapanese
            ? correct.ExampleJapanese
            : correct.Japanese;
        speak(listeningText);

        const options = [correct];
        const pool = vocabData.filter(v => v.Id !== correct.Id);
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
                    const isCorrect = opt.Id === correct.Id;
                    if (isCorrect) {
                        btn.classList.add('correct');
                    } else {
                        btn.classList.add('incorrect');
                    }
                    recordAttempt(correct.Id, 'listening', opt.Japanese);
                    listenOptions.querySelectorAll('button').forEach(b => b.disabled = true);
                };
                col.appendChild(btn);
                listenOptions.appendChild(col);
            });
        }
    }

    listenPlayBtn?.addEventListener('click', () => {
        const vIdx = listenOrder[listenIndex];
        if (vocabData[vIdx]) {
            const item = vocabData[vIdx];
            speak(item.PartOfSpeech === 'Kanji' && item.ExampleJapanese ? item.ExampleJapanese : item.Japanese);
        }
    });

    document.getElementById('listen-next-btn')?.addEventListener('click', () => {
        listenIndex++;
        showListenQuestion();
    });
    showListenQuestion();

    // ══════════════════════════════
    //  ACTIVE RECALL (no choices)
    // ══════════════════════════════
    let recallIndex = 0;
    const recallOrder = shuffle(vocabData.map((_, i) => i));
    const recallTarget = document.getElementById('recall-target');
    const recallInput = document.getElementById('recall-input');
    const recallFeedback = document.getElementById('recall-feedback');

    function showRecallQuestion() {
        if (recallIndex >= recallOrder.length) recallIndex = 0;
        const current = vocabData[recallOrder[recallIndex]];
        if (recallTarget) recallTarget.textContent = current.Romaji;
        if (recallInput) { recallInput.value = ''; recallInput.disabled = false; recallInput.focus(); }
        if (recallFeedback) recallFeedback.textContent = '';
    }

    document.getElementById('recall-check')?.addEventListener('click', () => {
        const current = vocabData[recallOrder[recallIndex]];
        const answer = recallInput?.value.trim() || '';
        const isCorrect = answer === current.Japanese;
        if (recallInput) recallInput.disabled = true;
        if (recallFeedback) {
            recallFeedback.innerHTML = isCorrect
                ? '<span class="text-success">Correct!</span>'
                : `<span class="text-danger">Answer: <strong>${current.Japanese}</strong></span>`;
        }
        recordAttempt(current.Id, 'recall', answer);
        setTimeout(() => { recallIndex++; showRecallQuestion(); }, 1100);
    });
    recallInput?.addEventListener('keydown', e => { if (e.key === 'Enter') document.getElementById('recall-check')?.click(); });
    showRecallQuestion();

    document.getElementById('production-complete')?.addEventListener('click', () => {
        const text = document.getElementById('production-input')?.value.trim() || '';
        const checks = [...document.querySelectorAll('.production-check')];
        const complete = text.length >= 8 && checks.length > 0 && checks.every(x => x.checked);
        const feedback = document.getElementById('production-feedback');
        if (!complete) {
            if (feedback) feedback.innerHTML = '<span class="text-danger">Write a complete response and confirm all three checks.</span>';
            return;
        }
        recordAttempt(vocabData[0].Id, 'production', text);
        if (feedback) feedback.innerHTML = '<span class="text-success">Output task recorded. Keep this response and try it again later without support.</span>';
    });

})();
