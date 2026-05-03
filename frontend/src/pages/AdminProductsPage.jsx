import { useEffect, useState } from "react";
import {
  adminDeleteProduct,
  adminExportProductsJson,
  adminGetProducts,
  adminImportProductsJson,
  adminUpdateProduct,
  getCategories
} from "../api";
import { useAuth } from "../context/AuthContext";

function emptyForm() {
  return {
    title: "",
    description: "",
    price: 1,
    stockQuantity: 0,
    categoryId: "",
    sellerId: "",
    imageUrl: ""
  };
}

export default function AdminProductsPage() {
  const { token } = useAuth();
  const [rows, setRows] = useState([]);
  const [categories, setCategories] = useState([]);
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [editingId, setEditingId] = useState("");
  const [form, setForm] = useState(emptyForm);
  const [importFile, setImportFile] = useState(null);

  async function reload() {
    const [productsData, categoriesData] = await Promise.all([adminGetProducts(token), getCategories()]);
    setRows(productsData ?? []);
    setCategories(categoriesData ?? []);
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

  function setField(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function startEdit(row) {
    const p = row.product;
    setEditingId(p.id);
    setForm({
      title: p.title ?? "",
      description: p.description ?? "",
      price: Number(p.price ?? 1),
      stockQuantity: Number(p.stockQuantity ?? 0),
      categoryId: p.categoryId ?? "",
      sellerId: p.sellerId ?? "",
      imageUrl: p.imageUrl ?? ""
    });
    setMessage("");
  }

  function reset() {
    setEditingId("");
    setForm(emptyForm());
  }

  async function onSave(e) {
    e.preventDefault();
    if (!editingId) return;

    setMessage("");
    try {
      const payload = {
        title: form.title,
        description: form.description,
        price: Number(form.price),
        stockQuantity: Number(form.stockQuantity),
        categoryId: form.categoryId,
        sellerId: form.sellerId,
        imageUrl: form.imageUrl
      };
      await adminUpdateProduct(editingId, payload, token);
      await reload();
      reset();
      setMessage("Товар оновлено.");
    } catch (e2) {
      setMessage(e2 instanceof Error ? e2.message : "Помилка оновлення");
    }
  }

  async function onDelete(productId) {
    setMessage("");
    try {
      await adminDeleteProduct(productId, token);
      await reload();
      setMessage("Товар повністю видалено з системи.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка видалення");
    }
  }

  if (isLoading) return <section className="card">Завантаження...</section>;

  async function onExportJson() {
    setMessage("");
    try {
      const blob = await adminExportProductsJson(token);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "products-export.json";
      a.click();
      window.URL.revokeObjectURL(url);
      setMessage("Експорт JSON виконано.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка експорту");
    }
  }

  async function onImportJson() {
    setMessage("");
    if (!importFile) {
      setMessage("Оберіть JSON-файл для імпорту.");
      return;
    }

    try {
      const result = await adminImportProductsJson(importFile, token);
      await reload();
      setImportFile(null);
      setMessage(`Імпорт виконано: ${result?.imported ?? 0} з ${result?.total ?? 0} записів.`);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка імпорту");
    }
  }

  return (
    <section className="stack">
      <div className="card">
        <h2>Товари всіх продавців (Admin)</h2>
        {message ? <p>{message}</p> : null}
        <div className="action-row">
          <button type="button" onClick={onExportJson}>
            Експорт у JSON
          </button>
          <input
            type="file"
            accept=".json,application/json"
            onChange={(e) => setImportFile(e.target.files?.[0] ?? null)}
          />
          <button className="secondary-button" type="button" onClick={onImportJson}>
            Імпорт з JSON
          </button>
        </div>
        {editingId ? (
          <form className="form" onSubmit={onSave}>
            <label>
              Назва
              <input value={form.title} onChange={(e) => setField("title", e.target.value)} required />
            </label>
            <label>
              Опис
              <input
                value={form.description}
                onChange={(e) => setField("description", e.target.value)}
              />
            </label>
            <label>
              Ціна
              <input
                type="number"
                min="1"
                step="0.01"
                value={form.price}
                onChange={(e) => setField("price", e.target.value)}
                required
              />
            </label>
            <label>
              Залишок
              <input
                type="number"
                min="0"
                step="1"
                value={form.stockQuantity}
                onChange={(e) => setField("stockQuantity", e.target.value)}
                required
              />
            </label>
            <label>
              Категорія
              <select
                value={form.categoryId}
                onChange={(e) => setField("categoryId", e.target.value)}
                required
              >
                <option value="">-- обери --</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              URL зображення
              <input value={form.imageUrl} onChange={(e) => setField("imageUrl", e.target.value)} />
            </label>
            <div className="qty-controls">
              <button type="submit">Зберегти</button>
              <button className="link-button" type="button" onClick={reset}>
                Скасувати
              </button>
            </div>
          </form>
        ) : (
          <p className="muted">Натисни "Редагувати" біля товару, щоб змінити дані.</p>
        )}
      </div>

      <div className="card">
        <ul className="list">
          {rows.map((row) => (
            <li className="list-item" key={row.product.id}>
              <strong>{row.product.title}</strong>
              <div className="muted">
                Продавець: {row.sellerName} | Ціна: {row.product.price} $ | Залишок:{" "}
                {row.product.stockQuantity} | Активний: {String(row.product.isActive)}
              </div>
              <div className="muted">
                Рейтинг: {Number(row.product.averageRating ?? 0).toFixed(1)} / 5 (
                {row.product.reviewsCount ?? 0} відгуків)
              </div>
              <div className="qty-controls">
                <button type="button" onClick={() => startEdit(row)}>
                  Редагувати
                </button>
                <button className="danger" type="button" onClick={() => onDelete(row.product.id)}>
                  Видалити
                </button>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
