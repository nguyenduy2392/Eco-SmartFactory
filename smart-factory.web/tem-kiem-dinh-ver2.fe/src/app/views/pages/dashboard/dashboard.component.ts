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
import { SharedModule } from '../../../shared.module';
import * as L from 'leaflet';
import { UiToastService } from '../../../services/shared/ui-toast.service';
import * as moment from 'moment';

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
      height: `100%`,
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

  /// Biểu đồ phân bố theo mẫu
  @ViewChild("pie-chart") pieChart: ChartComponent;
  public pieChartOptions: Partial<PieChartOptions> = {
    series: [0],
    chart: {
      type: "donut",
    },
    legend: {
      show: false
    },
    labels:
      [""],
    responsive: [
      {
        breakpoint: 480,
        options: {
          chart: {
            width: 200
          },
          dataLabels: {
            enabled: false,
          },
          legend: {
            position: "bottom",
            show: false
          }
        }
      }
    ]
  };

  /// Thời gian
  filter: any = {
    fromDate: null,
    toDate: null
  }

  /// Dữ liệu dashboard (Mock data)
  data: any = {
    summary: {
      quantityPurchased: 15000,
      quantityUsed: 8500,
      quantityUsedInWeek: 1200,
      quantityTemplate: 12
    },
    histories: [
      { key: '01/12', value: 150 },
      { key: '02/12', value: 230 },
      { key: '03/12', value: 180 },
      { key: '04/12', value: 290 },
      { key: '05/12', value: 350 },
      { key: '06/12', value: 420 },
      { key: '07/12', value: 380 },
      { key: '08/12', value: 450 },
      { key: '09/12', value: 520 },
      { key: '10/12', value: 480 },
      { key: '11/12', value: 550 },
      { key: '12/12', value: 620 },
      { key: '13/12', value: 580 },
      { key: '14/12', value: 650 },
      { key: '15/12', value: 700 }
    ],
    templates: [
      { key: "Mẫu tem A", value: 3500 },
      { key: "Mẫu tem B", value: 2800 },
      { key: "Mẫu tem C", value: 1500 },
      { key: "Mẫu tem D", value: 700 }
    ],
    publish: [
      {
        id: "lot-001",
        code: "LOT-2024-001",
        name: "Lô tem tháng 12/2024 - Đợt 1",
        labelTemplateId: "template-001",
        labelTemplateName: "Mẫu tem A",
        status: 1,
        quantity: 5000,
        startNo: 1,
        endNo: 5000
      },
      {
        id: "lot-002",
        code: "LOT-2024-002",
        name: "Lô tem tháng 12/2024 - Đợt 2",
        labelTemplateId: "template-002",
        labelTemplateName: "Mẫu tem B",
        status: 1,
        quantity: 3000,
        startNo: 5001,
        endNo: 8000
      },
      {
        id: "lot-003",
        code: "LOT-2024-003",
        name: "Lô tem tháng 11/2024",
        labelTemplateId: "template-001",
        labelTemplateName: "Mẫu tem A",
        status: 2,
        quantity: 4000,
        startNo: 8001,
        endNo: 12000
      }
    ]
  }

  /// Mock data lịch sử quét tem
  mockHistoryData: any[] = [
    { lat: 20.855403, lon: 106.682862, description: "Quét tem #001 - Hải Phòng" },
    { lat: 21.028511, lon: 105.804817, description: "Quét tem #002 - Hà Nội" },
    { lat: 20.940000, lon: 106.320000, description: "Quét tem #003 - Hải Dương" },
    { lat: 21.178500, lon: 106.050000, description: "Quét tem #004 - Bắc Ninh" },
    { lat: 20.750000, lon: 106.600000, description: "Quét tem #005 - Hải Phòng" }
  ];

  publishId: any = null;

  /// Bản đồ lịch sử quét tem
  private map;


  constructor(
    private _message: UiToastService
  ) { }

  ngOnInit() {
    this.filter.toDate = moment().toDate();
    this.filter.fromDate = moment().startOf('months').toDate();
    this.GetDashboard();
    this.initMap();
  }

  /// Dữ liệu dashboard (sử dụng mock data)
  GetDashboard() {
    // Mock data đã được định nghĩa sẵn trong this.data
    this.FillDataToChart();
  }

  GetHistory() {
    if (!this.publishId) {
      this._message.error("Vui lòng chọn lô xuất tem.");
      return;
    }

    // Sử dụng mock data
    this.mockHistoryData.forEach(element => {
      var LeafIcon = L.Icon.extend({
        options: {
          iconSize: [20, 20],
          shadowSize: [20, 24],
          iconAnchor: [22, 24],
          shadowAnchor: [4, 22],
          popupAnchor: [-3, -76]
        }
      });

      let lat = +element.lat;
      let long = +element.lon;
      var icon = new LeafIcon({
        iconUrl: '../../../../../assets/images/map-pin-icon.png',
      });

      let marker = L.marker([lat, long], { icon: icon });
      marker.bindTooltip(`${element.description}`);
      marker.addTo(this.map);
    });
  }

  /// Đưa dữ liệu vào biểu đồ
  FillDataToChart() {
    this.chartOptions = {
      series: [
        {
          name: "Số lượng tem",
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


    this.pieChartOptions = {
      series: (this.data.templates as any[]).map(x => x.value),
      chart: {
        type: "donut",
      },
      legend: {
        show: true,
        position: "bottom"
      },
      labels: (this.data.templates as any[]).map(x => x.key),
      responsive: [
        {
          breakpoint: 480,
          options: {
            chart: {
              width: 200
            },
            dataLabels: {
              enabled: false,
            },
            legend: {
              position: "bottom",
              show: false
            }
          }
        }
      ]
    };
  }

  private initMap(): void {
    this.map = L.map('maps', {
      center: [20.855403320210808, 106.68286287293593],
      zoom: 10
    });

    const tiles = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 30,
      minZoom: 3,
      attribution: '&copy; <a href="https://google.com/">Tên công ty</a>'
    });

    tiles.addTo(this.map);
  }
}
