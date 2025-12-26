export interface Tool {
  id: string;
  code: string;
  name: string;
  type: string;
  ownerId?: string;
  ownerName?: string;
  status: 'Available' | 'InUse' | 'Maintenance' | 'Returned';
  receivedDate?: Date;
  returnedDate?: Date;
  usageCount: number;
  estimatedLifespan?: number;
  location?: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateToolRequest {
  code: string;
  name: string;
  type: string;
  ownerId?: string;
  receivedDate?: Date;
  estimatedLifespan?: number;
  location?: string;
  description?: string;
}

export interface UpdateToolRequest {
  code: string;
  name: string;
  type: string;
  ownerId?: string;
  status: string;
  receivedDate?: Date;
  returnedDate?: Date;
  estimatedLifespan?: number;
  location?: string;
  description?: string;
  isActive: boolean;
}



