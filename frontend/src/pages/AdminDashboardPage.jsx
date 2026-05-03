import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function AdminDashboardPage() {
  const { user } = useAuth();

  return (
    <section className="card">
      <h2>Панель адміністратора</h2>
      <p className="muted">
        Вітаю, <strong>{user?.name ?? "Admin"}</strong>.
      </p>
      <ul className="list">
        <li className="list-item">
          <Link to="/admin/categories">Категорії</Link>
        </li>
        <li className="list-item">
          <Link to="/admin/users">Користувачі</Link>
        </li>
        <li className="list-item">
          <Link to="/admin/orders">Замовлення</Link>
        </li>
        <li className="list-item">
          <Link to="/admin/products">Товари продавців</Link>
        </li>
        <li className="list-item">
          <Link to="/admin/reviews">Модерація відгуків</Link>
        </li>
      </ul>
    </section>
  );
}

