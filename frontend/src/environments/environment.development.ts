// Configuration of local development and tests: no measurement ID, so nothing reaches
// the real Google Analytics.
export const environment = {
  gaMeasurementId: null as string | null,
};
