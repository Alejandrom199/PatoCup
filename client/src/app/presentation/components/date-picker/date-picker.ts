import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, forwardRef, HostListener, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

type CalendarView = 'days' | 'months' | 'years';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [DatePipe, CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatePicker),
      multi: true
    }
  ],
  templateUrl: './date-picker.html',
  styleUrl: './date-picker.scss',
  styles: [':host { display: block; width: 100%; }']
})

export class DatePicker implements ControlValueAccessor {

  isOpen = false;
  placement: 'top' | 'bottom' = 'bottom';
  currentView: CalendarView = 'days';
  selectedDate!: Date | null;
  displayValue = '';

  monthNames = [
    'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
  ];

  years: number[] = [];
  
  currentMonth = new Date();
  days: Date[] = [];

  @Input() minDate: Date | null = new Date();
  @Input() maxDate: Date | null = null;

  private _today = new Date(); // Referencia a la fecha actual

  onChange = (_: any) => { };
  onTouched = () => { };

  constructor(
    private cdr: ChangeDetectorRef,
    private elementRef: ElementRef) {
    this.generateCalendar();
  }

  writeValue(value: string | null): void {
    if (value && typeof value === 'string') {
      const [year, month, day] = value.split('-').map(Number);
      this.selectedDate = new Date(year, month - 1, day);
      this.displayValue = this.formatDisplay(this.selectedDate);
    } else {
      this.selectedDate = null;
      this.displayValue = '';
    }
  }

  switchToMonths() {
    this.currentView = 'months';
  }

