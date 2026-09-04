/* ===== NihongoAcademy — Subtle Sakura Cursor Trail (Global) ===== */
(function () {
    var lastTime = 0;
    var throttle = 80; // ms between particles
    var particles = [];
    var maxParticles = 8;
    var active = false;

    function createPetal(x, y) {
        var el = document.createElement('div');
        el.className = 'na-sakura-petal';
        el.style.left = x + 'px';
        el.style.top = y + 'px';
        // Slight random offset
        var ox = (Math.random() - 0.5) * 12;
        var oy = (Math.random() - 0.5) * 12;
        el.style.transform = 'translate(' + ox + 'px,' + oy + 'px) rotate(' + (Math.random() * 360) + 'deg)';
        document.body.appendChild(el);
        particles.push(el);

        setTimeout(function () {
            el.style.opacity = '0';
            el.style.transform += ' translateY(12px)';
        }, 100);

        setTimeout(function () {
            if (el.parentNode) el.parentNode.removeChild(el);
            var idx = particles.indexOf(el);
            if (idx > -1) particles.splice(idx, 1);
        }, 900);
    }

    function onMouseMove(e) {
        var now = Date.now();
        if (now - lastTime < throttle) return;
        if (particles.length >= maxParticles) return;
        lastTime = now;
        createPetal(e.clientX, e.clientY);
    }

    // Activate on every page — no dependency on AI Tutor FAB
    function activate() {
        if (active) return;
        active = true;
        document.addEventListener('mousemove', onMouseMove, { passive: true });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', activate);
    } else {
        activate();
    }
})();
