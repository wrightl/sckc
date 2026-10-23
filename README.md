# Sheffield City Kayak Club website

Source for [www.sheffieldcitykayakclub.co.uk](https://www.sheffieldcitykayakclub.co.uk): an Angular site and a small
ASP.NET Core API that runs alongside it on the same host. This document is the handover guide: how the project fits
together, how to run and deploy it, and the accounts, DNS and certificate details that live outside the code.

> Items marked **TODO** are facts that have to be filled in by the current maintainer. Never put passwords or API
> keys in this file. They belong in GitHub secrets or a password manager.

## Contents

- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Running locally](#running-locally)
- [Testing](#testing)
- [Deployment](#deployment)
- [Secrets and server-only files](#secrets-and-server-only-files)
- [External services and accounts](#external-services-and-accounts)
- [Domain, DNS and Cloudflare](#domain-dns-and-cloudflare)
- [SSL certificate (Cloudflare Origin CA)](#ssl-certificate-cloudflare-origin-ca)
- [Common maintenance tasks](#common-maintenance-tasks)
- [Gotchas](#gotchas)

## Architecture

```
Browser ──► Cloudflare (proxy, DNS) ──► 123reg Windows/IIS hosting
                                          ├─ Angular app (index.html, JS, CSS)        ← sckc-app
                                          ├─ /api/*  ASP.NET Core API (.NET 10)       ← sckc/sckc.api
                                          ├─ /gallery/<set>/<photo>  photos           ← server only
                                          ├─ /data/*  secrets and trip report files   ← server only, blocked from the web
                                          └─ /app/   staging copy of the Angular app (uses the production API)
```

- **Front end:** Angular 22 with Angular Material, Bootstrap 5 and ng-bootstrap. Most page content (committee
  members, pool and river sessions, membership, documents, home images) is hard-coded in
  [sckc-app/src/app/data/](sckc-app/src/app/data/).
- **API:** ASP.NET Core on .NET 10 ([Program.cs](sckc/sckc.api/Program.cs)). It also serves the Angular files and
  gallery images as static files from its content root, and falls back to `index.html` for Angular routes. A
  middleware blocks `/data`, `/Resources`, `/bin`, `/logs`, binaries and config files from being downloaded.
- **Everything is deployed to one folder on the host over FTP.** There is no database. State is kept in files under
  `data/` on the server.

### API endpoints

| Endpoint | Purpose | Depends on |
| --- | --- | --- |
| `GET api/health` | Returns `"Healthy"`. Used by the deploy health check | – |
| `GET api/GetEvents`, `api/GetEventsOfType`, `api/GetGroupedEvents` | Upcoming events from the club Google Calendar (`sheffieldcitykayakclub@gmail.com`) | `data/sckc.data` (Google service account JSON) |
| `GET api/gallery`, `api/gallery/{id}` | Lists photo sets, which are folders under `gallery/` named `yyyy-MM-dd@Title` | `gallery/` folder on server |
| `POST api/ContactUs` | Contact form email to `contactus@` | Resend |
| `POST api/BookingEnquiry` | Pool/river booking enquiry email to `bookingrequest@`, plus an auto-reply from [Resources/](sckc/sckc.api/Resources/) | Resend |
| `POST api/Book` | Event booking: either emails `bookings@`, or (`payNow`) creates a Stripe Checkout session and returns its URL | Resend, Stripe |
| `GET api/reports` | Published trip reports | `data/tripreports.data` |
| `POST api/reports` | Submits a trip report. It is saved as pending and emailed to `admin@` for review | `data/pendingreports.data`, Resend |
| `GET/POST api/reports/review/{id}` | Accept/reject links from the review email. GET only shows a confirm page, POST makes the change | token in the link |
| `POST api/stripe/webhook` | Called by Stripe when a Checkout payment is received; emails `payment@` with the amount and booking details | `data/stripe_webhook_secret.data`, Resend |

Swagger UI is available at `/swagger` (locally: <http://localhost:32767/swagger>).

Email addresses and the site base URL are set in [Constants.cs](sckc/sckc.api/APIs/Constants.cs).

## Repository layout

```
.github/workflows/   CI/CD (see Deployment)
run.sh               Starts API + Angular dev server (+ optional ngrok tunnel)
sckc-app/            Angular front end
  src/app/data/      Hard-coded site content
  src/environments/  Per-environment API URLs (see below)
  proxy.conf.json    Dev-server proxy for /api and /gallery (used by run.sh)
sckc/                .NET solution (sckc.slnx, global.json pins SDK 10.0.100+)
  sckc.api/          The API
  sckc.api.Tests/    MSTest unit and integration tests
  sckc.core/         Shared DTOs
```

## Running locally

### Prerequisites

- .NET SDK 10 (see [sckc/global.json](sckc/global.json))
- Node.js 24 (see [sckc-app/.nvmrc](sckc-app/.nvmrc)). Run `nvm use` inside `sckc-app`.
- Optional: [ngrok](https://ngrok.com/) to share your local copy with other people for testing

### Server-only files you need locally

The API reads these from `sckc/sckc.api/` at runtime. They are gitignored, so get copies from the current maintainer
or download them from the server over FTP:

- `data/sckc.data`: Google service account credentials (calendar). Without it the events endpoints fail.
- `data/resend_apikey.data`: Resend API key. Without it any form that sends email fails.
- `data/stripe_apikey.data`: Stripe secret key. **Use a test-mode key locally.** The bookings controller needs this
  file even for bookings that only send an email.
- `data/stripe_webhook_secret.data`: signing secret of the Stripe webhook (`whsec_…`). Only needed to try the
  payment webhook locally: run `stripe listen --forward-to localhost:32767/api/stripe/webhook` and save the secret it
  prints here.
- `data/tripreports.data`, `data/pendingreports.data`: trip report store (JSON). Missing files are treated as empty.
- `gallery/`: photo folders (optional, only needed for the gallery pages).

### Start everything

```bash
./run.sh              # API on :32767, Angular on :4200, plus an ngrok link to share
./run.sh --no-ngrok   # local only
```

Or run each part on its own:

```bash
cd sckc/sckc.api && dotnet run          # http://localhost:32767/swagger
cd sckc-app && npm install && npm start # http://localhost:4200
```

### Angular environments

| Config | File | API used |
| --- | --- | --- |
| default (`npm start`) | `environment.ts` | **Production API** (`baseApiUrl` points at the live site) |
| `local` (`npm run start:local`) | `environment.local.ts` | Gitignored, so create it yourself (copy `environment.ts` and point `baseApiUrl` at `http://localhost:32767/api/`) |
| `ngrok` (used by `run.sh`) | `environment.ngrok.ts` | Local API through the dev-server proxy |
| `staged` | `environment.staged.ts` | Production API, app served under `/app/` |
| `production` | `environment.prod.ts` | Same-origin `/api/` |

## Testing

```bash
cd sckc && dotnet test sckc.slnx      # API tests (run in CI on every deploy)
cd sckc-app && npm run lint           # run in CI
cd sckc-app && npm test               # Karma unit tests. Currently disabled in CI
```

The API tests replace `Helper.MailSender` and `Helper.ResendClient`, so they never send real email.

## Deployment

Deployment is fully automated through GitHub Actions in [.github/workflows/](.github/workflows/). The repo is
<https://github.com/wrightl/sckc>.

| Trigger | Workflow | What happens |
| --- | --- | --- |
| Push to `master` (or manual run) | [production.yml](.github/workflows/production.yml) | Runs API tests, builds the API (Windows runner) and the Angular production build, then uploads both over FTP to the site root |
| Pull request to `master` (or manual run) | [staged.yml](.github/workflows/staged.yml) | Runs API tests, builds the Angular staged build, then uploads it to `/app/` only. The API is **not** deployed, so staging uses the production API |

Both call [deployment.yml](.github/workflows/deployment.yml), which:

1. Runs `dotnet test`.
2. Publishes the API and writes `data/sckc.data`, `data/stripe_apikey.data`, `data/resend_apikey.data` and
   `data/stripe_webhook_secret.data` from GitHub secrets.
3. Builds and lints the Angular app.
4. Uploads over FTP with `SamKirkland/FTP-Deploy-Action`. This only uploads changed files and tracks state in
   `.ftp-deploy-app-sync-state.json` and `.ftp-deploy-api-sync-state.json` on the server. If you delete those files,
   the next deploy uploads everything again.
5. Checks that `api/health` returns `"Healthy"` and that `index.html` has the expected `<base href>`.

To deploy, merge to `master`. To roll back, revert the commit and push.

### GitHub secrets (Settings → Secrets and variables → Actions)

| Secret | Used for |
| --- | --- |
| `ftphost`, `ftpuser`, `ftppassword` | FTP access to the 123reg hosting |
| `google_calendar_creds` | Google service account JSON, written to `data/sckc.data` |
| `resend_apikey` | Resend API key |
| `stripe_apikey_live` | Stripe live secret key (production) |
| `stripe_webhook_secret_live` | Signing secret of the live Stripe webhook endpoint (production) |
| `stripe_apikey_test` | Stripe test secret key (passed to staging, but unused while staging doesn't deploy the API) |

## Secrets and server-only files

These files exist only on the server (and on developer machines). They are not in git:

- `data/*.data`: secrets (written by every API deploy) and the trip report store (**not** written by deploys, so
  this is live data and must be kept). Download `tripreports.data` occasionally as a backup.
- `gallery/`: all gallery photos. They are uploaded by hand over FTP and are not in git. **Back these up.**
- `Documents/` and other static files linked from the site, such as the code of conduct PDFs.
- `logs/errors.log`: unhandled API exceptions, written by the error handler in `Program.cs`. Download it over FTP
  when something goes wrong.
- `Web.config`: generated by `dotnet publish` for IIS.

`data/sendgrid_apikey.data` and `data/stripe.data` may still be on the server. They are left over from older versions
(email used to go through SendGrid) and the current code doesn't read them.

## External services and accounts

| Service | Used for | Account owner / login |
| --- | --- | --- |
| GitHub (`wrightl/sckc`) | Source and CI/CD | wrightl. **TODO:** transfer the repo or add the new maintainer as a collaborator |
| 123reg | Domain registrar **and** Windows/IIS web hosting (FTP) | **TODO** |
| Cloudflare | DNS and proxy in front of the site | **TODO** |
| Resend | Sending all site email. The `sheffieldcitykayakclub.co.uk` domain is verified in Resend, which needs its DNS records | **TODO** |
| Stripe | Card payments for event bookings (Checkout), and a webhook to `https://www.sheffieldcitykayakclub.co.uk/api/stripe/webhook` sending `checkout.session.completed` and `checkout.session.async_payment_succeeded` (Snapshot payload, API version `2026-09-30.endive`, matching Stripe.net 53.0.0; upgrading Stripe.net means creating a new endpoint at its version) | **TODO** |
| Google Cloud | Service account with read-only access to the club calendar `sheffieldcitykayakclub@gmail.com` | **TODO** |
| Club mailboxes | `admin@`, `contactus@`, `bookings@`, `bookingrequest@`, `payment@` | **TODO**: where the mailboxes are hosted |

When handing over, also hand over (or rotate) the API keys and FTP password, then update the GitHub secrets.

## Domain, DNS and Cloudflare

- **Domain:** `sheffieldcitykayakclub.co.uk`, registered with **123reg**.
- **DNS:** managed by **Cloudflare**, which also proxies web traffic to the 123reg hosting. To do this, the
  nameservers at 123reg were changed to the ones Cloudflare assigned (shown in the Cloudflare dashboard under the
  domain's DNS → Records → Cloudflare nameservers).

### Original nameservers (before Cloudflare)

These are the nameservers the domain used before the move to Cloudflare. Keep them in case Cloudflare is ever removed:

```
ns27.domaincontrol.com
ns28.domaincontrol.com
```

### Removing Cloudflare (reverting to 123reg DNS)

1. In Cloudflare, export the DNS zone (DNS → Records → Import and Export → Export) so you have a copy of every record.
2. Make sure the 123reg DNS zone has every record you still need, especially web (`@`, `www`), mail (MX), and the
   Resend records (SPF/DKIM/DMARC TXT records). Otherwise email stops working.
3. **Replace the SSL certificate first** (see below). The Cloudflare Origin certificate is only trusted by Cloudflare.
   Without the proxy, browsers will show certificate errors.
4. In 123reg, set the nameservers back to `ns27.domaincontrol.com` and `ns28.domaincontrol.com`.
5. Wait for the change to spread (it can take up to 48 hours), then remove the site from Cloudflare.

### Cloudflare notes

- Mail-related records (MX and the mail server's A record, if any) must be **DNS only** (grey cloud), never proxied.
- With the Origin certificate installed on the host, set Cloudflare SSL/TLS mode to **Full (strict)**, and turn on
  **Always Use HTTPS** (SSL/TLS → Edge Certificates) so `http://` links redirect.
- Cloudflare's bot protection can block Stripe's webhook calls. If Stripe's dashboard shows failed deliveries with a
  403 or a challenge page, add a WAF skip rule for the path `/api/stripe/webhook`.
- After a deploy, if users still see old files, purge the Cloudflare cache (Caching → Configuration → Purge
  Everything). Angular output files have hashed names, but `index.html` can be cached.

## SSL certificate (Cloudflare Origin CA)

The HTTPS certificate installed on the 123reg hosting is a **Cloudflare Origin CA certificate**. It encrypts traffic
between Cloudflare and the host.

| | |
| --- | --- |
| Type | Cloudflare Origin CA (created in Cloudflare → SSL/TLS → Origin Server) |
| Covers | **TODO**, e.g. `sheffieldcitykayakclub.co.uk`, `*.sheffieldcitykayakclub.co.uk` |
| Validity | 15 years |
| Issued | **TODO**: date issued |
| **Expires** | **TODO**: expiry date (15 years after issue, around 2041) |
| Installed at | 123reg hosting control panel → SSL |

Important:

- **Only Cloudflare trusts this certificate.** Browsers do not. It works only while the site is proxied through
  Cloudflare (orange cloud). If Cloudflare is removed, install a publicly trusted certificate on 123reg first.
- **The private key is shown only once**, when the certificate is created. Store it in the club's password manager.
  If it is lost, create a new certificate.

### Renewing / replacing the certificate

1. In Cloudflare: SSL/TLS → Origin Server → **Create Certificate**. Let Cloudflare generate the private key, add the
   hostnames (root and `*.` wildcard), and choose a validity of 15 years.
2. Copy the **Origin Certificate** and the **Private Key** (PEM format). Save the key now, because it can't be
   viewed again.
3. In the 123reg hosting control panel, open the SSL section for the domain, then remove the old certificate and
   upload or paste the new certificate and private key.
4. Check that <https://www.sheffieldcitykayakclub.co.uk> loads without errors with SSL/TLS mode set to Full (strict).
5. Revoke the old certificate in Cloudflare and update the dates in the table above.

## Common maintenance tasks

- **Change page content** (committee, sessions, prices, documents): edit the files in
  [sckc-app/src/app/data/](sckc-app/src/app/data/) or the component HTML, open a PR (this deploys staging to `/app/`),
  check it, then merge.
- **Add a gallery set:** upload a folder named `yyyy-MM-dd@Title_With_Underscores` containing the photos to
  `gallery/` on the server over FTP. No deploy is needed.
- **Events:** add them to the club Google Calendar. The site reads them live.
- **Review trip reports:** submissions are emailed to `admin@` with accept/reject links. Published reports are kept
  in `data/tripreports.data`.
- **Change notification email addresses:** edit [Constants.cs](sckc/sckc.api/APIs/Constants.cs). Senders must be on
  the domain verified in Resend.
- **Update dependencies:** the Angular and .NET versions were upgraded in October 2026. Upgrade Angular one major
  version at a time with `ng update`.

## Gotchas

- **The default `npm start` talks to the production API.** Submitting forms there sends real emails. Use `./run.sh`
  or `start:local` for local work.
- **Staging uses the production API and the live Stripe key.** A paid booking made on `/app/` is a real charge. The
  `isLive: false` flag only adds "TEST" to email subjects.
- The production workflow deploys to the site root (`deployment_path: ''`). The comment there about moving to a
  staged path is old.
- The site is served over HTTPS. `Constants.SiteBaseUrl` (used for the trip report review links) and the CI health
  checks use `https://`. Stripe only delivers live webhooks to HTTPS addresses, so keep it that way.
- The API returns PascalCase JSON (Newtonsoft `DefaultContractResolver`), which the Angular models expect. Don't
  switch to System.Text.Json defaults without updating the front end.
- Angular unit tests are disabled in CI (`if: ${{ false }}` in deployment.yml).
