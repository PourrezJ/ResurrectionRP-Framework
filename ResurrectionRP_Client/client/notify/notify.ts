import * as alt from 'alt';
import * as game from 'natives';

// https://github.com/combowomb0/altv-notification-system
export class Notify {
        public notify = {
            isLoaded: false,
            view: null
        };
    constructor() {

        this.notify.view = new alt.WebView('http://resource/client/cef/notify/index.html');
        this.notify.view.on('notify:loaded', () => {
            this.notify.isLoaded = true;
        });
        alt.onServer("notify", this.Notify);
        alt.on      ("notify", this.Notify);
        alt.onServer("successNotify", this.successNotify);
        alt.on      ("successNotify", this.successNotify);
        alt.onServer("alertNotify", this.alertNotify);
        alt.on      ("alertNotify", this.alertNotify);
    }

    Notify = (title: string, content: string, time: number=5000, r: number = 0, g: number = 0, b: number = 0) => {
        this.notify.view.emit('notify:send', {
            text: "<h1>" + title + "</h1><br/><b>" + content + "</b>",
            timeout: time,
            textColor: '#fff',
            backgroundColor: 'rgba(' + r + ',' + g + ',' + b + ',0.85)',
            lineColor: '#ff6535'
        });
    }

    successNotify = (title:string, content: string, time: number = 5000, r: number = 0, g:number = 0, b: number = 0) => {
        this.notify.view.emit('notify:send', {
            text: "<h1>"+title+"</h1><br/><b>" + content + "</b>",
            timeout: time,
            textColor: '#fff',
            backgroundColor: 'rgba('+r+','+g+','+b+',0.85)',
            lineColor: '#009900'
        });
    }
    alertNotify = (title: string, content: string, time: number = 5000, r: number = 0, g: number = 0, b: number = 0) => {
        this.notify.view.emit('notify:send', {
            text: "<h1>" + title + "</h1><br/><b>" + content + "</b>",
            timeout: time,
            textColor: '#fff',
            backgroundColor: 'rgba(' + r + ',' + g + ',' + b + ',0.85)',
            lineColor: '#b30000'
        });
    }
    
}