// Профиль запуска https (dotnet run --launch-profile https) слушает https://localhost:7269.
// Для IIS Express используйте https://localhost:44373.
const API = 'https://localhost:7269';

const URLS = {
    register: '/Auth/register',
    login: '/Auth/login',
    logout: '/Auth/logout',
    me: '/Auth/me',

    cells: '/StorageCell',
    warehouses: '/WareHouse',
    rentals: '/api/Rentalagreement'
};


// ============================================================
// УТИЛИТЫ
// ============================================================

const $ = (selector) => document.querySelector(selector);
const $$ = (selector) => document.querySelectorAll(selector);

let currentUser = null;

function isAdmin() {
    return currentUser?.role === 'Administrator';
}


// ============================================================
// ЕДИНЫЙ ЗАПРОС К BACKEND
// ============================================================

async function api(path, {
    method = 'GET',
    body = undefined,
    auth = true
} = {}) {

    const options = {
        method,
        credentials: 'include', // <-- отправляем HttpOnly cookie
        headers: {}
    };

    if (body !== undefined) {
        options.headers['Content-Type'] = 'application/json';
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(API + path, options);

        // Сессия закончилась
        if (response.status === 401) {
            currentUser = null;
            showAuth();
            throw new Error('Сессия истекла. Войдите снова.');
        }

        // Нет доступа
        if (response.status === 403) {
            throw new Error('У вас нет прав для выполнения этого действия.');
        }

        // Ошибка
        if (!response.ok) {
            let message = `Ошибка сервера: ${response.status}`;

            try {
                const errorData = await response.json();

                if (errorData?.message) {
                    message = errorData.message;
                } else if (errorData?.Message) {
                    message = errorData.Message;
                } else if (errorData?.error) {
                    message = errorData.error;
                }
            } catch {
                // Ответ не JSON
            }

            throw new Error(message);
        }

        // 204 No Content
        if (response.status === 204) {
            return null;
        }

        const text = await response.text();

        if (!text) {
            return null;
        }

        const data = JSON.parse(text);

        // Одиночные объекты бэк отдаёт в обёртке Envelope { result, statusCode, message, timestamp }.
        // Списки приходят «голыми» массивами - их не трогаем.
        if (
            data &&
            !Array.isArray(data) &&
            typeof data === 'object' &&
            'result' in data &&
            'timestamp' in data
        ) {
            return data.result;
        }

        return data;

    } catch (error) {

        // Ошибка подключения к API
        if (error instanceof TypeError) {
            throw new Error(
                'Не удалось подключиться к серверу. Проверьте, запущен ли ASP.NET Core API.'
            );
        }

        throw error;
    }
}


// ============================================================
// АВТОРИЗАЦИЯ
// ============================================================

let authMode = 'login';

$$('.tab').forEach(tab => {

    tab.addEventListener('click', () => {

        authMode = tab.dataset.tab;

        $$('.tab').forEach(t => {
            t.classList.toggle('active', t === tab);
        });

        const fullName = $('#auth-fullname');

        fullName.classList.toggle(
            'hidden',
            authMode !== 'register'
        );

        fullName.required = authMode === 'register';

        $('#auth-submit').textContent =
            authMode === 'login'
                ? 'Войти'
                : 'Зарегистрироваться';

        $('#auth-error').textContent = '';
    });
});


// ============================================================
// LOGIN / REGISTER
// ============================================================

$('#form-auth').addEventListener('submit', async (event) => {

    event.preventDefault();

    const email = $('#auth-email').value.trim();
    const password = $('#auth-password').value;
    const fullName = $('#auth-fullname').value.trim();

    const submitButton = $('#auth-submit');

    submitButton.disabled = true;
    $('#auth-error').textContent = '';

    try {

        if (authMode === 'login') {

            await api(URLS.login, {
                method: 'POST',
                body: {
                    email,
                    password
                },
                auth: false
            });

        } else {

            await api(URLS.register, {
                method: 'POST',
                body: {
                    email,
                    password,
                    fullName
                },
                auth: false
            });
        }

        // Backend кладёт JWT в HttpOnly cookie.
        // Поэтому получаем пользователя через /Auth/me.
        await loadCurrentUser();

        showApp();

    } catch (error) {

        $('#auth-error').textContent = error.message;

    } finally {

        submitButton.disabled = false;
    }
});


