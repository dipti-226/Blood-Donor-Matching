import { Pipe, PipeTransform } from '@angular/core';
import { BloodComponent } from '../../core/models/enums';
import { BLOOD_COMPONENT_LABELS } from '../../core/models/labels';

@Pipe({ name: 'componentLabel' })
export class ComponentLabelPipe implements PipeTransform {
  transform(value: BloodComponent | null | undefined): string {
    return value ? BLOOD_COMPONENT_LABELS[value] : '-';
  }
}