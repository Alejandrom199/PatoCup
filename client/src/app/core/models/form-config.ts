import { ValidatorFn } from '@angular/forms';

export type FormFieldType =
  | 'text'
  | 'email'
  | 'password'
  | 'number'
  | 'date'
  | 'select'
  | 'textarea'
  | 'checkbox';

export type FormFieldIcon =
  | 'user'
  | 'lock'
  | 'email'
  | 'game'
  | 'id'
  | 'calendar'
  | 'select'
  | 'state';

export interface FormFieldConfig {
  name: string;
  label: string;
  type: FormFieldType;
  placeholder?: string;
  icon?: FormFieldIcon;
  validators?: ValidatorFn[];
  value?: any;
  options?: { value: any; label: string }[];
  required?: boolean;
  disabled?: boolean;
}
