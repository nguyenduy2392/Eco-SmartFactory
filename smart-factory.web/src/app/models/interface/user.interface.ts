export interface User {
  id: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  password: string;
  phoneNumber?: string;
}

export interface UpdateUserRequest {
  fullName: string;
  phoneNumber?: string;
  password?: string;
  isActive: boolean;
}
