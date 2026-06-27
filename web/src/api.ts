import type {
  ConstructionRisk,
  MaintenanceAlert,
  PipelineRun,
  PortfolioSummary,
  Property,
  PropertyPerformance,
} from "./types";
import type { CorrectionSuggestion } from "./aiCorrectionTypes";

const apiBaseUrl = (
  import.meta.env.VITE_API_BASE_URL ??
  "http://localhost:5191"
).replace(/\/$/, "");

async function get<T>(path: string): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`);

  if (!response.ok) {
    const detail = await response.text();

    throw new Error(
      `Request failed (${response.status}). ${detail || "Please try again."}`
    );
  }

  return response.json() as Promise<T>;
}

export const propertyOpsApi = {
  getPortfolioSummary: () =>
    get<PortfolioSummary>("/api/dashboard/portfolio-summary"),

  getProperties: () => get<Property[]>("/api/properties"),

  getPropertyPerformance: (propertyId: number) =>
    get<PropertyPerformance>(
      `/api/dashboard/properties/${propertyId}/performance`
    ),

  getConstructionRisk: () =>
    get<ConstructionRisk[]>("/api/dashboard/construction-risk"),

  getMaintenanceAlerts: () =>
    get<MaintenanceAlert[]>("/api/dashboard/maintenance-alerts"),

  getPipelineRuns: () => get<PipelineRun[]>("/api/pipeline-runs?limit=5"),

  getCorrectionSuggestions: () =>
  get<CorrectionSuggestion[]>("/api/correction-suggestions"),
  
};