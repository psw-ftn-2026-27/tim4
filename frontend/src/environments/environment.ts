// Configuration of the production build. `ng serve` and the tests use
// environment.development.ts instead (fileReplacements in angular.json).
export const environment = {
  // Google Analytics 4 measurement ID (G-XXXXXXXXXX). It is not a secret: it is visible
  // in every page that loads the script. null means nothing is sent to Google Analytics.
  gaMeasurementId: 'G-Y01XEGKXP9' as string | null,
};
