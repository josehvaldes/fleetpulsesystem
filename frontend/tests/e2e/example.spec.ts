import { test, expect } from '@playwright/test';

test('has title', async ({ page }) => {
  //
  // or use await page.goto('http://127.0.0.1:5173');
  //
  await page.goto('/');  

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle("Fleetpulse SPA");
});

