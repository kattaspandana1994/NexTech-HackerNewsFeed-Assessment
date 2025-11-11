import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject,Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { SearchResult } from '../components/news-list/news-list.component';
import { BASE_URL } from '../app.config'; 
@Injectable({
  providedIn: 'root'
})

export class NewsService {

  constructor(private http: HttpClient,
     @Inject(BASE_URL) private baseUrl: string
  ) { }

  getNewestStories(page: number, pageSize: number): Observable<any> {
  return this.http.get(`${this.baseUrl}`, {
    params: new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString()),
    observe: 'response'
  }).pipe(
    map(response => response.status === 200 ? response.body : { stories: [], totalCount: 0 }),
    catchError(() => of({ stories: [], totalCount: 0 }))
  );
}

searchStories(query: string, currentPage: number, pageSize: number): Observable<any> {
  let params = new HttpParams()
    .set('query', query)
    .set('page', currentPage.toString())
    .set('pageSize', pageSize.toString());

  return this.http.get<SearchResult>(`${this.baseUrl}/search`, { params, observe: 'response' }).pipe(
    map(response => response.status === 200 ? response.body : { stories: [], totalCount: 0 }),
    catchError(() => of({ stories: [], totalCount: 0 }))
  );
}

}
