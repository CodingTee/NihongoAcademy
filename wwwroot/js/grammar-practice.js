(function () {
    "use strict";
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    async function submit(task, answer, control) {
        const card = task.closest(".grammar-card");
        const feedback = task.querySelector(".grammar-feedback");
        const body = new URLSearchParams({
            grammarPointId: card.dataset.grammarId,
            exerciseType: task.dataset.type,
            answer: answer
        });
        if (control) control.disabled = true;
        try {
            const response = await fetch("/Practice/RecordGrammarAttempt", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded", "RequestVerificationToken": token },
                body: body
            });
            if (!response.ok) throw new Error("Unable to save grammar attempt");
            const result = await response.json();
            feedback.className = `grammar-feedback small mt-2 ${result.isCorrect ? "text-success" : "text-danger"}`;
            feedback.textContent = result.isCorrect ? `Correct — ${result.accuracy}% accuracy.` :
                `Not quite. Correct answer: ${result.correctAnswer}${result.explanation ? ` — ${result.explanation}` : ""}`;
            const status = card.querySelector(".grammar-status");
            status.textContent = result.mastered ? "Mastered" : `${result.accuracy}%`;
            status.classList.toggle("bg-success", result.mastered);
            status.classList.toggle("text-white", result.mastered);
        } catch {
            feedback.className = "grammar-feedback small mt-2 text-danger";
            feedback.textContent = "This exercise could not be saved. Check your connection and try again.";
        } finally {
            if (control) control.disabled = false;
        }
    }

    document.querySelectorAll(".grammar-option").forEach(button => button.addEventListener("click", () =>
        submit(button.closest(".grammar-task"), button.dataset.answer, button)));
    document.querySelectorAll(".grammar-submit").forEach(button => button.addEventListener("click", () => {
        const task = button.closest(".grammar-task");
        const answer = task.dataset.type === "grammar-order"
            ? [...task.querySelectorAll(".grammar-build-answer .grammar-part")].map(part => part.dataset.part).join("")
            : task.querySelector(".grammar-answer").value;
        if (answer.trim()) submit(task, answer, button);
    }));
    document.querySelectorAll(".grammar-part").forEach(button => button.addEventListener("click", () =>
        button.closest(".grammar-task").querySelector(".grammar-build-answer").appendChild(button)));
    document.querySelectorAll(".grammar-clear").forEach(button => button.addEventListener("click", () => {
        const task = button.closest(".grammar-task");
        const parts = task.querySelector(".grammar-parts");
        [...task.querySelectorAll(".grammar-build-answer .grammar-part")].forEach(part => parts.appendChild(part));
        task.querySelector(".grammar-feedback").textContent = "";
    }));
})();
