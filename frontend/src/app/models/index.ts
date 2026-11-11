import { Role } from '../constants/roles';

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: Role[];
  isLocked?: boolean;
}

export interface UserRef {
  id: string;
  email: string;
  fullName: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

/** POST /api/v1/auth/login and /auth/refresh */
export interface TokenResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}

/** ASP.NET ProblemDetails / ValidationProblemDetails */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export type TicketStatus = 'New' | 'InProgress' | 'Resolved' | 'Closed';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';
/** Row of the ticket-categories table. */
export interface TicketCategory {
  id: number;
  name: string;
  description?: string | null;
}

export const STATUS_LABELS: Record<TicketStatus, string> = {
  New: 'Nowe',
  InProgress: 'W trakcie',
  Resolved: 'Rozwiązane',
  Closed: 'Zamknięte',
};

export const PRIORITY_LABELS: Record<TicketPriority, string> = {
  Low: 'Niski',
  Medium: 'Średni',
  High: 'Wysoki',
  Critical: 'Krytyczny',
};

export interface TicketListItem {
  id: number;
  number: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  category: TicketCategory;
  author: UserRef;
  assignee: UserRef | null;
  createdAt: string;
  barcode?: string | null;
  asset?: { id: number; name: string; inventoryNumber: string } | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Open tickets of the devices in a category (zeros for regular users). */
export interface ActiveTickets {
  low: number;
  medium: number;
  high: number;
  critical: number;
}

/** GET /api/v1/asset-categories */
export interface AssetCategoryDto {
  id: number;
  name: string;
  description: string | null;
  assetCount: number;
  activeTickets: ActiveTickets;
  /** EAN-13: prefix, zeros, id */
  barcode: string | null;
}

/** Row of GET /api/v1/asset-categories/{id}/tickets (the fields the page uses). */
export type CategoryTicket = Pick<
  TicketListItem,
  'id' | 'number' | 'title' | 'status' | 'priority' | 'createdAt'
>;

/** Row of GET /api/v1/assets/{id}/history */
export interface AssetHistoryEntry {
  id: number;
  field: string;
  oldValue: string | null;
  newValue: string | null;
  changedBy: UserRef;
  changedAt: string;
}

/** Body of POST/PUT /api/v1/asset-categories */
export interface SaveAssetCategory {
  name: string;
  description: string | null;
}

export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected';

export interface Address {
  street: string;
  buildingNumber: string;
  apartmentNumber: string | null;
  postalCode: string;
  city: string;
  country: string;
}

export interface AssetDto {
  id: number;
  name: string;
  category: { id: number; name: string };
  serialNumber: string;
  inventoryNumber: string;
  location: string | null;
  address: Address | null;
  owner: UserRef | null;
  inspectionIntervalMonths: number;
  /** yyyy-MM-dd */
  nextInspectionDate: string | null;
  description: string | null;
  isActive: boolean;
  approvalStatus: ApprovalStatus;
  /** EAN-13: prefix, zeros, id */
  barcode: string | null;
  photoCount: number;
}

/** Result of GET /api/v1/assets/availability */
export interface AssetAvailability {
  serialTaken: boolean;
  inventoryTaken: boolean;
}

export interface AssetPhoto {
  id: number;
  originalName: string;
  contentType: string;
  size: number;
  uploadedAt: string;
}

/** Body of POST /api/v1/assets (a user proposes a device). */
export interface RequestAsset {
  name: string;
  categoryId: number;
  serialNumber: string;
  inventoryNumber: string;
  description: string | null;
  location: string | null;
  address: Address | null;
}

/** Body of POST/PUT /api/v1/admin/assets */
/** Address suggested for a device: the user's own or one of their other devices. */
export interface KnownAddress {
  label: string;
  address: Address;
}

export interface SaveAsset extends RequestAsset {
  ownerId: string | null;
  isActive: boolean;
  inspectionIntervalMonths: number | null;
  nextInspectionDate: string | null;
}

/** Filters of GET /api/v1/admin/assets */
export interface AssetFilters {
  search?: string;
  owner?: string;
  address?: string;
  categoryId?: number | null;
  isActive?: boolean | null;
  status?: ApprovalStatus | null;
}

/** Device with a coming (or overdue) service, from GET /api/v1/dashboard/overview. */
export interface UpcomingService {
  assetId: number;
  name: string;
  inventoryNumber: string;
  barcode: string | null;
  /** yyyy-MM-dd */
  nextInspectionDate: string;
  /** Negative when overdue. */
  daysLeft: number;
  owner: UserRef | null;
}

export interface AdminOverview {
  upcomingServices: UpcomingService[];
  openTickets: TicketListItem[];
}

export interface DashboardStats {
  total: number;
  open: number;
  unassignedOpen: number;
}
