(function () {
    'use strict';
    const transcript = document.getElementById('speaking-transcript');
    if (!transcript) return;
    const promptText = document.getElementById('speaking-prompt-text')?.value || '';
    const taskId = document.getElementById('speaking-task-id')?.value;
    const count = document.getElementById('speaking-character-count');
    const feedback = document.getElementById('speaking-feedback');
    const record = document.getElementById('record-speaking');
    const note = document.getElementById('recognition-note');
    let inputMode = 'typed';

    function speak(text, rate) {
        if (!('speechSynthesis' in window)) { note.textContent = 'System speech is not available in this browser. The written prompt remains available.'; return; }
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(text); utterance.lang = 'ja-JP'; utterance.rate = rate || 0.86;
        window.speechSynthesis.speak(utterance);
    }
    function updateCount() { count.textContent = String((transcript.value.match(/[\u3040-\u30ff\u3400-\u9fff]/g) || []).length); }
    transcript.addEventListener('input', () => { inputMode = inputMode === 'voice' ? 'voice' : 'typed'; updateCount(); });
    document.getElementById('speak-prompt')?.addEventListener('click', () => speak(promptText));
    document.querySelector('.speak-model')?.addEventListener('click', e => speak(e.currentTarget.dataset.text));

    const Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!Recognition) { record.disabled = true; note.textContent = 'Speech recognition is unavailable in this browser. Type your spoken response instead; all course checks still work.'; }
    else record.addEventListener('click', () => {
        const recognition = new Recognition(); recognition.lang = 'ja-JP'; recognition.interimResults = false; recognition.continuous = false;
        record.disabled = true; record.innerHTML = '<i class="bi bi-mic-fill"></i> Listening…'; note.textContent = 'Speak now. You can correct the transcript before checking it.';
        recognition.onresult = event => { transcript.value = event.results[0][0].transcript; inputMode = 'voice'; updateCount(); note.textContent = 'Transcript captured. Check it, edit recognition mistakes if needed, then save.'; };
        recognition.onerror = () => { note.textContent = 'The microphone did not return a transcript. You can try again or type your response.'; };
        recognition.onend = () => { record.disabled = false; record.innerHTML = '<i class="bi bi-mic"></i> Speak'; };
        recognition.start();
    });

    document.getElementById('save-speaking')?.addEventListener('click', async e => {
        const value = transcript.value.trim(); if (!value) { feedback.innerHTML = '<div class="alert alert-warning">Speak or type a Japanese response first.</div>'; return; }
        e.currentTarget.disabled = true;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const body = new URLSearchParams({ taskId, transcript: value, inputMode });
        try {
            const response = await fetch('/Practice/SaveSpeakingAttempt', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token }, body });
            if (!response.ok) throw new Error(); const result = await response.json();
            const items = result.feedback.map(item => `<li>${escapeHtml(item)}</li>`).join('');
            feedback.innerHTML = `<div class="alert ${result.passing ? 'alert-success' : 'alert-warning'}"><strong>${result.score}%</strong> · ${result.requirementsMet}/${result.requirementCount} language goals · ${result.japaneseCharacters} Japanese characters<ul class="mb-0 mt-2">${items}</ul><small>Saved. Stable mastery requires passing practice on two different dates.</small></div>`;
            if (result.mastered) { const mastery = document.getElementById('speaking-mastery'); mastery.textContent = 'Stable mastery'; mastery.className = 'text-success'; }
        } catch { feedback.innerHTML = '<div class="alert alert-danger">The response could not be saved. Please try again.</div>'; }
        finally { e.currentTarget.disabled = false; }
    });
    function escapeHtml(value) { const div = document.createElement('div'); div.textContent = value; return div.innerHTML; }
    updateCount();
})();
