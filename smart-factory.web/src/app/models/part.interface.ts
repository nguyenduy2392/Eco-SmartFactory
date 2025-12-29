import { PartProcessingType } from './processing-type.interface';

export interface Part {
  id: string;
  code: string;
  name: string;
  productId: string;
  productName?: string;
  position?: string;
  material?: string;
  color?: string;
  weight?: number;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  processingTypes?: PartProcessingType[]; // Các loại hình gia công mà linh kiện này có thể trải qua
}

export interface CreatePartRequest {
  code: string;
  name: string;
  productId: string;
  position?: string;
  material?: string;
  color?: string;
  weight?: number;
  description?: string;
}




