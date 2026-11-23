import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { getProducts } from "../api";
import ProductCard from "../components/ProductCard";
import { useCart } from "../context/CartContext";

export default function HomePage() {
  const [products, setProducts] = useState([]);
  const { items, add, remove } = useCart();

  useEffect(() => {
    getProducts().then((d) => setProducts((d ?? []).slice(0, 6)));
  }, []);

  return (
    <section className="stack">
      <section className="hero">
        <h2>MarketplaceOOP</h2>
        <p>Популярні товари вже на головній сторінці.</p>
        <div className="hero-actions">
          <Link className="cta" to="/catalog">
            Весь каталог
          </Link>
          <Link className="ghost" to="/checkout">
            Кошик
          </Link>
        </div>
      </section>
      <div className="product-grid">
        {products.map((p) => (
          <ProductCard
            key={p.id}
            product={p}
            quantity={items[p.id] ?? 0}
            onAdd={add}
            onRemove={remove}
          />
        ))}
      </div>
    </section>
  );
}
