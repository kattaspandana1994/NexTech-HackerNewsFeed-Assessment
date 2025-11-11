import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PaginationComponent, // ✅ Standalone component goes here
        NoopAnimationsModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should calculate total pages based on totalCount and pageSize', () => {
    component.totalCount = 95;
    component.pageSize = 10;
    expect(component.totalPages()).toBe(10);
  });

  it('should emit pageChange on page change', () => {
    spyOn(component.pageChange, 'emit');

    component.currentPage = 2;
    component.onPageChange(3);

    expect(component.pageChange.emit).toHaveBeenCalledWith(3);
    expect(component.currentPage).toBe(3);
  });

  it('should reset currentPage and emit reloadStories when page size is updated', () => {
    spyOn(component.reloadStories, 'emit');

    component.currentPage = 5;
    component.updatePageSize(50);

    expect(component.pageSize).toBe(50);
    expect(component.currentPage).toBe(1);
    expect(component.reloadStories.emit).toHaveBeenCalledWith(50);
  });

  it('should disable Previous button when on the first page', () => {
    component.currentPage = 1;
    fixture.detectChanges();

    const prevBtn = fixture.debugElement.query(By.css('button')).nativeElement;
    expect(prevBtn.disabled).toBeTrue();
  });

  it('should disable Next button when on the last page', () => {
    component.pageSize = 20;
    component.totalCount = 100;
    component.currentPage = 5;
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    const nextBtn = buttons[1].nativeElement; // second button is Next

    expect(nextBtn.disabled).toBeTrue();
  });
});