  switchToYears() {
    const currentYear = this.currentMonth.getFullYear();
    // Generamos un rango de años (ej: 10 atrás y 10 adelante)
    let startYear = currentYear - 10;
    // Aseguramos que el rango no empiece antes del año mínimo permitido
    startYear = Math.max(startYear, this._minAllowedYear);

    this.years = Array.from({length: 20}, (_, i) => startYear + i); // Generar el rango completo, la plantilla se encargará de deshabilitar
    this.currentView = 'years';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  toggle() {
    if (!this.isOpen) {
      const rect = this.elementRef.nativeElement.getBoundingClientRect();
      const windowHeight = window.innerHeight;
      const spaceBelow = windowHeight - rect.bottom;

      // Asumimos que el calendario mide unos 350px de alto.
      // Si hay menos de 350px abajo, lo mandamos para arriba.
      this.placement = spaceBelow < 350 ? 'top' : 'bottom';

      this.isOpen = true;

      // Resetear vista al mes seleccionado o actual
      this.currentMonth = this.selectedDate ? new Date(this.selectedDate) : new Date();
      this.generateCalendar();
    } else {
      this.isOpen = false;
    }
  }

  selectDay(day: Date) {
    this.selectedDate = day;
    this.displayValue = this.formatDisplay(day);
    this.onChange(this.formatISO(day));
    this.onTouched(); // 👈 Avisamos al formulario que el campo ya fue editado
    this.isOpen = false;
  }

  selectMonth(monthIndex: number) {
    // Actualizamos el mes pero mantenemos el año
    const newDate = new Date(this.currentMonth);
    newDate.setMonth(monthIndex);
    this.currentMonth = newDate;
    
    // Volvemos a la vista de días y regeneramos
    this.currentView = 'days';
    this.generateCalendar();
  }

  selectYear(year: number) {
    // Actualizamos el año
    const newDate = new Date(this.currentMonth);
    newDate.setFullYear(year);
    this.currentMonth = newDate;

    // Volvemos a la vista de meses (Flujo: Año -> Mes -> Día)
    this.currentView = 'months';
  }

  prevMonth() {
    // Clona la fecha para evitar mutaciones raras que Angular no detecte
    const newDate = new Date(this.currentMonth);
    newDate.setMonth(newDate.getMonth() - 1);
    this.currentMonth = newDate;

    this.generateCalendar();
    // 🛠️ FIX 1: Forzar repintado
    this.cdr.detectChanges();
  }

  nextMonth() {
    const newDate = new Date(this.currentMonth);
    newDate.setMonth(newDate.getMonth() + 1);
    this.currentMonth = newDate;

    this.generateCalendar();
    // 🛠️ FIX 1: Forzar repintado
    this.cdr.detectChanges();
  }

  generateCalendar() {
    this.days = []; // Aseguramos que days sea de tipo (Date | null)[]
    const year = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();

    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    // 1. Calculamos el "hueco" inicial (lunes = 1, domingo = 0)
    // Ajustamos para que Lunes sea el inicio (index 0)
    let startDay = firstDayOfMonth.getDay();
    if (startDay === 0) startDay = 6; else startDay--;

    // 2. Agregamos días vacíos (o del mes anterior) para alinear el grid
    for (let i = 0; i < startDay; i++) {
      this.days.push(null as any);
    }

    // 3. Llenamos los días del mes
    for (let d = 1; d <= lastDayOfMonth.getDate(); d++) {
      this.days.push(new Date(year, month, d));
    }
  }

  get _minAllowedYear(): number {
    return this.minDate ? this.minDate.getFullYear() : this._today.getFullYear();
  }

  get _minAllowedMonthForCurrentYear(): number {
    const currentYear = this.currentMonth.getFullYear();
    if (this.minDate && currentYear === this.minDate.getFullYear()) {
      return this.minDate.getMonth();
    }
    if (currentYear === this._today.getFullYear()) {
      return this._today.getMonth();
    }
    return 0;
  }

  // Método auxiliar para obtener la fecha mínima de referencia (minDate o hoy)
  private _getMinDateBaseline(): Date {
    return this.minDate || this._today;
  }

  formatISO(date: Date | null): string {
    if (!date) return '';
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  formatDisplay(date: Date): string {
    return date.toLocaleDateString('es-EC');
  }

  isPrevPageDisabled(): boolean {
    const currentYear = this.currentMonth.getFullYear();
    const minDateBaseline = this._getMinDateBaseline();

    if (this.currentView === 'days') {
      // Comprueba si el mes anterior sería anterior al mes/año mínimo permitido
      const tempDate = new Date(this.currentMonth);
      tempDate.setMonth(tempDate.getMonth() - 1);

      return tempDate.getFullYear() < minDateBaseline.getFullYear() ||
             (tempDate.getFullYear() === minDateBaseline.getFullYear() && tempDate.getMonth() < minDateBaseline.getMonth());

    } else if (this.currentView === 'months') {
      // Comprueba si el año anterior sería anterior al año mínimo permitido
      return currentYear - 1 < this._minAllowedYear;

    } else if (this.currentView === 'years') {
      // Comprueba si el inicio del rango de 20 años actual (currentYear - 10)
      // ya es menor o igual al año mínimo permitido.
      return (currentYear - 10) <= this._minAllowedYear;
    }
    return false;
  }

  prevPage() {
    if (this.isPrevPageDisabled()) {
      return; // No hacer nada si el botón está deshabilitado
    }

    const newDate = new Date(this.currentMonth);

    if (this.currentView === 'days') {
      newDate.setMonth(newDate.getMonth() - 1);
    } else if (this.currentView === 'months') {
      newDate.setFullYear(newDate.getFullYear() - 1);
    } else {
      newDate.setFullYear(newDate.getFullYear() - 20);
    }
    
    this.currentMonth = newDate;
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  nextPage() {
    // No hay lógica de deshabilitación para nextPage según el requerimiento
    // ya que se permite cualquier fecha "en adelante".
    const newDate = new Date(this.currentMonth);
    if (this.currentView === 'days') {
      newDate.setMonth(newDate.getMonth() + 1);
    } else if (this.currentView === 'months') {
      newDate.setFullYear(newDate.getFullYear() + 1);
    } else {
      newDate.setFullYear(newDate.getFullYear() + 20); // Avanza 20 años
    }
    this.currentMonth = newDate;
    this.generateCalendar();
    this.cdr.detectChanges();
  }

  isDisabled(date: Date): boolean {
    if (!date) return false;

    // Normalizamos las horas para comparar solo fechas
    const d = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    const minDateBaseline = this._getMinDateBaseline();
    const min = new Date(minDateBaseline.getFullYear(), minDateBaseline.getMonth(), minDateBaseline.getDate());
    if (d < min) return true;

    // Si maxDate está definido, también deshabilitar fechas posteriores
    if (this.maxDate) {
      const max = new Date(this.maxDate.getFullYear(), this.maxDate.getMonth(), this.maxDate.getDate());
      if (d > max) return true;
    }
    return false;
  }

  isYearDisabled(year: number): boolean {
    // Deshabilitar si el año es anterior al año mínimo permitido
    if (year < this._minAllowedYear) {
      return true;
    }
    // Deshabilitar si el año es posterior al año máximo permitido (si maxDate está definido)
    if (this.maxDate && year > this.maxDate.getFullYear()) {
      return true;
    }
    return false;
  }

  isMonthDisabled(monthIndex: number): boolean {
    const currentYear = this.currentMonth.getFullYear();
    if (currentYear < this._minAllowedYear) {
      return true; 
    }
    if (currentYear === this._minAllowedYear) {
      return monthIndex < this._minAllowedMonthForCurrentYear;
    }
    return false;
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent) {
    const clickedInside = this.elementRef.nativeElement.contains(event.target);

    if (!clickedInside) {
      this.isOpen = false;
    }
  }
}
