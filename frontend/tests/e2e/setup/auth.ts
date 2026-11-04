import { test as setup, expect } from '@playwright/test';
import path from 'node:path';


const authFile = path.join(__dirname, '..', '..', '.auth', 'user.json');

setup('authenticate', async ({ page }) => {
  await page.goto('/');

  await page.getByPlaceholder('Username').fill('admin');
  await page.getByPlaceholder('Password').fill('adminfleet');
  await page.getByRole('button', { name: 'Sign In' }).click();

  await expect(page.getByText('Logged in as admin')).toBeVisible();
  await page.context().storageState({
    path: authFile,
  });
});