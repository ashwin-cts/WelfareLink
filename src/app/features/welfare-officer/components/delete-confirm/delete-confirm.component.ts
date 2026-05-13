import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-delete-confirm',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delete-confirm.component.html',
  styleUrls: ['./delete-confirm.component.css']
})
export class DeleteConfirmComponent {
  @Input() applicationData: any; 
  @Output() confirmed = new EventEmitter<number>();
  @Output() canceled = new EventEmitter<void>();

  onConfirm() {
    if (this.applicationData?.applicationID) {
      this.confirmed.emit(this.applicationData.applicationID);
    }
  }

  onCancel() {
    this.canceled.emit();
  }
}