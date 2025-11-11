import { Component, ViewChild } from '@angular/core';
import { NewsService } from '../../services/news.service';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { NewsItemComponent } from '../news-item/news-item.component';
import { PaginationComponent } from '../pagination/pagination.component';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpParams } from '@angular/common/http';
import { MatList } from '@angular/material/list';
import { MatProgressBar } from '@angular/material/progress-bar';

export interface StoryItem {
  title: string,
  url: string
}

export interface SearchResult {
  items: StoryItem[];
  totalCount: number;
}

@Component({
  selector: 'app-news-list',
  standalone: true,
  imports: [NewsItemComponent, PaginationComponent, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatPaginator, MatIcon, MatInput, FormsModule, MatList, MatProgressBar, HttpClientModule, SearchBarComponent],
  templateUrl: './news-list.component.html',
  styleUrl: './news-list.component.css'
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

  constructor(private NewsService: NewsService) { }

  ngOnInit() {
    this.loadStories();
  }

  loadStories() {
    this.isLoading = true;
    this.NewsService.getNewestStories(this.currentPage, this.pageSize).subscribe(data => {
      this.stories = data.stories;  
      this.totalCount = data.count;
      this.isLoading = false;
    })
  }

  onPageChange(page: number) {
    this.currentPage = page;
    if (this.isSearch) {
      this.onSearch(this.query);
    } else {
      this.loadStories();
    }
  }

  
  onPageSize(size: number) {
    this.pageSize = size;
    this.onSearch(this.query);
  }

  onSearch(query: string) {

    console.log("query: " + this.query);
    if (typeof query === "string" && query.trim() === "") {      
      this.isSearch = false;
      this.loadStories();
    } else {      
      this.isSearch = true;
      if (this.query != query) {
        this.isNewSearch = true;
        this.currentPage = 1;
      } else {
        this.isNewSearch = false;
      }
      this.query = query;

      this.isLoading = true;
      this.NewsService.searchStories(this.query, this.currentPage, this.pageSize).subscribe(data => {
        this.stories = data.stories;
        this.totalCount = data.count;
        this.isLoading = false;
      })
    }
  }
}
