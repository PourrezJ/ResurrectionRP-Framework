﻿var channel = null;
var favoris = [];

function initialize(frequencesList, chan) {
    favoris = frequencesList;
    channel = chan;

    if (channel !== null) {
        $('.frequence').show();
        $('#channelFrequence').html(channel + 1);
        $('#inputFrequence').val(favoris[channel]);
    }
}

function onOffRadio() {
    if (channel === null) {
        channel = 0;
        $('.frequence').show();
        alt.emit("RadioOnOff", true);
    } else {
        channel = null;
        $('.frequence').hide();
        alt.emit("RadioOnOff", false);
    }

    $('#channelFrequence').html(channel + 1);
    $('#inputFrequence').val(favoris[channel]);
}

function changeChannel(up) {
    if (up) {
        if (channel < 5)
            channel++;
        else
            channel = 0;
    } else {
        if (channel > 0)
            channel--;
        else
            channel = 5;
    }

    $('#channelFrequence').html(channel + 1);
    $('#inputFrequence').val(favoris[channel]);
}

function setChannel(chan) {
    channel = chan;
    $('#channelFrequence').html(channel + 1);
    $('#inputFrequence').val(favoris[channel]);
}

function upFrequence() {
    if (channel === null)
        return;

    changeChannel(true);
    frequence = $('#inputFrequence').val();
    alt.emit("ChangeChannel", channel);
}

function downFrequence() {
    if (channel === null)
        return;

    changeChannel(false);
    frequence = $('#inputFrequence').val();
    alt.emit("ChangeChannel", channel);
}

function changeFrequence() {
    if (channel === null)
        return;

    frequence = $('#inputFrequence').val();
    favoris[channel] = frequence;
    alt.emit("SaveFrequence", channel);
}


function saveFrequence() {
    if (channel === null)
        return;

    $('.frequence').addClass("blink_me");
    favoris[channel] = $('#inputFrequence').val();
    alt.emit("SaveFrequence", channel, $('#inputFrequence').val());
    setTimeout(function () { $('.frequence').removeClass("blink_me"); }, 2000);
}

function deleteFrequence() {
    if (channel === null)
        return;

    favoris[channel] = 0.0;
    $('#inputFrequence').val(null);
}

function unhide() {
    $('.talky').show();
}

function hide() {
    $('.talky').hide();
}

function volumeUP() {
    alt.emit("volumeUP");
}

function volumeDown() {
    alt.emit("volumeDown");
}

function muteRadio() {
    alt.emit("volumeMuted");
}

$(() => {
    if ('alt' in window) {
        alt.on('initialize', initialize);
        alt.on('hide', hide);
        alt.on('unhide', unhide);
        alt.on('setChannel', setChannel);
        alt.emit('initialize');
    }
});
