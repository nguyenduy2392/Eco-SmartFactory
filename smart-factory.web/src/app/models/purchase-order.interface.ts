export interface PurchaseOrder {
  id: string;
  poNumber: string;
  customerId: string;
  customerName: string;
  versionType: 'ORIGINAL' | 'FINAL' | 'PRODUCTION';
  templateType?: string;
  poDate: Date;
  expectedDeliveryDate?: Date;
  status: 'New' | 'InProgress' | 'Completed' | 'Cancelled';
  totalAmount: number;
  notes?: string;
  originalPOId?: string;
  versionNumber: number;
  isActive: boolean;
  createdAt: Date;
  createdBy?: string;
  products?: POProduct[];
  operations?: POOperation[];
}

export interface PurchaseOrderList {
  id: string;
  poNumber: string;
  customerName: string;
  versionType: string;
  poDate: Date;
  status: string;
  totalAmount: number;
  productCount: number;
  createdAt: Date;
}

export interface CreatePurchaseOrderRequest {
  poNumber: string;
  customerId: string;
  templateType?: string;
  poDate: Date;
  expectedDeliveryDate?: Date;
  notes?: string;
  products?: CreatePOProductRequest[];
}

export interface UpdatePurchaseOrderRequest {
  customerId: string;
  poDate: Date;
  expectedDeliveryDate?: Date;
  status: string;
  notes?: string;
}

export interface ClonePOVersionRequest {
  originalPOId: string;
  newVersionType: string;
  notes?: string;
}

export interface POProduct {
  id: string;
  purchaseOrderId: string;
  productId: string;
  productCode: string;
  productName: string;
  quantity: number;
  unitPrice?: number;
  totalAmount: number;
}

export interface CreatePOProductRequest {
  productId: string;
  quantity: number;
  unitPrice?: number;
}

export interface POOperation {
  id: string;
  purchaseOrderId: string;
  partId: string;
  partCode: string;
  partName: string;
  processingTypeId: string;
  processingTypeName: string;
  processMethodId?: string;
  processMethodName?: string;
  operationName: string;
  chargeCount: number;
  unitPrice: number;
  quantity: number;
  totalAmount: number;
  sprayPosition?: string;
  printContent?: string;
  cycleTime?: number;
  assemblyContent?: string;
  sequenceOrder: number;
}

export interface CreatePOOperationRequest {
  partId: string;
  processingTypeId: string;
  processMethodId?: string;
  operationName: string;
  chargeCount: number;
  unitPrice: number;
  quantity: number;
  sprayPosition?: string;
  printContent?: string;
  cycleTime?: number;
  assemblyContent?: string;
  sequenceOrder: number;
}

