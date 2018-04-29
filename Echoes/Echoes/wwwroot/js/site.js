function PositionOutput(text, parent, child, textEffects, additionalCss) {
    var writingChance = Math.random();
    var width = 0;
    var height = 20;

    if (parent === undefined) {
        parent = $(document);
    }

    if (child === undefined) {
        child = $('<div/>').text(text);

        width = text.length * 3 + 10;

        child.css({
            'width': width + 'px',
            'height': '20px',
            'padding': '5px'
        });

    } else {
        width = child.width();
        height = child.height();
    }

    if (width >= parent.width()) {
        width = parent.width() / 3;
    }

    if (writingChance >= 0.8) {
        child.css({
            'writing-mode': 'horizontal-tb',
            '-webkit-writing-mode': 'horizontal-tb',
            '-ms-writing-mode': 'horizontal-tb'
        });
    } else if (writingChance >= 0.6) {
        child.width(height);
        child.height(width + 10);

        child.css({
            'writing-mode': 'vertical-lr',
            '-webkit-writing-mode': 'vertical-lr',
            '-ms-writing-mode': 'vertical-lr'
        });
    } else if (writingChance >= 0.4) {
        child.width(height);
        child.height(width + 10);

        child.css({
            'writing-mode': 'vertical-rl',
            '-webkit-writing-mode': 'vertical-rl',
            '-ms-writing-mode': 'vertical-rl'
        });
    }

    var posx = (Math.random() * (parent.width() - child.width())).toFixed();
    var posy = (Math.random() * (parent.height() - child.height())).toFixed();

    child.css({
        'position': 'absolute',
        'left': posx + 'px',
        'top': posy + 'px'
    });

    if (additionalCss !== undefined) {
        child.css(additionalCss);
    }

    child.appendTo(parent);

    if (textEffects === undefined) {
        textEffects = { in: { effect: 'fadeIn' } };
    }

    $.extend(textEffects, {
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

    child.textillate(textEffects);
}

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
        currentDate = moment();
    }

    Cookies.set('lastSeenDate', currentDate);
}