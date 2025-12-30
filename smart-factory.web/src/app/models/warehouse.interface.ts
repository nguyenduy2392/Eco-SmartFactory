export interface Warehouse {
  id: string;
  code: string;
  name: string;
  address?: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface MaterialReceipt {
  id: string;
  customerId: string;
  customerName?: string;
  materialId: string;
  materialCode?: string;
  materialName?: string;
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;
  quantity: number;
  unit: string;
  batchNumber: string;
  receiptDate: Date;
  supplierCode?: string;
  purchasePOCode?: string;
  receiptNumber: string;
  notes?: string;
  status: string;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface MaterialIssue {
  id: string;
  customerId: string;
  customerName?: string;
  materialId: string;
  materialCode?: string;
  materialName?: string;
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;
  batchNumber: string;
  quantity: number;
  unit: string;
  issueDate: Date;
  reason: string;
  issueNumber: string;
  notes?: string;
  status: string;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface MaterialAdjustment {
  id: string;
  customerId: string;
  customerName?: string;
  materialId: string;
  materialCode?: string;
  materialName?: string;
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;
  batchNumber: string;
  adjustmentQuantity: number;
  unit: string;
  adjustmentDate: Date;
  reason: string;
  responsiblePerson: string;
  adjustmentNumber: string;
  notes?: string;
  status: string;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

export interface MaterialTransactionHistory {
  id: string;
  customerId: string;
  customerName?: string;
  materialId: string;
  materialCode?: string;
  materialName?: string;
  warehouseId: string;
  warehouseCode?: string;
  warehouseName?: string;
  batchNumber: string;
  transactionType: 'RECEIPT' | 'ISSUE' | 'ADJUSTMENT';
  referenceId?: string;
  referenceNumber?: string;
  stockBefore: number;
  quantityChange: number;
  stockAfter: number;
  unit: string;
  transactionDate: Date;
  createdBy?: string;
  notes?: string;
  createdAt: Date;
}

export interface MaterialStock {
  materialId: string;
  materialCode: string;
  materialName: string;
  customerId: string;
  customerName: string;
  warehouseId?: string;
  warehouseCode?: string;
  warehouseName?: string;
  currentStock: number;
  minStock: number;
  unit: string;
  batchStocks: BatchStock[];
}

export interface BatchStock {
  batchNumber: string;
  quantity: number;
  unit: string;
  lastReceiptDate?: Date;
  lastIssueDate?: Date;
}

// Request interfaces
export interface CreateMaterialReceiptRequest {
  customerId: string;
  materialId: string;
  warehouseId: string;
  quantity: number;
  unit: string;
  batchNumber: string;
  receiptDate: Date;
  supplierCode?: string;
  purchasePOCode?: string;
  receiptNumber: string;
  notes?: string;
}

export interface CreateMaterialIssueRequest {
  customerId: string;
  materialId: string;
  warehouseId: string;
  batchNumber: string;
  quantity: number;
  unit: string;
  issueDate: Date;
  reason: string;
  issueNumber: string;
  notes?: string;
}

export interface CreateMaterialAdjustmentRequest {
  customerId: string;
  materialId: string;
  warehouseId: string;
  batchNumber: string;
  adjustmentQuantity: number;
  unit: string;
  adjustmentDate: Date;
  reason: string;
  responsiblePerson: string;
  adjustmentNumber: string;
  notes?: string;
}

