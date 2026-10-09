import { expect, test } from '@playwright/test';
import {
  ADMIN, acceptDialogs, adminApi, cardOf, clientApi, createCell, loginViaUi, reserveViaApi,
} from './helpers';

test.describe('Администратор', () => {
  test('видит кнопки удаления и удаляет свободную ячейку', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin);
    acceptDialogs(page); // confirm("Удалить ячейку?") -> OK

    await loginViaUi(page, ADMIN.email, ADMIN.password);

    const card = cardOf(page, cell.number);
    await expect(card.getByRole('button', { name: 'Удалить' })).toBeVisible();

    await card.getByRole('button', { name: 'Удалить' }).click();
    await expect(card).toHaveCount(0);
  });

  test('ячейку с договором удалить нельзя: понятная ошибка вместо 500', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin);
    const { ctx } = await clientApi();
    await reserveViaApi(ctx, cell.id, 1, 2);

    const dialogs = acceptDialogs(page);
    await loginViaUi(page, ADMIN.email, ADMIN.password);

    await cardOf(page, cell.number).getByRole('button', { name: 'Удалить' }).click();

    await expect.poll(() => dialogs.some((m) => m.toLowerCase().includes('rental'))).toBeTruthy();
    await expect(cardOf(page, cell.number)).toContainText('занята'); // ячейка на месте
  });

  test('администратор видит аренды всех клиентов', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin);
    const { ctx } = await clientApi();
    await reserveViaApi(ctx, cell.id, 1, 3);

    acceptDialogs(page);
    await loginViaUi(page, ADMIN.email, ADMIN.password);
    await page.click('[data-view="rentals"]');

    await expect(page.locator('#view-rentals .item-card', { hasText: cell.number })).toBeVisible();
  });
});
