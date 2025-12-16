import { Component, OnInit, ViewChild } from '@angular/core';
import { PrimengModule } from '../../../primeng.module';
import {
  ChartComponent,
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexDataLabels,
  ApexStroke,
  ApexYAxis,
  ApexTitleSubtitle,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexResponsive
} from "ng-apexcharts";
import moment from 'moment';
import { TextGlobalConstants } from '../../../shared/TextGlobalContants';
import { SharedModule } from '../../../shared.module';
import * as L from 'leaflet';
import { IsNull } from '../../../services/shared/common';
import { UiToastService } from '../../../services/shared/ui-toast.service';
import { ReportService } from '../../../services/report/report.service';

export type LineChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  stroke: ApexStroke;
  dataLabels: ApexDataLabels;
  yaxis: ApexYAxis;
  title: ApexTitleSubtitle;
  labels: string[];
  legend: ApexLegend;
  subtitle: ApexTitleSubtitle;
};

export type PieChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  responsive: ApexResponsive[];
  labels: any;
  legend: ApexLegend
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [PrimengModule, SharedModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {

  /// Biểu đồ số lượng theo tháng
  @ViewChild("line-chart") chart: ChartComponent;
  public chartOptions: Partial<LineChartOptions> = {
    series: [
      {
        name: "",
        data: [0]
      }
    ],
    chart: {
      type: "area",
      height: 450,
      zoom: {
        enabled: false
      }
    },
    dataLabels: {
      enabled: false
    },
    stroke: {
      curve: "straight"
    },

    title: {
      text: "",
      align: "left"
    },
    subtitle: {
      text: "",
      align: "left"
    },
    labels: [""],
    xaxis: {
      type: "category"
    },
    yaxis: {
      opposite: true
    },
    legend: {
      horizontalAlign: "left",
      show: false
    }
  };

  /// Dữ liệu dashboard
  data: any = {
    summary: {
      totalDiary: 0,
      totalQuantity: 0,
      totalCustomer: 0,
    },
    histories: [
      {
        key: "01",
        value: 0
      },
      {
        key: "02",
        value: 0
      },
      {
        key: "03",
        value: 0
      },
      {
        key: "04",
        value: 0
      },
      {
        key: "05",
        value: 0
      },
      {
        key: "06",
        value: 0
      },
      {
        key: "07",
        value: 0
      },
      {
        key: "08",
        value: 0
      },
      {
        key: "09",
        value: 0
      },
      {
        key: "10",
        value: 0
      },
      {
        key: "11",
        value: 0
      },
      {
        key: "12",
        value: 0
      },
      {
        key: "13",
        value: 0
      },
      {
        key: "14",
        value: 0
      },
      {
        key: "15",
        value: 0
      },
      {
        key: "16",
        value: 0
      },
      {
        key: "17",
        value: 0
      },
      {
        key: "18",
        value: 0
      },
      {
        key: "19",
        value: 0
      },
      {
        key: "20",
        value: 0
      },
      {
        key: "21",
        value: 0
      },
      {
        key: "22",
        value: 0
      },
      {
        key: "23",
        value: 0
      },
      {
        key: "24",
        value: 0
      },
      {
        key: "25",
        value: 0
      },
      {
        key: "26",
        value: 0
      },
      {
        key: "27",
        value: 0
      },
      {
        key: "28",
        value: 0
      },
      {
        key: "29",
        value: 0
      },
      {
        key: "30",
        value: 0
      }
    ]
  }

  publishId: any = null;

  /// Bản đồ lịch sử quét tem
  private map;

  constructor(
    private _message: UiToastService,
    private _service: ReportService
  ) { }

  ngOnInit() {
    const documentStyle = getComputedStyle(document.documentElement);
    this.GetDashboard();
  }

  /// Dữ liệu dashboard
  GetDashboard() {
    this._service.GetDashboard().subscribe((response: any) => {

      let summaries = response.data.summary as any[];

      this.data.summary.totalDiary = summaries.find(x => x.key == "TotalDiary")?.value ?? 0;
      this.data.summary.totalQuantity = summaries.find(x => x.key == "TotalQuantity")?.value ?? 0;
      this.data.summary.totalCustomer = summaries.find(x => x.key == "TotalCustomer")?.value ?? 0;

      this.data.histories = response.data.monthly as any[];

      this.FillDataToChart();
    });
  }

  /// Đưa dữ liệu vào biểu đồ
  FillDataToChart() {
    this.chartOptions = {
      series: [
        {
          name: "",
          data: (this.data.histories as any[]).map(x => x.value)
        }
      ],
      chart: {
        type: "area",
        height: 350,
        zoom: {
          enabled: false
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        curve: "straight"
      },

      title: {
        text: "",
        align: "left"
      },
      subtitle: {
        text: "",
        align: "left"
      },
      labels: (this.data.histories as any[]).map(x => x.key),
      xaxis: {
        type: "category"
      },
      yaxis: {
        opposite: true
      },
      legend: {
        horizontalAlign: "left",
        show: false
      }
    };


  }
}
