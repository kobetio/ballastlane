import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';

import { Register } from './register';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), provideNoopAnimations()]
    }).compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should require a password of at least 8 characters', () => {
    component.form.controls.password.setValue('short');
    expect(component.form.controls.password.hasError('minlength')).toBe(true);
  });

  it('should be valid with well-formed input', () => {
    component.form.setValue({ name: 'Jane Doe', email: 'jane@example.com', password: 'P@ssword123' });
    expect(component.form.valid).toBe(true);
  });
});
