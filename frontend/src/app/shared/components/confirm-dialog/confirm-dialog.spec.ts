import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { ConfirmDialog, ConfirmDialogData } from './confirm-dialog';

describe('ConfirmDialog', () => {
  let component: ConfirmDialog;
  let fixture: ComponentFixture<ConfirmDialog>;
  let dialogRef: { close: (result?: boolean) => void };

  const data: ConfirmDialogData = { title: 'Delete book', message: 'Are you sure?' };

  beforeEach(async () => {
    dialogRef = { close: () => {} };

    await TestBed.configureTestingModule({
      imports: [ConfirmDialog],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmDialog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('confirm() closes the dialog with true', () => {
    let result: boolean | undefined;
    dialogRef.close = (value?: boolean) => (result = value);

    component.confirm();

    expect(result).toBe(true);
  });

  it('cancel() closes the dialog with false', () => {
    let result: boolean | undefined;
    dialogRef.close = (value?: boolean) => (result = value);

    component.cancel();

    expect(result).toBe(false);
  });

  it('falls back to default button labels when not provided', () => {
    expect(component.confirmLabel).toBe('Confirm');
    expect(component.cancelLabel).toBe('Cancel');
  });
});
