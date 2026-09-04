// ===== NihongoAcademy Avatar Builder =====
// Composes a simple SVG avatar from selectable parts.
// All hairstyles use PNG images with SVG filter recolouring.

(function () {
    var defaults = {
        skinTone: "#F5C4B3",
        hairColor: "#4A3426",
        hairStyle: "short-bob",
        expression: "smile",
        accessory: "none",
        background: "none"
    };

    var options = {
        skinTones: ["#F5C4B3", "#E5C2A8", "#C9966B", "#8D5E3C", "#5E3A22"],
        hairColors: ["#4A3426", "#222222", "#F0C040", "#A0522D", "#C84B31", "#FFFFFF"],
        hairStyles: ["short-bob", "long-wavy", "spiky", "ponytail", "buzz", "mens-spiky"],
        expressions: ["smile", "happy", "surprised", "calm", "wink"],
        accessories: ["none", "glasses", "headphones", "sakura-pin", "bow-tie"],
        backgrounds: [
            { value: "none", label: "None", color: "transparent" },
            { value: "#FFF0F3", label: "Pink", color: "#FFF0F3" },
            { value: "#FFF8E1", label: "Cream", color: "#FFF8E1" },
            { value: "#F5EDE0", label: "Beige", color: "#F5EDE0" },
            { value: "#E3F2FD", label: "Sky Blue", color: "#E3F2FD" },
            { value: "#E8F5E9", label: "Sage Green", color: "#E8F5E9" },
            { value: "#EDE7F6", label: "Lavender", color: "#EDE7F6" }
        ]
    };

    // ======================================================
    // HAIRSTYLE IMAGE CONFIG
    // All values are based on a 120 × 120 avatar.
    // ======================================================

    var hairImages = {
        "short-bob": {
            src: "/images/avatar/hair/short-bob.png",
            // 更宽一点 + 往左一点，盖住耳朵白边
            x: 1,
            y: -5,
            width: 116,
            height: 115
        },

        "long-wavy": {
            src: "/images/avatar/hair/long-wavy.png",
            x: 5,
            y: 1,
            width: 106,
            height: 110
        },

        "spiky": {
            src: "/images/avatar/hair/spiky.png",
            // 往左一点点
            x: 6,
            y: -6,
            width: 101,
            height: 93
        },

        "ponytail": {
            src: "/images/avatar/hair/ponytail.png",

            // 往左一点点
            x: 4,

            // 放大时往上一点维持中心
            y: -4,

            // 只放大一点点
            width: 120,
            height: 118
        },

        "buzz": {
            src: "/images/avatar/hair/buzz.png",
            x: 20.8,
            y: 18,
            width: 78,
            height: 62
        },

        "mens-spiky": {
            src: "/images/avatar/hair/mens-spiky.png",
            x: 4,
            y: -2,
            width: 109,
            height: 95
        }
    };


    // ======================================================
    // FACE PROFILE PER HAIRSTYLE
    //
    // top       = how far forehead extends upward
    // bottom    = lower face position
    // rx        = face width
    // cxOffset  = tiny horizontal face correction
    //
    // Each hairstyle now gets its OWN face.
    // ======================================================

    var faceProfiles = {

        "short-bob": {
            top: 0.262,
            bottom: 0.738,

            // 再宽一点，专门补左右耳朵白边
            rx: 0.316,

            // 不要再偏左了，回中一点
            cxOffset: 0,

            eyeY: 0.512,
            eyeOffsetX: 0.140,
            mouthY: 0.626
        },

        "long-wavy": {
            // expression 往上一点点
            top: 0.220,
            bottom: 0.736,
            rx: 0.250,
            cxOffset: 0,

            eyeY: 0.498,
            eyeOffsetX: 0.130,
            mouthY: 0.610
        },

        "spiky": {
            top: 0.282,
            bottom: 0.740,
            rx: 0.244,
            cxOffset: -0.002,

            eyeY: 0.510,
            eyeOffsetX: 0.134,
            mouthY: 0.626
        },

        "ponytail": {
            top: 0.294,
            bottom: 0.738,
            rx: 0.226,
            cxOffset: -0.010,

            eyeY: 0.512,
            eyeOffsetX: 0.130,
            mouthY: 0.628
        },

        "buzz": {
            top: 0.302,
            bottom: 0.744,
            rx: 0.242,
            cxOffset: 0,

            eyeY: 0.512,
            eyeOffsetX: 0.138,
            mouthY: 0.628
        },

        "mens-spiky": {
            top: 0.282,
            bottom: 0.740,
            rx: 0.244,
            cxOffset: -0.002,

            eyeY: 0.510,
            eyeOffsetX: 0.135,
            mouthY: 0.626
        }
    };


    // ======================================================
    // HAIR RENDERING
    // ======================================================

    function renderHair(config, size) {
        var hair = hairImages[config.hairStyle];

        if (!hair) {
            return "";
        }

        var scale = size / 120;

        return '<image' +
            ' href="' + hair.src + '"' +
            ' x="' + (hair.x * scale) + '"' +
            ' y="' + (hair.y * scale) + '"' +
            ' width="' + (hair.width * scale) + '"' +
            ' height="' + (hair.height * scale) + '"' +
            ' preserveAspectRatio="none"' +
            ' filter="url(#hairColorFilter)"' +
            '/>';
    }


    // ======================================================
    // AVATAR RENDERING
    // ======================================================

    function renderAvatar(config, container) {

        if (typeof container === "string") {
            container = document.querySelector(container);
        }

        if (!container) {
            return;
        }

        config = config || {};

        config.skinTone = config.skinTone || "#F2C3AF";
        config.hairColor = config.hairColor || "#5B3927";
        config.hairStyle = config.hairStyle || "short-bob";
        config.expression = config.expression || "smile";
        config.accessory = config.accessory || "";
        config.background = config.background || "none";


        // ==================================================
        // BASIC AVATAR COORDINATES
        // ==================================================

        var size = parseInt(container.dataset.size, 10) || 120;

        var cx = size / 2;
        var cy = size / 2;

        var profile =
            faceProfiles[config.hairStyle] ||
            faceProfiles["short-bob"];


        // ==================================================
        // FACE
        // ==================================================

        var faceCx =
            cx +
            size * profile.cxOffset;

        var faceTop =
            size * profile.top;

        var faceBottom =
            size * profile.bottom;

        var faceCy =
            (faceTop + faceBottom) / 2;

        var faceRy =
            (faceBottom - faceTop) / 2;

        var faceRx =
            size * profile.rx;


        // ==================================================
        // EYES + MOUTH
        // ==================================================

        var featureCx = faceCx;

        var eyeY =
            size * profile.eyeY;

        var eyeOffsetX =
            size * profile.eyeOffsetX;

        var eyeRx =
            size * 0.045;

        var eyeRy =
            size * 0.052;

        var mouthY =
            size * profile.mouthY;

        var mouthHalf =
            size * 0.074;


        // ==================================================
        // WHOLE AVATAR PLACEMENT
        //
        // Keep the avatar comfortably centred in pink circle.
        // ==================================================

        var avatarScale = 0.945;

        var avatarOffsetY =
            size * 0.030;

        var avatarGroupStart =
            '<g transform="' +
            'translate(' +
            cx + ',' +
            (cy + avatarOffsetY) +
            ') ' +
            'scale(' + avatarScale + ') ' +
            'translate(' +
            (-cx) + ',' +
            (-cy) +
            ')' +
            '">';

        var avatarGroupEnd =
            '</g>';


        // ==================================================
        // EXPRESSION
        // ==================================================

        var expressionParts = [];


        function makeBlush(opacity) {

            return (
                '<ellipse' +
                ' cx="' + (featureCx - size * 0.125) + '"' +
                ' cy="' + (size * 0.595) + '"' +
                ' rx="' + (size * 0.032) + '"' +
                ' ry="' + (size * 0.014) + '"' +
                ' fill="#F3A6A6"' +
                ' opacity="' + opacity + '"' +
                '/>' +

                '<ellipse' +
                ' cx="' + (featureCx + size * 0.125) + '"' +
                ' cy="' + (size * 0.595) + '"' +
                ' rx="' + (size * 0.032) + '"' +
                ' ry="' + (size * 0.014) + '"' +
                ' fill="#F3A6A6"' +
                ' opacity="' + opacity + '"' +
                '/>'
            );
        }


        // --------------------------------------------------
        // HAPPY
        // --------------------------------------------------

        if (config.expression === "happy") {

            expressionParts.push(
                makeBlush(0.14)
            );

            var happyEyeHalf =
                size * 0.030;


            expressionParts.push(

                '<path d="' +

                'M' +
                (featureCx - eyeOffsetX - happyEyeHalf) +
                ',' +
                eyeY +

                ' Q' +
                (featureCx - eyeOffsetX) +
                ',' +
                (eyeY + size * 0.023) +

                ' ' +
                (featureCx - eyeOffsetX + happyEyeHalf) +
                ',' +
                eyeY +

                '"' +

                ' fill="none"' +
                ' stroke="#2D2A26"' +
                ' stroke-width="' + (size * 0.022) + '"' +
                ' stroke-linecap="round"' +

                '/>'
            );


            expressionParts.push(

                '<path d="' +

                'M' +
                (featureCx + eyeOffsetX - happyEyeHalf) +
                ',' +
                eyeY +

                ' Q' +
                (featureCx + eyeOffsetX) +
                ',' +
                (eyeY + size * 0.023) +

                ' ' +
                (featureCx + eyeOffsetX + happyEyeHalf) +
                ',' +
                eyeY +

                '"' +

                ' fill="none"' +
                ' stroke="#2D2A26"' +
                ' stroke-width="' + (size * 0.022) + '"' +
                ' stroke-linecap="round"' +

                '/>'
            );


            expressionParts.push(

                '<path d="' +

                'M' +
                (featureCx - mouthHalf * 1.05) +
                ',' +
                mouthY +

                ' Q' +
                featureCx +
                ',' +
                (mouthY + size * 0.035) +

                ' ' +
                (featureCx + mouthHalf * 1.05) +
                ',' +
                mouthY +

                '"' +

                ' fill="none"' +
                ' stroke="#993C1D"' +
                ' stroke-width="' + (size * 0.028) + '"' +
                ' stroke-linecap="round"' +

                '/>'
            );
        }


        // --------------------------------------------------
        // WINK
        // --------------------------------------------------

        else if (config.expression === "wink") {

            expressionParts.push(
                makeBlush(0.10)
            );

            expressionParts.push(
                '<ellipse' +
                ' cx="' + (featureCx - eyeOffsetX) + '"' +
                ' cy="' + eyeY + '"' +
                ' rx="' + eyeRx + '"' +
                ' ry="' + eyeRy + '"' +
                ' fill="#2D2A26"' +
                '/>'
            );

            var winkHalf =
                size * 0.030;

            expressionParts.push(
                '<path d="' +
                'M' +
                (featureCx + eyeOffsetX - winkHalf) +
                ',' +
                (eyeY + size * 0.002) +
                ' Q' +
                (featureCx + eyeOffsetX) +
                ',' +
                (eyeY - size * 0.018) +
                ' ' +
                (featureCx + eyeOffsetX + winkHalf) +
                ',' +
                (eyeY + size * 0.002) +
                '"' +
                ' fill="none"' +
                ' stroke="#2D2A26"' +
                ' stroke-width="' + (size * 0.022) + '"' +
                ' stroke-linecap="round"' +
                '/>'
            );

            expressionParts.push(
                '<path d="' +
                'M' +
                (featureCx - mouthHalf * 0.90) +
                ',' +
                mouthY +
                ' Q' +
                featureCx +
                ',' +
                (mouthY + size * 0.032) +
                ' ' +
                (featureCx + mouthHalf * 0.90) +
                ',' +
                mouthY +
                '"' +
                ' fill="none"' +
                ' stroke="#993C1D"' +
                ' stroke-width="' + (size * 0.028) + '"' +
                ' stroke-linecap="round"' +
                '/>'
            );
        }


        // --------------------------------------------------
        // SURPRISED
        // --------------------------------------------------

        else if (config.expression === "surprised") {
            var surprisedEyeRy = eyeRy * 1.12;
            var surprisedEyeRx = eyeRx * 1.05;

            // slightly bigger round eyes
            expressionParts.push('<ellipse cx="' + (featureCx - eyeOffsetX) + '" cy="' + eyeY + '" rx="' + surprisedEyeRx + '" ry="' + surprisedEyeRy + '" fill="#2D2A26"/>');
            expressionParts.push('<ellipse cx="' + (featureCx + eyeOffsetX) + '" cy="' + eyeY + '" rx="' + surprisedEyeRx + '" ry="' + surprisedEyeRy + '" fill="#2D2A26"/>');

            // soft raised brows
            expressionParts.push('<path d="M' + (featureCx - eyeOffsetX - eyeRx * 0.9) + ',' + (eyeY - eyeRy * 1.9) +
                ' Q' + (featureCx - eyeOffsetX) + ',' + (eyeY - eyeRy * 2.35) +
                ' ' + (featureCx - eyeOffsetX + eyeRx * 0.9) + ',' + (eyeY - eyeRy * 1.9) +
                '" fill="none" stroke="#7A5238" stroke-width="' + (size * 0.018) + '" stroke-linecap="round"/>');

            expressionParts.push('<path d="M' + (featureCx + eyeOffsetX - eyeRx * 0.9) + ',' + (eyeY - eyeRy * 1.9) +
                ' Q' + (featureCx + eyeOffsetX) + ',' + (eyeY - eyeRy * 2.35) +
                ' ' + (featureCx + eyeOffsetX + eyeRx * 0.9) + ',' + (eyeY - eyeRy * 1.9) +
                '" fill="none" stroke="#7A5238" stroke-width="' + (size * 0.018) + '" stroke-linecap="round"/>');

            // small round mouth, slightly higher
            expressionParts.push(
                '<ellipse cx="' + featureCx + '" cy="' + (mouthY - size * 0.015) + '"' +
                ' rx="' + (size * 0.028) + '"' +
                ' ry="' + (size * 0.04) + '"' +
                ' fill="#2D2A26"/>'
            );

            // optional soft blush to make the expression cuter
            expressionParts.push('<ellipse cx="' + (featureCx - eyeOffsetX * 1.1) + '" cy="' + (mouthY - size * 0.02) + '" rx="' + (size * 0.04) + '" ry="' + (size * 0.022) + '" fill="#F7B7BE" opacity="0.35"/>');
            expressionParts.push('<ellipse cx="' + (featureCx + eyeOffsetX * 1.1) + '" cy="' + (mouthY - size * 0.02) + '" rx="' + (size * 0.04) + '" ry="' + (size * 0.022) + '" fill="#F7B7BE" opacity="0.35"/>');
        }


        // --------------------------------------------------
        // CALM
        // --------------------------------------------------

        else if (config.expression === "calm") {

            expressionParts.push(

                '<ellipse' +
                ' cx="' + (featureCx - eyeOffsetX) + '"' +
                ' cy="' + eyeY + '"' +
                ' rx="' + eyeRx + '"' +
                ' ry="' + eyeRy + '"' +
                ' fill="#2D2A26"' +
                '/>'
            );


            expressionParts.push(

                '<ellipse' +
                ' cx="' + (featureCx + eyeOffsetX) + '"' +
                ' cy="' + eyeY + '"' +
                ' rx="' + eyeRx + '"' +
                ' ry="' + eyeRy + '"' +
                ' fill="#2D2A26"' +
                '/>'
            );


            expressionParts.push(

                '<path d="' +

                'M' +
                (featureCx - mouthHalf * 0.45) +
                ',' +
                mouthY +

                ' L' +
                (featureCx + mouthHalf * 0.45) +
                ',' +
                mouthY +

                '"' +

                ' fill="none"' +
                ' stroke="#993C1D"' +
                ' stroke-width="' + (size * 0.024) + '"' +
                ' stroke-linecap="round"' +

                '/>'
            );
        }


        // --------------------------------------------------
        // DEFAULT / SMILE
        // --------------------------------------------------

        else {

            expressionParts.push(

                '<ellipse' +
                ' cx="' + (featureCx - eyeOffsetX) + '"' +
                ' cy="' + eyeY + '"' +
                ' rx="' + eyeRx + '"' +
                ' ry="' + eyeRy + '"' +
                ' fill="#2D2A26"' +
                '/>'
            );


            expressionParts.push(

                '<ellipse' +
                ' cx="' + (featureCx + eyeOffsetX) + '"' +
                ' cy="' + eyeY + '"' +
                ' rx="' + eyeRx + '"' +
                ' ry="' + eyeRy + '"' +
                ' fill="#2D2A26"' +
                '/>'
            );


            expressionParts.push(

                '<path d="' +

                'M' +
                (featureCx - mouthHalf) +
                ',' +
                mouthY +

                ' Q' +
                featureCx +
                ',' +
                (mouthY + size * 0.030) +

                ' ' +
                (featureCx + mouthHalf) +
                ',' +
                mouthY +

                '"' +

                ' fill="none"' +
                ' stroke="#993C1D"' +
                ' stroke-width="' + (size * 0.028) + '"' +
                ' stroke-linecap="round"' +

                '/>'
            );
        }


        // ==================================================
        // ACCESSORIES
        // ==================================================

        var accessoryParts = [];


        // --------------------------------------------------
        // GLASSES
        // --------------------------------------------------

        if (config.accessory === "glasses") {

            var gR =
                size * 0.090;

            var gY =
                eyeY + size * 0.010;


            accessoryParts.push(

                '<circle' +
                ' cx="' + (featureCx - eyeOffsetX) + '"' +
                ' cy="' + gY + '"' +
                ' r="' + gR + '"' +
                ' fill="none"' +
                ' stroke="#444"' +
                ' stroke-width="' + (size * 0.024) + '"' +
                '/>'
            );


            accessoryParts.push(

                '<circle' +
                ' cx="' + (featureCx + eyeOffsetX) + '"' +
                ' cy="' + gY + '"' +
                ' r="' + gR + '"' +
                ' fill="none"' +
                ' stroke="#444"' +
                ' stroke-width="' + (size * 0.024) + '"' +
                '/>'
            );


            accessoryParts.push(

                '<path d="' +

                'M' +
                (featureCx - eyeOffsetX + gR) +
                ',' +
                gY +

                ' L' +
                (featureCx + eyeOffsetX - gR) +
                ',' +
                gY +

                '"' +

                ' fill="none"' +
                ' stroke="#444"' +
                ' stroke-width="' + (size * 0.024) + '"' +

                '/>'
            );
        }


        // --------------------------------------------------
        // HEADPHONES
        // --------------------------------------------------

        else if (config.accessory === "headphones") {

            // Headphones 微微往左、往上
            var headphoneOffsetX = -size * 0.020;
            var headphoneOffsetY = -size * 0.020;

            accessoryParts.push(
                '<g transform="translate(' +
                headphoneOffsetX + ',' +
                headphoneOffsetY + ')">' +

                    '<path d="' +

                    'M' +
                    (faceCx - faceRx * 1.05) +
                    ',' +
                    faceCy +

                    ' Q' +
                    faceCx +
                    ',' +
                    (faceTop - size * 0.08) +

                    ' ' +
                    (faceCx + faceRx * 1.05) +
                    ',' +
                    faceCy +

                    '"' +

                    ' fill="none"' +
                    ' stroke="#555"' +
                    ' stroke-width="' + (size * 0.05) + '"' +
                    ' stroke-linecap="round"' +

                    '/>' +

                    '<rect' +
                    ' x="' + (faceCx - faceRx * 1.12) + '"' +
                    ' y="' + (faceCy - size * 0.06) + '"' +
                    ' width="' + (size * 0.13) + '"' +
                    ' height="' + (size * 0.18) + '"' +
                    ' rx="' + (size * 0.035) + '"' +
                    ' fill="#555"' +
                    '/>' +

                    '<rect' +
                    ' x="' + (faceCx + faceRx * 0.84) + '"' +
                    ' y="' + (faceCy - size * 0.06) + '"' +
                    ' width="' + (size * 0.13) + '"' +
                    ' height="' + (size * 0.18) + '"' +
                    ' rx="' + (size * 0.035) + '"' +
                    ' fill="#555"' +
                    '/>' +

                '</g>'
            );
        }


        // --------------------------------------------------
        // SAKURA PIN
        // --------------------------------------------------

        else if (config.accessory === "sakura-pin") {

            var pinX =
                faceCx + faceRx * 0.90;

            var pinY =
                faceTop + size * 0.04;


            accessoryParts.push(
                '<circle cx="' + pinX +
                '" cy="' + pinY +
                '" r="' + (size * 0.055) +
                '" fill="#FFB7C5"/>'
            );

            accessoryParts.push(
                '<circle cx="' + (pinX + size * 0.025) +
                '" cy="' + (pinY - size * 0.025) +
                '" r="' + (size * 0.038) +
                '" fill="#E8837A"/>'
            );

            accessoryParts.push(
                '<circle cx="' + (pinX - size * 0.025) +
                '" cy="' + (pinY - size * 0.025) +
                '" r="' + (size * 0.038) +
                '" fill="#E8837A"/>'
            );

            accessoryParts.push(
                '<circle cx="' + pinX +
                '" cy="' + (pinY + size * 0.025) +
                '" r="' + (size * 0.038) +
                '" fill="#E8837A"/>'
            );
        }


        // --------------------------------------------------
        // BOW TIE (was MASK)
        // --------------------------------------------------

        else if (
            config.accessory === "bow-tie" ||
            config.accessory === "mask"
        ) {
            // Keep "mask" here only for backward compatibility
            // Old saved data with accessory:"mask" should still show the bow tie

            var bowX = faceCx;
            var bowY = faceBottom + size * 0.02;
            var bowScale = size / 135; // smaller than before

            accessoryParts.push(
                '<g transform="translate(' + bowX + ',' + bowY + ') scale(' + bowScale + ')">' +

                    // Left wing
                    '<path d="' +
                        'M -4,-2 ' +
                        'C -10,-8 -24,-9 -29,-5 ' +
                        'C -32,-1 -30,7 -27,11 ' +
                        'C -20,10 -11,7 -4,3 Z' +
                    '"' +
                    ' fill="#9FD4F6"' +
                    ' stroke="#76B9E8"' +
                    ' stroke-width="1.6"' +
                    ' stroke-linejoin="round"/>' +

                    // Right wing
                    '<path d="' +
                        'M 4,-2 ' +
                        'C 10,-8 24,-9 29,-5 ' +
                        'C 32,-1 30,7 27,11 ' +
                        'C 20,10 11,7 4,3 Z' +
                    '"' +
                    ' fill="#9FD4F6"' +
                    ' stroke="#76B9E8"' +
                    ' stroke-width="1.6"' +
                    ' stroke-linejoin="round"/>' +

                    // Center knot
                    '<rect x="-5" y="-4" width="10" height="12" rx="3" fill="#89C8F2" stroke="#76B9E8" stroke-width="1.2"/>' +

                    // Left wing white dots
                    '<circle cx="-18" cy="-2" r="1.7" fill="#FFFFFF"/>' +
                    '<circle cx="-23" cy="5" r="1.5" fill="#FFFFFF"/>' +
                    '<circle cx="-12" cy="6" r="1.4" fill="#FFFFFF"/>' +

                    // Right wing white dots
                    '<circle cx="18" cy="-2" r="1.7" fill="#FFFFFF"/>' +
                    '<circle cx="23" cy="5" r="1.5" fill="#FFFFFF"/>' +
                    '<circle cx="12" cy="6" r="1.4" fill="#FFFFFF"/>' +

                    // Fold lines - subtle
                    '<path d="M -7,1 Q -13,0 -19,-3" fill="none" stroke="#76B9E8" stroke-width="1.1" stroke-linecap="round" opacity="0.55"/>' +
                    '<path d="M 7,1 Q 13,0 19,-3" fill="none" stroke="#76B9E8" stroke-width="1.1" stroke-linecap="round" opacity="0.55"/>' +

                '</g>'
            );
        }


        // ==================================================
        // HAIR
        // ==================================================

        var hairSvg =
            renderHair(config, size);


        // ==================================================
        // HAIR COLOUR FILTER
        // ==================================================

        var filterDef =

            '<defs>' +

                '<filter' +
                ' id="hairColorFilter"' +
                ' x="-20%"' +
                ' y="-20%"' +
                ' width="140%"' +
                ' height="140%"' +
                '>' +

                    '<feFlood' +
                    ' flood-color="' + config.hairColor + '"' +
                    ' result="hairColor"' +
                    '/>' +

                    '<feComposite' +
                    ' in="hairColor"' +
                    ' in2="SourceAlpha"' +
                    ' operator="in"' +
                    ' result="coloredHair"' +
                    '/>' +

                '</filter>' +

            '</defs>';


        // ==================================================
        // FINAL SVG
        // ==================================================

        // Background rect (if set)
        var bgRect = '';
        if (config.background && config.background !== 'none') {
            bgRect = '<rect x="0" y="0" width="' + size + '" height="' + size + '" fill="' + config.background + '" rx="8"/>';
        }

        var svg =

            '<svg' +
            ' xmlns="http://www.w3.org/2000/svg"' +
            ' width="' + size + '"' +
            ' height="' + size + '"' +
            ' viewBox="0 0 ' + size + ' ' + size + '"' +
            '>' +

                filterDef +

                bgRect +

                avatarGroupStart +

                    // FACE
                    '<ellipse' +
                    ' cx="' + faceCx + '"' +
                    ' cy="' + faceCy + '"' +
                    ' rx="' + faceRx + '"' +
                    ' ry="' + faceRy + '"' +
                    ' fill="' + config.skinTone + '"' +
                    '/>' +

                    // HAIR
                    hairSvg +

                    // EXPRESSION
                    expressionParts.join("") +

                    // ACCESSORY
                    accessoryParts.join("") +

                avatarGroupEnd +

            '</svg>';


        container.innerHTML = svg;
    }

    function buildControls(wrapper, config, onChange) {
        var swatchGroup = function (label, key, values) {
            var group = document.createElement("div");
            group.className = "mb-3";
            group.innerHTML = '<label class="form-label na-form-label">' + label + '</label>';
            var row = document.createElement("div");
            row.className = "d-flex flex-wrap gap-2";
            values.forEach(function (val) {
                var btn = document.createElement("button");
                btn.type = "button";
                btn.className = "na-color-swatch" + (config[key] === val ? " active" : "");
                btn.title = val;
                btn.dataset.key = key;
                btn.dataset.value = val;
                if (key === "skinTone" || key === "hairColor") {
                    btn.style.backgroundColor = val;
                } else {
                    btn.style.backgroundColor = "var(--na-sakura-light)";
                    btn.style.color = "var(--na-primary)";
                    btn.style.fontSize = "0.75rem";
                    btn.textContent = val;
                }
                btn.addEventListener("click", function () {
                    config[key] = val;
                    row.querySelectorAll(".na-color-swatch").forEach(function (b) { b.classList.remove("active"); });
                    btn.classList.add("active");
                    onChange(config);
                });
                row.appendChild(btn);
            });
            group.appendChild(row);
            return group;
        };

        var selectGroup = function (label, key, values) {
            var group = document.createElement("div");
            group.className = "mb-3";
            group.innerHTML = '<label class="form-label na-form-label">' + label + '</label>';
            var select = document.createElement("select");
            select.className = "form-select na-form-select";
            values.forEach(function (val) {
                var opt = document.createElement("option");
                opt.value = val;
                opt.textContent = val.replace(/-/g, " ");
                if (config[key] === val) opt.selected = true;
                select.appendChild(opt);
            });
            select.addEventListener("change", function () {
                config[key] = select.value;
                onChange(config);
            });
            group.appendChild(select);
            return group;
        };

        wrapper.appendChild(swatchGroup("Skin tone", "skinTone", options.skinTones));
        wrapper.appendChild(swatchGroup("Hair color", "hairColor", options.hairColors));
        wrapper.appendChild(selectGroup("Hairstyle", "hairStyle", options.hairStyles));
        wrapper.appendChild(selectGroup("Expression", "expression", options.expressions));
        wrapper.appendChild(selectGroup("Accessory", "accessory", options.accessories));
        wrapper.appendChild(backgroundGroup("Background", "background", options.backgrounds, config, onChange));
    }

    function backgroundGroup(label, key, values, config, onChange) {
        var group = document.createElement("div");
        group.className = "mb-3";
        group.innerHTML = '<label class="form-label na-form-label">' + label + '</label>';
        var row = document.createElement("div");
        row.className = "d-flex flex-wrap gap-2";
        values.forEach(function (opt) {
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "na-color-swatch" + (config[key] === opt.value ? " active" : "");
            btn.title = opt.label;
            btn.dataset.key = key;
            btn.dataset.value = opt.value;
            if (opt.color === "transparent") {
                btn.style.background = "#fff";
                btn.style.border = "2px dashed var(--na-border)";
                btn.innerHTML = '<i class="bi bi-slash-circle" style="font-size:0.6rem;color:var(--na-text-muted);"></i>';
            } else {
                btn.style.backgroundColor = opt.color;
                btn.style.border = "2px solid " + opt.color;
            }
            btn.addEventListener("click", function () {
                config[key] = opt.value;
                row.querySelectorAll(".na-color-swatch").forEach(function (b) { b.classList.remove("active"); });
                btn.classList.add("active");
                onChange(config);
            });
            row.appendChild(btn);
        });
        group.appendChild(row);
        return group;
    }

    function initAvatarBuilder(opts) {
        opts = opts || {};
        var previewSelector = opts.preview || "#avatar-preview";
        var controlsSelector = opts.controls || "#avatar-controls";
        var inputSelector = opts.input || "#avatar-json";
        var randomBtnSelector = opts.randomButton || "#avatar-random";

        var preview = document.querySelector(previewSelector);
        var controls = document.querySelector(controlsSelector);
        var input = document.querySelector(inputSelector);
        var randomBtn = document.querySelector(randomBtnSelector);

        if (!preview || !controls) return;

        var initial = {};
        try {
            var parsed = JSON.parse(input && input.value ? input.value : "{}");
            initial = parsed;
        } catch (e) {
            initial = {};
        }

        var config = Object.assign({}, defaults, initial);

        var update = function () {
            renderAvatar(config, preview);
            if (input) input.value = JSON.stringify(config);
        };

        buildControls(controls, config, function () {
            update();
        });

        if (randomBtn) {
            randomBtn.addEventListener("click", function () {
                config.skinTone = options.skinTones[Math.floor(Math.random() * options.skinTones.length)];
                config.hairColor = options.hairColors[Math.floor(Math.random() * options.hairColors.length)];
                config.hairStyle = options.hairStyles[Math.floor(Math.random() * options.hairStyles.length)];
                config.expression = options.expressions[Math.floor(Math.random() * options.expressions.length)];
                config.accessory = options.accessories[Math.floor(Math.random() * options.accessories.length)];
                config.background = options.backgrounds[Math.floor(Math.random() * options.backgrounds.length)].value;
                // rebuild controls to reflect new values
                controls.innerHTML = "";
                buildControls(controls, config, function () { update(); });
                update();
            });
        }

        update();
    }

    window.AvatarBuilder = {
        defaults: defaults,
        options: options,
        renderAvatar: renderAvatar,
        init: initAvatarBuilder
    };
})();
