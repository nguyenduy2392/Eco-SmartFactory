import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { PartService, PartDetail as ApiPartDetail } from '../../../services/part.service';
import { MessageService } from 'primeng/api';

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
    private location: Location,
    private partService: PartService,
    private messageService: MessageService
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
    if (!this.componentId || !this.poId) {
      return;
    }

    this.loading = true;
    
    this.partService.getById(this.componentId, this.poId).subscribe({
      next: (apiPart: ApiPartDetail) => {
        // Map API response to component interface
        this.part = {
          id: apiPart.id,
          productId: apiPart.productId,
          poId: this.poId!,
          componentName: apiPart.name,
          partNumber: apiPart.code,
          revision: apiPart.productCode ? `Rev ${apiPart.productCode}` : 'Rev 1.0',
          status: apiPart.status || (apiPart.isActive ? 'In Production' : 'Draft'),
          processes: apiPart.processes.map(process => ({
            id: process.id,
            name: process.name,
            icon: process.icon,
            color: process.color,
            description: process.description || '',
            isExpanded: true, // Default to expanded
            stages: process.stages.map(stage => ({
              id: stage.id,
              name: stage.operationName,
              machineId: stage.machineId,
              cycleTime: stage.cycleTime,
              materials: stage.materials.map(m => ({
                id: m.id,
                name: m.name,
                quantity: m.quantity,
                unit: m.unit
              })),
              tools: stage.tools.map(t => ({
                id: t.id,
                name: t.name,
                toolId: t.toolId
              }))
            }))
          }))
        };
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading part detail:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải thông tin chi tiết linh kiện'
        });
        // Fallback to empty part
        this.part = {
          id: this.componentId!,
          productId: this.productId!,
          poId: this.poId!,
          componentName: '',
          partNumber: '',
          revision: '',
          status: 'Draft',
          processes: []
        };
      }
    });
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

