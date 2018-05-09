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

    child.appendTo(parentContainer);

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);

    return child;
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
        var newWidth = GetTextFontWidth(element, parentContainer) + 20;

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

    child.appendTo(parentContainer);

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);

    return child;
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

    child.css({
        'position': 'absolute'
    });

    child.appendTo(parentContainer);

    var width = child[0].clientWidth + 20;

    child.css({
        'width': width + 'px',
        'padding': '5px'
    });

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);

    return child;
}

//Kinetic Text Helpers
function Kinesis_Finale(child, parentContainer, modeOptions, styles, effects) {
    var width = child.width();
    var height = child.height();

    var posx = 0;
    var posy = 0;

    //absolute mode is absolute
    if (modeOptions.Absolute === true && modeOptions.xPos !== undefined && modeOptions.yPos !== undefined) {
        posx = modeOptions.xPos;
        posy = modeOptions.yPos;

        child.css({
            'position': 'absolute',
            'left': posx + 'px',
            'top': posy + 'px'
        });

        child.width(constrainBounds(0, true, child.width(), posx, parentContainer.width()));
        child.height(height * Math.ceil(child.width() / width));
    } else {
        var angle = OrientText(modeOptions.Orientation, child);

        var rotatedWidth = child.width();
        var rotatedHeight = child.height();

        if (angle != 0) {
            var angleDeformation = 0;

            if (angle <= 270) {
                angleDeformation = Math.abs(90 - angle);
            } else {
                angleDeformation = Math.abs(270 - angle);
            }

            rotatedWidth = Math.abs(child.width() * Math.cos(Math.PI * angleDeformation / 180.0));
            rotatedHeight = Math.abs(child.width() * Math.sin(Math.PI * angleDeformation / 180.0));
        }

        var appropriateWidthBounding = Math.max(25, parentContainer.width() - rotatedWidth - 25);
        var appropriateHeightBounding = Math.max(50, parentContainer.height() - rotatedHeight / 2 - 50);

        //Absolute X bounding is within 25, width - 25
        //Absolute Y bounding is within 50 (for padding), height - 25

        if (angle >= 0 && angle <= 180) {
            posx = Math.min(rotatedWidth, Math.max(50, (Math.random() * appropriateWidthBounding).toFixed()));
        } else {
            posx = Math.max(25, (Math.random() * appropriateWidthBounding).toFixed());
        }

        posy = Math.max(rotatedHeight / 2, (Math.random() * appropriateHeightBounding).toFixed());

        child.height(height * Math.ceil(child.width() / rotatedWidth));

        child.css({
            'position': 'absolute',
            'left': posx + 'px',
            'top': posy + 'px',
            'width': rotatedWidth + 'px'
        });

        child.width(constrainBounds(angle, true, rotatedWidth, posx, parentContainer.width()));

        if (angle != 0) {
            child.width(constrainBounds(angle, false, rotatedWidth, posy, parentContainer.height()));
        }
    }

    //Make things easier to find
    child.addClass('kinetic-text');

    if (effects === undefined) {
        effects = { in: { effect: 'fadeIn' } };
    }

    if (modeOptions.RandomizeEffect !== undefined) {
        if (modeOptions.RandomizeEffect.In !== undefined) {
            var inEffect = getRandomEffect(false);

            $.extend(effects, {
                in: { effect: inEffect }
            });
        }

        if (modeOptions.RandomizeEffect.Out !== undefined) {
            var outEffect = getRandomEffect(true);

            $.extend(effects, {
                out: { effect: outEffect }
            });
        }
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

    if (modeOptions.Color !== undefined) {
        var colorText = 'white';

        if (modeOptions.Color === 'random') {
            colorText = getRandomColor();
        } else {
            colorText = modeOptions.Color;
        }

        child.css({
            'color': colorText
        });
    }

    child.textillate(effects);

    return child;
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
            } else if (isNumeric(textOrientation.direction)) {
                direction = textOrientation.direction;
            }
        }

        if (textOrientation.writingMode !== undefined) {
            writingMode = textOrientation.writingMode;
        }
    }

    //90 is the same as not rotating at all
    if (direction === 90) {
        direction = 0;
    }

    if (direction != 0) {
        child.css({
            transform: 'rotate(' + direction + 'deg)'
        });

        direction += 90;

        if (direction > 360) {
            direction = direction - 360;
        }
    }

    //TODO: Incorporate css text-direction
    if (writingMode === 'random') {
        if (Math.random() <= 0.2) {
            writingMode = 'vertical';
        } else {
            writingMode = 'normal';
        }
    }

    if (writingMode === 'vertical') {
        child.css({
            'writing-mode': 'vertical-lr',
            '-webkit-writing-mode': 'vertical-lr',
            '-ms-writing-mode': 'vertical-lr'
        });

        var height = child.width();
        var width = child.height() + 10;

        child.height(height);
        child.width(width);
    }

    return direction;
}

