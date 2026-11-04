import { defineConfig, devices } from '@playwright/test';
import path from 'node:path';
const frontendRoot = path.resolve(__dirname, '..');
const authFile = path.join(__dirname, '.auth', 'user.json');

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

  // Vite/Angular dev servers compile on first request, which is slow when all projects run together
  expect: { timeout: 30000 },

  use: {
    /* Base URL to use in actions like `await page.goto('')`. */
    baseURL: 'http://127.0.0.1:5173',

    trace: 'on-first-retry',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'setup',
      testMatch: 'setup/auth.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:5173',
      },
    },
    {
      name: 'authentication',
      testMatch: 'local-authentication/**/*.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:5173',
      }
    },
    {
      name: 'spa',
      testMatch: 'spa/**/*.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:5173',
        storageState: authFile,
      },
      dependencies: ['setup']
    },
    {
      name: 'alerts',
      testMatch: 'alerts/**/*.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:5176', // use the standalone server for e2e testing
      },
    },
    {
      name: 'drivers',
      testMatch: 'drivers/**/*.spec.ts',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:5175',
      },
    },
  ],

  webServer: [
    {
      command: 'npm run dev -- --port 5173 --host 127.0.0.1',
      cwd: path.join(frontendRoot, 'fleetpulse.frontend'),
      url: 'http://127.0.0.1:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
      env: {
        VITE_AUTH_MODE: 'local',
        VITE_ENABLE_MOCKS: 'true',
      }
    },
    {
      command: 'npm run dev -- --port 5174 --host 127.0.0.1',
      cwd: path.join(frontendRoot, 'fleetpulse.alert-mgmt'),
      url: 'http://127.0.0.1:5174',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
      env: {
        VITE_AUTH_MODE: 'local',
        VITE_ENABLE_MOCKS: 'true'
      }
    },
    {
      command: 'npm run dev:standalone -- --port 5176 --host 127.0.0.1',
      cwd: path.join(frontendRoot, 'fleetpulse.alert-mgmt'),
      url: 'http://127.0.0.1:5176',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
      env: {
        VITE_AUTH_MODE: 'local',
        VITE_ENABLE_MOCKS: 'true'
      }
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
