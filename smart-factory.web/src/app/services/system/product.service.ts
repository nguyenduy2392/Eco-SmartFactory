import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  public apiURL = this.env.baseApiUrl;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  GetAllProducts() {
    return this.master.get(`${this.apiURL}api/products`);
  }

  GetProductById(id: string) {
    return this.master.get(`${this.apiURL}api/products/${id}`);
  }

  CreateProduct(model: any) {
    return this.master.post(`${this.apiURL}api/products`, model);
  }

  UpdateProduct(id: string, model: any) {
    return this.master.put(`${this.apiURL}api/products/${id}`, model);
  }

  DeleteProduct(id: string) {
    return this.master.delete(`${this.apiURL}api/products/${id}`);
  }
}
