import { test, expect } from '@playwright/test';

test('alerts mfe dashboard is visible', async ({ page }) => {
  await page.goto('/alerts');
  await expect(page.getByText('Alerts Dashboard from MFE')).toBeVisible();
});

