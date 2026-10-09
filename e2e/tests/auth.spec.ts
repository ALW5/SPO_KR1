import { expect, test } from '@playwright/test';
import { ADMIN, acceptDialogs, loginViaUi, registerViaUi } from './helpers';

test.describe('Авторизация', () => {
  test('без входа виден экран авторизации', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('#screen-auth')).toBeVisible();
    await expect(page.locator('#screen-app')).toBeHidden();
  });

  test('регистрация входит в систему, перезагрузка страницы сохраняет сессию (кука)', async ({ page }) => {
    await registerViaUi(page);
    await expect(page.locator('#user-name')).not.toBeEmpty();

    await page.reload();
    await expect(page.locator('#screen-app')).toBeVisible();
  });

  test('выход возвращает на экран авторизации и закрывает сессию', async ({ page }) => {
    await registerViaUi(page);

    await page.click('#btn-logout');
    await expect(page.locator('#screen-auth')).toBeVisible();

    await page.reload();
    await expect(page.locator('#screen-auth')).toBeVisible();
  });

  test('неверный пароль показывает ошибку и не пускает в приложение', async ({ page }) => {
    await page.goto('/');
    await page.fill('#auth-email', ADMIN.email);
    await page.fill('#auth-password', 'wrong-password');
    await page.click('#auth-submit');

    await expect(page.locator('#auth-error')).not.toBeEmpty();
    await expect(page.locator('#screen-app')).toBeHidden();
  });

  test('администратор входит и видит вкладку складов', async ({ page }) => {
    acceptDialogs(page);
    await loginViaUi(page, ADMIN.email, ADMIN.password);
    await expect(page.locator('[data-view="warehouses"]')).toBeVisible();
  });
});
