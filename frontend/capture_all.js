const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

const outDir = path.join(__dirname, 'screenshots');
if (!fs.existsSync(outDir)) {
  fs.mkdirSync(outDir, { recursive: true });
}

const routes = [
  '/',
  '/auth/login',
  '/auth/register',
  '/auth/two-factor',
  '/auth/forgot-password',
  '/auth/reset-password',
  '/home',
  '/profile',
  '/research/submit',
  '/research/timeline',
  '/research/timeline/123',
  '/research/corrections/123',
  '/research/history',
  '/committee/workspace',
  '/meetings/scheduler',
  '/meetings/studio/123',
  '/meetings/rsvp',
  '/evaluator/portfolio',
  '/notifications',
  '/chat',
  '/admin/analytics',
  '/admin/roster',
  '/admin/ministry',
  '/admin/sla-violations',
  '/admin/plagiarism-override',
  '/admin/global-search',
  '/admin/export',
  '/admin/config',
  '/admin/audit-logs'
];

(async () => {
  const browser = await puppeteer.launch({ 
    headless: "new",
    defaultViewport: { width: 1440, height: 900 }
  });
  
  const page = await browser.newPage();
  console.log("Started capturing screenshots...");

  for (let i = 0; i < routes.length; i++) {
    const route = routes[i];
    const url = `http://localhost:4200${route}`;
    try {
      console.log(`Navigating to ${url}...`);
      await page.goto(url, { waitUntil: 'networkidle2', timeout: 15000 });
      
      // small delay for animations
      await new Promise(r => setTimeout(r, 2000));
      
      let safeName = route === '/' ? 'landing' : route.replace(/\//g, '_').replace(/^_/, '');
      const filepath = path.join(outDir, `${i.toString().padStart(2, '0')}_${safeName}.png`);
      
      await page.screenshot({ path: filepath, fullPage: true });
      console.log(`Captured: ${filepath}`);
    } catch (e) {
      console.error(`Failed to capture ${route}:`, e.message);
    }
  }

  await browser.close();
  console.log("Done!");
})();
