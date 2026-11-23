import { useEffect, useMemo, useState } from "react";
import { createMyProduct, deleteMyProduct, getCategories, getProducts, updateMyProduct } from "../api";
import { useAuth } from "../context/AuthContext";

function emptyForm() {
  return {
    title: "",
    description: "",
    price: 1,
    stockQuantity: 0,
    categoryId: "",
    imageUrl: ""
  };
}

export default function SellerProductsPage() {
  const { token, user } = useAuth();
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  const myUserId = user?.id ?? "";

  async function reload() {
    const [p, c] = await Promise.all([getProducts(), getCategories()]);
    setProducts(p ?? []);
    setCategories(c ?? []);
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

  const myProducts = useMemo(() => {
    return (products ?? []).filter((p) => !myUserId || p.sellerId === myUserId);
  }, [products, myUserId]);

  function setField(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function startEdit(p) {
    setEditingId(p.id);
    setForm({
      title: p.title ?? "",
      description: p.description ?? "",
      price: Number(p.price ?? 1),
      stockQuantity: Number(p.stockQuantity ?? 0),
      categoryId: p.categoryId ?? ""
      ,
      imageUrl: p.imageUrl ?? ""
    });
    setMessage("");
  }

  function reset() {
    setEditingId("");
    setForm(emptyForm());
  }

  async function onSubmit(e) {
    e.preventDefault();
    setMessage("");
    try {
      if (!form.categoryId) {
        setMessage("Обери категорію");
        return;
      }

      const payload = {
        title: form.title,
        description: form.description,
        price: Number(form.price),
        stockQuantity: Number(form.stockQuantity),
        categoryId: form.categoryId
        ,
        imageUrl: form.imageUrl
      };

      if (editingId) {
        await updateMyProduct(editingId, payload, token);
        setMessage("Оновлено");
      } else {
        await createMyProduct(payload, token);
        setMessage("Створено");
      }

      await reload();
      reset();
    } catch (e2) {
      setMessage(e2 instanceof Error ? e2.message : "Помилка");
    }
  }

  async function onDelete(id) {
    setMessage("");
    try {
      await deleteMyProduct(id, token);
      await reload();
      setMessage("Товар повністю видалено з системи.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка");
    }
  }

  if (isLoading) return <section className="card">Завантаження...</section>;

  return (
    <section className="stack">
      <div className="card">
        <h2>Мої товари</h2>
        {message ? <p>{message}</p> : null}

        <form className="form" onSubmit={onSubmit}>
          <label>
            Назва
            <input value={form.title} onChange={(e) => setField("title", e.target.value)} required />
          </label>
          <label>
            Опис
            <input value={form.description} onChange={(e) => setField("description", e.target.value)} />
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
            <select value={form.categoryId} onChange={(e) => setField("categoryId", e.target.value)} required>
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

          <button type="submit">{editingId ? "Зберегти" : "Створити"}</button>
          {editingId ? (
            <button className="link-button" type="button" onClick={reset}>
              Скасувати
            </button>
          ) : null}
        </form>
      </div>

      <div className="card">
        <h3>Список</h3>
        {myProducts.length === 0 ? (
          <p className="muted">Поки що немає товарів.</p>
        ) : (
          <ul className="list">
            {myProducts.map((p) => (
              <li key={p.id} className="list-item">
                <strong>{p.title}</strong>
                <div className="muted">
                  Ціна: {p.price} | Залишок: {p.stockQuantity} | Активний: {String(p.isActive)}
                </div>
                <div className="qty-controls">
                  <button type="button" onClick={() => startEdit(p)}>
                    Редагувати
                  </button>
                  <button type="button" onClick={() => onDelete(p.id)}>
                    Видалити
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}

