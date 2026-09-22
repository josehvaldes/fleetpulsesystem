import { TestBed } from '@angular/core/testing';
import { Drivers } from './drivers';

describe('Drivers', () => {
  let service: Drivers;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Drivers);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
