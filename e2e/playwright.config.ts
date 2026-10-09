import { defineConfig } from '@playwright/test';

const API = process.env.API_URL ?? 'https://localhost:7269';
const FRONT = process.env.FRONT_URL ?? 'http://localhost:5173';

// Перед запуском должна работать БД: `docker compose up -d` в корне репозитория.
// Фронт и бэк поднимаются автоматически (или используются уже запущенные).
// ВАЖНО: адрес API также прописан константой API в front/app.js - при смене порта поправьте и её.
export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  expect: { timeout: 7_000 },
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: FRONT,
    ignoreHTTPSErrors: true, // самоподписанный dev-сертификат
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  webServer: [
    {
      command: 'npx --yes serve -l 5173 ../front',
      url: FRONT,
      reuseExistingServer: true,
      timeout: 60_000,
    },
    {
      command: 'dotnet run --project ../ProjectOne --launch-profile https',
      url: `${API}/swagger/index.html`,
      ignoreHTTPSErrors: true,
      reuseExistingServer: true,
      timeout: 180_000,
    },
  ],
});
