export interface PortfolioSummary {
  totalProperties: number;
  totalUnits: number;
  activeLeases: number;
  occupiedUnits: number;
  occupancyRate: number;
  monthlyScheduledRent: number;
  openWorkOrders: number;
  averageOpenWorkOrderAgeDays: number;
  projectsOverBudget: number;
  totalConstructionBudget: number;
  totalConstructionActualCost: number;
}

export interface Property {
  id: number;
  propertyCode: string;
  name: string;
  city: string;
  state: string;
  totalUnits: number;
}

export interface PropertyPerformance {
  propertyId: number;
  propertyCode: string;
  propertyName: string;
  totalUnits: number;
  activeLeases: number;
  occupiedUnits: number;
  occupancyRate: number;
  monthlyScheduledRent: number;
  openWorkOrders: number;
  averageOpenWorkOrderAgeDays: number;
}

export interface ConstructionRisk {
  projectId: number;
  propertyCode: string;
  projectCode: string;
  projectName: string;
  approvedBudget: number;
  actualCost: number;
  budgetVariance: number;
  budgetVariancePercent: number;
  percentComplete: number;
  status: string;
  targetCompletionDate: string;
}

export interface MaintenanceAlert {
  workOrderId: number;
  externalWorkOrderId: string;
  propertyCode: string;
  propertyName: string;
  category: string;
  priority: string;
  status: string;
  openedAt: string;
  ageDays: number;
  estimatedCost: number;
}

export interface PipelineRun {
  id: number;
  pipelineName: string;
  status: string;
  startedAtUtc: string;
  finishedAtUtc: string | null;
  recordsReceived: number;
  recordsLoaded: number;
  recordsRejected: number;
  errorMessage: string | null;
}
