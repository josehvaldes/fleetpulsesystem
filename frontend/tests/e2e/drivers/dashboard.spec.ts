import { test, expect } from '@playwright/test';

test('drivers mfe dashboard is visible', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Drivers Management Dashboard from MFE')).toBeVisible();
});

