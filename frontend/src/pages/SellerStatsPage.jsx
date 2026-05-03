import { useEffect, useState } from "react";
import { getSellerStats } from "../api";
import { useAuth } from "../context/AuthContext";

export default function SellerStatsPage() {
  const { token } = useAuth();
  const [stats, setStats] = useState(null);
  const [message, setMessage] = useState("");
  const [periodDays, setPeriodDays] = useState(7);

  useEffect(() => {
    getSellerStats(token, periodDays)
      .then((s) => setStats(s))
      .catch((e) => setMessage(e instanceof Error ? e.message : "Помилка"));
  }, [periodDays, token]);

  const maxRevenue = Math.max(...((stats?.points ?? []).map((point) => Number(point.revenue)) || [1]), 1);

  return (
    <section className="card">
      <h2>Статистика продажів</h2>
      {message ? <p>{message}</p> : null}
      {!stats ? (
        <p className="muted">Завантаження...</p>
      ) : (
        <>
          <div className="period-toggle">
            <button
              className={periodDays === 7 ? "" : "secondary-button"}
              type="button"
              onClick={() => setPeriodDays(7)}
            >
              7 днів
            </button>
            <button
              className={periodDays === 30 ? "" : "secondary-button"}
              type="button"
              onClick={() => setPeriodDays(30)}
            >
              30 днів
            </button>
          </div>
          <p>
            <strong>Виконаних замовлень:</strong> {stats.completedOrdersCount}
          </p>
          <p>
            <strong>Продано одиниць:</strong> {stats.soldItemsCount}
          </p>
          <p>
            <strong>Виручка:</strong> {stats.revenue} $
          </p>
          <div className="stats-chart">
            {stats.points.map((point) => (
              <div key={point.label} className="stats-bar-card">
                <div
                  className="stats-bar"
                  style={{ height: `${Math.max((Number(point.revenue) / maxRevenue) * 180, 8)}px` }}
                  title={`${point.label}: ${point.revenue} $`}
                />
                <strong>{point.label}</strong>
                <span className="muted">{point.revenue} $</span>
                <span className="muted">{point.ordersCount} зам.</span>
              </div>
            ))}
          </div>
          <p className="muted">Статистика рахується лише для статусу Completed.</p>
        </>
      )}
    </section>
  );
}

