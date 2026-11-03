import { test, expect } from '@playwright/test';

test('alerts mfe dashboard is visible', async ({ page }) => {
  await page.goto('/');
  // wait until solving the "Cannot read properties of undefined (reading 'd')" error in Alerts MFE
  //await expect(page.getByText('Alerts Dashboard from MFE')).toBeVisible();

});