// ============================================================
// ПОЛУЧЕНИЕ ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ
// ============================================================

async function loadCurrentUser() {

    const user = await api(URLS.me);

    currentUser = user;

    return user;
}


// ============================================================
// LOGOUT
// ============================================================

$('#btn-logout').addEventListener('click', async () => {

    try {

        await api(URLS.logout, {
            method: 'POST'
        });

    } catch {
        // Даже если сервер вернул ошибку,
        // локально всё равно возвращаемся на экран входа.
    }

    currentUser = null;

    showAuth();
});


function showAuth() {

    $('#screen-app').classList.remove('active');
    $('#screen-auth').classList.add('active');

    $('#form-auth').reset();
    $('#auth-error').textContent = '';

    authMode = 'login';

    $$('.tab').forEach(tab => {
        tab.classList.toggle(
            'active',
            tab.dataset.tab === 'login'
        );
    });

    $('#auth-fullname').classList.add('hidden');
    $('#auth-fullname').required = false;

    $('#auth-submit').textContent = 'Войти';
}


// ============================================================
// ПОКАЗ ПРИЛОЖЕНИЯ
// ============================================================

function showApp() {

    $('#screen-auth').classList.remove('active');
    $('#screen-app').classList.add('active');

    $('#user-name').textContent =
        currentUser?.fullName ||
        currentUser?.email ||
        'Пользователь';

    // Показываем интерфейс
    // и сразу загружаем ячейки.
    loadCells();
}


// ============================================================
// ПРОВЕРКА СЕССИИ ПРИ ЗАПУСКЕ
// ============================================================

async function checkSession() {

    try {

        await loadCurrentUser();

        showApp();

    } catch {

        currentUser = null;
        showAuth();
    }
}


// ============================================================
// НАВИГАЦИЯ
// ============================================================

$$('.tab-main').forEach(tab => {

    tab.addEventListener('click', () => {

        const view = tab.dataset.view;

        $$('.tab-main').forEach(t => {
            t.classList.toggle('active', t === tab);
        });

        $$('.view').forEach(v => {
            v.classList.toggle(
                'active',
                v.id === 'view-' + view
            );
        });

        if (view === 'cells') {
            loadCells();
        }

        if (view === 'warehouses') {
            loadWarehouses();
        }

        if (view === 'rentals') {
            loadRentals();
        }
    });
});


// ============================================================
// ЯЧЕЙКИ
// ============================================================

async function loadCells() {

    const container = $('#view-cells');

    container.innerHTML =
        '<div class="loading">Загрузка…</div>';

    try {

        const cells = await api(URLS.cells);

        console.log('CELLS FROM API:', cells);
        
        const array = Array.isArray(cells)
            ? cells
            : [];

        if (array.length === 0) {

            container.innerHTML =
                '<div class="empty">Ячеек пока нет</div>';

            return;
        }

        container.innerHTML = `
            <div class="grid">
                ${array.map(renderCell).join('')}
            </div>
        `;

    } catch (error) {

        container.innerHTML = `
            <p class="error">${escapeHtml(error.message)}</p>
        `;
    }
}


function renderCell(cell) {

    const id = cell.storageId;

    const number =
        cell.numberStorageCalls ??
        cell.number ??
        '—';

    const price =
        cell.price ??
        '—';

    const floor =
        cell.floor ??
        '—';

    const reserved =
        cell.status === 1;

    const reserveButton = reserved
        ? ''
        : `
            <button
                class="btn-primary"
                onclick="reserveCell('${id}')">
                Забронировать
            </button>
        `;

    const deleteButton = isAdmin()
        ? `
            <button
                class="btn-danger"
                onclick="deleteCell('${id}')">
                Удалить
            </button>
        `
        : '';

    return `
        <div class="item-card">

            <h3>Ячейка ${escapeHtml(number)}</h3>

            <p>
                Этаж: ${escapeHtml(floor)}
            </p>

            <p>
                Цена: ${escapeHtml(price)}
            </p>

            <p>
                Статус:
                ${
                    reserved
                        ? '<span class="status-busy">занята</span>'
                        : '<span class="status-free">свободна</span>'
                }
            </p>

            <div class="actions">
                ${reserveButton}
                ${deleteButton}
            </div>

        </div>
    `;
}


