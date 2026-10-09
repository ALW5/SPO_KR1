import { expect, test } from '@playwright/test';
import type { Page } from '@playwright/test';
import {
  acceptDialogs, adminApi, cardOf, createCell, openBookingModal,
  registerViaUi, rentalCardOf, utcDate,
} from './helpers';

test.describe('Бронирование (клиент)', () => {
  test('клиент бронирует ячейку, видит аренду и отменяет её', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin, 100);
    const dialogs = acceptDialogs(page);

    await registerViaUi(page);

    const card = cardOf(page, cell.number);
    await expect(card).toContainText('свободна');
    // у клиента нет админских кнопок
    await expect(card.getByRole('button', { name: 'Удалить' })).toHaveCount(0);

    await openBookingModal(page, cell.number, 1, 4);

    // цена в окне посчитана правильно (раньше было NaN из-за Envelope)
    await expect(page.locator('#booking-price')).toHaveText('100');
    await expect(page.locator('#booking-days')).toHaveText('3');
    await expect(page.locator('#booking-total')).toHaveText('300.00');

    await page.click('#booking-confirm');

    await expect(card).toContainText('занята');
    await expect(card.getByRole('button', { name: 'Забронировать' })).toHaveCount(0);
    expect(dialogs.some((m) => m.includes('успешно забронирована'))).toBeTruthy();

    // вкладка "Мои аренды"
    await page.click('[data-view="rentals"]');
    const rental = rentalCardOf(page, cell.number);
    await expect(rental).toBeVisible();
    await expect(rental).toContainText('300');

    // отмена аренды освобождает ячейку
    await rental.getByRole('button', { name: 'Отменить аренду' }).click();
    await expect(rental).toHaveCount(0);

    await page.click('[data-view="cells"]');
    await expect(cardOf(page, cell.number)).toContainText('свободна');
  });

  test('при многократной смене дат уходит ровно один POST /reserve', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin, 50);
    acceptDialogs(page);

    const reserveCalls: string[] = [];
    page.on('request', (req) => {
      if (req.method() === 'POST' && req.url().includes('/reserve')) {
        reserveCalls.push(req.url());
      }
    });

    await registerViaUi(page);
    await openBookingModal(page, cell.number, 1, 2);
    // несколько раз меняем даты: раньше при каждом изменении навешивался ещё один обработчик "Подтвердить"
    await openBookingDatesAgain(page);
    await page.click('#booking-confirm');

    await expect(cardOf(page, cell.number)).toContainText('занята');
    expect(reserveCalls).toHaveLength(1);
  });

  test('конец периода раньше начала: сумма 0 и бронь не создаётся', async ({ page }) => {
    const admin = await adminApi();
    const cell = await createCell(admin, 100);
    const dialogs = acceptDialogs(page);

    await registerViaUi(page);
    await openBookingModal(page, cell.number, 3, 1);

    await expect(page.locator('#booking-days')).toHaveText('0');
    await page.click('#booking-confirm');

    expect(dialogs.some((m) => m.includes('позже'))).toBeTruthy();
    await page.click('#booking-cancel');
    await expect(cardOf(page, cell.number)).toContainText('свободна');
  });

  test('аренды одного клиента не видны другому', async ({ page, browser }) => {
    const admin = await adminApi();
    const cell = await createCell(admin, 10);
    acceptDialogs(page);

    await registerViaUi(page);
    await openBookingModal(page, cell.number, 1, 2);
    await page.click('#booking-confirm');
    await expect(cardOf(page, cell.number)).toContainText('занята');

    // второй клиент в отдельном контексте браузера (свои куки)
    const otherContext = await browser.newContext({ ignoreHTTPSErrors: true });
    const otherPage = await otherContext.newPage();
    acceptDialogs(otherPage);
    await registerViaUi(otherPage);

    await expect(cardOf(otherPage, cell.number)).toContainText('занята'); // ячейка занята для всех
    await otherPage.click('[data-view="rentals"]');
    await expect(rentalCardOf(otherPage, cell.number)).toHaveCount(0);   // но чужой договор не виден

    await otherContext.close();
  });
});

async function openBookingDatesAgain(page: Page) {
  for (const [from, to] of [[1, 3], [2, 5], [1, 2]]) {
    await page.fill('#booking-start-date', utcDate(from));
    await page.fill('#booking-end-date', utcDate(to));
  }
}
