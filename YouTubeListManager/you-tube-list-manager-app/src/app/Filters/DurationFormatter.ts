import { Pipe, PipeTransform } from '@angular/core';

@Pipe({name: 'durationFormatter'})
export class DurationFormatterPipe implements PipeTransform {
  transform(duration: number, args: string[]): any {
    if (!duration) {
      return duration;
    }

    let minutes: number = Math.floor(duration / 60);
    let seconds: number = duration % 60;

    let stringifiedSeconds: string = seconds.toString();
    if (seconds < 10) {
      stringifiedSeconds = '0' + seconds;
    }

    let stringifiedMinutes: string = minutes.toString();
    if (minutes < 10) {
      stringifiedMinutes = '0' + minutes;
    }

    return stringifiedMinutes + ':' + stringifiedSeconds;
  }
}
