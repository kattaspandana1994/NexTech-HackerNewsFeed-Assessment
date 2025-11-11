import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NewsService } from './news.service';
import { BASE_URL } from '../app.config';
import { HttpParams } from '@angular/common/http';

describe('NewsService', () => {
  let service: NewsService;
  let httpMock: HttpTestingController;
  const baseUrl = 'http://fakeapi.com/news';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        NewsService,
        { provide: BASE_URL, useValue: baseUrl }
      ]
    });

    service = TestBed.inject(NewsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getNewestStories', () => {
    it('should fetch newest stories with correct params and return data on 200', () => {
      const mockResponse = { stories: [{ title: 'test' }], totalCount: 1 };

      service.getNewestStories(1, 10).subscribe(result => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(r =>
        r.url === baseUrl &&
        r.params.get('page') === '1' &&
        r.params.get('pageSize') === '10'
      );

      expect(req.request.method).toBe('GET');

      req.flush(mockResponse, { status: 200, statusText: 'OK' });
    });

    it('should return empty results if status is not 200', () => {
      service.getNewestStories(1, 10).subscribe(result => {
        expect(result).toEqual({ stories: [], totalCount: 0 });
      });

       const req = httpMock.expectOne(request =>
    request.url === baseUrl &&
    request.params.get('page') === '1' &&
    request.params.get('pageSize') === '10'
  );

      req.flush(null, { status: 404, statusText: 'Not Found' });
    });

    it('should return empty results on error', () => {
      service.getNewestStories(1, 10).subscribe(result => {
        expect(result).toEqual({ stories: [], totalCount: 0 });
      });

       const req = httpMock.expectOne(request =>
    request.url === baseUrl &&
    request.params.get('page') === '1' &&
    request.params.get('pageSize') === '10'
  );

      req.error(new ErrorEvent('Network error'));
    });
  });

  describe('searchStories', () => {
    it('should fetch searched stories with correct params and return data on 200', () => {
      const query = 'angular';
      const currentPage = 2;
      const pageSize = 5;
      const mockResponse = { stories: [{ title: 'search test' }], totalCount: 1 };

      service.searchStories(query, currentPage, pageSize).subscribe(result => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(r =>
        r.url === `${baseUrl}/search` &&
        r.params.get('query') === query &&
        r.params.get('page') === currentPage.toString() &&
        r.params.get('pageSize') === pageSize.toString()
      );

      expect(req.request.method).toBe('GET');

      req.flush(mockResponse, { status: 200, statusText: 'OK' });
    });

    it('should return empty results if status is not 200', () => {
      service.searchStories('angular', 1, 10).subscribe(result => {
        expect(result).toEqual({ stories: [], totalCount: 0 });
      });

      const req = httpMock.expectOne(r =>
        r.url === `${baseUrl}/search` &&
        r.params.get('page') === '1' &&
        r.params.get('pageSize') === '10'
      );

      req.flush(null, { status: 500, statusText: 'Server Error' });
    });

    it('should return empty results on error', () => {
      service.searchStories('angular', 1, 10).subscribe(result => {
        expect(result).toEqual({ stories: [], totalCount: 0 });
      });

      const req = httpMock.expectOne(r =>
        r.url === `${baseUrl}/search` &&
        r.params.get('page') === '1' &&
        r.params.get('pageSize') === '10'
      );

      req.error(new ErrorEvent('Network error'));
    });
  });
});
