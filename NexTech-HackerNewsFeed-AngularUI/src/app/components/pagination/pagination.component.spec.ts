import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationComponent } from './pagination.component';
import { FormsModule } from '@angular/forms';

describe('PaginationComponent', () => {
  let component: PaginationComponent;
  let fixture: ComponentFixture<PaginationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaginationComponent, FormsModule]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaginationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // 1️⃣ Component creation
  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // 2️⃣ Should emit pageChange event when onPageChange is called
  it('should emit pageChange when onPageChange is called', () => {
    spyOn(component.pageChange, 'emit');
    component.onPageChange(3);

    expect(component.currentPage).toBe(3);
    expect(component.pageChange.emit).toHaveBeenCalledWith(3);
  });

  // 3️⃣ Should emit reloadStories event when updatePageSize is called
  it('should emit reloadStories when updatePageSize is called', () => {
    spyOn(component.reloadStories, 'emit');

    component.pageSize = 10;
    component.currentPage = 2;
    component.updatePageSize(20);

    expect(component.pageSize).toBe(20);
    expect(component.currentPage).toBe(1);
    expect(component.reloadStories.emit).toHaveBeenCalledWith(20);
  });

  // 4️⃣ Should correctly calculate total pages
  it('should calculate totalPages correctly', () => {
    component.totalCount = 100;
    component.pageSize = 10;

    const total = component.totalPages();
    expect(total).toBe(10);
  });

  // 5️⃣ Should return 0 totalPages when pageSize is 0 (avoid division by zero)
  it('should return Infinity or handle zero pageSize gracefully', () => {
    component.totalCount = 50;
    component.pageSize = 0;

    const total = component.totalPages();
   expect(total).toBe(0); // since dividing by 0 returns Infinity
  });

  // 6️⃣ Should handle onPageSizeChange with valid event target
  it('should call updatePageSize when onPageSizeChange is triggered', () => {
    spyOn(component, 'updatePageSize');

    const mockEvent = {
      target: { value: '25' }
    } as unknown as Event;

    component.onPageSizeChange(mockEvent);
    expect(component.updatePageSize).toHaveBeenCalledWith(25);
  });

  // 8️⃣ Should correctly bind inputs and compute total pages
  it('should reflect input changes and compute correctly', () => {
    component.currentPage = 2;
    component.pageSize = 20;
    component.totalCount = 200;

    expect(component.totalPages()).toBe(10);
    expect(component.currentPage).toBe(2);
  });

  // 9️⃣ Should log size change to console when updatePageSize is called
  it('should log the new size to console', () => {
    spyOn(console, 'log');
    component.updatePageSize(15);
    expect(console.log).toHaveBeenCalledWith('size: 15');
  });

  // 🔟 Should reset to first page when page size changes
  it('should reset currentPage to 1 when updatePageSize is called', () => {
    component.currentPage = 5;
    component.updatePageSize(30);
    expect(component.currentPage).toBe(1);
  });
});
