import { CommonModule } from '@angular/common';
import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { FilterByValuePipe } from '../../../core/utils/pipes/filter-by-value-pipe';
import { SelectOption } from '../../../core/models/select-option.model';
import { NgIcon } from "@ng-icons/core";

@Component({
    selector: 'app-custom-select',
    standalone: true,
    // 1. Agregamos el Pipe a los imports para que el HTML lo pueda usar
    imports: [CommonModule, ReactiveFormsModule, FilterByValuePipe],
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => CustomSelectComponent),
            multi: true
        }
    ],
    templateUrl: './custom-select.html'
})
export class CustomSelectComponent implements ControlValueAccessor {
    @Input() options: SelectOption[] = [];
    @Input() placeholder: string = 'Selecciona una opción';
    selectedValue: any = null;
    isOpen: boolean = false;

    onChange: any = () => { };
    onTouched: any = () => { };

    iconMap: { [key: string]: string } = {
    'state': 'heroBars3',
    'game': 'heroTrophy',
    'calendar': 'heroCalendar',
    'user': 'heroUser'
    };

    toggle() {
        this.isOpen = !this.isOpen;
    }

    selectOption(option: any) {
        this.selectedValue = option.value;
        this.onChange(option.value);
        this.onTouched();
        this.isOpen = false;
    }

    writeValue(value: any): void {
        this.selectedValue = value;
    }

    registerOnChange(fn: any): void { this.onChange = fn; }
    registerOnTouched(fn: any): void { this.onTouched = fn; }

}