import { test, expect } from '@playwright/test';
import { ping, alert } from './mock-data';

test('dashboard is visible', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Fleet Pulse Monitor')).toBeVisible();
});

test('dashboard has map', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Fleet Map')).toBeVisible();
});

test('dashboard has alerts section', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Live Alerts')).toBeVisible();
});

test('dashboard has drivers section', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Live Drivers')).toBeVisible();
});


test('Receive live gps pings', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Fleet Map')).toBeVisible();
  await page.evaluate((payload) => {
    window.__mockHub.emit('ReceiveGpsPing', payload)
  }, ping);
  
  // Verify that the GPS ping was received and processed by the map component
  await expect(page.locator('.leaflet-marker-icon')).toBeVisible(); // Assuming the map marker has a class 'leaflet-marker-icon'
  await page.waitForTimeout(1000); // Wait for a short period to allow the map to update with the new marker
  await expect(page.getByTestId('live-drivers-list').getByText('driver_123')).toBeVisible();
  await expect(page.getByTestId('gps-pings-log')).toHaveValue(/driver_123/);

});

test('Receive live alert', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Live Alerts')).toBeVisible();
  await page.evaluate((payload) => {
    window.__mockHub.emit('ReceiveAlert', payload)
  }, alert);

  // Verify that the alert was received and displayed in the alerts section
  await expect(page.getByTestId('live-alerts-list').getByText('driver_2')).toBeVisible();
});