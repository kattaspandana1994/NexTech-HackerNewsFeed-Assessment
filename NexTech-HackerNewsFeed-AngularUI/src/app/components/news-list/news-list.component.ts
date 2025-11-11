import { Component, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { NewsService } from '../../services/news.service';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { NewsItemComponent } from '../news-item/news-item.component';
import { PaginationComponent } from '../pagination/pagination.component';

export interface StoryItem {
  title: string;
  url: string;
}

export interface SearchResult {
  items: StoryItem[];
  totalCount: number;
}

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [
    FormsModule,
    HttpClientModule,
    NewsItemComponent,
    PaginationComponent,
    SearchBarComponent
  ],
  templateUrl: './news-list.component.html',
  styleUrls: ['./news-list.component.css']
})
export class NewsListComponent {
  @ViewChild(PaginationComponent) paginationComponent!: PaginationComponent;

  stories: any[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 200;
  displayedColumns: string[] = ['title', 'url'];
  searchQuery: string = '';
  isLoading: boolean = false;
  isSearch: boolean = false;
  isNewSearch: boolean = false;
  query: string = '';

  constructor(private newsService: NewsService) {}

  ngOnInit(): void {
    this.loadStories();
  }

  loadStories(): void {
    this.isLoading = true;
    this.newsService.getNewestStories(this.currentPage, this.pageSize).subscribe(data => {
      this.stories = data.items;
      this.totalCount = data.count;
      this.isLoading = false;
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    if (this.isSearch) {
      this.onSearch(this.query);
    } else {
      this.loadStories();
    }
  }

  onPageSize(size: number): void {
    this.pageSize = size;
    this.onSearch(this.query);
  }

  onSearch(query: string): void {
    console.log('query:', query);
    if (typeof query === 'string' && query.trim() === '') {
      this.isSearch = false;
      this.loadStories();
    } else {
      this.isSearch = true;
      if (this.query !== query) {
        this.isNewSearch = true;
        this.currentPage = 1;
      } else {
        this.isNewSearch = false;
      }
      this.query = query;

      this.isLoading = true;
      this.newsService.searchStories(this.query, this.currentPage, this.pageSize).subscribe(data => {
        this.stories = data.items;
        this.totalCount = data.count;
        this.isLoading = false;
      });
    }
  }
}
