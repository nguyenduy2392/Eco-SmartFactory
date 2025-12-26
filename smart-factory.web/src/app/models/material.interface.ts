export interface Material {
  id: string;
  code: string;
  name: string;
  type: string;
  colorCode?: string;
  supplier?: string;
  unit: string;
  currentStock: number;
  minStock: number;
  unitCost?: number;
  description?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateMaterialRequest {
  code: string;
  name: string;
  type: string;
  colorCode?: string;
  supplier?: string;
  unit: string;
  currentStock: number;
  minStock: number;
  unitCost?: number;
  description?: string;
}

export interface UpdateMaterialRequest {
  code: string;
  name: string;
  type: string;
  colorCode?: string;
  supplier?: string;
  unit: string;
  currentStock: number;
  minStock: number;
  unitCost?: number;
  description?: string;
  isActive: boolean;
}



