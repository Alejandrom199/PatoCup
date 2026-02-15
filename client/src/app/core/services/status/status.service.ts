import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ModalState {
  isOpen: boolean;
  succeeded: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class StatusService {
  private modalSubject = new BehaviorSubject<ModalState>({
    isOpen: false,
    succeeded: false,
    message: ''
  });

  // Observable para que el AppComponent escuche los cambios
  modalState$ = this.modalSubject.asObservable();

  show(succeeded: boolean, message: string) {
    this.modalSubject.next({ isOpen: true, succeeded, message });
  }

  close() {
    this.modalSubject.next({ ...this.modalSubject.value, isOpen: false });
  }
}
