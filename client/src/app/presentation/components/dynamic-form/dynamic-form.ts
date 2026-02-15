import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormFieldConfig } from '../../../core/models/form-config';
import { CommonModule } from '@angular/common';
import { DatePicker } from '../date-picker/date-picker';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroBars3, heroCalendar, heroEnvelope, heroLockClosed, heroTrophy, heroUser, heroXMark } from '@ng-icons/heroicons/outline';
import { CustomSelectComponent } from '../custom-select/custom-select';


@Component({
  selector: 'app-dynamic-form',
  imports: [CommonModule, ReactiveFormsModule, DatePicker, NgIconComponent, CustomSelectComponent],
  viewProviders: [provideIcons({ heroUser, heroLockClosed, heroEnvelope, heroCalendar, heroTrophy, heroXMark, heroBars3 })],
  templateUrl: './dynamic-form.html',
})
export class DynamicForm {

  @Input() maxWidth: string = 'max-w-md';

  @Input() title: string = '';
  @Input() subTitle: string = '';
  @Input() submitLabel: string = 'Enviar';

  @Input() headerBackground: string = '';
  @Input() titleClass: string = 'text-slate-800'; 
  @Input() subTitleClass: string = 'text-slate-500';

  @Input() fields: FormFieldConfig[] = [];
  @Input() isLoading: boolean = false;
  @Input() glass: boolean = false;
  @Input() errorMessage: string | null = null;

  @Input() isOpen: boolean = false;

  @Output() formSubmit = new EventEmitter<any>();
  @Output() close = new EventEmitter<void>();

  dynamicForm!: FormGroup;
  visibleFields = new Set<string>();

  iconMap: Record<string, string> = {
    'user': 'heroUser',
    'lock': 'heroLockClosed',
    'email': 'heroEnvelope',
    'calendar': 'heroCalendar',
    'game': 'heroTrophy',
    'id': 'heroIdentification',
    'select': 'heroListBullet',
    'state' : 'heroBars3'
  };

  constructor(private fb: FormBuilder) { }

  ngOnInit() {
    const group: any = {};
    this.fields.forEach(field => {
      group[field.name] = [(field as any).value || '', field.validators || []];
    });
    this.dynamicForm = this.fb.group(group);
  }
  @HostListener('window:keydown.escape', ['$event'])
  handleEscapeKey(event: Event) {
    this.onCancel();
  }
  
  onSubmit() {
    if (this.dynamicForm.valid) {
      this.formSubmit.emit(this.dynamicForm.value);
    } else {
      this.dynamicForm.markAllAsTouched();
    }
  }

  onCancel() {
    this.close.emit();
  }

  hasError(fieldName: string): boolean {
    const field = this.dynamicForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  toggleVisibility(fieldName: string) {
    if (this.visibleFields.has(fieldName)) {
      this.visibleFields.delete(fieldName);
    } else {
      this.visibleFields.add(fieldName);
    }
  }

  getInputType(field: FormFieldConfig): string {
    if (field.type === 'password') {
      return this.visibleFields.has(field.name) ? 'text' : 'password';
    }
    return field.type;
  }
}