import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { NewsItemComponent } from './news-item.component';

describe('NewsItemComponent', () => {
  let component: NewsItemComponent;
  let fixture: ComponentFixture<NewsItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, NewsItemComponent]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NewsItemComponent);
    component = fixture.componentInstance;
  });

  // 1️⃣ Should render story card correctly when story is provided
  it('should render story title and Open Story button when story exists', () => {
    const mockStory = {
      title: 'Exploring AI Frontiers',
      url: 'https://example.com/ai'
    };

    component.story = mockStory;
    fixture.detectChanges();

    const cardEl = fixture.debugElement.query(By.css('.story-card')).nativeElement;
    const titleEl = fixture.debugElement.query(By.css('.story-title')).nativeElement;
    const buttonEl = fixture.debugElement.query(By.css('.open-story-btn')).nativeElement;

    expect(cardEl).toBeTruthy();
    expect(titleEl.textContent.trim()).toBe(mockStory.title);
    expect(buttonEl.textContent.trim()).toBe('Open Story');
    expect(buttonEl.getAttribute('href')).toBe(mockStory.url);
    expect(buttonEl.getAttribute('target')).toBe('_blank');
  });

  // 2️⃣ Should not render anything if story is null or undefined
  it('should not render story card if story is null or undefined', () => {
    component.story = null;
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('.story-card'))).toBeNull();

    (component as any).story = undefined;
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('.story-card'))).toBeNull();
  });

  // 3️⃣ Should not display Open Story button if URL is empty or missing
  it('should not display Open Story button when URL is missing', () => {
    const mockStory = {
      title: 'Story Without Link',
      url: ''
    };

    component.story = mockStory;
    fixture.detectChanges();

    const buttonEl = fixture.debugElement.query(By.css('.open-story-btn'));
    expect(buttonEl).toBeNull();
  });

  // 4️⃣ Should handle empty title gracefully
  it('should render empty title text if story.title is empty', () => {
    const mockStory = {
      title: '',
      url: 'https://example.com/story'
    };

    component.story = mockStory;
    fixture.detectChanges();

    const titleEl = fixture.debugElement.query(By.css('.story-title')).nativeElement;
    expect(titleEl.textContent.trim()).toBe('');
  });

  // 5️⃣ Should apply proper Bootstrap structure and classes
  it('should have proper Bootstrap card structure and classes', () => {
    const mockStory = {
      title: 'Bootstrap Styled Story',
      url: 'https://bootstrap.com/story'
    };

    component.story = mockStory;
    fixture.detectChanges();

    const cardEl = fixture.debugElement.query(By.css('.card'));
    const bodyEl = fixture.debugElement.query(By.css('.card-body'));
    const buttonEl = fixture.debugElement.query(By.css('.btn-sm'));

    expect(cardEl).toBeTruthy();
    expect(bodyEl).toBeTruthy();
    expect(buttonEl).toBeTruthy();
  });

  // 6️⃣ Should have correct link behavior (open in new tab)
  it('should open link in a new tab', () => {
    const mockStory = {
      title: 'External Resource',
      url: 'https://test.com/'
    };

    component.story = mockStory;
    fixture.detectChanges();

    const buttonEl = fixture.debugElement.query(By.css('.open-story-btn')).nativeElement;
    expect(buttonEl.getAttribute('target')).toBe('_blank');
    expect(buttonEl.getAttribute('href')).toBe(mockStory.url);
  });
});