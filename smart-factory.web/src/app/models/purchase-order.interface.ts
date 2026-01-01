// Processing PO - FINANCIAL BASELINE only
export interface PurchaseOrder {
  id: string;
  poNumber: string;
  customerId: string;
  customerName: string;
  processingType: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP'; // Processing type determines template
  version: string; // V0, V1, V2, ...
  status: 'DRAFT' | 'APPROVED_FOR_PMC' | 'LOCKED'; // Version status
  poDate: Date;
  expectedDeliveryDate?: Date;
  totalAmount: number;
  notes?: string;
  originalPOId?: string; // For version tracking
  createdAt: Date;
  createdBy?: string;
  operations?: POOperation[]; // NHAP_PO sheet data
  materialBaseline?: POMaterialBaseline[]; // NHAP_NGUYEN_VAT_LIEU sheet data
  products?: POProduct[]; // Products in this PO
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

export interface PurchaseOrderList {
  id: string;
  poNumber: string;
  customerName: string;
  processingType: string; // EP_NHUA / PHUN_IN / LAP_RAP
  version: string; // V0, V1, V2
  status: string; // DRAFT / APPROVED_FOR_PMC / LOCKED
  poDate: Date;
  totalAmount: number;
  operationCount: number; // Number of PO Operations
  createdAt: Date;
}

// Import PO from Excel - ONLY way to create PO
export interface ImportPORequest {
  file: File;
  poNumber: string;
  customerId: string;
  processingType: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP';
  poDate: Date;
  expectedDeliveryDate?: Date;
  notes?: string;
}

// Import response with validation errors
export interface ImportPOResponse {
  success: boolean;
  purchaseOrderId?: string;
  version?: string; // Always V0 for new import
  status?: string; // Always DRAFT for new import
  errors?: ImportError[];
}

export interface ImportError {
  row: number;
  field: string;
  errorMessage: string;
  severity: 'error' | 'warning';
}

// Clone PO to create new version
export interface ClonePOVersionRequest {
  originalPOId: string;
  notes?: string;
}

// Approve PO version for PMC
export interface ApprovePORequest {
  purchaseOrderId: string;
}


// PO Operation - from NHAP_PO sheet
// Represents chargeable operations ONLY (pricing, revenue, settlement)
// Must NOT contain: Tool, Machine, BOM, Production logic
export interface POOperation {
  id: string;
  purchaseOrderId: string;
  partId: string;
  partCode: string;
  partName?: string;
  productId?: string; // Product that contains this part
  productCode?: string;
  productName?: string;
  processingTypeId: string;
  processingTypeName?: string;
  processMethodId?: string;
  processMethodName?: string;
  operationName: string; // Tên công đoạn
  chargeCount: number; // Số lần (加工次数)
  unitPrice: number; // Đơn giá (VND)
  quantity: number; // Số lượng hợp đồng (SL HỢP ĐỒNG)
  totalAmount: number; // Thành tiền
  sprayPosition?: string;
  printContent?: string;
  cycleTime?: number;
  assemblyContent?: string;
  // ÉP NHỰA specific fields
  modelNumber?: string;
  material?: string;
  colorCode?: string;
  color?: string;
  cavityQuantity?: number;
  set?: number;
  netWeight?: number;
  totalWeight?: number;
  machineType?: string;
  requiredMaterial?: number;
  requiredColor?: number;
  notes?: string;
  completionDate?: Date; // Ngày hoàn thành
  sequenceOrder: number;
}

// PO Material Baseline - from NHAP_NGUYEN_VAT_LIEU sheet
// Represents customer-committed materials for availability check ONLY
// Must NOT affect pricing or settlement
export interface POMaterialBaseline {
  id: string;
  purchaseOrderId: string;
  materialCode: string; // From Excel
  materialName?: string;
  quantity: number; // From Excel - committed quantity from customer
  uom?: string; // Unit of measure
  notes?: string;
  rowNumber: number; // For error tracking
}

export interface AvailabilityCheckRequest {
  purchaseOrderId?: string;
  plannedQuantity?: number;
  // For component-based check (not PO-based)
  partId?: string;
  processingTypeId?: string;
  quantity?: number;
}

export interface AvailabilityCheckResult {
  overallStatus: 'PASS' | 'FAIL' | 'WARNING';
  purchaseOrderId?: string;
  plannedQuantity?: number;
  // For component-based check
  partId?: string;
  processingTypeId?: string;
  quantity?: number;
  checkDate: Date;
  partResults: PartAvailabilityResult[];
  totalRequired?: number;
  totalAvailable?: number;
  materialResults?: MaterialAvailabilityResult[];
}

export interface MaterialAvailabilityResult {
  materialCode: string;
  materialName: string;
  requiredQty: number;
  inventoryQty: number;
  poBaselineQty: number;
  availableQty: number;
  shortage: number;
  severity: 'OK' | 'WARNING' | 'CRITICAL';
}

export interface PartAvailabilityResult {
  partId: string;
  partCode: string;
  partName: string;
  processingType: string;
  processingTypeName: string;
  requiredQty: number;
  canProduce: boolean;
  severity: 'OK' | 'WARNING' | 'CRITICAL';
  bomVersion?: string;
  hasActiveBOM: boolean;
}
