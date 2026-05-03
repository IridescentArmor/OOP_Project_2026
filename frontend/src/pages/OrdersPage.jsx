import { useEffect, useState } from "react";
import { cancelMyOrder, getMyOrders } from "../api";
import { useAuth } from "../context/AuthContext";

function canCancel(status) {
  return status !== "Shipped" && status !== "Completed" && status !== "Canceled";
}

function getStatusMeta(status) {
  switch (status) {
    case "Created":
      return { label: "Нове замовлення", tone: "" };
    case "Processing":
      return { label: "Обробляється", tone: "" };
    case "Shipped":
      return { label: "Відправлено", tone: "" };
    case "Completed":
      return { label: "Отримано", tone: "" };
    case "Canceled":
      return { label: "Скасовано", tone: " danger" };
    default:
      return { label: status, tone: "" };
  }
}

export default function OrdersPage() {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState("");

  async function reload() {
    const data = await getMyOrders(token);
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
  }, []);

  async function onCancel(orderId) {
    setMessage("");
    try {
      await cancelMyOrder(orderId, token);
      await reload();
      setMessage("Замовлення скасовано.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка");
    }
  }

  if (isLoading) return <section className="card">Завантаження...</section>;

  return (
    <section className="stack">
      <div className="card">
        <h2>Мої замовлення</h2>
        {message ? <p>{message}</p> : null}
        {orders.length === 0 ? (
          <p className="muted">Поки що немає замовлень.</p>
        ) : (
          <div className="stack">
            {orders.map((o) => (
              <div key={o.id} className="card subtle">
                <div className="row">
                  <div>
                    <strong>№ {o.id}</strong>
                    <div className="muted">
                      Створено:{" "}
                      {o.createdAtUtc ? new Date(o.createdAtUtc).toLocaleString("uk-UA") : "-"}
                    </div>
                  </div>
                  <div>
                    <span className={`status-badge${getStatusMeta(o.status).tone}`}>
                      {getStatusMeta(o.status).label}
                    </span>
                    <br />
                    <strong>{o.total} $</strong>
                  </div>
                </div>

                <div className="muted">
                  Отримувач: {o.recipientName} ({o.recipientPhone})
                  <br />
                  Адреса: {o.shippingAddress}
                  {o.trackingNumber ? (
                    <>
                      <br />
                      ТТН: <strong>{o.trackingNumber}</strong>
                    </>
                  ) : null}
                </div>

                {Array.isArray(o.items) && o.items.length > 0 ? (
                  <ul className="list">
                    {o.items.map((it) => (
                      <li className="list-item" key={`${o.id}-${it.productId}`}>
                        <strong>{it.productTitle}</strong> — {it.unitPrice} $ x {it.quantity} ={" "}
                        {it.subtotal} $
                      </li>
                    ))}
                  </ul>
                ) : null}

                {canCancel(o.status) ? (
                  <button type="button" onClick={() => onCancel(o.id)}>
                    Скасувати замовлення
                  </button>
                ) : (
                  <p className="muted">Скасування недоступне для цього статусу.</p>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}

