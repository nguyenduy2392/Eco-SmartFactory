// Stock In (Nhập Kho) interfaces

export interface StockInRequest {
  purchaseOrderId?: string | null;
  customerId: string;
  warehouseId: string;
  receiptDate: Date;
  receiptNumber: string;
  notes?: string;
  materials: StockInMaterialItem[];
}

export interface StockInMaterialItem {
  materialId: string;
  quantity: number;
  unit: string;
  batchNumber: string;
  supplierCode?: string;
  purchasePOCode?: string;
  notes?: string;
}

export interface StockInResponse {
  success: boolean;
  message?: string;
  receiptIds?: string[];
  historyIds?: string[];
  errors?: string[];
}

// Material Receipt History (Lịch sử nhập kho cho PO)
export interface MaterialReceiptHistory {
  id: string;
  purchaseOrderId?: string;
  poNumber?: string;
  materialReceiptId: string;
  receiptNumber: string;
  materialId: string;
  materialCode: string;
  materialName: string;
  quantity: number;
  unit: string;
  batchNumber: string;
  receiptDate: Date;
  createdBy?: string;
  notes?: string;
}

// PO for selection (dropdown/search)
export interface POForSelection {
  id: string;
  poNumber: string;
  customerName: string;
  poDate: Date;
  status: string;
  isMaterialFullyReceived: boolean;
}

// Update PO Material Status
export interface UpdatePOMaterialStatusRequest {
  isMaterialFullyReceived: boolean;
}

// Purchase Order Material (từ sheet NVL trong Excel)
export interface PurchaseOrderMaterial {
  id: string;
  purchaseOrderId: string;
  materialCode: string;
  materialName: string;
  materialType?: string;
  plannedQuantity: number;
  unit: string;
  colorCode?: string;
  notes?: string;
}
