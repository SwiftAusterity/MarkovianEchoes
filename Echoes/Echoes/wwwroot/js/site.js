//Kinetic text functions
function Kinesis_Text(text, parentContainer, modeOptions, styles, effects) {
    if (text === undefined || text.length == 0) {
        return;
    }

    if (parentContainer === undefined) {
        parentContainer = $(document);
    }

    var child = $('<div/>').text(text);

    var width = text.length * 7 + 10;

    child.css({
        'width': width + 'px',
        'height': '30px',
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

    child.css({
        'width': '100px',
        'height': '30px',
        'padding': '5px'
    });

    list.forEach(function (element) {
        var newItem = $('<li/>').text(element);

        child.append(newItem);
    });

    Kinesis_Finale(child, parentContainer, modeOptions, styles, effects);
}

function Kinesis_Element(element, parentContainer, modeOptions, styles, effects) {
    if (element === undefined) {
        return;
    }

    if (parentContainer === undefined) {
        parentContainer = $(document);
    }

    Kinesis_Finale(element, parentContainer, modeOptions, styles, effects);
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
            'writing-mode': 'horizontal-tb',
            '-webkit-writing-mode': 'horizontal-tb',
            '-ms-writing-mode': 'horizontal-tb'
        });

        parentContainer = $('body');
    } else {
        posx = (Math.random() * parentContainer.width()).toFixed();
        posy = (Math.random() * parentContainer.height()).toFixed();

        if (SelectOrientation(modeOptions.Orientation, child)) {
            height = constrainBounds(child.height(), posy, parentContainer.height());
            width = width * Math.ceil(child.height() / height);
        } else {
            width = constrainBounds(child.width(), posx, parentContainer.width());
            height = height * Math.ceil(child.width() / width);
        }
    }

    child.width(width);
    child.height(height);

    child.css({
        'position': 'absolute',
        'left': posx + 'px',
        'top': posy + 'px'
    });

    if (styles !== undefined) {
        child.css(styles);
    }

    child.appendTo(parentContainer);

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

function SelectOrientation(orientation, child) {
    var width = child.width();
    var height = child.height();
    var flip = false;

    if (orientation === 'normal') {
        child.css({
            'writing-mode': 'horizontal-tb',
            '-webkit-writing-mode': 'horizontal-tb',
            '-ms-writing-mode': 'horizontal-tb'
        });
    } else if (orientation === 'vertical') {
        child.width(height);
        child.height(width + 10);

        child.css({
            'writing-mode': 'vertical-lr',
            '-webkit-writing-mode': 'vertical-lr',
            '-ms-writing-mode': 'vertical-lr'
        });

        flip = true;
    } else {
        var writingChance = Math.random();

        if (writingChance >= 0.5) {
            child.css({
                'writing-mode': 'horizontal-tb',
                '-webkit-writing-mode': 'horizontal-tb',
                '-ms-writing-mode': 'horizontal-tb'
            });
        } else if (writingChance >= 0.2) {
            child.width(height);
            child.height(width + 10);

            child.css({
                'writing-mode': 'vertical-lr',
                '-webkit-writing-mode': 'vertical-lr',
                '-ms-writing-mode': 'vertical-lr'
            });

            flip = true;
        } else {
            child.width(height);
            child.height(width + 10);

            child.css({
                'writing-mode': 'vertical-rl',
                '-webkit-writing-mode': 'vertical-rl',
                '-ms-writing-mode': 'vertical-rl'
            });

            flip = true;
        }
    }

    return flip;
}

function constrainBounds(childWidth, startingPosition, parentWidth) {
    var roomToMove = parentWidth - startingPosition;

    if (roomToMove < childWidth) {
        childWidth = childWidth * (roomToMove / childWidth);
    }

    return childWidth;
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
    if (Cookies.get('tutorialMode') !== 'on') {
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

    Kinesis_Text(text, parent, modeOptions, { 'color': 'green', 'z-index': 999, 'background-color': 'lightgrey', 'border': '1px solid white' });
}