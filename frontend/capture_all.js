const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

const outDir = path.join(__dirname, 'screenshots');
if (!fs.existsSync(outDir)) {
  fs.mkdirSync(outDir, { recursive: true });
}

const routes = [
  '/landing',
  '/auth/login',
  '/auth/register',
  '/auth/2fa',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/home',
  '/profile',
  '/research/submit',
  '/research/history',
  '/research/timeline/123',
  '/research/corrections/123',
  '/triage',
  '/committee/workspace',
  '/committee/scheduler',
  '/committee/studio/123',
  '/committee/rsvp',
  '/evaluator/portfolio',
  '/admin/notifications',
  '/admin/chat',
  '/admin/analytics',
  '/admin/roster',
  '/admin/ministry',
  '/admin/audit',
  '/admin/violations',
  '/admin/plagiarism',
  '/admin/search',
  '/admin/export',
  '/admin/config',
  '/errors/not-found',
  '/errors/access-denied'
];

(async () => {
  const browser = await puppeteer.launch({
    headless: 'new',
    defaultViewport: { width: 1440, height: 900 }
  });

  const page = await browser.newPage();
  console.log('Started capturing screenshots...');

  for (let i = 0; i < routes.length; i++) {
    const route = routes[i];
    const url = `http://localhost:4200${route}`;
    try {
      console.log(`Navigating to ${url}...`);
      await page.goto(url, { waitUntil: 'networkidle2', timeout: 15000 });
      await new Promise(r => setTimeout(r, 2000));

      const filename = route.replace(/\//g, '_').replace(/^_/, '') || 'landing';
      const filepath = path.join(outDir, `${filename}.png`);
      await page.screenshot({ path: filepath, fullPage: true });
      console.log(`Saved: ${filepath}`);
    } catch (err) {
      console.error(`Failed to capture ${route}:`, err.message);
    }
  }

  await browser.close();
  console.log('Screenshot capture complete.');
})();
