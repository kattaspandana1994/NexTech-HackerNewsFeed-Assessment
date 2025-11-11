import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { By } from '@angular/platform-browser';
import { NewsListComponent } from './news-list.component';
import { NewsService } from '../../services/news.service';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { PaginationComponent } from '../pagination/pagination.component';
import { NewsItemComponent } from '../news-item/news-item.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

// Mock data
const mockStories = [
  { title: 'AI Revolution Begins', url: 'https://news.com/ai' },
  { title: 'SpaceX Launch Success', url: 'https://news.com/spacex' }
];

const mockPagedResult = {
  items: mockStories,
  count: 2
};

// Mock Service
class MockNewsService {
  getNewestStories = jasmine.createSpy('getNewestStories').and.returnValue(of(mockPagedResult));
  searchStories = jasmine.createSpy('searchStories').and.returnValue(of(mockPagedResult));
}

describe('NewsListComponent', () => {
  let component: NewsListComponent;
  let fixture: ComponentFixture<NewsListComponent>;
  let newsService: MockNewsService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FormsModule,
        HttpClientModule,
        NewsListComponent,
        SearchBarComponent,
        PaginationComponent,
        NewsItemComponent
      ],
      providers: [{ provide: NewsService, useClass: MockNewsService }]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewsListComponent);
    component = fixture.componentInstance;
    newsService = TestBed.inject(NewsService) as unknown as MockNewsService;
    fixture.detectChanges();
  });

  // 1️⃣ Component Creation
  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // 2️⃣ Initial Load Stories Call
  it('should call loadStories on init', () => {
    spyOn(component, 'loadStories');
    component.ngOnInit();
    expect(component.loadStories).toHaveBeenCalled();
  });

  // 3️⃣ loadStories should populate stories and totalCount
  it('should populate stories and totalCount when loadStories is called', () => {
    component.loadStories();
    expect(newsService.getNewestStories).toHaveBeenCalledWith(component.currentPage, component.pageSize);
    expect(component.stories.length).toBe(2);
    expect(component.totalCount).toBe(2);
    expect(component.isLoading).toBeFalse();
  });

  // 4️⃣ onPageChange should call loadStories when not searching
  it('should call loadStories on page change when not searching', () => {
    spyOn(component, 'loadStories');
    component.isSearch = false;
    component.onPageChange(2);
    expect(component.currentPage).toBe(2);
    expect(component.loadStories).toHaveBeenCalled();
  });

  // 5️⃣ onPageChange should call onSearch when searching
  it('should call onSearch when isSearch is true', () => {
    spyOn(component, 'onSearch');
    component.isSearch = true;
    component.query = 'AI';
    component.onPageChange(3);
    expect(component.currentPage).toBe(3);
    expect(component.onSearch).toHaveBeenCalledWith('AI');
  });

  // 6️⃣ onSearch should call loadStories when query is empty
  it('should call loadStories when search query is empty', () => {
    spyOn(component, 'loadStories');
    component.onSearch('');
    expect(component.isSearch).toBeFalse();
    expect(component.loadStories).toHaveBeenCalled();
  });

  // 7️⃣ onSearch should call newsService.searchStories when query is not empty
  it('should search and update stories when query is valid', () => {
    component.query = 'AI';
    component.onSearch('AI');
    expect(newsService.searchStories).toHaveBeenCalledWith('AI', component.currentPage, component.pageSize);
    expect(component.stories.length).toBe(2);
    expect(component.totalCount).toBe(2);
    expect(component.isLoading).toBeFalse();
  });

  // 8️⃣ onSearch should reset currentPage to 1 for new searches
  it('should reset to page 1 when a new search query is provided', () => {
    component.currentPage = 5;
    component.query = 'old';
    component.onSearch('new');
    expect(component.currentPage).toBe(1);
    expect(component.isNewSearch).toBeTrue();
  });

  // 9️⃣ onPageSize should update pageSize and trigger onSearch
  it('should update pageSize and call onSearch', () => {
    spyOn(component, 'onSearch');
    component.onPageSize(20);
    expect(component.pageSize).toBe(20);
    expect(component.onSearch).toHaveBeenCalledWith(component.query);
  });

  // 🔟 should handle service response properly
  it('should update isLoading correctly around API calls', () => {
    component.isLoading = false;
    component.loadStories();
    expect(component.isLoading).toBeFalse(); // it should return to false after fetching
  });
});