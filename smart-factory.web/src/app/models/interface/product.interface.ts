export interface Product {
  id: string;
  code: string;
  name: string;
  description?: string;
  imageUrl?: string;
  category?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface ProductWithPrice extends Product {
  latestUnitPrice?: number;
  averageUnitPrice?: number;
  totalPOs: number;
  totalQuantity: number;
}

export interface CreateProductRequest {
  code: string;
  name: string;
  description?: string;
  imageUrl?: string;
  category?: string;
}

export interface UpdateProductRequest {
  code: string;
  name: string;
  description?: string;
  imageUrl?: string;
  category?: string;
  isActive: boolean;
}
