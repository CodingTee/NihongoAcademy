/* ===== NihongoAcademy AI Tutor (Ask Sensei) ===== */

(function () {
    var panel = null;
    var fab = null;
    var messagesEl = null;
    var inputEl = null;
    var quickPromptsEl = null;
    var isOpen = false;
    var conversationHistory = [];
    var lessonContext = null;
    var isSending = false;

    // Sakura SVG icon — 5-petal sakura using 5 rotated ellipses, viewBox 0 0 100 100
    var sakuraSvg = '<svg class="ai-sakura-icon" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">'
        + '<g transform="translate(50 50)">'
        // 5 petals: ellipses rotated 0, 72, 144, 216, 288 degrees
        + '<ellipse cx="0" cy="-22" rx="14" ry="20" fill="#FFB7C5"/>'
        + '<ellipse cx="0" cy="-22" rx="14" ry="20" fill="#FFB7C5" transform="rotate(72)"/>'
        + '<ellipse cx="0" cy="-22" rx="14" ry="20" fill="#FFB7C5" transform="rotate(144)"/>'
        + '<ellipse cx="0" cy="-22" rx="14" ry="20" fill="#FFB7C5" transform="rotate(216)"/>'
        + '<ellipse cx="0" cy="-22" rx="14" ry="20" fill="#FFB7C5" transform="rotate(288)"/>'
        // Center stamen
        + '<circle cx="0" cy="0" r="7" fill="#C84B31"/>'
        // Tiny highlight
        + '<circle cx="-2" cy="-2" r="1.8" fill="#FFD9DD"/>'
        + '</g>'
        + '</svg>';

    function init() {
        if (document.getElementById('na-tutor-fab')) return;

        lessonContext = window.__naLessonContext || null;

        // Create FAB
        fab = document.createElement('div');
        fab.id = 'na-tutor-fab';
        fab.className = 'na-tutor-fab';
        fab.innerHTML = sakuraSvg + '<span class="na-tutor-tooltip">Ask Sensei</span>';
        fab.addEventListener('click', togglePanel);
        document.body.appendChild(fab);

        // Create panel
        panel = document.createElement('div');
        panel.id = 'na-tutor-panel';
        panel.className = 'na-tutor-panel';

        var headerContext = lessonContext
            ? '<small>' + escapeHtml(lessonContext.title) + (lessonContext.japaneseTitle ? ' / ' + escapeHtml(lessonContext.japaneseTitle) : '') + '</small>'
            : '<small>NihongoAcademy</small>';

        panel.innerHTML =
            '<div class="na-tutor-header">'
            + '<div class="na-tutor-header-left">'
            + sakuraSvg
            + '<div><h6>Ask Sensei</h6>' + headerContext + '</div>'
            + '</div>'
            + '<button class="na-tutor-close" title="Close">&times;</button>'
            + '</div>'
            + '<div class="na-tutor-messages" id="na-tutor-messages"></div>'
            + '<div class="na-tutor-quick-prompts" id="na-tutor-quick-prompts">'
            + '<button class="na-tutor-quick-btn" data-prompt="Explain this lesson in simpler terms">Explain this lesson</button>'
            + '<button class="na-tutor-quick-btn" data-prompt="Give me more examples related to this lesson">Give me examples</button>'
            + '<button class="na-tutor-quick-btn" data-prompt="Quiz me on what this lesson teaches">Quiz me</button>'
            + '<button class="na-tutor-quick-btn" data-prompt="Help me with pronunciation and romanization">Pronunciation help</button>'
            + '</div>'
            + '<div class="na-tutor-input-area">'
            + '<input type="text" class="na-tutor-input" id="na-tutor-input" placeholder="Ask a question..." />'
            + '<button class="na-tutor-send" id="na-tutor-send"><i class="bi bi-arrow-up"></i></button>'
            + '</div>';

        document.body.appendChild(panel);

        messagesEl = document.getElementById('na-tutor-messages');
        inputEl = document.getElementById('na-tutor-input');
        quickPromptsEl = document.getElementById('na-tutor-quick-prompts');

        // Close button
        panel.querySelector('.na-tutor-close').addEventListener('click', closePanel);

        // Send on button click
        document.getElementById('na-tutor-send').addEventListener('click', sendMessage);

        // Send on Enter
        inputEl.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        // Quick prompts
        quickPromptsEl.addEventListener('click', function (e) {
            var btn = e.target.closest('.na-tutor-quick-btn');
            if (btn) {
                var prompt = btn.getAttribute('data-prompt');
                inputEl.value = prompt;
                sendMessage();
            }
        });

        // Initial greeting
        addBotMessage('Hi! Ask me anything about this lesson. \ud83c\udf38');
    }

    function togglePanel() {
        if (isOpen) closePanel();
        else openPanel();
    }

    function openPanel() {
        panel.classList.add('na-tutor-open');
        fab.style.display = 'none';
        isOpen = true;
        inputEl.focus();
    }

    function closePanel() {
        panel.classList.remove('na-tutor-open');
        fab.style.display = 'flex';
        isOpen = false;
    }

    function addBotMessage(text) {
        var div = document.createElement('div');
        div.className = 'na-tutor-msg na-tutor-msg-bot';
        div.textContent = text;
        messagesEl.appendChild(div);
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    function addUserMessage(text) {
        var div = document.createElement('div');
        div.className = 'na-tutor-msg na-tutor-msg-user';
        div.textContent = text;
        messagesEl.appendChild(div);
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    function showTyping() {
        var div = document.createElement('div');
        div.className = 'na-tutor-msg na-tutor-msg-bot na-tutor-typing';
        div.id = 'na-tutor-typing-indicator';
        div.innerHTML = '<span></span><span></span><span></span>';
        messagesEl.appendChild(div);
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    function hideTyping() {
        var el = document.getElementById('na-tutor-typing-indicator');
        if (el) el.remove();
    }

    async function sendMessage() {
        if (isSending) return;

        var text = inputEl.value.trim();
        if (!text) return;

        inputEl.value = '';
        addUserMessage(text);
        conversationHistory.push({ role: 'user', content: text });

        // Hide quick prompts after first message
        if (quickPromptsEl) quickPromptsEl.style.display = 'none';

        isSending = true;
        document.getElementById('na-tutor-send').disabled = true;
        showTyping();

        try {
            var response = await callTutorApi(text);
            hideTyping();
            addBotMessage(response);
            conversationHistory.push({ role: 'assistant', content: response });
        } catch (err) {
            hideTyping();
            addBotMessage('Sorry, I couldn\'t process that. Please try again.');
        }

        isSending = false;
        document.getElementById('na-tutor-send').disabled = false;
        inputEl.focus();
    }

    async function callTutorApi(userMessage) {
        var payload = {
            message: userMessage,
            lessonId: lessonContext ? lessonContext.lessonId : null,
            history: conversationHistory.slice(-10) // last 10 messages
        };

        var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

        var res = await fetch('/api/tutor', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            throw new Error('API error: ' + res.status);
        }

        var data = await res.json();
        return data.reply || 'I\'m not sure how to answer that. Try rephrasing!';
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // Init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
