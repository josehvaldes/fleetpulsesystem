import { test, expect } from '@playwright/test';

test('drivers mfe dashboard is visible', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Drivers Management Dashboard from MFE')).toBeVisible();
});

test('drivers mfe dashboard shows correct number of drivers', async ({ page }) => {
  await page.goto('/');
  // 3 items are expected from mock data
  await expect(page.getByText('Showing 3')).toBeVisible();
});