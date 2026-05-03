import { useEffect, useState } from "react";
import { adminDeleteOrder, adminGetOrders } from "../api";
import { useAuth } from "../context/AuthContext";

export default function AdminOrdersPage() {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  async function reload() {
    const data = await adminGetOrders(token);
    setOrders(data ?? []);
  }

  useEffect(() => {
    async function load() {
      try {
        setMessage("");
        setIsLoading(true);
        await reload();
      } catch (e) {
        setMessage(e instanceof Error ? e.message : "Помилка завантаження");
      } finally {
        setIsLoading(false);
      }
    }

    load();
  }, [token]);

  async function onDelete(orderId) {
    setMessage("");
    try {
      await adminDeleteOrder(orderId, token);
      await reload();
      setMessage("Замовлення видалено з системи.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка видалення");
    }
  }

  if (isLoading) return <section className="card">Завантаження...</section>;

  return (
    <section className="stack">
      <div className="card">
        <h2>Усі замовлення (Admin)</h2>
        {message ? <p>{message}</p> : null}
      </div>
      {orders.map((order) => (
        <article className="card" key={order.id}>
          <div className="row">
            <div>
              <strong>№ {order.id}</strong>
              <div className="muted">
                Покупець: {order.customerName} | Продавець: {order.sellerName}
              </div>
            </div>
            <span className={`status-badge${order.status === "Canceled" ? " danger" : ""}`}>
              {order.status}
            </span>
          </div>
          <p>
            <strong>{order.total} $</strong> |{" "}
            {order.createdAtUtc ? new Date(order.createdAtUtc).toLocaleString("uk-UA") : "-"}
          </p>
          <p className="muted">
            {order.recipientName} ({order.recipientPhone}) | {order.shippingAddress}
          </p>
          {order.trackingNumber ? <p className="muted">ТТН: {order.trackingNumber}</p> : null}
          <button className="danger" type="button" onClick={() => onDelete(order.id)}>
            Видалити замовлення
          </button>
        </article>
      ))}
      {orders.length === 0 ? (
        <section className="card">
          <p className="muted">Поки що немає замовлень.</p>
        </section>
      ) : null}
    </section>
  );
}