function constrainBounds(angle, width, childDimension, startingPosition, parentDimension) {
    var roomToMove = parentDimension - startingPosition;

    //Is this going left-to-right or up-to-down or the opposite of either
    var normalDirection = (width && angle >= 0 && angle <= 180) || (!width && angle >= 90 && angle <= 270);

    //Is this going left-to-right or up-to-down or the opposite of either
    var normalDirection = (angle >= 0 && angle <= 180);

    if (normalDirection) {
        if (childDimension > roomToMove) {
            childDimension = childDimension * (roomToMove / childDimension) - 10;
        }
    } else {
        if (childDimension > startingPosition) {
            childDimension = childDimension * (startingPosition / childDimension) - 10;
        }
    }

    return childDimension;
}

function isNumeric(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

/*
 * Returns a random integer between min (inclusive) and max (inclusive)
 * Using Math.round() will give you a non-uniform distribution!
 */
function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

function getRandomColor() {
    var rand = Math.random();

    if (rand <= 0.1) {
        return 'blue';
    } else if (rand <= 0.2) {
        return 'green';
    } else if (rand <= 0.3) {
        return 'greenyellow';
    } else if (rand <= 0.4) {
        return 'mediumpurple';
    } else if (rand <= 0.5) {
        return 'coral';
    } else if (rand <= 0.6) {
        return 'indianred';
    } else if (rand <= 0.7) {
        return 'aquamarine';
    } else if (rand <= 0.8) {
        return 'chartreuse';
    } else if (rand <= 0.9) {
        return 'chocolate';
    } 

    return 'white';
}

function getRandomEffect(out) {
    var rand = Math.random();
    var returnValue = 'fadeIn';

    if (rand <= 0.025) {
        return 'flash';
    } else if (rand <= 0.05) {
        return 'bounce';
    } else if (rand <= 0.075) {
        return 'shake';
    } else if (rand <= 0.1) {
        return 'tada';
    } else if (rand <= 0.125) {
        return 'swing';
    } else if (rand <= 0.15) {
        return 'wobble';
    } else if (rand <= 0.175) {
        return 'pulse';
    } else if (rand <= 0.2) {
        return 'flip';
    } else if (rand <= 0.225) {
        return 'flipInX';
    } else if (rand <= 0.25) {
        return 'flipInY';
    } else if (rand <= 0.275) {
        return 'rollIn';
    } else if (rand <= 0.3) {
        return 'fadeInUp';
    } else if (rand <= 0.325) {
        return 'fadeInDown';
    } else if (rand <= 0.35) {
        return 'fadeInLeft';
    } else if (rand <= 0.375) {
        return 'fadeInRight';
    } else if (rand <= 0.4) {
        return 'fadeInUpBig';
    } else if (rand <= 0.425) {
        return 'fadeInDownBig';
    } else if (rand <= 0.45) {
        return 'fadeInLeftBig';
    } else if (rand <= 0.475) {
        return 'fadeInRightBig';
    } else if (rand <= 0.5) {
        return 'bounceIn';
    } else if (rand <= 0.525) {
        return 'bounceInDown';
    } else if (rand <= 0.55) {
        return 'bounceInUp';
    } else if (rand <= 0.575) {
        return 'bounceInLeft';
    } else if (rand <= 0.6) {
        return 'bounceInRight';
    } else if (rand <= 0.625) {
        return 'rotateIn';
    } else if (rand <= 0.65) {
        return 'rotateInDownLeft';
    } else if (rand <= 0.675) {
        return 'rotateInDownRight';
    } else if (rand <= 0.7) {
        return 'rotateInUpLeft';
    } else if (rand <= 0.725) {
        return 'rotateInUpRight';
    } 

    if (out) {
        return returnValue.replace('In', 'Out');
    }

    return returnValue;
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

    Kinesis_Text(text, $('body'), modeOptions, { 'color': 'green', 'z-index': 999, 'background-color': 'lightgrey', 'border': '1px solid white' }).addClass('tutorial');
}