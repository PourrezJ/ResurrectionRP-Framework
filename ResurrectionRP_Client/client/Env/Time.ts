import * as game from 'natives';

export class Time {

    public Hours: number;
    public Minutes: number;
    public Seconds: number;

    public Multiplicator: number = 4;
    private lastcheck: number;

    public constructor(Hours: number = 8, Minutes: number = 0, Seconds: number = 0) {
        this.Hours = Hours;
        this.Minutes = Minutes;
        this.Seconds = Seconds;
    }

    public OnTick() {
        if (game.getGameTimer() - this.lastcheck < 1000)
            return;
        this.lastcheck = game.getGameTimer();

        this.Seconds += this.Multiplicator;
        if (this.Seconds >= 60) {
            this.Seconds = 0;
            this.Minutes++;
        }
        if (this.Minutes == 60) {
            this.Minutes = 0;
            this.Hours++;
        }
        if (this.Hours == 24)
            this.Hours = 0;
        game.setClockTime(this.Hours, this.Minutes, this.Seconds);
    }
}