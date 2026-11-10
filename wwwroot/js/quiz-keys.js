/* ==========================================================================
   quiz-keys.js — the quiz runner answers to the keyboard

       1 2 3 4    pick an answer for the question on screen
       ← / →      previous / next question
       Enter      submit — or step to the next question when the submit
                  button isn't showing yet

   selectAnswer / nextQuestion / prevQuestion / submitQuiz are the page's own
   functions (site.js); these keys call them directly, so the answer data the
   form posts is exactly what the clicks would have produced.
   ========================================================================== */

(function () {
    'use strict';

    if (!window.naKeys) return;
    var NA = window.naKeys;
    var GROUP = 'Quiz — クイズ';

    function current() {
        return typeof window.currentQuestion === 'number' ? window.currentQuestion : 0;
    }

    ['1', '2', '3', '4'].forEach(function (k) {
        var n = parseInt(k, 10);
        NA.register({
            keys: [k],
            group: GROUP,
            label: 'Answer option ' + n,
            run: function () {
                if (typeof window.selectAnswer !== 'function') return false;
                window.selectAnswer(current(), n);
                NA.toast('Option ' + n, k);
                return true;
            }
        });
    });

    NA.register({
        keys: ['arrowleft'],
        group: GROUP,
        label: 'Previous question',
        run: function () {
            if (typeof window.prevQuestion !== 'function') return false;
            window.prevQuestion();
            NA.toast('Previous question', '←');
            return true;
        }
    });

    NA.register({
        keys: ['arrowright'],
        group: GROUP,
        label: 'Next question',
        run: function () {
            if (typeof window.nextQuestion !== 'function') return false;
            window.nextQuestion();
            NA.toast('Next question', '→');
            return true;
        }
    });

    NA.register({
        keys: ['enter'],
        group: GROUP,
        label: 'Submit the quiz / next question',
        run: function () {
            var submit = document.getElementById('submit-quiz');
            if (submit && submit.offsetParent !== null) {
                if (typeof window.submitQuiz === 'function') window.submitQuiz();
                NA.toast('Submitting', '⏎');
            } else if (typeof window.nextQuestion === 'function') {
                window.nextQuestion();
                NA.toast('Next question', '⏎');
            }
            return true;
        }
    });
})();