// ============================================================
// БРОНИРОВАНИЕ ЯЧЕЙКИ
// ============================================================
async function reserveCell(id) {

    try {

        // Получаем информацию о ячейке
        const cell = await api(`${URLS.cells}/${id}`);

        if (!cell) {
            throw new Error('Не удалось получить данные ячейки.');
        }

        const number =
            cell.numberStorageCalls ??
            cell.number ??
            '—';

        const pricePerDay = Number(cell.price);

        // Открываем окно бронирования
        openBookingModal({
            id,
            number,
            pricePerDay
        });

    } catch (error) {

        alert(error.message);
    }
}

let bookingData = null;

function openBookingModal(data) {

    bookingData = data;

    $('#booking-cell-number').textContent =
        data.number;

    $('#booking-price').textContent =
        data.pricePerDay;

    const today = new Date();

    const todayString =
        today.toISOString().split('T')[0];

    $('#booking-start-date').value =
        todayString;

    const tomorrow = new Date(today);

    tomorrow.setDate(
        tomorrow.getDate() + 1
    );

    $('#booking-end-date').value =
        tomorrow.toISOString().split('T')[0];

    updateBookingPrice();

    $('#booking-modal').classList.remove('hidden');
}

function getBookingDays() {

    const startValue = $('#booking-start-date').value;
    const endValue = $('#booking-end-date').value;

    if (!startValue || !endValue) {
        return 0;
    }

    // 'YYYY-MM-DD' парсится как UTC-полночь, поэтому разница всегда кратна суткам (без влияния DST)
    const difference =
        new Date(endValue).getTime() - new Date(startValue).getTime();

    return Math.round(difference / (1000 * 60 * 60 * 24));
}

// Только пересчитывает цену. Обработчики событий здесь НЕ навешиваются.
function updateBookingPrice() {

    if (!bookingData) {
        return;
    }

    const days = getBookingDays();

    if (days <= 0) {
        $('#booking-days').textContent = '0';
        $('#booking-total').textContent = '0';
        return;
    }

    const total = days * bookingData.pricePerDay;

    $('#booking-days').textContent = days;
    $('#booking-total').textContent = total.toFixed(2);
}

let bookingInProgress = false;

async function confirmBooking() {

    if (!bookingData || bookingInProgress) {
        return;
    }

    const startDate = $('#booking-start-date').value;
    const endDate = $('#booking-end-date').value;

    if (!startDate || !endDate) {
        alert('Выберите даты.');
        return;
    }

    const days = getBookingDays();

    if (days <= 0) {
        alert('Дата окончания должна быть позже даты начала.');
        return;
    }

    // Итоговая сумма считается на сервере; здесь только для показа пользователю.
    const totalPrice = days * bookingData.pricePerDay;

    bookingInProgress = true;
    $('#booking-confirm').disabled = true;

    try {

        await api(
            `${URLS.cells}/${bookingData.id}/reserve`,
            {
                method: 'POST',
                body: {
                    // Отправляем даты как UTC-полночь (без сдвига часового пояса)
                    startDate: new Date(startDate).toISOString(),
                    endDate: new Date(endDate).toISOString(),
                    totalPrice
                }
            }
        );

        closeBookingModal();

        alert(
            `Ячейка успешно забронирована.\n` +
            `Период: ${startDate} — ${endDate}\n` +
            `Стоимость: ${totalPrice.toFixed(2)} ₽`
        );

        await loadCells();

        if ($('#view-rentals').classList.contains('active')) {
            await loadRentals();
        }

    } catch (error) {

        alert(error.message);

    } finally {

        bookingInProgress = false;
        $('#booking-confirm').disabled = false;
    }
}

// Обработчики модального окна навешиваются один раз
$('#booking-start-date').addEventListener('change', updateBookingPrice);
$('#booking-end-date').addEventListener('change', updateBookingPrice);
$('#booking-cancel').addEventListener('click', () => closeBookingModal());
$('#booking-confirm').addEventListener('click', confirmBooking);

function closeBookingModal() {

    bookingData = null;

    $('#booking-modal')
        .classList.add('hidden');
}

// ============================================================
// УДАЛЕНИЕ ЯЧЕЙКИ
// ============================================================

async function deleteCell(id) {

    if (!isAdmin()) {
        alert('Удалять ячейки может только администратор.');
        return;
    }

    if (!confirm('Удалить ячейку?')) {
        return;
    }

    try {

        await api(
            `${URLS.cells}/${id}`,
            {
                method: 'DELETE'
            }
        );

        await loadCells();

    } catch (error) {

        alert(error.message);
    }
}


