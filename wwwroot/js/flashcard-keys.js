/* ==========================================================================
   flashcard-keys.js — the Flashcard Practice page answers to the keyboard

       Space      flip the card
       1 2 3 4    rate it Again / Hard / Good / Easy  (the SM-2 buttons)
       ← / →      previous / next card
       V          hear the word
       X          hear the example sentence

   The SRS maths, the speak buttons and the card flip all stay exactly where
   they were — these keys simply click the same controls, so the numbers on
   the page and the numbers Sensei sees can never disagree.
   ========================================================================== */

(function () {
    'use strict';

    if (!window.naKeys) return;
    var NA = window.naKeys;
    var GROUP = 'Flashcard review — 単語帳';

    function call(name, note, key) {
        return function () {
            var fn = window[name];
            if (typeof fn !== 'function') return false;
            fn();
            NA.toast(note, key);
            return true;
        };
    }

    NA.register({
        keys: [' '],
        group: GROUP,
        label: 'Flip the card — カードをめくる',
        run: call('flipCard', 'Flip', 'Space')
    });

    [['1', 1, 'Again'], ['2', 2, 'Hard'], ['3', 3, 'Good'], ['4', 4, 'Easy']].forEach(function (r) {
        NA.register({
            keys: [r[0]],
            group: GROUP,
            label: 'Rate: ' + r[2],
            run: function () {
                if (typeof window.markSRS !== 'function') return false;
                window.markSRS(r[1]);
                NA.toast(r[2], r[0]);
                return true;
            }
        });
    });

    NA.register({ keys: ['arrowleft'],  group: GROUP, label: 'Previous card', run: call('prevCard', 'Previous card', '←') });
    NA.register({ keys: ['arrowright'], group: GROUP, label: 'Next card',     run: call('nextCard', 'Next card', '→') });
    NA.register({ keys: ['v'],          group: GROUP, label: 'Hear the word',        run: call('speakFlashcardFront', 'Playing', 'V') });
    NA.register({ keys: ['x'],          group: GROUP, label: 'Hear the example',     run: call('speakFlashcardExample', 'Example', 'X') });
})();
