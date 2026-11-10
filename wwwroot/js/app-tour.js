/**
 * NihongoAcademy - Multi-Page Interactive Campus Tour (Silky Smooth 60FPS Edition)
 * High-precision SVG cutout spotlight + smooth rAF scroll tracking + morphing tooltip:
 * Learn (Station 1) -> Practice (Station 2) -> Community (Station 3) -> Progress (Station 4)
 */
(function () {
    var TOUR_KEY = "na_app_tour_completed_v3";
    var SESSION_KEY = "na_tour_active_session";

    var stations = {
        lessons: {
            name: "Learn",
            stationIndex: 1,
            totalStations: 4,
            steps: [
                {
                    selector: "[data-tour='tour-curriculum-scope']",
                    title: "2. Complete Curriculum Overview",
                    text: "Follow a guided path: from Hiragana and Katakana characters to JLPT N5/N4 core grammar patterns and 100 essential Kanji.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-category-filters']",
                    title: "3. Curriculum Stages & Search",
                    text: "Quickly switch between stages (Hiragana, Katakana, A1 Course, Kanji) or search for specific words and grammar topics.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-lesson-card']",
                    title: "4. Interactive Lesson Checkpoints",
                    text: "Every lesson is packed with native pronunciation audio, flashcard decks, and instant recall quizzes to test your mastery.",
                    position: "bottom"
                }
            ],
            nextStationKey: "practice",
            nextStationName: "Practice Drills",
            nextStationUrl: "/Practice?tour=practice"
        },
        practice: {
            name: "Practice",
            stationIndex: 2,
            totalStations: 4,
            steps: [
                {
                    selector: "[data-tour='tour-practice-today']",
                    title: "4. Today's Spaced Repetition",
                    text: "Smart review queue based on the Ebbinghaus forgetting curve. Keep your study streak alive and tackle weak areas before they fade.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-practice-skills']",
                    title: "5. Targeted 5-Skill Training",
                    text: "Drill down into individual skills: listening audio challenges, reading comprehension, speaking prompts, writing canvas, and grammar workouts.",
                    position: "bottom"
                }
            ],
            nextStationKey: "community",
            nextStationName: "Learner Community",
            nextStationUrl: "/Community?tour=community"
        },
        community: {
            name: "Community",
            stationIndex: 3,
            totalStations: 4,
            steps: [
                {
                    selector: "[data-tour='tour-community-tabs']",
                    title: "6. Discussion Channels",
                    text: "Browse discussions filtered by topic: ask grammar questions, exchange study tips, or share travel and Japanese culture stories.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-community-create']",
                    title: "7. Ask Questions & Connect",
                    text: "Whenever you get stuck on a tricky phrase, post your question here for friendly support from other learners.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-community-first-post']",
                    title: "8. Learner Profiles & Post History",
                    text: "Click any learner's avatar or username to view their public profile card, custom anime avatar, badges, and all their shared posts!",
                    position: "bottom"
                }
            ],
            nextStationKey: "dashboard",
            nextStationName: "Progress Cockpit",
            nextStationUrl: "/Dashboard?tour=dashboard"
        },
        dashboard: {
            name: "Progress",
            stationIndex: 4,
            totalStations: 4,
            steps: [
                {
                    selector: "[data-tour='tour-goal-plan']",
                    fallbackSelector: "[data-tour='tour-next-action']",
                    title: "9. Your Personalized Goal Plan",
                    text: "Tailored specifically for you based on the goal and level you picked in onboarding! Includes your weekly study pace and milestone targets.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-next-action']",
                    title: "10. Next Recommended Action",
                    text: "Your intelligent compass. Whenever you log in, this card tells you exactly what lesson or review is waiting for you next.",
                    position: "bottom"
                },
                {
                    selector: "[data-tour='tour-radar']",
                    title: "11. 5-Skill Radar & JLPT Readiness",
                    text: "Real-time visual tracker measuring your Vocabulary, Grammar, Listening, Reading, and Writing strengths, plus JLPT certification readiness.",
                    position: "bottom"
                }
            ],
            nextStationKey: null,
            nextStationName: null,
            nextStationUrl: null
        }
    };

    var currentStation = "lessons";
    var currentStepIndex = 0;
    var svgOverlay = null;
    var spotlightBorder = null;
    var tooltip = null;
    var rafTrackingId = null;
    var keyListenerBound = false;

    function detectStation() {
        var path = window.location.pathname.toLowerCase();
        var urlParams = new URLSearchParams(window.location.search);
        var tourParam = urlParams.get("tour");

        if (tourParam === "practice" || path.includes("/practice")) return "practice";
        if (tourParam === "community" || path.includes("/community")) return "community";
        if (tourParam === "dashboard" || path.includes("/dashboard")) return "dashboard";
        return "lessons";
    }

    /* ===== High-Precision SVG Cutout Spotlight ===== */
    function createCutoutOverlay() {
        if (svgOverlay) return;

        svgOverlay = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svgOverlay.setAttribute("class", "na-tour-svg-cutout");
        svgOverlay.setAttribute("aria-hidden", "true");

        var path = document.createElementNS("http://www.w3.org/2000/svg", "path");
        path.setAttribute("id", "naTourCutoutPath");
        path.setAttribute("fill-rule", "evenodd");
        svgOverlay.appendChild(path);

        document.body.appendChild(svgOverlay);

        spotlightBorder = document.createElement("div");
        spotlightBorder.className = "na-tour-spotlight-border";
        document.body.appendChild(spotlightBorder);

        tooltip = document.createElement("div");
        tooltip.className = "na-tour-tooltip";
        tooltip.innerHTML = '<div class="na-tour-tooltip-body" id="naTourTooltipBody"></div>';
        document.body.appendChild(tooltip);

        // Smooth fade-in
        requestAnimationFrame(function () {
            if (svgOverlay) svgOverlay.classList.add("active");
        });

        window.addEventListener("resize", handleWindowSync);
        window.addEventListener("scroll", handleWindowSync, { passive: true });

        if (!keyListenerBound) {
            window.addEventListener("keydown", handleKeyNavigation);
            keyListenerBound = true;
        }
    }

    function removeCutoutOverlay() {
        if (rafTrackingId) {
            cancelAnimationFrame(rafTrackingId);
            rafTrackingId = null;
        }

        if (svgOverlay && svgOverlay.parentNode) {
            svgOverlay.classList.remove("active");
            setTimeout(function () {
                if (svgOverlay && svgOverlay.parentNode) svgOverlay.parentNode.removeChild(svgOverlay);
                svgOverlay = null;
            }, 250);
        }

        if (spotlightBorder && spotlightBorder.parentNode) {
            spotlightBorder.parentNode.removeChild(spotlightBorder);
            spotlightBorder = null;
        }

        if (tooltip && tooltip.parentNode) {
            tooltip.classList.remove("visible");
            setTimeout(function () {
                if (tooltip && tooltip.parentNode) tooltip.parentNode.removeChild(tooltip);
                tooltip = null;
            }, 200);
        }

        window.removeEventListener("resize", handleWindowSync);
        window.removeEventListener("scroll", handleWindowSync);
        if (keyListenerBound) {
            window.removeEventListener("keydown", handleKeyNavigation);
            keyListenerBound = false;
        }
    }

    function updateCutout(rect) {
        if (!svgOverlay) return;
        var pathEl = document.getElementById("naTourCutoutPath");
        if (!pathEl) return;

        var w = window.innerWidth;
        var h = window.innerHeight;
        var pad = 10;
        var r = 12; // sleek rounded corner radius

        var x = Math.max(0, rect.left - pad);
        var y = Math.max(0, rect.top - pad);
        var rw = Math.min(w, rect.width + pad * 2);
        var rh = Math.min(h, rect.height + pad * 2);

        // SVG Path with evenodd: Outer viewport rectangle + inner rounded cutout
        var d = "M 0 0 h " + w + " v " + h + " h -" + w + " Z ";
        d += "M " + (x + r) + " " + y + " ";
        d += "h " + (rw - 2 * r) + " ";
        d += "a " + r + " " + r + " 0 0 1 " + r + " " + r + " ";
        d += "v " + (rh - 2 * r) + " ";
        d += "a " + r + " " + r + " 0 0 1 -" + r + " " + r + " ";
        d += "h -" + (rw - 2 * r) + " ";
        d += "a " + r + " " + r + " 0 0 1 -" + r + " -" + r + " ";
        d += "v -" + (rh - 2 * r) + " ";
        d += "a " + r + " " + r + " 0 0 1 " + r + " -" + r + " Z";

        pathEl.setAttribute("d", d);

        if (spotlightBorder) {
            spotlightBorder.style.top = y + "px";
            spotlightBorder.style.left = x + "px";
            spotlightBorder.style.width = rw + "px";
            spotlightBorder.style.height = rh + "px";
            spotlightBorder.style.display = "block";
        }
    }

    function positionTooltip(rect) {
        if (!tooltip) return;
        var tipW = 350;
        var tipH = tooltip.offsetHeight || 180;
        var spaceBelow = window.innerHeight - rect.bottom;
        var spaceAbove = rect.top;

        var top = rect.bottom + 16;
        var left = rect.left;

        // If not enough room below, place above element
        if (spaceBelow < (tipH + 24) && spaceAbove > (tipH + 24)) {
            top = Math.max(16, rect.top - tipH - 16);
        }

        // Clamp horizontally within screen
        if (left + tipW > window.innerWidth - 16) {
            left = Math.max(16, window.innerWidth - tipW - 16);
        }
        if (left < 16) left = 16;

        tooltip.style.top = top + "px";
        tooltip.style.left = left + "px";
        tooltip.classList.add("visible");
    }

    /* ===== Real-Time 60FPS Scroll Tracking Loop ===== */
    function trackElementSmooth(el, durationMs, onFinished) {
        if (rafTrackingId) cancelAnimationFrame(rafTrackingId);

        var startTime = performance.now();
        var prevTop = null;
        var stableCount = 0;

        function tick(now) {
            if (!el || !document.contains(el)) return;
            var rect = el.getBoundingClientRect();
            updateCutout(rect);
            positionTooltip(rect);

            var elapsed = now - startTime;
            // Check if scroll has settled
            if (prevTop !== null && Math.abs(rect.top - prevTop) < 0.5) {
                stableCount++;
            } else {
                stableCount = 0;
            }
            prevTop = rect.top;

            // Continue tracking until both duration has passed AND scroll has stabilized
            if (elapsed < durationMs || stableCount < 5) {
                rafTrackingId = requestAnimationFrame(tick);
            } else {
                rafTrackingId = null;
                if (onFinished) onFinished(rect);
            }
        }

        rafTrackingId = requestAnimationFrame(tick);
    }

    function handleWindowSync() {
        if (!tooltip || !tooltip.classList.contains("visible")) return;
        var stationObj = stations[currentStation];
        if (!stationObj) return;
        var step = stationObj.steps[currentStepIndex];
        if (!step) return;
        var el = document.querySelector(step.selector) || (step.fallbackSelector ? document.querySelector(step.fallbackSelector) : null);
        if (el) {
            var rect = el.getBoundingClientRect();
            updateCutout(rect);
            positionTooltip(rect);
        }
    }

    function handleKeyNavigation(e) {
        if (!tooltip || !tooltip.classList.contains("visible")) return;
        if (e.key === "ArrowRight" || e.key === "Enter") {
            e.preventDefault();
            var nextBtn = document.getElementById("naTourNext");
            if (nextBtn) nextBtn.click();
        } else if (e.key === "ArrowLeft") {
            e.preventDefault();
            var prevBtn = document.getElementById("naTourPrev");
            if (prevBtn) prevBtn.click();
        } else if (e.key === "Escape") {
            e.preventDefault();
            endTour();
        }
    }

    function showModal(config) {
        var existing = document.getElementById("naTourModal");
        if (existing) existing.remove();

        var modal = document.createElement("div");
        modal.id = "naTourModal";
        modal.className = "na-tour-welcome-backdrop";
        modal.innerHTML =
            '<div class="na-tour-welcome-card">' +
                '<div class="na-tour-welcome-badge"><i class="bi bi-compass"></i> ' + (config.badge || "Campus Tour") + '</div>' +
                '<h3 class="mt-2 mb-2">' + config.title + '</h3>' +
                '<p class="text-muted mb-4">' + config.text + '</p>' +
                '<div class="d-flex justify-content-center gap-3">' +
                    (config.secondaryText ? '<button type="button" class="btn-na-outline btn-sm px-3" id="naTourModalSecBtn">' + config.secondaryText + '</button>' : '') +
                    '<button type="button" class="btn-na-primary btn-sm px-4" id="naTourModalPriBtn">' + config.primaryText + '</button>' +
                '</div>' +
            '</div>';

        document.body.appendChild(modal);

        document.getElementById("naTourModalPriBtn").onclick = function () {
            modal.remove();
            if (config.onPrimary) config.onPrimary();
        };

        if (config.secondaryText) {
            document.getElementById("naTourModalSecBtn").onclick = function () {
                modal.remove();
                if (config.onSecondary) config.onSecondary();
            };
        }
    }

    function renderStep(stationKey, stepIndex) {
        var stationObj = stations[stationKey];
        if (!stationObj || stepIndex >= stationObj.steps.length) {
            handleStationComplete(stationKey);
            return;
        }

        var step = stationObj.steps[stepIndex];
        var el = document.querySelector(step.selector) || (step.fallbackSelector ? document.querySelector(step.fallbackSelector) : null);

        if (!el) {
            // Target not found on page, skip smoothly to next
            renderStep(stationKey, stepIndex + 1);
            return;
        }

        // 1. Cross-fade tooltip content out slightly during transition
        var bodyEl = document.getElementById("naTourTooltipBody");
        if (bodyEl) bodyEl.classList.add("fading");

        // 2. Smoothly scroll target into center
        el.scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });

        // 3. Track spotlight coordinates at 60FPS across the entire scroll glide
        trackElementSmooth(el, 400, function (rect) {
            var globalStepNumber = 1;
            if (stationKey === "practice") globalStepNumber = 5 + stepIndex;
            else if (stationKey === "community") globalStepNumber = 7 + stepIndex;
            else if (stationKey === "dashboard") globalStepNumber = 10 + stepIndex;
            else globalStepNumber = 1 + stepIndex;

            // Generate Step Progress Dots
            var dotsHtml = '<div class="na-tour-step-dots">';
            for (var i = 0; i < stationObj.steps.length; i++) {
                dotsHtml += '<span class="na-tour-dot ' + (i === stepIndex ? 'active' : '') + '"></span>';
            }
            dotsHtml += '</div>';

            if (bodyEl) {
                bodyEl.innerHTML =
                    dotsHtml +
                    '<div class="na-tour-step-count">Station ' + stationObj.stationIndex + ' of ' + stationObj.totalStations + ' (' + stationObj.name + ') · Step ' + globalStepNumber + ' of 12</div>' +
                    '<h5 class="na-tour-step-title">' + step.title + '</h5>' +
                    '<p class="na-tour-step-text">' + step.text + '</p>' +
                    '<div class="na-tour-step-actions">' +
                        (stepIndex > 0 ? '<button type="button" class="btn-na-outline btn-sm" id="naTourPrev"><i class="bi bi-arrow-left"></i> Back</button>' : '<span></span>') +
                        '<div class="d-flex align-items-center gap-2">' +
                            '<button type="button" class="btn btn-link btn-sm text-muted text-decoration-none" id="naTourExit">Skip</button>' +
                            '<button type="button" class="btn-na-primary btn-sm" id="naTourNext">' +
                                (stepIndex === stationObj.steps.length - 1 ? 'Station Done <i class="bi bi-check-lg"></i>' : 'Next <i class="bi bi-arrow-right"></i>') +
                            '</button>' +
                        '</div>' +
                    '</div>';

                // Fade content back in smoothly
                requestAnimationFrame(function () {
                    bodyEl.classList.remove("fading");
                });
            }

            positionTooltip(rect);

            document.getElementById("naTourNext").onclick = function () {
                currentStepIndex++;
                renderStep(stationKey, currentStepIndex);
            };

            if (document.getElementById("naTourPrev")) {
                document.getElementById("naTourPrev").onclick = function () {
                    currentStepIndex--;
                    renderStep(stationKey, currentStepIndex);
                };
            }

            document.getElementById("naTourExit").onclick = function () {
                endTour();
            };
        });
    }

    function handleStationComplete(stationKey) {
        removeCutoutOverlay();
        var stationObj = stations[stationKey];

        if (stationObj.nextStationKey) {
            showModal({
                badge: "Station " + stationObj.stationIndex + " of 4 Complete",
                title: "Next: " + stationObj.nextStationName + " 🎯",
                text: "Great job exploring " + stationObj.name + "! Ready to visit the next space along the navigation path?",
                primaryText: "Explore " + stationObj.nextStationName + " →",
                secondaryText: "Finish for now",
                onPrimary: function () {
                    sessionStorage.setItem(SESSION_KEY, stationObj.nextStationKey);
                    window.location.href = stationObj.nextStationUrl;
                },
                onSecondary: function () {
                    endTour();
                }
            });
        } else {
            // All 4 stations completed!
            endTour();
            showModal({
                badge: "Campus Tour Complete! 🌸",
                title: "You're All Set! 🎉",
                text: "You've toured all 4 key spaces: Learn, Practice, Community, and Progress. Time to dive into your first lesson!",
                primaryText: "Start Learning Now 🚀",
                secondaryText: "Explore freely",
                onPrimary: function () {
                    window.location.href = "/Lessons";
                },
                onSecondary: function () {
                    // Stay on current page
                }
            });
        }
    }

    function startStation(stationKey) {
        currentStation = stationKey;
        currentStepIndex = 0;
        createCutoutOverlay();
        renderStep(stationKey, 0);
    }

    function endTour() {
        removeCutoutOverlay();
        sessionStorage.removeItem(SESSION_KEY);
        localStorage.setItem(TOUR_KEY, "1");
    }

    // Auto-bootstrap
    document.addEventListener("DOMContentLoaded", function () {
        var urlParams = new URLSearchParams(window.location.search);
        var tourParam = urlParams.get("tour");
        var activeSession = sessionStorage.getItem(SESSION_KEY);
        var forceTrigger = window.naTriggerAppTour === true;
        var hasSeen = localStorage.getItem(TOUR_KEY) === "1";
        var currentRouteStation = detectStation();

        // 1. Continuing a multi-page session from previous transition
        if (tourParam && stations[tourParam]) {
            sessionStorage.removeItem(SESSION_KEY);
            setTimeout(function () {
                startStation(tourParam);
            }, 600);
            return;
        }

        if (activeSession && stations[activeSession]) {
            sessionStorage.removeItem(SESSION_KEY);
            setTimeout(function () {
                startStation(activeSession);
            }, 600);
            return;
        }

        // 2. Initial Onboarding Completion: welcome modal points directly to Station 1 (Learn / Lessons)
        if (forceTrigger) {
            setTimeout(function () {
                showModal({
                    badge: "Welcome Campus Tour",
                    title: "Welcome to NihongoAcademy! 🌸",
                    text: "Let's take a quick guided tour following the navigation bar: Learn → Practice → Community → Progress.",
                    primaryText: "Start Tour <i class='bi bi-arrow-right'></i>",
                    secondaryText: "Skip for now",
                    onPrimary: function () {
                        if (window.location.pathname.toLowerCase().includes("/lessons")) {
                            startStation("lessons");
                        } else {
                            window.location.href = "/Lessons?tour=lessons";
                        }
                    },
                    onSecondary: function () {
                        endTour();
                    }
                });
            }, 700);
            return;
        }

        // 3. First time on lessons if never seen
        if (!hasSeen && currentRouteStation === "lessons" && window.location.pathname.toLowerCase().includes("/lessons")) {
            setTimeout(function () {
                showModal({
                    badge: "Campus Tour",
                    title: "Welcome to NihongoAcademy! 🌸",
                    text: "Would you like a quick walkthrough following our core spaces: Learn, Practice, Community, and Progress?",
                    primaryText: "Start Tour <i class='bi bi-arrow-right'></i>",
                    secondaryText: "Skip for now",
                    onPrimary: function () {
                        startStation("lessons");
                    },
                    onSecondary: function () {
                        endTour();
                    }
                });
            }, 1000);
        }
    });

    window.SakuraTour = {
        start: function (force) {
            localStorage.removeItem(TOUR_KEY);
            sessionStorage.removeItem(SESSION_KEY);
            window.location.href = "/Lessons?tour=lessons";
        }
    };
})();
