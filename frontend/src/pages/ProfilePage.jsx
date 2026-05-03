import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { changeMyPassword, deleteMyAccount, updateMyProfile } from "../api";
import { useAuth } from "../context/AuthContext";
import { validatePersonName, validatePhoneNumber } from "../utils/formHelpers";

function hasRole(user, role) {
  return (user?.roleNames ?? []).includes(role);
}

export default function ProfilePage() {
  const { isAuthenticated, user, token, updateUser, logout } = useAuth();
  const [form, setForm] = useState({
    name: user?.name ?? "",
    email: user?.email ?? "",
    phoneNumber: user?.phoneNumber ?? ""
  });
  const [message, setMessage] = useState("");
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: "",
    newPassword: ""
  });
  const isAdmin = hasRole(user, "Admin");
  const isSeller = hasRole(user, "Seller");
  const isCustomer = hasRole(user, "Customer");
  const nameError = form.name ? validatePersonName(form.name) : "";
  const phoneError = form.phoneNumber ? validatePhoneNumber(form.phoneNumber) : "";

  useEffect(() => {
    setForm({
      name: user?.name ?? "",
      email: user?.email ?? "",
      phoneNumber: user?.phoneNumber ?? ""
    });
  }, [user?.name, user?.email, user?.phoneNumber]);

  if (!isAuthenticated) {
    return (
      <section className="card">
        <h2>Профіль покупця</h2>
        <p>
          Ти ще не увійшов. Перейди на сторінку <Link to="/login">входу</Link>.
        </p>
      </section>
    );
  }

  return (
    <section className="card">
      <h2>Профіль</h2>
      <form
        className="form"
        onSubmit={async (e) => {
          e.preventDefault();
          setMessage("");
          if (nameError || phoneError) {
            setMessage(nameError || phoneError);
            return;
          }

          try {
            const updated = await updateMyProfile(form, token);
            updateUser(updated);
            setMessage("Дані профілю оновлено.");
          } catch (err) {
            setMessage(err instanceof Error ? err.message : "Помилка");
          }
        }}
      >
        <label>
          Ім'я
          <input
            value={form.name}
            onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
            required
          />
        </label>
        {nameError ? <p className="error form-hint">{nameError}</p> : null}
        <label>
          Email
          <input
            type="email"
            value={form.email}
            onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
            required
          />
        </label>
        <label>
          Телефон
          <input
            value={form.phoneNumber}
            onChange={(e) => setForm((p) => ({ ...p, phoneNumber: e.target.value }))}
            required
          />
        </label>
        {phoneError ? <p className="error form-hint">{phoneError}</p> : null}
        <button type="submit">Зберегти зміни</button>
      </form>

      <p>
        <strong>Ролі:</strong> {(user?.roleNames ?? []).join(", ") || "немає"}
      </p>
      {message ? <p>{message}</p> : null}

      <form
        className="form"
        onSubmit={async (e) => {
          e.preventDefault();
          setMessage("");
          try {
            await changeMyPassword(passwordForm, token);
            setPasswordForm({ currentPassword: "", newPassword: "" });
            setMessage("Пароль успішно змінено.");
          } catch (err) {
            setMessage(err instanceof Error ? err.message : "Помилка зміни пароля");
          }
        }}
      >
        <label>
          Поточний пароль
          <input
            type="password"
            value={passwordForm.currentPassword}
            onChange={(e) => setPasswordForm((p) => ({ ...p, currentPassword: e.target.value }))}
            required
          />
        </label>
        <label>
          Новий пароль
          <input
            type="password"
            minLength={8}
            value={passwordForm.newPassword}
            onChange={(e) => setPasswordForm((p) => ({ ...p, newPassword: e.target.value }))}
            required
          />
        </label>
        <button type="submit">Змінити пароль</button>
      </form>

      <div className="dashboard-links-grid">
        {isCustomer ? (
          <>
            <Link className="dashboard-link-card" to="/orders">
              <strong>Customer Dashboard</strong>
              <span>Мої замовлення та покупки</span>
            </Link>
            <Link className="dashboard-link-card" to="/catalog">
              <strong>Каталог</strong>
              <span>Швидкий доступ до товарів</span>
            </Link>
          </>
        ) : null}

        {isSeller ? (
          <>
            <Link className="dashboard-link-card" to="/seller/products">
              <strong>Seller Dashboard</strong>
              <span>Товари магазину</span>
            </Link>
            <Link className="dashboard-link-card" to="/seller/orders">
              <strong>Замовлення продавця</strong>
              <span>Обробка та відправлення</span>
            </Link>
            <Link className="dashboard-link-card" to="/seller/stats">
              <strong>Статистика</strong>
              <span>Продажі за період</span>
            </Link>
          </>
        ) : null}

        {isAdmin ? (
          <>
            <Link className="dashboard-link-card" to="/admin/users">
              <strong>Admin Dashboard</strong>
              <span>Користувачі та модерація</span>
            </Link>
            <Link className="dashboard-link-card" to="/admin/orders">
              <strong>Замовлення</strong>
              <span>Перегляд усіх замовлень</span>
            </Link>
            <Link className="dashboard-link-card" to="/admin/categories">
              <strong>Категорії</strong>
              <span>Дерево категорій і підкатегорій</span>
            </Link>
            <Link className="dashboard-link-card" to="/admin/reviews">
              <strong>Відгуки</strong>
              <span>Hide/Delete з причиною</span>
            </Link>
          </>
        ) : null}
      </div>

      {!isAdmin ? (
        <button
          className="danger"
          type="button"
          onClick={async () => {
            if (!window.confirm("Точно видалити акаунт?")) return;
            try {
              await deleteMyAccount(token);
              logout();
            } catch (err) {
              setMessage(err instanceof Error ? err.message : "Помилка видалення акаунта");
            }
          }}
        >
          Видалити акаунт
        </button>
      ) : null}
    </section>
  );
}
