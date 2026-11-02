import { test, expect } from '@playwright/test';

test('dashboard is visible', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Fleet Pulse Monitor')).toBeVisible();
});