import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { NewsListComponent } from './news-list.component';
import { NewsService } from '../../services/news.service';
import { of } from 'rxjs';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { BASE_URL } from '../../app.config';

describe('NewsListComponent', () => {
  let component: NewsListComponent;
  let fixture: ComponentFixture<NewsListComponent>;
  let mockNewsService: jasmine.SpyObj<NewsService>;

  beforeEach(waitForAsync(() => {
    mockNewsService = jasmine.createSpyObj('NewsService', ['getNewestStories', 'searchStories']);

    TestBed.configureTestingModule({
      imports: [NewsListComponent, HttpClientTestingModule,NoopAnimationsModule],
      providers: [
        { provide: NewsService, useValue: mockNewsService },
        { provide: BASE_URL, useValue: 'http://mock-api.com' },
        provideHttpClient(withInterceptorsFromDi())
      ]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewsListComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load stories on init', () => {
    const mockData = {
      stories: [{ title: 'Story 1', url: 'http://story1.com' }],
      count: 1
    };

    mockNewsService.getNewestStories.and.returnValue(of(mockData));

    fixture.detectChanges(); // triggers ngOnInit()

    expect(component.stories.length).toBe(1);
    expect(component.totalCount).toBe(1);
    expect(component.isLoading).toBeFalse();
    expect(mockNewsService.getNewestStories).toHaveBeenCalledWith(1, 10);
  });

  it('should perform search and update results', () => {
    const searchData = {
      stories: [{ title: 'Search Result', url: 'http://search.com' }],
      count: 1
    };

    mockNewsService.searchStories.and.returnValue(of(searchData));
    component.onSearch('Angular');

    expect(component.isSearch).toBeTrue();
    expect(component.stories.length).toBe(1);
    expect(component.totalCount).toBe(1);
    expect(mockNewsService.searchStories).toHaveBeenCalledWith('Angular', 1, 10);
  });

  it('should fallback to loadStories when empty query passed in onSearch', () => {
    const mockData = {
      stories: [{ title: 'Fallback Story', url: 'http://fallback.com' }],
      count: 1
    };

    mockNewsService.getNewestStories.and.returnValue(of(mockData));
    component.onSearch('');

    expect(component.isSearch).toBeFalse();
    expect(component.stories.length).toBe(1);
    expect(mockNewsService.getNewestStories).toHaveBeenCalled();
  });

  it('should handle page change correctly', () => {
    const page = 2;
    const mockData = {
      stories: [{ title: 'Page Story', url: 'http://page.com' }],
      count: 1
    };

    mockNewsService.getNewestStories.and.returnValue(of(mockData));
    component.onPageChange(page);

    expect(component.currentPage).toBe(page);
    expect(mockNewsService.getNewestStories).toHaveBeenCalledWith(page, 10);
  });
});
