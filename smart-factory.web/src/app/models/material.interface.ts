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
  description?: string;
  isActive: boolean;
  createdAt: Date;
  // Gắn với chủ hàng (Customer)
  customerId: string;
  customerCode?: string;
  customerName?: string;
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
  description?: string;
  customerId: string; // Bắt buộc - Material phải gắn với chủ hàng
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
  description?: string;
  isActive: boolean;
  customerId: string; // Bắt buộc - Material phải gắn với chủ hàng
}





