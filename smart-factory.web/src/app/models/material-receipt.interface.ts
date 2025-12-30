/**
 * Phiếu nhập kho nguyên vật liệu - Quản lý các lần nhập kho thực tế từ chủ hàng
 * File Excel NHAP_NGUYEN_VAT_LIEU từ chủ hàng gửi cho Hải Tân sẽ tạo các MaterialReceipt này
 */
export interface MaterialReceipt {
  id: string;
  customerId: string;
  customerCode?: string;
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
  status: 'PENDING' | 'RECEIVED' | 'CANCELLED';
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
}

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

export interface UpdateMaterialReceiptRequest {
  warehouseId?: string;
  quantity?: number;
  unit?: string;
  batchNumber?: string;
  receiptDate?: Date;
  supplierCode?: string;
  purchasePOCode?: string;
  notes?: string;
  status?: 'PENDING' | 'RECEIVED' | 'CANCELLED';
}

