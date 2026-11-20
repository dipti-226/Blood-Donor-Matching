import { Pipe, PipeTransform } from '@angular/core';
import { BloodGroup } from '../../core/models/enums';
import { BLOOD_GROUP_LABELS } from '../../core/models/labels';

@Pipe({ name: 'bloodGroupLabel' })
export class BloodGroupLabelPipe implements PipeTransform {
  transform(value: BloodGroup | null | undefined): string {
    return value ? BLOOD_GROUP_LABELS[value] : '-';
  }
}