import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function SellerDashboardPage() {
  const { user } = useAuth();

  return (
    <section className="card">
      <h2>Кабінет продавця</h2>
      <p className="muted">
        Вітаю, <strong>{user?.name ?? "Seller"}</strong>.
      </p>
      <ul className="list">
        <li className="list-item">
          <Link to="/seller/products">Мої товари</Link>
        </li>
        <li className="list-item">
          <Link to="/seller/orders">Замовлення</Link>
        </li>
        <li className="list-item">
          <Link to="/seller/stats">Статистика</Link>
        </li>
      </ul>
    </section>
  );
}

