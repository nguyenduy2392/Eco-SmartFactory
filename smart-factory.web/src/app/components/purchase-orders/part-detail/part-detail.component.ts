import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';

export interface PartDetail {
  id: string;
  productId: string;
  poId: string;
  componentName: string;
  partNumber: string;
  revision: string;
  status: string;
  processes: ProcessType[];
}

export interface ProcessType {
  id: string;
  name: string;
  icon: string;
  color: string;
  description: string;
  isExpanded: boolean;
  stages: WorkStage[];
}

export interface WorkStage {
  id: string;
  name: string;
  machineId?: string;
  cycleTime?: number;
  materials: Material[];
  tools: Tool[];
}

export interface Material {
  id: string;
  name: string;
  quantity: number;
  unit: string;
}

export interface Tool {
  id: string;
  name: string;
  toolId: string;
}

@Component({
  selector: 'app-part-detail',
  templateUrl: './part-detail.component.html',
  styleUrls: ['./part-detail.component.scss'],
  standalone: true,
  imports: [SharedModule, PrimengModule]
})
export class PartDetailComponent implements OnInit {
  poId: string | null = null;
  productId: string | null = null;
  componentId: string | null = null;
  part: PartDetail | null = null;
  loading = false;

  statusOptions = [
    { label: 'Đang sản xuất', value: 'In Production' },
    { label: 'Nháp', value: 'Draft' },
    { label: 'Đã lưu trữ', value: 'Archived' }
  ];

  unitOptions = [
    { label: 'kg', value: 'kg' },
    { label: 'gr', value: 'gr' },
    { label: 'pcs', value: 'pcs' },
    { label: 'L', value: 'L' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.poId = params['poId'];
      this.productId = params['productId'];
      this.componentId = params['componentId'];
      if (this.poId && this.productId && this.componentId) {
        this.loadPartDetail();
      }
    });
  }

  loadPartDetail(): void {
    // TODO: Replace with actual API call
    this.loading = true;
    
    // Mock data for demonstration
    setTimeout(() => {
      this.part = {
        id: this.componentId!,
        productId: this.productId!,
        poId: this.poId!,
        componentName: 'Vỏ Nhựa Ngoài - Top Housing',
        partNumber: 'PN-2023-8821',
        revision: 'Rev 1.2',
        status: 'In Production',
        processes: [
          {
            id: '1',
            name: 'Ép Phun (Injection)',
            icon: 'pi-box',
            color: 'blue',
            description: 'Primary forming process',
            isExpanded: true,
            stages: [
              {
                id: '1-1',
                name: 'Chuẩn bị & Sấy nguyên liệu',
                machineId: 'MAY-EP-01',
                materials: [
                  { id: 'm1', name: 'Nhựa ABS - Black', quantity: 120, unit: 'kg' },
                  { id: 'm2', name: 'Hạt màu đen', quantity: 2, unit: 'kg' }
                ],
                tools: [
                  { id: 't1', name: 'Máy sấy hạt nhựa', toolId: 'TOOL-005' }
                ]
              },
              {
                id: '1-2',
                name: 'Ép định hình sản phẩm',
                cycleTime: 45,
                materials: [],
                tools: [
                  { id: 't2', name: 'Khuôn ép chính', toolId: 'MOLD-A88' }
                ]
              }
            ]
          },
          {
            id: '2',
            name: 'Sơn (Painting)',
            icon: 'pi-palette',
            color: 'orange',
            description: 'Finishing process',
            isExpanded: true,
            stages: [
              {
                id: '2-1',
                name: 'Phun sơn lót',
                materials: [
                  { id: 'm3', name: 'Sơn lót xám', quantity: 0.5, unit: 'L' }
                ],
                tools: []
              }
            ]
          },
          {
            id: '3',
            name: 'Lắp ráp (Assembly)',
            icon: 'pi-wrench',
            color: 'purple',
            description: 'Final assembly',
            isExpanded: false,
            stages: []
          }
        ]
      };
      this.loading = false;
    }, 500);
  }

  goBack(): void {
    this.location.back();
  }

  goToProduct(): void {
    if (this.poId && this.productId) {
      this.router.navigate(['/purchase-orders', this.poId, 'products', this.productId]);
    }
  }

  saveChanges(): void {
    // TODO: Implement save functionality
    console.log('Save changes', this.part);
  }

  discardChanges(): void {
    this.goBack();
  }

  toggleProcess(process: ProcessType): void {
    process.isExpanded = !process.isExpanded;
  }

  addWorkStage(process: ProcessType): void {
    // TODO: Implement add work stage
    console.log('Add work stage to process', process);
  }

  deleteStage(processId: string, stageId: string): void {
    // TODO: Implement delete stage
    console.log('Delete stage', processId, stageId);
  }

  addMaterial(stage: WorkStage): void {
    stage.materials.push({
      id: Date.now().toString(),
      name: '',
      quantity: 0,
      unit: 'kg'
    });
  }

  removeMaterial(stage: WorkStage, materialId: string): void {
    stage.materials = stage.materials.filter(m => m.id !== materialId);
  }

  addTool(stage: WorkStage): void {
    stage.tools.push({
      id: Date.now().toString(),
      name: '',
      toolId: ''
    });
  }

  removeTool(stage: WorkStage, toolId: string): void {
    stage.tools = stage.tools.filter(t => t.id !== toolId);
  }

  addNewProcessType(): void {
    // TODO: Implement add new process type
    console.log('Add new process type');
  }

  getProcessColor(color: string): string {
    const colors: { [key: string]: string } = {
      'blue': '#3B82F6',
      'orange': '#F97316',
      'purple': '#A855F7'
    };
    return colors[color] || '#6B7280';
  }
}

