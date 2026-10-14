// Staging is deployed under /app/ and has no API of its own (the host can't run a second API there),
// so it uses the production API and the production gallery images
export const environment = {
  production: false,
  baseUrl: '/',
  baseApiUrl: '/api/',
  baseHref: '/app',
};
