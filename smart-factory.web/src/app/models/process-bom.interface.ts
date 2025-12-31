// Process BOM - defines material consumption per 1 PCS of a part
// BOM belongs to "HOW TO MAKE", not "HOW TO CHARGE"
// BOM is independent from PO and pricing

export interface ProcessBOM {
  id: string;
  partId: string;
  partCode: string;
  partName?: string;
  processingType: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP'; // Must match PO processing type
  version: string; // BOM version (e.g., V1, V2, ...)
  status: 'ACTIVE' | 'INACTIVE'; // Only ONE ACTIVE BOM per (part + processing type)
  effectiveDate: Date;
  notes?: string;
  createdAt: Date;
  createdBy?: string;
  bomDetails?: BOMDetail[]; // Material lines
}

export interface BOMDetail {
  id: string;
  bomId: string;
  materialCode: string;
  materialName?: string;
  qtyPerUnit: number; // Quantity per 1 PCS of part
  scrapRate: number; // >= 0 (e.g., 0.05 = 5%)
  uom: string; // Unit of measure
  processStep?: string; // For traceability (optional)
  notes?: string;
}

export interface ProcessBOMList {
  id: string;
  partCode: string;
  partName?: string;
  processingType: string;
  version: string;
  status: string;
  materialCount: number;
  effectiveDate: Date;
  createdAt: Date;
}

// Create new BOM version
export interface CreateBOMRequest {
  partId: string;
  processingType: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP';
  processingTypeId:string,
  effectiveDate: Date;
  notes?: string;
  details: CreateBOMDetailRequest[];
}

export interface CreateBOMDetailRequest {
  materialCode: string;
  materialName?: string;
  qtyPerUnit: number;
  scrapRate: number;
  uom: string;
  processStep?: string;
  notes?: string;
}

// Get BOM history for a part
export interface GetBOMHistoryRequest {
  partId: string;
  processingType: string;
}






