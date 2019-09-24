'use strict';

function setHUD(hunger, thirst, vocal, vocaltype, money, mute)
{
    $('.water span').html(`${thirst}%`);
    $('.burger span').html(`${hunger}%`);

    $(".water-on").css("height", `${thirst}%`);
    $(".burger-on").css("height", `${hunger}%`);

    if (hunger <= 10) {
      $('.burger img').css('animation', 'shake 1s infinite');
    }

    if (thirst <= 10) {
      $('.water img').css('animation', 'shake 1s infinite');
    }

    if (thirst > 10) {
      $('.water img').css('animation', 'none');
    }

    if (hunger > 10) {
      $('.burger img').css('animation', 'none');
    }

    if (mute === "1")
    {
        $('#speak span').html(`MUTE`);
        $('#speak').addClass('mute');
        $('#speak').removeClass('parler crier chuchoter');
    }
    else
    {
        $('#speak span').html(`${vocaltype}`);
        if (vocal === "1") {
            $('.listening').css('opacity', '1');
        }
        else {
            $('.listening').css('opacity', '0');
        }

        switch (vocaltype) {
            case "Chuchoter":
                $('#speak').addClass('chuchoter');
                $('#speak').removeClass('parler crier mute');
                break;

            case "Parler":
                $('#speak').addClass('parler');
                $('#speak').removeClass('chuchoter crier mute');
                break;

            case "Crier":
                $('#speak').addClass('crier');
                $('#speak').removeClass('chuchoter parler mute');
                break;
        }
    }

    $('.money').html(`${money}<span>$</span>`);
}


function MakeProgressBar(duration)
{
    if (circle !== null)
    {
        circle.destroy();
        circle = null;
    }

    var opt = {
        duration: duration, // A modifier
        class: 'progress__label',
        lineStart: 20,
        lineEnd: 20,
        colorStart: '#ff370d',
        colorEnd: '#12ff5b',
        anim: 'linear',
        trail: 15,
        trailColor: 'rgba(0, 0, 0, 0.2)'
    };

    circle = new ProgressBar.Circle('#progress', {
        color: '#000',
        strokeWidth: 20,
        trailColor: opt.trailColor,
        trailWidth: opt.trail,
        easing: opt.anim,
        duration: opt.duration,
        text: {
            autoStyleContainer: false,
            className: opt.class
        },
        from: {
            color: opt.colorStart,
            width: opt.lineStart
        },
        to: {
            color: opt.colorEnd,
            width: opt.lineEnd
        },
        step: function (state, circle) {
            circle.path.setAttribute('stroke', state.color);
            circle.path.setAttribute('stroke-width', state.width);

            var value = Math.round(circle.value() * 100);
            if (value === 0) {
                circle.setText('');
            } else {
                circle.setText(value + '%');
            }

        }
    });

    // .animate(progress, [options], [cb])
    circle.animate(1, duration);
    setTimeout(function () {
        circle.setText('');
        circle.destroy();
        circle = null;
    }, duration);
}

function FinishCB() {
    // console.log('Progress complete');

    if (circle === null)
        return;
    circle.setText('');
    circle.destroy();
    circle = null;
    /*
    mp.trigger("ProgressComplete");
    setTimeout(function () {
        circle.setText('');
        circle.destroy();
    }, 2000);*/
}

function showHide(value) {
    if (value)
        $('body').hide();
    else
        $('body').show();
}

if ('alt' in window) {
    alt.on('setHUD', setHUD);
    alt.on('showHide', showHide);
}
