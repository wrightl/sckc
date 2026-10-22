// Used by ./run.sh when sharing the dev server through ngrok. The API and gallery images are reached through the
// dev server's proxy (proxy.conf.json), so remote testers never need to reach localhost:32767 themselves.
export const environment = {
  production: false,
  baseUrl: '/',
  baseApiUrl: '/api/',
  baseHref: '',
};
