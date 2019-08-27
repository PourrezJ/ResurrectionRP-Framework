var channel = null;
var favoris = [];

function loadFavoris(frequencesList, chan) {
    favoris = frequencesList;
    channel = chan;
    console.log(favoris);
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
    }
    else {
        channel = null;
        $('.frequence').hide();
        alt.emit("RadioOnOff", false);
    }
    $('#channelFrequence').html(channel+1);
    $('#inputFrequence').val(favoris[channel]);
}

function changeChannel(up) {
    if (up) {
        if (channel < 5)
            channel++;
        else
            channel = 0;
    }
    else {
        if (channel > 0)
            channel--;
        else
            channel = 5;
    }
    $('#channelFrequence').html(channel+1);
    $('#inputFrequence').val(favoris[channel]);
}

function upFrequence() {
    if (channel === null)
        return;

    changeChannel(true);
    frequence = $('#inputFrequence').val();
    alt.emit("ConnectFrequence", channel);
}

function downFrequence() {
    if (channel === null)
        return;

    changeChannel(false);
    frequence = $('#inputFrequence').val();
    alt.emit("ConnectFrequence", channel);
}

function changeFrequence() {
    if (channel === null)
        return;

    frequence = $('#inputFrequence').val();
    favoris[channel] = frequence;
    mp.trigger("SaveFrequence", channel, frequence);
}


function saveFrequence() {
    if (channel === null)
        return;

    $('.frequence').addClass("blink_me");
    //favoris[channel] = frequence;
    favoris[channel] = $('#inputFrequence').val();
    alt.emit("SaveFrequence", channel, favoris[channel]);
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
    
}

function volumeDown() {
   
}

$(() => {
    if ('alt' in window) {
        alt.on('loadFavoris', loadFavoris);
        alt.on('hide', hide);
        alt.on('unhide', unhide);
        alt.emit('GetFavoris');
    }
});
