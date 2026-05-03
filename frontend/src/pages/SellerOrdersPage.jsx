import { useEffect, useState } from "react";
import { changeSellerOrderStatus, getSellerOrders } from "../api";
import { useAuth } from "../context/AuthContext";

const statusFlow = ["Created", "Processing", "Shipped", "Completed", "Canceled"];

export default function SellerOrdersPage() {
  const { token } = useAuth();
  const [orders, setOrders] = useState([]);
  const [message, setMessage] = useState("");
  const [ttnByOrder, setTtnByOrder] = useState({});

  async function reload() {
    const data = await getSellerOrders(token);
    setOrders(data ?? []);
  }

  useEffect(() => {
    reload().catch((e) => setMessage(e instanceof Error ? e.message : "Помилка"));
  }, []);

  async function updateStatus(orderId, status) {
    setMessage("");
    try {
      await changeSellerOrderStatus(
        orderId,
        {
          status,
          trackingNumber: status === "Shipped" ? ttnByOrder[orderId] ?? "" : null
        },
        token
      );
      await reload();
      setMessage("Статус оновлено");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка");
    }
  }

  return (
    <section className="card">
      <h2>Замовлення продавця</h2>
      {message ? <p>{message}</p> : null}
      {orders.length === 0 ? (
        <p className="muted">Поки що немає замовлень.</p>
      ) : (
        <ul className="list">
          {orders.map((o) => (
            <li key={o.id} className="list-item">
              <strong>№ {o.id}</strong> — сума: {o.total} $
              <div className="order-stage-row">
                {statusFlow.map((status) => (
                  <span
                    key={status}
                    className={`order-stage${status === o.status ? " current" : ""}${
                      o.status !== "Canceled" && statusFlow.indexOf(status) < statusFlow.indexOf(o.status)
                        ? " done"
                        : ""
                    }`}
                  >
                    {status}
                  </span>
                ))}
              </div>
              {o.status === "Canceled" ? (
                <div className="error">Замовлення скасовано покупцем.</div>
              ) : null}
              <div className="muted">
                {o.recipientName} ({o.recipientPhone}) | {o.shippingAddress}
              </div>
              {o.trackingNumber ? <div className="muted">ТТН: {o.trackingNumber}</div> : null}
              <div className="action-row">
                {getAvailableTransitions(o.status).map((s) => (
                  <button
                    key={s}
                    type="button"
                    onClick={() => updateStatus(o.id, s)}
                    disabled={s === "Shipped" && !(ttnByOrder[o.id] ?? "").trim()}
                  >
                    {s}
                  </button>
                ))}
              </div>
              {getAvailableTransitions(o.status).includes("Shipped") ? (
                <input
                  placeholder="ТТН (для Shipped)"
                  value={ttnByOrder[o.id] ?? ""}
                  onChange={(e) => setTtnByOrder((p) => ({ ...p, [o.id]: e.target.value }))}
                />
              ) : null}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

function getAvailableTransitions(status) {
  switch (status) {
    case "Created":
      return ["Processing"];
    case "Processing":
      return ["Shipped"];
    case "Shipped":
      return ["Completed"];
    default:
      return [];
  }
}

