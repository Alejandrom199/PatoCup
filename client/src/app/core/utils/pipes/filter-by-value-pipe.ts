// filter-by-value-pipe.ts
import { Pipe, PipeTransform } from '@angular/core';
import { SelectOption } from '../../models/select-option.model'; // Asegura la ruta

@Pipe({
  name: 'filterByValue',
  standalone: true
})
export class FilterByValuePipe implements PipeTransform {
  transform(options: SelectOption[] | null, value: any): SelectOption | null {
    if (!options || value === undefined || value === null) return null;
    return options.find(opt => opt.value === value) || null;
  }
}