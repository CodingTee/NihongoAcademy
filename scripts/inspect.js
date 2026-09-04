const WebSocket = require('ws');

async function inspect() {
    const targets = await fetch('http://localhost:9222/json/list').then(r => r.json());
    const page = targets.find(t => t.type === 'page' && t.url.includes('5023'));
    if (!page) {
        console.error('No page target');
        process.exit(1);
    }

    const ws = new WebSocket(page.webSocketDebuggerUrl);
    let id = 0;
    const pending = new Map();

    ws.on('message', (data) => {
        const msg = JSON.parse(data);
        if (msg.id && pending.has(msg.id)) {
            pending.get(msg.id)(msg.result);
            pending.delete(msg.id);
        }
    });

    function send(method, params = {}) {
        return new Promise((resolve, reject) => {
            const reqId = ++id;
            pending.set(reqId, resolve);
            ws.send(JSON.stringify({ id: reqId, method, params }));
            setTimeout(() => {
                if (pending.has(reqId)) {
                    pending.delete(reqId);
                    reject(new Error('Timeout: ' + method));
                }
            }, 5000);
        });
    }

    await new Promise(r => ws.once('open', r));

    // Wait briefly for page load
    await new Promise(r => setTimeout(r, 1500));

    // Evaluate inspection JS
    const expr = `
        (function() {
            const btn = document.querySelector('.btn-na-primary.btn-lg');
            if (!btn) return JSON.stringify({error: 'btn not found'});
            const cs = window.getComputedStyle(btn);
            const rect = btn.getBoundingClientRect();
            const i = btn.querySelector('i');
            const iCs = i ? window.getComputedStyle(i) : null;
            return JSON.stringify({
                text: btn.textContent,
                innerHTML: btn.innerHTML,
                classList: Array.from(btn.classList),
                rect: { x: rect.x, y: rect.y, w: rect.width, h: rect.height },
                color: cs.color,
                bg: cs.backgroundColor,
                display: cs.display,
                visibility: cs.visibility,
                opacity: cs.opacity,
                overflow: cs.overflow,
                fontSize: cs.fontSize,
                fontFamily: cs.fontFamily,
                textIndent: cs.textIndent,
                lineHeight: cs.lineHeight,
                width: cs.width,
                height: cs.height,
                padding: cs.padding,
                border: cs.border,
                position: cs.position,
                zIndex: cs.zIndex,
                // Check text node rendering
                hasFirstChild: !!btn.firstChild,
                firstChildNodeName: btn.firstChild ? btn.firstChild.nodeName : null,
                firstChildText: btn.firstChild ? btn.firstChild.textContent : null,
                // Check icon
                iconFontFamily: iCs ? iCs.fontFamily : null,
                iconContent: iCs ? iCs.content : null,
                iconDisplay: iCs ? iCs.display : null,
                // Check matched rules
                cssText: btn.style.cssText,
                computedCSS: btn.style.length + ' inline styles'
            }, null, 2);
        })()
    `;

    const result = await send('Runtime.evaluate', {
        expression: expr,
        returnByValue: true
    });

    if (result.exceptionDetails) {
        console.error('JS Error:', JSON.stringify(result.exceptionDetails, null, 2));
    } else {
        console.log('=== BUTTON INSPECTION ===');
        console.log(result.result.value);
    }

    // Also check if Bootstrap Icons CSS loaded
    const expr2 = `
        (function() {
            const styles = document.querySelectorAll('link[rel="stylesheet"]');
            const iconLink = Array.from(styles).find(l => l.href.includes('bootstrap-icons'));
            return JSON.stringify({
                iconLinkHref: iconLink ? iconLink.href : null,
                iconLinkLoaded: iconLink ? iconLink.sheet !== null : false,
                failedToLoad: Array.from(styles).map(l => ({href: l.href, ok: l.sheet !== null}))
            }, null, 2);
        })()
    `;
    const r2 = await send('Runtime.evaluate', { expression: expr2, returnByValue: true });
    console.log('\\n=== STYLESHEETS ===');
    console.log(r2.result.value);

    // Take screenshot
    const shot = await send('Page.captureScreenshot', { format: 'png' });
    const fs = require('fs');
    fs.writeFileSync('/c/Users/nicol/Downloads/NihongoAcademy/NihongoAcademy/screenshots/cdp_home.png', Buffer.from(shot.data, 'base64'));
    console.log('\\nScreenshot saved');

    ws.close();
    process.exit(0);
}

inspect().catch(e => { console.error(e); process.exit(1); });
