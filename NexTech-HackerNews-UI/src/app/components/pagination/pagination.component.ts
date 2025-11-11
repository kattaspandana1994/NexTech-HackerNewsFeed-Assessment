import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [MatButtonModule, MatSelectModule],
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
  
  totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  updatePageSize(size: number) {
    console.log("size: " + size)
    this.pageSize = size;
    this.currentPage = 1;
    this.reloadStories.emit(size);
  }
}
