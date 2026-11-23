import { useEffect, useMemo, useState } from "react";
import { getCategories, getProducts } from "../api";
import ProductCard from "../components/ProductCard";
import { useCart } from "../context/CartContext";

export default function CatalogPage() {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [query, setQuery] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [onlyInStock, setOnlyInStock] = useState(false);
  const [sortBy, setSortBy] = useState("relevance");
  const { items, add, remove } = useCart();

  useEffect(() => {
    async function loadData() {
      try {
        setError("");
        setIsLoading(true);

        const [productsResult, categoriesResult] = await Promise.all([
          getProducts(),
          getCategories()
        ]);

        setProducts(productsResult ?? []);
        setCategories(categoriesResult ?? []);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Помилка завантаження");
      } finally {
        setIsLoading(false);
      }
    }

    loadData();
  }, []);

  const filteredProducts = useMemo(() => {
    const normalized = query.trim().toLowerCase();
    let result = [...products];
    const selectedIds = categoryId ? collectCategoryIds(categories, categoryId) : [];

    if (normalized) {
      result = result.filter(
        (product) =>
          product.title.toLowerCase().includes(normalized) ||
          product.description.toLowerCase().includes(normalized)
      );
    }

    if (categoryId) {
      result = result.filter((p) => selectedIds.includes(p.categoryId));
    }

    if (onlyInStock) {
      result = result.filter((p) => Number(p.stockQuantity) > 0);
    }

    if (sortBy === "priceAsc") {
      result.sort((a, b) => Number(a.price) - Number(b.price));
    } else if (sortBy === "priceDesc") {
      result.sort((a, b) => Number(b.price) - Number(a.price));
    } else if (sortBy === "nameAsc") {
      result.sort((a, b) => a.title.localeCompare(b.title));
    }

    return result;
  }, [products, query, categoryId, onlyInStock, sortBy]);

  if (isLoading) {
    return <section className="card">Завантаження каталогу...</section>;
  }

  return (
    <section className="stack">
      <div className="card">
        <h2>Каталог товарів</h2>
        <p className="muted">
          Категорій: {categories.length}. Додай товари у кошик і переходь до
          оформлення.
        </p>
        <input
          className="search-input"
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Пошук по назві або опису"
          type="text"
          value={query}
        />
        <div className="filters-row">
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            <option value="">Усі категорії</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <select value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
            <option value="relevance">За релевантністю</option>
            <option value="priceAsc">Ціна: від дешевих</option>
            <option value="priceDesc">Ціна: від дорогих</option>
            <option value="nameAsc">Назва: A-Z</option>
          </select>
          <label className="checkbox-inline">
            <input
              type="checkbox"
              checked={onlyInStock}
              onChange={(e) => setOnlyInStock(e.target.checked)}
            />
            Лише в наявності
          </label>
        </div>
        {error ? <p className="error">{error}</p> : null}
      </div>

      <div className="catalog-layout">
        <aside className="card categories-sidebar">
          <h3>Категорії</h3>
          <div className="category-sidebar-list">
            <button
              className={`sidebar-category-button${!categoryId ? " active" : ""}`}
              type="button"
              onClick={() => setCategoryId("")}
            >
              Усі категорії
            </button>
            {renderCategoryButtons(categories, categoryId, setCategoryId)}
          </div>
        </aside>

        {!error && filteredProducts.length === 0 ? (
          <section className="card">Нічого не знайдено за твоїм запитом.</section>
        ) : (
          <div className="product-grid">
            {filteredProducts.map((product) => (
              <ProductCard
                key={product.id}
                onAdd={add}
                onRemove={remove}
                product={product}
                quantity={items[product.id] ?? 0}
              />
            ))}
          </div>
        )}
      </div>
    </section>
  );
}

function renderCategoryButtons(categories, selectedCategoryId, setCategoryId, depth = 0) {
  return categories.flatMap((category) => [
    <button
      key={category.id}
      className={`sidebar-category-button${selectedCategoryId === category.id ? " active" : ""}`}
      style={{ paddingLeft: `${12 + depth * 18}px` }}
      type="button"
      onClick={() => setCategoryId(category.id)}
    >
      {category.name}
    </button>,
    ...(category.children?.length
      ? renderCategoryButtons(category.children, selectedCategoryId, setCategoryId, depth + 1)
      : [])
  ]);
}

function collectCategoryIds(categories, targetId) {
  for (const category of categories) {
    if (category.id === targetId) {
      return [category.id, ...flattenChildren(category.children ?? [])];
    }

    const nested = collectCategoryIds(category.children ?? [], targetId);
    if (nested.length > 0) {
      return nested;
    }
  }

  return [];
}

function flattenChildren(categories) {
  return categories.flatMap((category) => [category.id, ...flattenChildren(category.children ?? [])]);
}
