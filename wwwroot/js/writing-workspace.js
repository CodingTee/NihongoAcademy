(function () {
    "use strict";
    const input = document.getElementById("writing-content");
    const jpCount = document.getElementById("jp-character-count");
    const sentenceCount = document.getElementById("sentence-count");
    function updateCounts() {
        const text = input?.value || "";
        jpCount.textContent = (text.match(/[\u3040-\u30ff\u3400-\u9fff]/g) || []).length;
        sentenceCount.textContent = text.split(/[。！？\n]+/).filter(part => part.trim()).length;
    }
    input?.addEventListener("input", updateCounts); updateCounts();
    document.getElementById("save-writing")?.addEventListener("click", async event => {
        const feedback = document.getElementById("writing-feedback"); const content = input.value.trim();
        if (!content) { feedback.className = "mt-3 text-danger"; feedback.textContent = "Write something before saving."; return; }
        const button = event.currentTarget; button.disabled = true;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        try {
            const response = await fetch("/Practice/SaveWritingSubmission", { method: "POST", headers: { "Content-Type": "application/x-www-form-urlencoded", "RequestVerificationToken": token }, body: new URLSearchParams({ taskId: document.getElementById("writing-task-id").value, content }) });
            if (!response.ok) throw new Error();
            const result = await response.json(); feedback.className = `mt-3 alert ${result.passing ? "alert-success" : "alert-warning"}`;
            const heading = document.createElement("strong"); heading.textContent = `Saved · ${result.score}% structure score`;
            const list = document.createElement("ul"); list.className = "mb-0 mt-2"; result.feedback.forEach(note => { const item = document.createElement("li"); item.textContent = note; list.appendChild(item); });
            feedback.replaceChildren(heading, list); const mastery = document.getElementById("writing-mastery"); mastery.textContent = result.mastered ? "Stable mastery" : "Saved — revisit on a later date"; mastery.className = result.mastered ? "text-success" : "text-na-primary";
        } catch {
            feedback.className = "mt-3 alert alert-danger"; feedback.textContent = "This version could not be saved. Check your connection and try again.";
        } finally { button.disabled = false; }
    });
})();
