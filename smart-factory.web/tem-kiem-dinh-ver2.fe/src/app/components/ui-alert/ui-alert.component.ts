import { Component } from '@angular/core';
import { UiAlert } from '../../models/interface/uiInterface';
import { UiAlertService } from '../../services/shared/ui-alert.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ui-alert',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ui-alert.component.html',
  styleUrl: './ui-alert.component.scss'
})
export class UiAlertComponent {
  alerts: UiAlert[] = [];
  constructor(private alertService: UiAlertService) { }

  ngOnInit() {
    this.alertService.alertState.subscribe(alert => {
      this.alerts.push(alert);
      setTimeout(() => this.removeAlert(alert), 5000);  // Tự động loại bỏ alert sau 5 giây
    });
  }

  removeAlert(alert: UiAlert) {
    this.alerts = this.alerts.filter(a => a !== alert);
  }
}
