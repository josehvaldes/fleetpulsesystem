import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';
const frontendRoot = path.resolve(__dirname, '..');

export default defineConfig({
  testDir: './e2e',
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  reporter: 'html',

  use: {
    /* Base URL to use in actions like `await page.goto('')`. */
    baseURL: 'http://127.0.0.1:5173',

    trace: 'on-first-retry',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: [
    {
      command: 'npm run dev -- --port 5173 --host 127.0.0.1',
      cwd: path.join(frontendRoot, 'fleetpulse.frontend'),
      url: 'http://127.0.0.1:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
    },
    {
      command: 'npm run dev -- --port 5174 --host 127.0.0.1',
      cwd: path.join(frontendRoot, 'fleetpulse.alert-mgmt'),
      url: 'http://127.0.0.1:5174',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
    },
    {
      command: 'npx ng serve --port 5175',
      cwd: path.join(frontendRoot, 'fleetpulse.drivers-mgmt'),
      url: 'http://localhost:5175',
      reuseExistingServer: !process.env.CI,
      timeout: 180000, // Angular's first build is slow
    },
  ]
});
