export interface UnitOfMeasure {
  id: string;
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateUnitOfMeasureRequest {
  code: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
}
