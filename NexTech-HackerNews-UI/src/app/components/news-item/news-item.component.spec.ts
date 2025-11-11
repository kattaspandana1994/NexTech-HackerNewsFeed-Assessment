import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { By } from '@angular/platform-browser';
import { NewsItemComponent } from './news-item.component';

describe('NewsItemComponent', () => {
  let component: NewsItemComponent;
  let fixture: ComponentFixture<NewsItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [],
      imports: [NewsItemComponent,MatCardModule, MatButtonModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewsItemComponent);
    component = fixture.componentInstance;
  });

  // 1. Test for Story Rendering (When `story` Exists)
  it('should display the story title, and button', () => {
    const mockStory = {
      title: 'Test Story Title',
      url: 'https://example.com/'
    };
    component.story = mockStory;
    fixture.detectChanges(); // Trigger change detection

    const cardTitle = fixture.debugElement.query(By.css('mat-card-title')).nativeElement;
    const button = fixture.debugElement.query(By.css('.myBtnOpen')).nativeElement;

    expect(cardTitle.textContent).toBe(mockStory.title);
    expect(button.href).toBe(mockStory.url);
    expect(button.textContent).toBe(' Open story ');
  });

  // 2. Test for No Story (When `story` is Undefined or Null)
  it('should not render the card if story is undefined or null', () => {
    component.story = undefined;
    fixture.detectChanges();

    const card = fixture.debugElement.query(By.css('mat-card'));
    expect(card).toBeNull();

    component.story = null;
    fixture.detectChanges();

    const cardNull = fixture.debugElement.query(By.css('mat-card'));
    expect(cardNull).toBeNull();
  });

 

  // 3. Test for `Open story` Button Link
  it('should link to the correct URL when "Open story" is clicked', () => {
    const mockStory = {
      title: 'Test Story',
      text: 'Test content for the story.',
      url: 'https://test-url.com/'
    };
    component.story = mockStory;
    fixture.detectChanges();

    const button = fixture.debugElement.query(By.css('.myBtnOpen')).nativeElement;
    expect(button.href).toBe('https://test-url.com/');
  });

  // 4. Test for Empty Story (Empty Title and has URL)
  it('should handle an empty story gracefully', () => {
    const mockStory = {
      title: '',
      url: 'https://example.com/'
    };
    component.story = mockStory;
    fixture.detectChanges();

    const cardTitle = fixture.debugElement.query(By.css('mat-card-title')).nativeElement;

    expect(cardTitle.textContent).toBe('');
  });

  // 5. Test for Button Click Opens Correct URL in a New Tab
  it('should open the correct URL in a new tab', () => {
    const mockStory = {
      title: 'Test Story',
      url: 'https://test-url.com/'
    };
    component.story = mockStory;
    fixture.detectChanges();

    const button = fixture.debugElement.query(By.css('.myBtnOpen')).nativeElement;
    expect(button.target).toBe('_blank');
    expect(button.href).toBe('https://test-url.com/');
  });

 
});
