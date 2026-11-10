(function () {
    'use strict';
    const script = document.getElementById('listening-script')?.value || '';
    const taskId = document.getElementById('listening-task-id')?.value;
    const status = document.getElementById('playback-status');
    let playbackMode = 'natural';
    function speak(rate, mode) {
        playbackMode = mode;
        if (!('speechSynthesis' in window)) { status.textContent = 'System speech is unavailable in this browser. Try another installed browser.'; return; }
        speechSynthesis.cancel(); const utterance = new SpeechSynthesisUtterance(script); utterance.lang = 'ja-JP'; utterance.rate = rate;
        utterance.onstart = () => status.textContent = `${mode === 'slow' ? 'Slow' : 'Natural'} playback in progress…`;
        utterance.onend = () => status.textContent = 'Playback finished. Answer from what you heard.'; speechSynthesis.speak(utterance);
    }
    document.getElementById('play-slow')?.addEventListener('click', () => speak(0.68, 'slow'));
    document.getElementById('play-natural')?.addEventListener('click', () => speak(0.95, 'natural'));
    document.querySelectorAll('.listening-answer').forEach(button => button.addEventListener('click', () => submit(button.closest('.listening-question'), button.dataset.answer, button)));
    document.querySelectorAll('.listening-submit').forEach(button => button.addEventListener('click', () => { const card=button.closest('.listening-question'); submit(card, card.querySelector('.listening-input').value, button); }));
    async function submit(card, answer, control) {
        if (!answer?.trim()) return; control.disabled = true;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        try {
            const response = await fetch('/Practice/RecordListeningAttempt', { method:'POST', headers:{'Content-Type':'application/x-www-form-urlencoded','RequestVerificationToken':token}, body:new URLSearchParams({taskId,questionId:card.dataset.questionId,answer,playbackMode}) });
            if (!response.ok) throw new Error(); const result=await response.json();
            card.querySelector('.question-feedback').innerHTML=`<div class="alert ${result.isCorrect?'alert-success':'alert-warning'} py-2"><strong>${result.isCorrect?'Correct':'Try again'}</strong>${result.isCorrect?'':` · Answer: ${escapeHtml(result.correctAnswer)}`}<div class="small">${escapeHtml(result.explanation||'')}</div></div>`;
            if (result.mastered) { const el=document.getElementById('listening-mastery'); el.textContent='Stable mastery'; el.className='text-na-accent'; }
            else document.getElementById('listening-mastery').textContent=`${result.accuracy}% accuracy`;
        } catch { card.querySelector('.question-feedback').innerHTML='<div class="alert alert-danger py-2">The answer could not be saved. Please try again.</div>'; }
        finally { control.disabled=false; }
    }
    function escapeHtml(value){const div=document.createElement('div');div.textContent=value;return div.innerHTML;}
})();
