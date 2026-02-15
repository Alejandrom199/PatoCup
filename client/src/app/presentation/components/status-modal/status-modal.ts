import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnDestroy, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
// Importamos los iconos específicos para este componente
import { heroCheckCircle, heroXCircle } from '@ng-icons/heroicons/outline';

export type ModalType = 'status' | 'confirmation';

@Component({
  selector: 'app-status-modal',
  standalone: true,
  imports: [CommonModule, NgIconComponent],
  viewProviders: [provideIcons({ heroCheckCircle, heroXCircle })],
  templateUrl: './status-modal.html'
})
export class StatusModal implements OnChanges, OnDestroy {
  @Input() isOpen = false;

  @Input() type: ModalType = 'status';

  @Input() succeeded = false;
  @Input() title = '';
  @Input() message = '';
  
  // Datos para Confirmación (El modo nuevo)
  @Input() confirmText = 'Confirmar';
  @Input() cancelText = 'Cancelar';
  @Input() confirmColor: 'red' | 'blue' | 'orange' = 'red';

  @Input() icon: string = '';
  @Input() isJson: boolean = false;

  @Input() maxWidth: string = 'max-w-sm';

  @Output() close = new EventEmitter<void>();
  @Output() finished = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  @Input() showCancelButton: boolean = true;
  
  progressWidth = 0;
  private progressInterval: any;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isOpen']?.currentValue === true) {
      if (this.type === 'status') {
        this.startProgressBar();
      }
    }
    
    if (changes['isOpen']?.currentValue === false) {
      this.resetTimer();
    }
  }

  @HostListener('window:keydown.escape', ['$event'])
  handleEscapeKey(event: Event) {
    this.onCancel();
  }

  startProgressBar() {
    this.resetTimer();

    const totalDuration = this.succeeded ? 4000 : 6000;
    const startTime = Date.now(); 

    this.progressInterval = setInterval(() => {
      const currentTime = Date.now();
      const elapsed = currentTime - startTime;

      this.progressWidth = Math.min((elapsed / totalDuration) * 100, 100);
      
      this.cdr.detectChanges(); 

      if (elapsed >= totalDuration) {
        this.progressWidth = 100;
        this.resetTimer();
        this.finished.emit(); 
      }
    }, 30);
  }

  private resetTimer() {
    this.progressWidth = 0;
    if (this.progressInterval) {
      clearInterval(this.progressInterval);
      this.progressInterval = null;
    }
  }

  onClose() {
    this.resetTimer();
    this.close.emit();
  }

  ngOnDestroy() {
    this.resetTimer();
  }

  onCancel() {
    this.resetTimer();
    this.cancel.emit();
    this.close.emit();
  }

  onConfirmAction() {
    this.resetTimer();
    this.confirm.emit();
  }

}