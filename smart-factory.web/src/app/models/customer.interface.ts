export interface Customer {
  id: string;
  code: string;
  name: string;
  address?: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  paymentTerms?: string;
  notes?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateCustomerRequest {
  code: string;
  name: string;
  address?: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  paymentTerms?: string;
  notes?: string;
}

export interface UpdateCustomerRequest {
  code: string;
  name: string;
  address?: string;
  contactPerson?: string;
  email?: string;
  phone?: string;
  paymentTerms?: string;
  notes?: string;
  isActive: boolean;
}






