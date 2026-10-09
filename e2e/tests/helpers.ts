import { APIRequestContext, Locator, Page, expect, request } from '@playwright/test';

export const API = process.env.API_URL ?? 'https://localhost:7269';
export const ADMIN = { email: 'admin@projectone.local', password: 'Admin123!' };

export function uniq(prefix: string): string {
  return `${prefix}-${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`;
}

/** Дата 'YYYY-MM-DD' в UTC со сдвигом от сегодняшней (так же считает фронт). */
export function utcDate(offsetDays: number): string {
  const d = new Date();
  d.setUTCDate(d.getUTCDate() + offsetDays);
  return d.toISOString().slice(0, 10);
}

// ---------------------------------------------------------------- API (подготовка данных)

export async function adminApi(): Promise<APIRequestContext> {
  const ctx = await request.newContext({ baseURL: API, ignoreHTTPSErrors: true });
  const res = await ctx.post('/Auth/login', { data: ADMIN });
  expect(res.ok(), 'вход администратора через API').toBeTruthy();
  return ctx;
}

export async function clientApi(): Promise<{ ctx: APIRequestContext; email: string }> {
  const ctx = await request.newContext({ baseURL: API, ignoreHTTPSErrors: true });
  const email = `${uniq('api')}@e2e.local`;
  const res = await ctx.post('/Auth/register', {
    data: { email, password: 'Passw0rd!', fullName: 'API User' },
  });
  expect(res.ok(), 'регистрация клиента через API').toBeTruthy();
  return { ctx, email };
}

export interface TestCell {
  id: string;
  number: string;
  price: number;
}

/** Создаёт склад и свободную ячейку с уникальным номером (под администратором). */
export async function createCell(admin: APIRequestContext, price = 100): Promise<TestCell> {
  const wh = await admin.post('/WareHouse', {
    data: { name: uniq('wh'), description: 'e2e', address: 'e2e street', floor: 1 },
  });
  expect(wh.ok()).toBeTruthy();
  const warehouseId = (await wh.json()).result as string;

  const number = uniq('E2E').toUpperCase();
  const cell = await admin.post('/StorageCell', {
    data: {
      wareHouseId: warehouseId,
      numberStorageCalls: number,
      sizeOfStorage: { width: 1, height: 1, depth: 1 },
      price,
      floor: 1,
      size: 0,
    },
  });
  expect(cell.ok()).toBeTruthy();
  return { id: (await cell.json()).result as string, number, price };
}

export async function reserveViaApi(ctx: APIRequestContext, cellId: string, from: number, to: number) {
  const res = await ctx.post(`/StorageCell/${cellId}/reserve`, {
    data: {
      startDate: `${utcDate(from)}T00:00:00.000Z`,
      endDate: `${utcDate(to)}T00:00:00.000Z`,
      totalPrice: 0,
    },
  });
  expect(res.status()).toBe(204);
}

// ---------------------------------------------------------------- UI

/** Принимает все alert/confirm и копит их тексты. */
export function acceptDialogs(page: Page): string[] {
  const messages: string[] = [];
  page.on('dialog', async (dialog) => {
    messages.push(dialog.message());
    await dialog.accept();
  });
  return messages;
}

export async function registerViaUi(page: Page): Promise<string> {
  const email = `${uniq('ui')}@e2e.local`;
  await page.goto('/');
  await page.click('.tab[data-tab="register"]');
  await page.fill('#auth-email', email);
  await page.fill('#auth-fullname', 'E2E User');
  await page.fill('#auth-password', 'Passw0rd!');
  await page.click('#auth-submit');
  await expect(page.locator('#screen-app')).toBeVisible();
  return email;
}

export async function loginViaUi(page: Page, email: string, password: string) {
  await page.goto('/');
  await page.fill('#auth-email', email);
  await page.fill('#auth-password', password);
  await page.click('#auth-submit');
  await expect(page.locator('#screen-app')).toBeVisible();
}

/** Карточка ячейки по её уникальному номеру. */
export function cardOf(page: Page, number: string): Locator {
  return page.locator('#view-cells .item-card', { hasText: `Ячейка ${number}` });
}

export function rentalCardOf(page: Page, number: string): Locator {
  return page.locator('#view-rentals .item-card', { hasText: number });
}

export async function openBookingModal(page: Page, number: string, from: number, to: number) {
  await cardOf(page, number).getByRole('button', { name: 'Забронировать' }).click();
  await expect(page.locator('#booking-modal')).toBeVisible();
  await page.fill('#booking-start-date', utcDate(from));
  await page.fill('#booking-end-date', utcDate(to));
}
