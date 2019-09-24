/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */

function hideSpeedometer() {
    $('body').hide();
    $(".speedometer").css('display', 'none');
}

function showSpeedometer() {
    $('body').show();
    $(".speedometer").css('display', 'block');
}

function setSpeed(speed, rpm, gear, light, engineOn, engineHealth, currentFuel, fuelMax, milage) {
    $('.speed').html(Math.floor(speed));

    if (currentFuel === 0 || !engineOn) {
        $('.gear').html(`<span>0</span>`);
        $(".compteur .on").css("width", `0px`);
    } else {
        $('.gear').html(`<span>${gear}</span>`);
        $(".compteur .on").css("width", `${rpm}px`);
    }

    $('.information .stats .fuel').html(`<img src="img/fuel.png">${Math.ceil(currentFuel * 10) / 10}L/${fuelMax}L`);
    $('.information .stats .km').html(`<img src="img/km.png">${(Math.floor(milage * 10) / 10)}km`);

    switch (light) {
        case 0:
            $('.information .icons-status .phare').css('opacity', '0');
            break;

        case 1:
            $('.information .icons-status .phare').html(`<img src="img/phare.png">`);
            $('.information .icons-status .phare').css('opacity', '1'); 
            break;

        case 2:
            $('.information .icons-status .phare').html(`<img src="img/phare-blue.png">`);
            $('.information .icons-status .phare').css('opacity', '1');
            break;
    }

    let styles = "";

    if (engineHealth <= 200) {
        styles = {
            'opacity': '1',
            'background': 'red'
        };
        $('.information .icons-status .engine span').css(styles);
        $('.information .icons-status .engine').css('opacity', '1');
    }
    else if (engineHealth <= 500) {
        styles = {
            'opacity': '1',
            'background': 'yellow'
        };
        $('.information .icons-status .engine span').css(styles);
        $('.information .icons-status .engine').css('opacity', '1');
    }
    else {
        $('.information .icons-status .engine').css('opacity', '0');
    }
}

if ('alt' in window) {
    alt.on('hideSpeedometer', hideSpeedometer);
    alt.on('showSpeedometer', showSpeedometer);
    alt.on('setSpeed', setSpeed);
}