import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css'
})
export class PaginationComponent {
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 0;
  @Input() totalCount: number = 200;
  @Output() pageChange = new EventEmitter<number>();
  @Output() reloadStories = new EventEmitter<number>();

  onPageChange(page: number) {
    this.currentPage = page;
    this.pageChange.emit(page);
  }

  onPageSizeChange(event: Event) {
  const selectElement = event.target as HTMLSelectElement;
  const value = Number(selectElement.value);
  this.updatePageSize(value);
}

  
 totalPages(): number {
  return this.pageSize > 0 ? Math.ceil(this.totalCount / this.pageSize) : 0;
}

  updatePageSize(size: number) {
    console.log("size: " + size)
    this.pageSize = size;
    this.currentPage = 1;
    this.reloadStories.emit(size);
  }
}
