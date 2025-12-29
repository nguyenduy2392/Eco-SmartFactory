/**
 * Loại hình gia công - Nhóm gia công lớn (ÉP, SƠN, LẮP RÁP)
 */
export interface ProcessingType {
  id: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: Date;
  processMethods?: ProcessMethod[];
  parts?: PartProcessingType[];
}

/**
 * Phương pháp gia công - Công đoạn trong một loại hình gia công
 * Ví dụ (thuộc SƠN): Phun kẹp, Phun tay biên, In sơn, Kẻ vẽ, Xóc màu
 */
export interface ProcessMethod {
  id: string;
  code: string;
  name: string;
  processingTypeId: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: Date;
  processingType?: ProcessingType;
}

/**
 * Quan hệ nhiều-nhiều giữa Linh kiện (Part) và Loại hình gia công (ProcessingType)
 * Một linh kiện có thể trải qua nhiều loại hình gia công
 * Một loại hình gia công có thể áp dụng cho nhiều linh kiện
 */
export interface PartProcessingType {
  id: string;
  partId: string;
  processingTypeId: string;
  sequenceOrder: number;
  notes?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  createdBy?: string;
  updatedBy?: string;
  part?: {
    id: string;
    code: string;
    name: string;
  };
  processingType?: ProcessingType;
}

/**
 * Request để tạo hoặc cập nhật PartProcessingType
 */
export interface CreatePartProcessingTypeRequest {
  partId: string;
  processingTypeId: string;
  sequenceOrder?: number;
  notes?: string;
}

export interface UpdatePartProcessingTypeRequest {
  sequenceOrder?: number;
  notes?: string;
  isActive?: boolean;
}


