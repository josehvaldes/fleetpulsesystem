import { test, expect } from '@playwright/test';

test('alerts mfe dashboard is visible', async ({ page }) => {
  await page.goto('/');
  // wait until solving the "Cannot read properties of undefined (reading 'd')" error in Alerts MFE
  await expect(page.getByText('Alerts Dashboard from MFE')).toBeVisible();

});

test('alerts mfe dashboard shows correct number of alerts', async ({ page }) => {
  await page.goto('/');
  // 5 items are expected from mock data
  await expect(page.getByText('Showing 5 of 5')).toBeVisible();
});