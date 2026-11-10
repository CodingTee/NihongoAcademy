(function () {
    "use strict";
    const toggle = (buttonId, panelId) => document.getElementById(buttonId)?.addEventListener("click", () => {
        const panel = document.getElementById(panelId); if (panel) panel.style.display = panel.style.display === "none" ? "block" : "none";
    });
    toggle("toggle-glossary", "reading-glossary"); toggle("toggle-notes", "reading-notes");
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    async function submit(card, answer, control) {
        const feedback = card.querySelector(".reading-feedback");
        if (control) control.disabled = true;
        try {
            const response = await fetch("/Practice/RecordReadingAttempt", { method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded", "RequestVerificationToken": token },
                body: new URLSearchParams({ passageId: window.__readingPassageId, questionId: card.dataset.questionId, answer }) });
            if (!response.ok) throw new Error();
            const result = await response.json();
            feedback.className = `reading-feedback mt-2 small ${result.isCorrect ? "text-success" : "text-danger"}`;
            feedback.textContent = result.isCorrect ? `Correct. ${result.explanation || ""}` : `Not quite. Answer: ${result.correctAnswer}. ${result.explanation || ""}`;
            const status = document.getElementById("reading-status");
            status.textContent = result.mastered ? "Stable mastery" : `${result.accuracy}% accuracy`;
            status.classList.toggle("text-success", result.mastered);
        } catch {
            feedback.className = "reading-feedback mt-2 small text-danger"; feedback.textContent = "Could not save this answer. Check your connection and try again.";
        } finally { if (control) control.disabled = false; }
    }
    document.querySelectorAll(".reading-choice").forEach(button => button.addEventListener("click", () => submit(button.closest(".reading-question"), button.dataset.answer, button)));
    document.querySelectorAll(".reading-submit").forEach(button => button.addEventListener("click", () => { const card = button.closest(".reading-question"); const answer = card.querySelector(".reading-input").value.trim(); if (answer) submit(card, answer, button); }));
})();
