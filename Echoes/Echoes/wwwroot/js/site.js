//Kinetic text functions
function Kinesis_Text(text, parentContainer, modeOptions, styles, effects) {
    if (text === undefined || text.length == 0) {
        return;
    }

    if (parentContainer === undefined) {
        parentContainer = $(document);
    }

    var child = $('<div/>').text(text);

    if (styles !== undefined) {
        child.css(styles);
    }

    var height = 30;
    var width = GetTextFontWidth(text, parentContainer) + 20;

    child.css({
        'width': width + 'px',
        'height': height + 'px',
        'padding': '5px'
    });

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);
}

function Kinesis_Array(list, parentContainer, modeOptions, styles, effects) {
    if (list.constructor !== Array) {
        return;
    }

    if (parentContainer === undefined) {
        parentContainer = $(document);
    }

    var child = $('<ul class="animated-list"/>');

    if (styles !== undefined) {
        child.css(styles);
    }

    var width = 0;
    list.forEach(function (element) {
        var newWidth = GetTextFontWidth(element, child) + 20;

        if (newWidth > width) {
            width = newWidth;
        }

        var newItem = $('<li/>').text(element);

        child.append(newItem);
    });

    child.css({
        'width': width + 'px',
        'height': '30px',
        'padding': '5px'
    });

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);
}

function Kinesis_Element(child, parentContainer, modeOptions, styles, effects) {
    if (child === undefined) {
        return;
    }

    if (parentContainer === undefined) {
        parentContainer = $(document);
    }

    if (styles !== undefined) {
        child.css(styles);
    }

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);
}

//Kinetic Text Helpers
function Kinesis_Finale(child, parentContainer, modeOptions, styles, effects) {
    child.appendTo(parentContainer);

    var width = child.width();
    var height = child.height();

    var posx = 0;
    var posy = 0;

    //absolute mode is absolute
    if (modeOptions.Absolute === true && modeOptions.xPos !== undefined && modeOptions.yPos !== undefined) {
        posx = modeOptions.xPos;
        posy = modeOptions.yPos;

        child.css({
            'writing-mode': 'horizontal-tb',
            '-webkit-writing-mode': 'horizontal-tb',
            '-ms-writing-mode': 'horizontal-tb'
        });

        child.width(constrainBounds(child.width(), posx, parentContainer.width()));
        child.height(height * Math.ceil(child.width() / width));
    } else {
        OrientText(modeOptions.Orientation, child);

        var appropriateWidthBounding = Math.max(0, parentContainer.width() - width);
        var appropriateHeightBounding = Math.max(0, parentContainer.height() - height);

        posx = (Math.random() * appropriateWidthBounding).toFixed();
        posy = (Math.random() * appropriateHeightBounding).toFixed();

        width = constrainBounds(child.width(), posx, appropriateWidthBounding);
        height = constrainBounds(child.height(), posy, appropriateHeightBounding);

        /*
        if (OrientText(modeOptions.Orientation, child)) {
            var appropriateWidthBounding = Math.max(0, parentContainer.width() - width);
            var appropriateHeightBounding = Math.max(0, parentContainer.height() - height);

            posx = (Math.random() * appropriateWidthBounding).toFixed();
            posy = (Math.random() * appropriateHeightBounding).toFixed();

            height = constrainBounds(child.height(), posy, appropriateHeightBounding);
            width = width * Math.ceil(child.height() / height);
        } else {
            width = constrainBounds(child.width(), posx, appropriateWidthBounding);
            height = height * Math.ceil(child.width() / width);
        }
        */
    }

    child.css({
        'position': 'absolute',
        'left': posx + 'px',
        'top': posy + 'px'
    });

    if (effects === undefined) {
        effects = { in: { effect: 'fadeIn' } };
    }

    if (modeOptions.FadeOnSweep === true) {
        $.extend(effects, {
            callback: function () {
                child.mouseover(function () {
                    $(this).hide({
                        duration: 1000,
                        complete: function () {
                            $(this).remove();
                        }
                    });
                });
            }
        });
    }

    child.textillate(effects);
}

