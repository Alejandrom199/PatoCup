import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroChevronLeft, heroChevronRight } from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, NgIconComponent, FormsModule],
  viewProviders: [provideIcons({ heroChevronLeft, heroChevronRight })],
  templateUrl: './pagination.html'
})
export class Pagination {
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 10;
  @Input() totalItems: number = 0;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  // Opciones para el combo "Filas por página"
  pageSizeOptions = [5, 10, 20, 50];

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize) || 1;
  }

  get startItem(): number {
    if (this.totalItems === 0) return 0;
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  // Cambiar página (Anterior / Siguiente)
  changePage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  // Cambiar tamaño de filas
  onSizeChange(event: any) {
    const newSize = Number(event.target.value);
    this.pageSizeChange.emit(newSize);
  }
}