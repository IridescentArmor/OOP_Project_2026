import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { createProductReview, getProductById, getProductReviews } from "../api";
import { useAuth } from "../context/AuthContext";
import { useCart } from "../context/CartContext";

export default function ProductDetailsPage() {
  const { id } = useParams();
  const { isAuthenticated, user, token } = useAuth();
  const { items, add, remove } = useCart();
  const [product, setProduct] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [reviewText, setReviewText] = useState("");
  const [reviewRating, setReviewRating] = useState(5);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
    async function load() {
      try {
        setError("");
        const [data, productReviews] = await Promise.all([
          getProductById(id),
          getProductReviews(id)
        ]);
        setProduct(data);
        setReviews(productReviews ?? []);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Помилка завантаження");
      }
    }
    load();
  }, [id]);

  if (error) return <section className="card">{error}</section>;
  if (!product) return <section className="card">Завантаження...</section>;

  const qty = items[product.id] ?? 0;
  return (
    <section className="card">
      <img
        className="product-image-large"
        src={product.imageUrl || "https://via.placeholder.com/900x500?text=No+Image"}
        alt={product.title}
      />
      <h2>{product.title}</h2>
      <p>{product.description}</p>
      <p className="muted">В наявності: {product.stockQuantity}</p>
      <p>
        <strong>{product.price} $</strong>
      </p>
      <div className="qty-controls">
        <button className="icon-button" onClick={() => remove(product.id)} type="button">
          -
        </button>
        <span>{qty}</span>
        <button className="icon-button" onClick={() => add(product.id)} type="button">
          +
        </button>
      </div>

      <section className="card subtle">
        <h3>Відгуки</h3>
        {Array.isArray(reviews) && reviews.length > 0 ? (
          <ul className="list">
            {reviews.map((r) => (
              <li key={r.id} className="list-item">
                <strong>Оцінка: {r.rating}/5</strong>
                {r.isHidden ? <span className="status-badge danger">Приховано</span> : null}
                <div>{r.text}</div>
                {r.moderationReason ? (
                  <div className="muted">Причина модерації: {r.moderationReason}</div>
                ) : null}
              </li>
            ))}
          </ul>
        ) : (
          <p className="muted">Поки що немає відгуків.</p>
        )}

        {isAuthenticated && (user?.roleNames ?? []).includes("Customer") ? (
          <form
            className="form"
            onSubmit={async (e) => {
              e.preventDefault();
              setMessage("");
              try {
                await createProductReview(
                  product.id,
                  { rating: Number(reviewRating), text: reviewText },
                  token
                );
                const fresh = await getProductReviews(product.id);
                setReviews(fresh ?? []);
                setReviewText("");
                setReviewRating(5);
                setMessage("Відгук додано.");
              } catch (err) {
                setMessage(err instanceof Error ? err.message : "Помилка");
              }
            }}
          >
            <label>
              Оцінка
              <select
                value={reviewRating}
                onChange={(e) => setReviewRating(e.target.value)}
              >
                <option value={5}>5</option>
                <option value={4}>4</option>
                <option value={3}>3</option>
                <option value={2}>2</option>
                <option value={1}>1</option>
              </select>
            </label>
            <label>
              Текст відгуку
              <input
                value={reviewText}
                onChange={(e) => setReviewText(e.target.value)}
                required
                minLength={3}
              />
            </label>
            <button type="submit">Залишити відгук</button>
          </form>
        ) : (
          <p className="muted">Лише покупці можуть залишати відгуки.</p>
        )}
        {message ? <p>{message}</p> : null}
      </section>
    </section>
  );
}

