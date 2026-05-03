import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../api";
import { useAuth } from "../context/AuthContext";
import { validatePersonName, validatePhoneNumber } from "../utils/formHelpers";

export default function RegisterPage() {
  const navigate = useNavigate();
  const { saveAuth } = useAuth();
  const [form, setForm] = useState({
    name: "",
    email: "",
    phoneNumber: "+380",
    password: "",
    includeCustomerRole: true,
    includeSellerRole: false,
    includeAdminRole: false,
    sellerCompanyName: "",
    adminAccessLevel: 0
  });
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState("");

  const phoneError = form.phoneNumber ? validatePhoneNumber(form.phoneNumber) : "";
  const nameError = form.name ? validatePersonName(form.name) : "";

  function setField(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function onSubmit(event) {
    event.preventDefault();
    if (nameError || phoneError) {
      setMessage(nameError || phoneError);
      return;
    }

    setIsLoading(true);
    setMessage("");

    try {
      const result = await register(form);
      saveAuth(result);
      setMessage("Реєстрація успішна. Переходимо в каталог...");
      setTimeout(() => navigate("/catalog"), 700);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка реєстрації");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="card">
      <h2>Реєстрація</h2>
      <div className="auth-switch">
        <Link className="nav-link" to="/login">
          Вхід
        </Link>
        <Link className="nav-link active" to="/register">
          Реєстрація
        </Link>
      </div>
      <form className="form" onSubmit={onSubmit}>
        <label>
          Ім'я
          <input
            onChange={(e) => setField("name", e.target.value)}
            required
            type="text"
            value={form.name}
          />
        </label>
        {nameError ? <p className="error form-hint">{nameError}</p> : null}
        <label>
          Email
          <input
            onChange={(e) => setField("email", e.target.value)}
            required
            type="email"
            value={form.email}
          />
        </label>
        <label>
          Телефон
          <input
            onChange={(e) => setField("phoneNumber", e.target.value)}
            required
            type="text"
            value={form.phoneNumber}
          />
        </label>
        {phoneError ? <p className="error form-hint">{phoneError}</p> : null}
        <label>
          Пароль
          <input
            minLength={8}
            onChange={(e) => setField("password", e.target.value)}
            required
            type="password"
            value={form.password}
          />
        </label>
        <label>
          Роль
          <select
            value={form.includeSellerRole ? "seller" : "customer"}
            onChange={(e) => {
              const isSeller = e.target.value === "seller";
              setForm((prev) => ({
                ...prev,
                includeSellerRole: isSeller,
                includeCustomerRole: !isSeller
              }));
            }}
          >
            <option value="customer">Покупець</option>
            <option value="seller">Продавець</option>
          </select>
        </label>
        {form.includeSellerRole ? (
          <label>
            Назва магазину
            <input
              value={form.sellerCompanyName}
              onChange={(e) => setField("sellerCompanyName", e.target.value)}
              required
            />
          </label>
        ) : null}
        <button disabled={isLoading} type="submit">
          {isLoading ? "Створюємо..." : "Створити акаунт"}
        </button>
      </form>
      {message ? <p>{message}</p> : null}
    </section>
  );
}
