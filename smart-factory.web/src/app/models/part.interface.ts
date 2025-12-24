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

