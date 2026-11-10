/**
 * practice-mode.js
 * Handles all skill-based practice modes via ModeRunner view
 * Data injected via #mode-vocab-data script tag
 */
(function () {
    'use strict';

    // ── Load vocab data ──
    const dataEl = document.getElementById('mode-vocab-data');
    if (!dataEl) return;

    // The payload is server-injected JSON; an unreadable one must log and bail rather
    // than throw and leave the mode runner half-wired.
    let vocabData = [];
    try {
        vocabData = JSON.parse(dataEl.textContent || '');
    } catch (error) {
        console.warn('practice-mode: unreadable injected data', error);
        return;
    }
    if (!Array.isArray(vocabData) || vocabData.length === 0) return;

    // Anything typed by the learner is echoed back into feedback boxes, so it has to
    // be escaped on the way into every HTML string below.
    const escapeHtml = (value) => String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');

    // ── Helpers ──
    function speak(text) {
        if (typeof window.speakJapanese === 'function') return window.speakJapanese(text);
    }

    function shuffle(arr) {
        const a = [...arr];
        for (let i = a.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [a[i], a[j]] = [a[j], a[i]];
        }
        return a;
    }

    function randomVocab() {
        return vocabData[Math.floor(Math.random() * vocabData.length)];
    }

    // ══════════════════════════════
    //  LISTEN & CHOOSE
    // ══════════════════════════════
    const modePlayBtn = document.getElementById('mode-play-btn');
    const modeOptions = document.getElementById('mode-options');
    const modeNextBtn = document.getElementById('mode-next-btn');
    const modeQuestion = document.getElementById('mode-question');

    let currentListenVocab = null;

    function listenChooseQuestion() {
        currentListenVocab = randomVocab();
        speak(currentListenVocab.Japanese);

        if (modeQuestion) modeQuestion.innerHTML = 'Which word did you hear?';

        const options = [currentListenVocab];
        const pool = vocabData.filter(v => v.Id !== currentListenVocab.Id);
        const sp = shuffle(pool);
        for (let i = 0; i < 3 && i < sp.length; i++) options.push(sp[i]);
        const shuffled = shuffle(options);

        if (modeOptions) {
            modeOptions.innerHTML = '';
            shuffled.forEach(opt => {
                const col = document.createElement('div');
                col.className = 'col-6 col-md-3';
                const btn = document.createElement('button');
                btn.className = 'quiz-option w-100';
                btn.innerHTML = `<strong>${opt.Japanese}</strong><br/><small>${opt.Romaji || ''}</small>`;
                btn.onclick = function () {
                    if (opt.Id === currentListenVocab.Id) {
                        btn.classList.add('correct');
                    } else {
                        btn.classList.add('incorrect');
                    }
                    modeOptions.querySelectorAll('button').forEach(b => b.disabled = true);
                    if (modeNextBtn) modeNextBtn.style.display = 'inline-block';
                };
                col.appendChild(btn);
                modeOptions.appendChild(col);
            });
        }
    }

    modeNextBtn?.addEventListener('click', () => {
        if (modeNextBtn) modeNextBtn.style.display = 'none';
        listenChooseQuestion();
    });

    // Auto-start listen-choose if present
    if (modePlayBtn && document.querySelector('[data-mode="listen-choose"]') || modePlayBtn && modeOptions) {
        listenChooseQuestion();
    }

    // ══════════════════════════════
    //  DICTATION
    // ══════════════════════════════
    const dictationInput = document.getElementById('dictation-input');
    const dictationCheck = document.getElementById('dictation-check');
    const dictationFeedback = document.getElementById('dictation-feedback');
    let currentDictationVocab = null;

    function dictationQuestion() {
        currentDictationVocab = randomVocab();
        speak(currentDictationVocab.Japanese);
        if (dictationInput) {
            dictationInput.value = '';
            dictationInput.focus();
        }
        if (dictationFeedback) dictationFeedback.style.display = 'none';
    }

    // One handler only: ModeRunner renders a single mode per page, so this button
    // replays whichever mode currently owns the stage. Binding it twice made each
    // click speak two different words.
    modePlayBtn?.addEventListener('click', () => {
        if (currentListenVocab) speak(currentListenVocab.Japanese);
        else if (currentDictationVocab) speak(currentDictationVocab.Japanese);
    });

    dictationCheck?.addEventListener('click', () => {
        if (!currentDictationVocab || !dictationInput) return;
        const userAnswer = dictationInput.value.trim();
        const correct = currentDictationVocab.Japanese;

        if (dictationFeedback) {
            dictationFeedback.style.display = 'block';
            if (userAnswer === correct) {
                dictationFeedback.innerHTML = `<div class="p-3 rounded" style="background:#E5F8E5;color:#2C5F2D;">
                    <i class="bi bi-check-circle-fill"></i> Correct!
                </div>`;
            } else {
                dictationFeedback.innerHTML = `<div class="p-3 rounded" style="background:#FFE5E5;color:#D44;">
                    <i class="bi bi-x-circle-fill"></i> Not quite.<br/>
                    <strong>You wrote:</strong> ${escapeHtml(userAnswer) || '(empty)'}<br/>
                    <strong>Correct:</strong> ${escapeHtml(correct)}<br/>
                    <small>${currentDictationVocab.Romaji} — ${currentDictationVocab.English}</small>
                </div>`;
            }
            dictationFeedback.innerHTML += `<div class="mt-2 text-center"><button class="btn-na-primary btn-sm" id="dictation-next">Next</button></div>`;
            document.getElementById('dictation-next')?.addEventListener('click', dictationQuestion);
        }
    });

    // Auto-start dictation
    if (dictationInput) dictationQuestion();

    // ══════════════════════════════
    //  READING COMPREHENSION
    // ══════════════════════════════
    const readingText = document.getElementById('reading-text');
    const readingQuestion = document.getElementById('reading-question');

    if (readingText && vocabData.length) {
        // MVP: show a few vocab as a "passage", then ask comprehension
        const passage = shuffle(vocabData).slice(0, 4);
        readingText.textContent = passage.map(v => `${v.Japanese}（${v.Romaji}）— ${v.English}`).join('。');

        if (readingQuestion && modeOptions) {
            const target = passage[0];
            readingQuestion.innerHTML = `What does "<strong>${target.Japanese}</strong>" mean?`;

            const options = [target];
            const pool = vocabData.filter(v => v.Id !== target.Id);
            const sp = shuffle(pool);
            for (let i = 0; i < 3 && i < sp.length; i++) options.push(sp[i]);
            const shuffled = shuffle(options);

            modeOptions.innerHTML = '';
            shuffled.forEach(opt => {
                const col = document.createElement('div');
                col.className = 'col-6 col-md-3';
                const btn = document.createElement('button');
                btn.className = 'quiz-option w-100';
                btn.textContent = opt.English;
                btn.onclick = function () {
                    if (opt.Id === target.Id) btn.classList.add('correct');
                    else btn.classList.add('incorrect');
                    modeOptions.querySelectorAll('button').forEach(b => b.disabled = true);
                    if (modeNextBtn) modeNextBtn.style.display = 'inline-block';
                };
                col.appendChild(btn);
                modeOptions.appendChild(col);
            });
        }
    }

    // ══════════════════════════════
    //  SENTENCE WRITING
    // ══════════════════════════════
    const writingPrompt = document.getElementById('writing-prompt');
    const writingInput = document.getElementById('writing-input');
    const writingCheck = document.getElementById('writing-check');
    const writingFeedback = document.getElementById('writing-feedback');

    if (writingPrompt && vocabData.length) {
        const target = randomVocab();
        writingPrompt.textContent = `Translate: "${target.English}"`;

        writingCheck?.addEventListener('click', () => {
            const userAnswer = writingInput?.value.trim() || '';
            if (writingFeedback) {
                writingFeedback.style.display = 'block';
                if (userAnswer.includes(target.Japanese)) {
                    writingFeedback.innerHTML = `<div class="p-3 rounded" style="background:#E5F8E5;color:#2C5F2D;">
                        <i class="bi bi-check-circle-fill"></i> Good! <strong>${target.Japanese}</strong>（${target.Romaji}）
                    </div>`;
                } else {
                    writingFeedback.innerHTML = `<div class="p-3 rounded" style="background:#FFF8E8;color:#B8860B;">
                        <i class="bi bi-info-circle-fill"></i> Expected: <strong>${target.Japanese}</strong>（${target.Romaji}）<br/>
                        <small>Your answer: ${escapeHtml(userAnswer) || '(empty)'}</small>
                    </div>`;
                }
            }
        });
    }

    // ══════════════════════════════
    //  FREE CONVERSATION
    // ══════════════════════════════
    const convArea = document.getElementById('conversation-area');
    const convInput = document.getElementById('conv-input');
    const convSend = document.getElementById('conv-send');

    function addConvMsg(text, isUser) {
        if (!convArea) return;
        const div = document.createElement('div');
        div.className = isUser ? 'na-conv-msg na-conv-msg--user' : 'na-conv-msg na-conv-msg--bot';
        div.textContent = text;
        convArea.appendChild(div);
        convArea.scrollTop = convArea.scrollHeight;
    }

    // Rule-based conversation bot for MVP
    const convResponses = {
        'casual': [
            'そうですか！それは面白いですね。',
            '日本語の勉強はどうですか？',
            '毎日少しずつ練習しましょう！',
            '日本の文化に興味がありますか？',
        ],
        'travel': [
            '日本のどこに行きたいですか？',
            '東京はとても便利な都市ですよ。',
            '京都のお寺がおすすめです。',
            '新幹線で移動すると便利ですよ。',
        ],
        'food': [
            '日本の食べ物が好きですか？',
            'ラーメンは人気がありますね！',
            'お寿司は美味しいですよ。',
            '何が一番好きですか？',
        ],
        'daily': [
            '朝は何時に起きますか？',
            '毎日日本語を練習していますか？',
            '週末は何をしますか？',
            '趣味は何ですか？',
        ],
    };

    let currentTopic = 'casual';

    document.querySelectorAll('.conv-topic').forEach(btn => {
        btn.addEventListener('click', () => {
            currentTopic = btn.dataset.topic?.toLowerCase() || 'casual';
            addConvMsg(`Topic changed: ${btn.dataset.topic}`, false);
        });
    });

    convSend?.addEventListener('click', () => {
        const text = convInput?.value.trim();
        if (!text) return;
        addConvMsg(text, true);
        convInput.value = '';

        // Simple response
        setTimeout(() => {
            const pool = convResponses[currentTopic] || convResponses['casual'];
            const reply = pool[Math.floor(Math.random() * pool.length)];
            addConvMsg(reply, false);
        }, 500);
    });

    convInput?.addEventListener('keydown', e => {
        if (e.key === 'Enter') convSend?.click();
    });

    // ══════════════════════════════
    //  ROLE PLAY
    // ══════════════════════════════
    const roleplayArea = document.getElementById('roleplay-area');
    const roleplayInput = document.getElementById('roleplay-input');
    const roleplaySend = document.getElementById('roleplay-send');

    const roleScenarios = {
        'ordering': {
            greeting: 'いらっしゃいませ。何にしますか？',
            responses: [
                'かしこまりました。他にご注文はありますか？',
                'お飲み物はいかがですか？',
                'ありがとうございます。少々お待ちください。',
                'お会計はこちらです。ありがとうございました。',
            ]
        },
        'hotel': {
            greeting: 'いらっしゃいませ。ご予約はされていますか？',
            responses: [
                'はい、確認いたします。お名前をお願いします。',
                'お部屋は3階でございます。朝食は7時からです。',
                'Wi-Fiのパスワードはこちらです。',
                'ご滞在を楽しんでください！',
            ]
        },
        'train': {
            greeting: 'いらっしゃいませ。どちらまで行きますか？',
            responses: [
                '東京までですね。自由席でよろしいですか？',
                '特急券が別途必要です。',
                '14時30分の新幹線があります。',
                'platform 2から出発します。',
            ]
        },
        'shopping': {
            greeting: 'いらっしゃいませ。何かお探しですか？',
            responses: [
                'こちらは人気の商品ですよ。',
                'サイズはいかがですか？',
                'お試しできますよ。',
                'ありがとうございます。またお越しください！',
            ]
        },
        'classmate': {
            greeting: 'こんにちは！久しぶり！元気だった？',
            responses: [
                '最近何してるの？',
                '日本語上手になったね！',
                '今度一緒に勉強しない？',
                'またね！',
            ]
        },
    };

    let currentScenario = null;

    document.querySelectorAll('.role-scenario').forEach(btn => {
        btn.addEventListener('click', () => {
            const key = btn.dataset.scenario;
            currentScenario = roleScenarios[key];
            if (!currentScenario || !roleplayArea) return;

            roleplayArea.innerHTML = '';
            const div = document.createElement('div');
            div.className = 'na-conv-msg na-conv-msg--bot';
            div.textContent = currentScenario.greeting;
            roleplayArea.appendChild(div);
        });
    });

    function addRoleMsg(text, isUser) {
        if (!roleplayArea) return;
        const div = document.createElement('div');
        div.className = isUser ? 'na-conv-msg na-conv-msg--user' : 'na-conv-msg na-conv-msg--bot';
        div.textContent = text;
        roleplayArea.appendChild(div);
        roleplayArea.scrollTop = roleplayArea.scrollHeight;
    }

    roleplaySend?.addEventListener('click', () => {
        const text = roleplayInput?.value.trim();
        if (!text || !currentScenario) return;
        addRoleMsg(text, true);
        roleplayInput.value = '';

        setTimeout(() => {
            const pool = currentScenario.responses;
            const reply = pool[Math.floor(Math.random() * pool.length)];
            addRoleMsg(reply, false);
        }, 500);
    });

    roleplayInput?.addEventListener('keydown', e => {
        if (e.key === 'Enter') roleplaySend?.click();
    });

    // ══════════════════════════════
    //  PRONUNCIATION
    // ══════════════════════════════
    const pronWord = document.getElementById('pron-word');
    const pronRomaji = document.getElementById('pron-romaji');
    const pronPlayBtn = document.getElementById('pron-play-btn');
    const pronRecordBtn = document.getElementById('pron-record-btn');
    const pronFeedback = document.getElementById('pron-feedback');
    const pronNextBtn = document.getElementById('pron-next-btn');

    let currentPronVocab = null;

    function pronQuestion() {
        currentPronVocab = randomVocab();
        if (pronWord) pronWord.textContent = currentPronVocab.Japanese;
        if (pronRomaji) pronRomaji.textContent = currentPronVocab.Romaji;
        if (pronFeedback) pronFeedback.style.display = 'none';
        speak(currentPronVocab.Japanese);
    }

    pronPlayBtn?.addEventListener('click', () => {
        if (currentPronVocab) speak(currentPronVocab.Japanese);
    });

    pronRecordBtn?.addEventListener('click', () => {
        if (!currentPronVocab || !pronFeedback) return;

        // MVP: use speech recognition to compare
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            pronFeedback.style.display = 'block';
            pronFeedback.innerHTML = `<div class="p-2 rounded" style="background:#FFF8E8;color:#B8860B;">
                Speech recognition not available in this browser. Try Chrome!
            </div>`;
            return;
        }

        pronRecordBtn.disabled = true;
        pronRecordBtn.innerHTML = '<i class="bi bi-mic-fill"></i> Listening...';

        const recognition = new SpeechRecognition();
        recognition.lang = 'ja-JP';
        recognition.interimResults = false;

        // Recognition also ends on its own after silence, producing no result at all.
        // Restoring the button only inside onresult left it stuck on "Listening...",
        // so every exit path shares one settle function.
        let recordingSettled = false;
        const settleRecording = () => {
            if (recordingSettled) return;
            recordingSettled = true;
            pronRecordBtn.disabled = false;
            pronRecordBtn.innerHTML = '<i class="bi bi-mic"></i> Repeat';
        };

        recognition.onresult = (event) => {
            const first = event.results && event.results.length > 0 ? event.results[0] : null;
            settleRecording();
            if (!first || first.length === 0) {
                pronFeedback.style.display = 'block';
                pronFeedback.innerHTML = `<div class="p-2 rounded" style="background:#FFF8E8;color:#B8860B;">
                    Nothing was picked up. Please try again.
                </div>`;
                return;
            }

            const transcript = first[0].transcript || '';
            pronFeedback.style.display = 'block';

            if (transcript === currentPronVocab.Japanese || transcript.includes(currentPronVocab.Japanese)) {
                pronFeedback.innerHTML = `<div class="p-3 rounded" style="background:#E5F8E5;color:#2C5F2D;">
                    <i class="bi bi-check-circle-fill"></i> Great! Recognised: <strong>${escapeHtml(transcript)}</strong>
                </div>`;
            } else {
                pronFeedback.innerHTML = `<div class="p-3 rounded" style="background:#FFF8E8;color:#B8860B;">
                    <i class="bi bi-info-circle-fill"></i> Heard: <strong>${escapeHtml(transcript)}</strong><br/>
                    <small>Expected: ${escapeHtml(currentPronVocab.Japanese)}（${escapeHtml(currentPronVocab.Romaji)}）</small>
                </div>`;
            }
        };

        recognition.onerror = () => {
            pronFeedback.style.display = 'block';
            settleRecording();
            pronFeedback.innerHTML = `<div class="p-2 rounded" style="background:#FFE5E5;color:#D44;">
                Could not hear you. Please try again.
            </div>`;
        };

        recognition.onend = settleRecording;

        recognition.start();
    });

    pronNextBtn?.addEventListener('click', pronQuestion);
    if (pronWord) pronQuestion();

    // ══════════════════════════════
    //  HANDWRITING
    // ══════════════════════════════
    const hwTarget = document.getElementById('hw-target');
    const hwRomaji = document.getElementById('hw-romaji');
    const hwCanvas = document.getElementById('hw-canvas');
    const hwGuideCanvas = document.getElementById('hw-guide-canvas');
    const hwClear = document.getElementById('hw-clear');
    const hwUndo = document.getElementById('hw-undo');
    const hwCheck = document.getElementById('hw-check');
    const hwNext = document.getElementById('hw-next');
    const hwStrokeBtn = document.getElementById('hw-stroke-btn');
    const hwHear = document.getElementById('hw-hear');
    const hwMeaning = document.getElementById('hw-meaning');
    const hwStrokeCount = document.getElementById('hw-stroke-count');
    const hwFeedback = document.getElementById('hw-feedback');

    let hwCtx = null;
    let hwGuideCtx = null;
    let hwDrawing = false;
    let currentHwVocab = null;
    let hwGuideVisible = true;
    let hwSnapshots = [];
    let hwStrokes = 0;

    function hwQuestion() {
        currentHwVocab = randomVocab();
        if (hwTarget) hwTarget.textContent = currentHwVocab.Japanese;
        if (hwRomaji) hwRomaji.textContent = currentHwVocab.Romaji;
        if (hwMeaning) hwMeaning.textContent = currentHwVocab.English || '';
        if (hwFeedback) hwFeedback.textContent = '';
        hwGuideVisible = true;
        if (hwStrokeBtn) hwStrokeBtn.innerHTML = '<i class="bi bi-eye"></i> Hide guide';
        drawGuide();
        clearCanvas();
    }

    function drawGuide() {
        if (!hwGuideCtx || !hwGuideCanvas) return;
        hwGuideCtx.clearRect(0, 0, hwGuideCanvas.width, hwGuideCanvas.height);
        if (!hwGuideVisible || !currentHwVocab) return;
        hwGuideCtx.save();
        hwGuideCtx.strokeStyle = '#d8d8d8';
        hwGuideCtx.lineWidth = 1;
        hwGuideCtx.setLineDash([7, 7]);
        hwGuideCtx.beginPath();
        hwGuideCtx.moveTo(150, 0); hwGuideCtx.lineTo(150, 300);
        hwGuideCtx.moveTo(0, 150); hwGuideCtx.lineTo(300, 150);
        hwGuideCtx.stroke();
        hwGuideCtx.setLineDash([]);
        hwGuideCtx.fillStyle = 'rgba(80, 80, 80, 0.16)';
        hwGuideCtx.font = "220px 'Noto Sans JP', sans-serif";
        hwGuideCtx.textAlign = 'center';
        hwGuideCtx.textBaseline = 'middle';
        hwGuideCtx.fillText(currentHwVocab.Japanese, 150, 158, 250);
        hwGuideCtx.restore();
    }

    function clearCanvas() {
        if (!hwCtx) return;
        hwCtx.clearRect(0, 0, hwCanvas.width, hwCanvas.height);
        hwSnapshots = [];
        hwStrokes = 0;
        if (hwStrokeCount) hwStrokeCount.textContent = '0';
    }

    function canvasPoint(event) {
        const rect = hwCanvas.getBoundingClientRect();
        return {
            x: (event.clientX - rect.left) * hwCanvas.width / rect.width,
            y: (event.clientY - rect.top) * hwCanvas.height / rect.height
        };
    }

    if (hwCanvas) {
        hwCtx = hwCanvas.getContext('2d');
        hwGuideCtx = hwGuideCanvas?.getContext('2d') || null;
        hwCtx.lineWidth = 7;
        hwCtx.lineCap = 'round';
        hwCtx.lineJoin = 'round';
        hwCtx.strokeStyle = '#C84B31';

        hwCanvas.addEventListener('pointerdown', e => {
            e.preventDefault();
            hwCanvas.setPointerCapture(e.pointerId);
            hwDrawing = true;
            hwSnapshots.push(hwCtx.getImageData(0, 0, hwCanvas.width, hwCanvas.height));
            hwStrokes++;
            if (hwStrokeCount) hwStrokeCount.textContent = String(hwStrokes);
            const point = canvasPoint(e);
            hwCtx.beginPath();
            hwCtx.moveTo(point.x, point.y);
        });
        hwCanvas.addEventListener('pointermove', e => {
            if (!hwDrawing) return;
            const point = canvasPoint(e);
            hwCtx.lineTo(point.x, point.y);
            hwCtx.stroke();
        });
        hwCanvas.addEventListener('pointerup', () => { hwDrawing = false; });
        hwCanvas.addEventListener('pointercancel', () => { hwDrawing = false; });
    }

    hwClear?.addEventListener('click', clearCanvas);
    hwUndo?.addEventListener('click', () => {
        const previous = hwSnapshots.pop();
        if (!previous || !hwCtx) return;
        hwCtx.putImageData(previous, 0, 0);
        hwStrokes = Math.max(0, hwStrokes - 1);
        if (hwStrokeCount) hwStrokeCount.textContent = String(hwStrokes);
    });
    hwNext?.addEventListener('click', hwQuestion);
    hwHear?.addEventListener('click', () => { if (currentHwVocab) speak(currentHwVocab.Japanese); });
    hwCheck?.addEventListener('click', () => {
        if (!hwStrokes) {
            if (hwFeedback) hwFeedback.innerHTML = '<span class="text-danger">Write the character before comparing it.</span>';
            return;
        }
        hwGuideVisible = true;
        drawGuide();
        if (hwStrokeBtn) hwStrokeBtn.innerHTML = '<i class="bi bi-eye"></i> Hide guide';
        if (hwFeedback) hwFeedback.innerHTML = '<span class="text-success">Compare proportion, position and balance with the pale model. Hide the guide and repeat once from memory.</span>';
    });

    hwStrokeBtn?.addEventListener('click', () => {
        hwGuideVisible = !hwGuideVisible;
        drawGuide();
        hwStrokeBtn.innerHTML = hwGuideVisible
            ? '<i class="bi bi-eye"></i> Hide guide'
            : '<i class="bi bi-eye-slash"></i> Show guide';
    });

    if (hwTarget) hwQuestion();

})();