function GetTextFontWidth(text, containerElement) {
    var fontSize = GetStyleProperty(containerElement[0], 'font-size');
    var fontVariant = GetStyleProperty(containerElement[0], 'font-variant');
    var fontName = GetStyleProperty(containerElement[0], 'font-family');
    var fontStyle = GetStyleProperty(containerElement[0], 'font-style');
    var fontWeight = GetStyleProperty(containerElement[0], 'font-weight');

    var font = fontStyle + ' ' + fontVariant + ' ' + fontWeight + ' ' + fontSize + ' ' + fontName;

    var canvas = GetTextFontWidth.canvas || (GetTextFontWidth.canvas = document.createElement("canvas"));

    var context = canvas.getContext("2d");
    context.font = font;

    var metrics = context.measureText(text);

    return metrics.width;
}

function GetStyleProperty(element, property) {
    return window.getComputedStyle(element, null).getPropertyValue(property);
}

function OrientText(textOrientation, child) {
    var direction = '0';
    var writingMode = 'normal';

    if (textOrientation !== undefined) {
        if (textOrientation.direction !== undefined) {
            if (textOrientation.direction == 'random') {
                direction = getRandomInt(0, 360);
            } else if (textOrientation.direction == 'upRandom') {
                direction = getRandomInt(181, 360);
            } else if (textOrientation.direction == 'downRandom') {
                direction = getRandomInt(0, 180);
            } else {
                direction = textOrientation.direction;
            }
        }

        if (textOrientation.writingMode !== undefined) {
            writingMode = textOrientation.writingMode;
        }
    }

    var width = child.width();
    var height = child.height();

    child.css({
        transform: 'rotate(' + direction + 'deg)'
    });

    var angleDeformation = Math.abs(180 - direction) / 180;

    if (angleDeformation != 0 && angleDeformation != 1) {
        width = width * angleDeformation;
        height = height / angleDeformation + 10;
    }

    if (writingMode = 'random' && Math.random() <= 0.2) {
        writingMode = 'vertical';
    } else {
        writingMode = 'normal';
    }

    if (writingMode === 'vertical') {
        child.css({
            'writing-mode': 'vertical-lr',
            '-webkit-writing-mode': 'vertical-lr',
            '-ms-writing-mode': 'vertical-lr'
        });

        height = child.width();
        width = child.height() + 10;
    } else {
        child.css({
            'writing-mode': 'horizontal-tb',
            '-webkit-writing-mode': 'horizontal-tb',
            '-ms-writing-mode': 'horizontal-tb'
        });
    }

    child.height(height);
    child.width(width);
}

function constrainBounds(childWidth, startingPosition, parentWidth) {
    var roomToMove = parentWidth - startingPosition;

    if (roomToMove < childWidth) {
        childWidth = childWidth * (roomToMove / childWidth);
    }

    return childWidth;
}

/**
 * Returns a random integer between min (inclusive) and max (inclusive)
 * Using Math.round() will give you a non-uniform distribution!
 */
function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

//Cookies stuff

function rememberPersona(personaName, personaSetter) {
    if (personaSetter === undefined) {
        personaSetter = $('#personaLabel');
    }

    personaSetter.addClass('activated');
    Cookies.set('persona', personaName);
}

function forgetPersona(personaSetter) {
    if (personaSetter === undefined) {
        personaSetter = $('#personaLabel');
    }

    personaSetter.removeClass('activated');
    Cookies.set('persona', '');
}

function setAkashicDate(currentDate) {
    if (currentDate === undefined) {
        currentDate = moment().format();
    }

    Cookies.set('lastSeenDate', currentDate);
}

function setMode(acting) {
    if (acting) {
        Cookies.set('modality', 'act');
    } else {
        Cookies.set('modality', 'speak');
    }
}

//Tutorial TODO put this in its own js along with kinesis

function Tutorial(parent, text) {
    if (Cookies.get('tutorialMode') === 'off') {
        return;
    }

    var parentBounds = parent[0].getBoundingClientRect();

    var modeOptions = {
        Orientation: 'normal',
        FadeOnSweep: true,
        Absolute: true,
        xPos: parentBounds.left + 15,
        yPos: parentBounds.bottom + 15
    };

    text = '^ ' + text;

    Kinesis_Text(text, $('body'), modeOptions, { 'color': 'green', 'z-index': 999, 'background-color': 'lightgrey', 'border': '1px solid white' });
}