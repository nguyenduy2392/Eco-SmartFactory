export interface Product {
  id: string;
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
}

export interface UpdateProductRequest {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
}
