import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoryItem } from '../news-list/news-list.component';

@Component({
  selector: 'app-news-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './news-item.component.html',
  styleUrls: ['./news-item.component.css']
})
export class NewsItemComponent {
  @Input() story: StoryItem | null = null;
}