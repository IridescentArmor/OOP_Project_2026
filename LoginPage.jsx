import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api";
import { useAuth } from "../context/AuthContext";

function pickRedirect(roleNames) {
  const roles = roleNames ?? [];
  if (roles.includes("Admin")) return "/admin";
  if (roles.includes("Seller")) return "/seller";
  return "/profile";
}

export default function LoginPage() {
  const navigate = useNavigate();
  const { saveAuth } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState("");

  async function onSubmit(event) {
    event.preventDefault();
    setIsLoading(true);
    setMessage("");

    try {
      const result = await login(email, password);
      saveAuth(result);
      const to = pickRedirect(result?.user?.roleNames);
      setMessage("Успішний вхід. Переходимо...");
      setTimeout(() => navigate(to), 700);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка входу");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="card">
      <h2>Вхід</h2>
      <div className="auth-switch">
        <Link className="nav-link active" to="/login">
          Вхід
        </Link>
        <Link className="nav-link" to="/register">
          Реєстрація
        </Link>
      </div>
      <form className="form" onSubmit={onSubmit}>
        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>
        <button type="submit" disabled={isLoading}>
          {isLoading ? "Входимо..." : "Увійти"}
        </button>
      </form>
      <p className="muted">
        {/* Демо-дані залишаємо під формою, без автопідстановки у поля вводу. */}
        Демо-акаунти: <code>buyer@market.local</code>, <code>seller@market.local</code>,
        <code> admin@market.local</code>, <code>superadmin@market.local</code> / <code>Demo123!</code>
      </p>
      {message ? <p>{message}</p> : null}
    </section>
  );
}
