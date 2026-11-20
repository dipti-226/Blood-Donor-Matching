import { Pipe, PipeTransform } from '@angular/core';
import { relativeTime } from '../utils/dates';

@Pipe({ name: 'relativeTime' })
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return value ? relativeTime(value) : '-';
  }
}