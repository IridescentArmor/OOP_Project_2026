import { Link } from "react-router-dom";

export default function ProductCard({ product, quantity, onAdd, onRemove }) {
  return (
    <article className="product-card">
      <img
        className="product-image"
        src={product.imageUrl || "https://via.placeholder.com/640x360?text=No+Image"}
        alt={product.title}
      />
      <h3>
        <Link to={`/products/${product.id}`}>{product.title}</Link>
      </h3>
      <p>{product.description}</p>
      <div className="muted">
        Рейтинг: {Number(product.averageRating ?? 0).toFixed(1)} / 5 ({product.reviewsCount ?? 0} відгуків)
      </div>
      <div className="muted">В наявності: {product.stockQuantity}</div>
      <div className="product-footer">
        <strong>{product.price} $</strong>
        <div className="qty-controls">
          <button className="icon-button" onClick={() => onRemove(product.id)} type="button">
            -
          </button>
          <span>{quantity}</span>
          <button className="icon-button" onClick={() => onAdd(product.id)} type="button">
            +
          </button>
        </div>
      </div>
    </article>
  );
}
