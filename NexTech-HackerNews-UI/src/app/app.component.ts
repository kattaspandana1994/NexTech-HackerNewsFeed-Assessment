import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NewsListComponent } from './components/news-list/news-list.component';
import { HeaderComponent } from './header/header.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NewsListComponent, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',

})
export class AppComponent {
}
