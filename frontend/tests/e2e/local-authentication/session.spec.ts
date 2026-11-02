import { test, expect } from '@playwright/test';

test('displays title', async ({ page }) => {
  await page.goto('/');  
  await expect(page).toHaveTitle("Fleetpulse SPA");
});

test('displays username, password, signin fields', async ({ page }) => {
    await page.goto('/');  


    await expect(page.getByPlaceholder('Username')).toBeVisible();
    await expect(page.getByPlaceholder('Password')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Sign In' })).toBeVisible();
});

test('displays error on invalid login', async ({ page }) => {
    await page.goto('/');

    await page.getByPlaceholder('Username').fill('invaliduser');
    await page.getByPlaceholder('Password').fill('invalidpassword');
    await page.getByRole('button', { name: 'Sign In' }).click();

    await expect(page.locator('.error-message')).toBeVisible();
});

test('successful login', async ({ page }) => {
    await page.goto('/');

    //get the user credentials from the mock file: /src/mocks/data.ts
    await page.getByPlaceholder('Username').fill('admin');
    await page.getByPlaceholder('Password').fill('adminfleet');
    await page.getByRole('button', { name: 'Sign In' }).click();

    // Expect some element that is only visible after successful login
    await expect(page.getByText('Fleet Pulse Monitor')).toBeVisible();
    await expect(page.getByText('Logged in as admin')).toBeVisible();
});