// ============================================================
// СКЛАДЫ
// ============================================================

async function loadWarehouses() {

    const container = $('#view-warehouses');

    container.innerHTML =
        '<div class="loading">Загрузка…</div>';

    try {

        const warehouses =
            await api(URLS.warehouses);

        const array =
            Array.isArray(warehouses)
                ? warehouses
                : [];

        if (array.length === 0) {

            container.innerHTML =
                '<div class="empty">Складов пока нет</div>';

            return;
        }

        container.innerHTML = `
            <div class="grid">
                ${array.map(renderWarehouse).join('')}
            </div>
        `;

    } catch (error) {

        container.innerHTML = `
            <p class="error">
                ${escapeHtml(error.message)}
            </p>
        `;
    }
}


function renderWarehouse(warehouse) {

    return `
        <div class="item-card">

            <h3>
                ${escapeHtml(
                    warehouse.name ?? 'Склад'
                )}
            </h3>

            <p>
                Адрес:
                ${escapeHtml(
                    warehouse.address ?? '—'
                )}
            </p>

            <p>
                Этажей:
                ${escapeHtml(
                    warehouse.floor ?? '—'
                )}
            </p>

        </div>
    `;
}


// ============================================================
// АРЕНДЫ
// ============================================================

async function loadRentals() {

    const container = $('#view-rentals');

    container.innerHTML =
        '<div class="loading">Загрузка…</div>';

    try {

        const [rentals, cells] = await Promise.all([
            api(URLS.rentals),
            api(URLS.cells)
        ]);

        const rentalArray =
            Array.isArray(rentals)
                ? rentals
                : [];

        const cellArray =
            Array.isArray(cells)
                ? cells
                : [];

        if (rentalArray.length === 0) {

            container.innerHTML =
                '<div class="empty">Аренд пока нет</div>';

            return;
        }

        const cellsById = new Map(
            cellArray.map(cell => [
                cell.storageId,
                cell
            ])
        );

        container.innerHTML = `
            <div class="grid">
                ${rentalArray
            .map(rental =>
                renderRental(
                    rental,
                    cellsById.get(rental.storageId)
                )
            )
            .join('')}
            </div>
        `;

    } catch (error) {

        container.innerHTML = `
            <p class="error">
                ${escapeHtml(error.message)}
            </p>
        `;
    }
}


function renderRental(rental, cell) {

    const shortId =
        rental.id
            ? String(rental.id).slice(0, 8)
            : '—';

    const start =
        rental.startDate
            ? new Date(rental.startDate)
                .toLocaleDateString('ru-RU')
            : '—';

    const end =
        rental.endDate
            ? new Date(rental.endDate)
                .toLocaleDateString('ru-RU')
            : '—';

    const cellNumber =
        cell?.numberStorageCalls ?? '—';

    const floor =
        cell?.floor ?? '—';

    const price =
        rental.totalPrice ?? '—';

    return `
        <div class="item-card">

            <h3>
                Аренда #${escapeHtml(shortId)}
            </h3>

            <p>
                Ячейка №:
                <strong>
                    ${escapeHtml(cellNumber)}
                </strong>
            </p>

            <p>
                Этаж:
                ${escapeHtml(floor)}
            </p>

            <p>
                С:
                ${escapeHtml(start)}
            </p>

            <p>
                По:
                ${escapeHtml(end)}
            </p>

            <p>
                Сумма:
                ${escapeHtml(price)} ₽
            </p>

            <div class="actions">

                <button
                    class="btn-danger"
                    onclick="cancelRental('${rental.id}')">
                    Отменить аренду
                </button>

            </div>

        </div>
    `;
}


// ============================================================
// ОТМЕНА АРЕНДЫ
// ============================================================

async function cancelRental(id) {

    if (!confirm('Отменить аренду?')) {
        return;
    }

    try {

        await api(
            `${URLS.rentals}/${id}`,
            {
                method: 'DELETE'
            }
        );

        await loadRentals();

    } catch (error) {

        alert(error.message);
    }
}


// ============================================================
// ЗАЩИТА ОТ HTML ВСТАВОК
// ============================================================

function escapeHtml(value) {

    return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}


// ============================================================
// СТАРТ ПРИЛОЖЕНИЯ
// ============================================================

checkSession();
