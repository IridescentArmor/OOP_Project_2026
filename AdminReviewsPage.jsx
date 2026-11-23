import { useEffect, useState } from "react";
import { adminDeleteReview, adminGetReviews, adminHideReview, adminUnhideReview } from "../api";
import { useAuth } from "../context/AuthContext";

export default function AdminReviewsPage() {
  const { token } = useAuth();
  const [reviews, setReviews] = useState([]);
  const [message, setMessage] = useState("");
  const [reasonByReviewId, setReasonByReviewId] = useState({});

  async function reload() {
    const data = await adminGetReviews(token);
    setReviews(data ?? []);
  }

  useEffect(() => {
    reload().catch((e) => setMessage(e instanceof Error ? e.message : "Помилка завантаження"));
  }, []);

  async function moderate(review, action) {
    const reason = (reasonByReviewId[review.id] ?? "").trim();
    if (!reason) {
      setMessage("Вкажи причину модерації.");
      return;
    }

    setMessage("");
    try {
      if (action === "hide") {
        await adminHideReview(review.productId, review.id, reason, token);
      } else {
        await adminDeleteReview(review.productId, review.id, reason, token);
      }

      await reload();
      setMessage(action === "hide" ? "Відгук приховано." : "Відгук видалено.");
      setReasonByReviewId((prev) => ({ ...prev, [review.id]: "" }));
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка модерації");
    }
  }

  return (
    <section className="stack">
      <div className="card">
        <h2>Модерація відгуків</h2>
        <p className="muted">
          Адміністратор може приховувати або видаляти відгуки з обов&apos;язковою причиною.
        </p>
        {message ? <p>{message}</p> : null}
      </div>

      <div className="stack">
        {reviews.length === 0 ? (
          <section className="card">
            <p className="muted">Відгуків для модерації поки немає.</p>
          </section>
        ) : (
          reviews.map((review) => (
            <article className="card" key={review.id}>
              <div className="row">
                <div>
                  <h3>{review.productTitle}</h3>
                  <p className="muted">
                    Покупець: {review.customerName} | Оцінка: {review.rating}/5
                  </p>
                </div>
                <span className={`status-badge${review.isHidden ? " danger" : ""}`}>
                  {review.isHidden ? "Приховано" : "Видимо"}
                </span>
              </div>
              <p>{review.text}</p>
              {review.moderationReason ? (
                <p className="muted">Остання причина: {review.moderationReason}</p>
              ) : null}
              <label className="form">
                Причина модерації
                <input
                  value={reasonByReviewId[review.id] ?? ""}
                  onChange={(e) =>
                    setReasonByReviewId((prev) => ({ ...prev, [review.id]: e.target.value }))
                  }
                  placeholder="Наприклад: ненормативна лексика"
                />
              </label>
              <div className="action-row">
                {!review.isHidden ? (
                  <button type="button" onClick={() => moderate(review, "hide")}>
                    Hide
                  </button>
                ) : (
                  <button
                    className="secondary-button"
                    type="button"
                    onClick={async () => {
                      try {
                        await adminUnhideReview(review.productId, review.id, token);
                        await reload();
                        setMessage("Відгук повернуто до показу.");
                      } catch (e) {
                        setMessage(e instanceof Error ? e.message : "Помилка");
                      }
                    }}
                  >
                    Unhide
                  </button>
                )}
                <button className="danger" type="button" onClick={() => moderate(review, "delete")}>
                  Delete
                </button>
              </div>
            </article>
          ))
        )}
      </div>
    </section>
  );
}
