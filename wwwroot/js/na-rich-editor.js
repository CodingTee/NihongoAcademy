/* na-rich-editor.js — lightweight WYSIWYG editor for admin lesson Content.
   Auto-initialises every [data-rich-editor]; a MutationObserver keeps it
   working inside the admin shell pane (content swapped via fetch). */
(function () {
    'use strict';

    var BLOCKS = { p: '<p>', h2: '<h2>', h3: '<h3>', blockquote: '<blockquote>', pre: '<pre>' };
    var STATE_CMDS = ['bold', 'italic', 'underline', 'strikeThrough', 'insertUnorderedList', 'insertOrderedList'];

    function initEditor(root) {
        if (root.__naRichInit) return;
        var area = root.querySelector('[data-rich-area]');
        var source = root.querySelector('[data-rich-source-text]');
        var toolbar = root.querySelector('[data-rich-toolbar]');
        if (!area || !source || !toolbar) return;
        root.__naRichInit = true;

        var form = root.closest('form');

        function isVisual() { return root.getAttribute('data-rich-mode') !== 'html'; }
        function sync() { if (isVisual()) source.value = area.innerHTML; }
        if (form) form.addEventListener('submit', sync);

        /* Keep text selection alive when clicking toolbar buttons */
        toolbar.addEventListener('mousedown', function (e) {
            if (e.target.closest('button, select, input, label')) e.preventDefault();
        });

        function refocus() { area.focus(); }

        toolbar.addEventListener('click', function (e) {
            var btn = e.target.closest('button');
            if (!btn) return;
            if (btn.hasAttribute('data-rich-source-btn')) { toggleSource(btn); return; }
            if (btn.hasAttribute('data-rich-link-btn')) { insertLink(); return; }
            var cmd = btn.getAttribute('data-cmd');
            if (!cmd) return;
            refocus();
            document.execCommand(cmd, false, null);
            updateState();
        });

        var blockSel = toolbar.querySelector('[data-rich-block]');
        if (blockSel) blockSel.addEventListener('change', function () {
            refocus();
            document.execCommand('formatBlock', false, BLOCKS[this.value] || '<p>');
            updateState();
        });

        var colorInput = toolbar.querySelector('[data-rich-color]');
        if (colorInput) colorInput.addEventListener('input', function () {
            refocus();
            document.execCommand('styleWithCSS', false, true);
            document.execCommand('foreColor', false, this.value);
        });

        function insertLink() {
            var sel = window.getSelection();
            var collapsed = !sel || sel.rangeCount === 0 || sel.isCollapsed;
            var url = window.prompt('Link URL', 'https://');
            if (!url || url === 'https://') return;
            refocus();
            if (collapsed) {
                var safe = url.replace(/"/g, '&quot;').replace(/</g, '&lt;');
                document.execCommand('insertHTML', false, '<a href="' + safe + '">' + safe + '</a>');
            } else {
                document.execCommand('createLink', false, url);
            }
        }

        function toggleSource(btn) {
            if (isVisual()) {
                sync();
                area.textContent = source.value;          /* textContent auto-escapes the HTML */
                root.setAttribute('data-rich-mode', 'html');
                btn.classList.add('active');
            } else {
                area.innerHTML = source.value;
                root.removeAttribute('data-rich-mode');
                btn.classList.remove('active');
            }
        }

        function updateState() {
            if (!isVisual()) return;
            var sel = window.getSelection();
            var inside = sel && sel.anchorNode && root.contains(sel.anchorNode);
            STATE_CMDS.forEach(function (cmd) {
                var btn = toolbar.querySelector('[data-cmd="' + cmd + '"]');
                if (btn) btn.classList.toggle('active', inside && document.queryCommandState(cmd));
            });
            if (blockSel) {
                var val = 'p';
                if (inside && sel.anchorNode.nodeType === 1 || (sel && sel.anchorNode && sel.anchorNode.parentElement)) {
                    var node = sel.anchorNode.nodeType === 1 ? sel.anchorNode : sel.anchorNode.parentElement;
                    var block = node && node.closest ? node.closest('h2,h3,blockquote,pre') : null;
                    if (block) val = block.tagName.toLowerCase();
                }
                blockSel.value = val;
            }
        }

        document.addEventListener('selectionchange', updateState);
    }

    function initAll() {
        document.querySelectorAll('[data-rich-editor]').forEach(initEditor);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }

    /* Admin shell swaps pane content without a page load — watch for new editors */
    if (window.MutationObserver && document.body) {
        new MutationObserver(initAll).observe(document.body, { childList: true, subtree: true });
    }
})();
