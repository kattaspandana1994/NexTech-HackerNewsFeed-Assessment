import { Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { StoryItem } from '../news-list/news-list.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-news-item',
  standalone: true,
  imports: [MatListModule, MatButtonModule, MatDialogModule, MatCardModule,CommonModule],
  templateUrl: './news-item.component.html',
  styleUrl: './news-item.component.css'
})
export class NewsItemComponent {

  @Input() story: StoryItem  | null | undefined;

  constructor(public matDialog: MatDialog) { }
}
