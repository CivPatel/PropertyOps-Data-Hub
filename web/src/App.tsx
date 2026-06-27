import { useCallback, useEffect, useMemo, useState } from "react";
import { propertyOpsApi } from "./api";
import type { CorrectionSuggestion } from "./aiCorrectionTypes";
import type {
  ConstructionRisk,
  MaintenanceAlert,
  PipelineRun,
  PortfolioSummary,
  Property,
  PropertyPerformance,
} from "./types";
import "./index.css";

const currencyFormatter = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  maximumFractionDigits: 0,
});

const numberFormatter = new Intl.NumberFormat("en-US");

function formatCurrency(value: number) {
  return currencyFormatter.format(value ?? 0);
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(new Date(value));
}

function formatDateTime(value: string | null) {
  if (!value) {
    return "—";
  }

  return new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatConfidence(value: number) {
  return `${Math.round((value ?? 0) * 100)}%`;
}

function statusClass(value: string) {
  return value.toLowerCase().replace(/\s+/g, "-");
}

function App() {
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [properties, setProperties] = useState<Property[]>([]);
  const [selectedPropertyId, setSelectedPropertyId] = useState<number | null>(
    null
  );
  const [propertyPerformance, setPropertyPerformance] =
    useState<PropertyPerformance | null>(null);
  const [constructionRisk, setConstructionRisk] = useState<ConstructionRisk[]>(
    []
  );
  const [maintenanceAlerts, setMaintenanceAlerts] = useState<
    MaintenanceAlert[]
  >([]);
  const [pipelineRuns, setPipelineRuns] = useState<PipelineRun[]>([]);
  const [correctionSuggestions, setCorrectionSuggestions] = useState<
    CorrectionSuggestion[]
  >([]);
  const [loading, setLoading] = useState(true);
  const [propertyLoading, setPropertyLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const loadDashboard = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const [
        nextSummary,
        nextProperties,
        nextConstructionRisk,
        nextMaintenanceAlerts,
        nextPipelineRuns,
        nextCorrectionSuggestions,
      ] = await Promise.all([
        propertyOpsApi.getPortfolioSummary(),
        propertyOpsApi.getProperties(),
        propertyOpsApi.getConstructionRisk(),
        propertyOpsApi.getMaintenanceAlerts(),
        propertyOpsApi.getPipelineRuns(),
        propertyOpsApi.getCorrectionSuggestions(),
      ]);

      setSummary(nextSummary);
      setProperties(nextProperties);
      setConstructionRisk(nextConstructionRisk);
      setMaintenanceAlerts(nextMaintenanceAlerts);
      setPipelineRuns(nextPipelineRuns);
      setCorrectionSuggestions(nextCorrectionSuggestions);

      setSelectedPropertyId(
        (currentId) => currentId ?? nextProperties[0]?.id ?? null
      );

      setLastUpdated(new Date());
    } catch (caughtError) {
      const message =
        caughtError instanceof Error
          ? caughtError.message
          : "Unable to load dashboard data.";

      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadDashboard();
  }, [loadDashboard]);

  useEffect(() => {
    if (selectedPropertyId === null) {
      setPropertyPerformance(null);
      return;
    }

    const loadProperty = async () => {
      setPropertyLoading(true);

      try {
        const nextPerformance =
          await propertyOpsApi.getPropertyPerformance(selectedPropertyId);

        setPropertyPerformance(nextPerformance);
      } catch (caughtError) {
        const message =
          caughtError instanceof Error
            ? caughtError.message
            : "Unable to load property performance.";

        setError(message);
      } finally {
        setPropertyLoading(false);
      }
    };

    void loadProperty();
  }, [selectedPropertyId]);

  const atRiskBudget = useMemo(
    () =>
      constructionRisk.reduce(
        (total, item) => total + item.budgetVariance,
        0
      ),
    [constructionRisk]
  );

  const latestPipelineRun = pipelineRuns[0];

  const appliedSuggestions = useMemo(
    () =>
      correctionSuggestions.filter(
        (suggestion) => suggestion.status === "Applied"
      ),
    [correctionSuggestions]
  );

  if (loading) {
    return (
      <main className="loading-screen">
        <div className="loading-card">
          <span className="eyebrow">PropertyOps Data Hub</span>
          <h1>Loading operational data…</h1>
          <p>Connecting to the live Azure API and reporting database.</p>
        </div>
      </main>
    );
  }

  return (
    <main className="app-shell">
      <header className="topbar">
        <div>
          <span className="eyebrow">PropertyOps Data Hub</span>
          <h1>Portfolio Operations Dashboard</h1>
          <p>
            Live leasing, construction, maintenance, pipeline, and data-quality
            insights.
          </p>
        </div>

        <div className="header-actions">
          <div className="updated-at">
            <span>Last refreshed</span>
            <strong>
              {lastUpdated ? lastUpdated.toLocaleTimeString() : "—"}
            </strong>
          </div>

          <button
            className="refresh-button"
            onClick={() => void loadDashboard()}
          >
            Refresh data
          </button>
        </div>
      </header>

      {error && (
        <section className="error-banner">
          <strong>Dashboard request failed.</strong>
          <span>{error}</span>
          <button onClick={() => void loadDashboard()}>Try again</button>
        </section>
      )}

      {summary && (
        <>
          <section
            className="metric-grid"
            aria-label="Portfolio summary metrics"
          >
            <MetricCard
              label="Portfolio occupancy"
              value={`${summary.occupancyRate.toFixed(1)}%`}
              detail={`${numberFormatter.format(
                summary.occupiedUnits
              )} of ${numberFormatter.format(summary.totalUnits)} units occupied`}
              tone="success"
            />

            <MetricCard
              label="Scheduled monthly rent"
              value={formatCurrency(summary.monthlyScheduledRent)}
              detail={`${numberFormatter.format(
                summary.activeLeases
              )} active leases`}
              tone="primary"
            />

            <MetricCard
              label="Open work orders"
              value={numberFormatter.format(summary.openWorkOrders)}
              detail={`Average age: ${summary.averageOpenWorkOrderAgeDays.toFixed(
                1
              )} days`}
              tone="warning"
            />

            <MetricCard
              label="Projects over budget"
              value={numberFormatter.format(summary.projectsOverBudget)}
              detail={`${formatCurrency(atRiskBudget)} total variance`}
              tone="danger"
            />
          </section>

          <section className="content-grid">
            <article className="panel performance-panel">
              <div className="panel-heading">
                <div>
                  <span className="section-label">Property performance</span>
                  <h2>Property-level operating metrics</h2>
                </div>

                <label className="property-picker">
                  <span>Selected property</span>

                  <select
                    value={selectedPropertyId ?? ""}
                    onChange={(event) =>
                      setSelectedPropertyId(Number(event.target.value))
                    }
                  >
                    {properties.map((property) => (
                      <option key={property.id} value={property.id}>
                        {property.name} · {property.city}
                      </option>
                    ))}
                  </select>
                </label>
              </div>

              {propertyLoading ? (
                <p className="empty-state">Loading property performance…</p>
              ) : propertyPerformance ? (
                <div className="property-summary">
                  <div className="property-title">
                    <span className="property-code">
                      {propertyPerformance.propertyCode}
                    </span>

                    <h3>{propertyPerformance.propertyName}</h3>
                  </div>

                  <div className="progress-row">
                    <div>
                      <span>Occupancy</span>
                      <strong>
                        {propertyPerformance.occupancyRate.toFixed(1)}%
                      </strong>
                    </div>

                    <div className="progress-track" aria-label="Occupancy rate">
                      <div
                        className="progress-value"
                        style={{
                          width: `${Math.min(
                            Math.max(propertyPerformance.occupancyRate, 0),
                            100
                          )}%`,
                        }}
                      />
                    </div>
                  </div>

                  <div className="mini-metric-grid">
                    <MiniMetric
                      label="Occupied units"
                      value={`${propertyPerformance.occupiedUnits}/${propertyPerformance.totalUnits}`}
                    />

                    <MiniMetric
                      label="Active leases"
                      value={numberFormatter.format(
                        propertyPerformance.activeLeases
                      )}
                    />

                    <MiniMetric
                      label="Scheduled rent"
                      value={formatCurrency(
                        propertyPerformance.monthlyScheduledRent
                      )}
                    />

                    <MiniMetric
                      label="Open work orders"
                      value={numberFormatter.format(
                        propertyPerformance.openWorkOrders
                      )}
                    />
                  </div>
                </div>
              ) : (
                <p className="empty-state">No property data is available.</p>
              )}
            </article>

            <article className="panel pipeline-panel">
              <div className="panel-heading">
                <div>
                  <span className="section-label">Data operations</span>
                  <h2>Latest pipeline activity</h2>
                </div>

                <span
                  className={`status-pill ${
                    latestPipelineRun
                      ? statusClass(latestPipelineRun.status)
                      : "unknown"
                  }`}
                >
                  {latestPipelineRun?.status ?? "No runs"}
                </span>
              </div>

              {latestPipelineRun ? (
                <>
                  <div className="pipeline-highlight">
                    <span className="pipeline-title">
                      {latestPipelineRun.pipelineName}
                    </span>
                    <span>{formatDate(latestPipelineRun.startedAtUtc)}</span>
                  </div>

                  <div className="mini-metric-grid">
                    <MiniMetric
                      label="Received"
                      value={numberFormatter.format(
                        latestPipelineRun.recordsReceived
                      )}
                    />

                    <MiniMetric
                      label="Loaded"
                      value={numberFormatter.format(
                        latestPipelineRun.recordsLoaded
                      )}
                    />

                    <MiniMetric
                      label="Rejected"
                      value={numberFormatter.format(
                        latestPipelineRun.recordsRejected
                      )}
                    />

                    <MiniMetric
                      label="Run ID"
                      value={`#${latestPipelineRun.id}`}
                    />
                  </div>

                  <div className="pipeline-list">
                    {pipelineRuns.slice(1).map((run) => (
                      <div className="pipeline-list-item" key={run.id}>
                        <div>
                          <strong>{run.pipelineName}</strong>
                          <span>{formatDate(run.startedAtUtc)}</span>
                        </div>

                        <span
                          className={`status-pill ${statusClass(run.status)}`}
                        >
                          {run.status}
                        </span>
                      </div>
                    ))}
                  </div>
                </>
              ) : (
                <p className="empty-state">No pipeline runs are available.</p>
              )}
            </article>
          </section>

          <section className="table-grid">
            <article className="panel table-panel">
              <div className="panel-heading">
                <div>
                  <span className="section-label">Construction oversight</span>
                  <h2>Budget risk register</h2>
                </div>

                <span className="panel-count">
                  {constructionRisk.length} project
                  {constructionRisk.length === 1 ? "" : "s"}
                </span>
              </div>

              {constructionRisk.length > 0 ? (
                <div className="table-scroll">
                  <table>
                    <thead>
                      <tr>
                        <th>Project</th>
                        <th>Property</th>
                        <th>Completion</th>
                        <th>Variance</th>
                      </tr>
                    </thead>

                    <tbody>
                      {constructionRisk.map((project) => (
                        <tr key={project.projectId}>
                          <td>
                            <strong>{project.projectName}</strong>
                            <span>{project.projectCode}</span>
                          </td>

                          <td>{project.propertyCode}</td>

                          <td>
                            <span className="completion-value">
                              {project.percentComplete.toFixed(0)}%
                            </span>
                          </td>

                          <td>
                            <strong className="negative-amount">
                              +{formatCurrency(project.budgetVariance)}
                            </strong>
                            <span>
                              {project.budgetVariancePercent.toFixed(1)}% above
                              budget
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="empty-state">
                  No projects are currently above the risk threshold.
                </p>
              )}
            </article>

            <article className="panel table-panel">
              <div className="panel-heading">
                <div>
                  <span className="section-label">Maintenance operations</span>
                  <h2>Open work-order alerts</h2>
                </div>

                <span className="panel-count">
                  {maintenanceAlerts.length} open
                </span>
              </div>

              {maintenanceAlerts.length > 0 ? (
                <div className="table-scroll">
                  <table>
                    <thead>
                      <tr>
                        <th>Work order</th>
                        <th>Property</th>
                        <th>Priority</th>
                        <th>Age</th>
                      </tr>
                    </thead>

                    <tbody>
                      {maintenanceAlerts.map((workOrder) => (
                        <tr key={workOrder.workOrderId}>
                          <td>
                            <strong>{workOrder.category}</strong>
                            <span>{workOrder.externalWorkOrderId}</span>
                          </td>

                          <td>{workOrder.propertyCode}</td>

                          <td>
                            <span
                              className={`priority ${statusClass(
                                workOrder.priority
                              )}`}
                            >
                              {workOrder.priority}
                            </span>
                          </td>

                          <td>
                            <strong>{workOrder.ageDays} days</strong>
                            <span>
                              {formatCurrency(workOrder.estimatedCost)} est.
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="empty-state">
                  No open maintenance alerts are available.
                </p>
              )}
            </article>

            <article className="panel table-panel">
              <div className="panel-heading">
                <div>
                  <span className="section-label">
                    AI-assisted data quality
                  </span>
                  <h2>Correction audit trail</h2>
                </div>

                <span className="panel-count">
                  {appliedSuggestions.length} applied
                </span>
              </div>

              {correctionSuggestions.length > 0 ? (
                <div className="table-scroll">
                  <table>
                    <thead>
                      <tr>
                        <th>Source record</th>
                        <th>Correction</th>
                        <th>Confidence</th>
                        <th>Review status</th>
                        <th>Applied result</th>
                      </tr>
                    </thead>

                    <tbody>
                      {correctionSuggestions.slice(0, 10).map((suggestion) => (
                        <tr key={suggestion.id}>
                          <td>
                            <strong>{suggestion.sourceRecordId}</strong>
                            <span>
                              Error #{suggestion.dataQualityErrorId} ·{" "}
                              {suggestion.fieldName}
                            </span>
                          </td>

                          <td>
                            <strong>{suggestion.originalValue}</strong>
                            <span>
                              → {suggestion.suggestedValue ?? "No safe suggestion"}
                            </span>
                          </td>

                          <td>
                            <strong>{formatConfidence(suggestion.confidence)}</strong>
                            <span>{suggestion.modelName}</span>
                          </td>

                          <td>
                            <span
                              className={`status-pill ${statusClass(
                                suggestion.status
                              )}`}
                            >
                              {suggestion.status}
                            </span>

                            <span>
                              {suggestion.reviewedBy
                                ? `Reviewed by ${suggestion.reviewedBy}`
                                : "Awaiting review"}
                            </span>
                          </td>

                          <td>
                            {suggestion.appliedLeaseId ? (
                              <>
                                <strong>Lease #{suggestion.appliedLeaseId}</strong>
                                <span>
                                  {suggestion.appliedBy
                                    ? `Applied by ${suggestion.appliedBy}`
                                    : "Applied"}
                                  {" · "}
                                  {formatDateTime(suggestion.appliedAtUtc)}
                                </span>
                              </>
                            ) : (
                              <span>Not applied</span>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="empty-state">
                  No AI correction suggestions are available yet.
                </p>
              )}
            </article>
          </section>
        </>
      )}
    </main>
  );
}

function MetricCard({
  label,
  value,
  detail,
  tone,
}: {
  label: string;
  value: string;
  detail: string;
  tone: "primary" | "success" | "warning" | "danger";
}) {
  return (
    <article className={`metric-card ${tone}`}>
      <span>{label}</span>
      <strong>{value}</strong>
      <small>{detail}</small>
    </article>
  );
}

function MiniMetric({ label, value }: { label: string; value: string }) {
  return (
    <div className="mini-metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

export default App;