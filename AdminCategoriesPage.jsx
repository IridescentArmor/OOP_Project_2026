import { useEffect, useState } from "react";
import { adminCreateCategory, adminDeleteCategory, adminUpdateCategory, getCategories } from "../api";
import { useAuth } from "../context/AuthContext";

export default function AdminCategoriesPage() {
  const { token } = useAuth();
  const [categories, setCategories] = useState([]);
  const [name, setName] = useState("");
  const [parentCategoryId, setParentCategoryId] = useState("");
  const [editingId, setEditingId] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);

  async function reload() {
    const data = await getCategories();
    setCategories(data ?? []);
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

  function startEdit(c) {
    setEditingId(c.id);
    setName(c.name ?? "");
    setParentCategoryId(c.parentCategoryId ?? "");
    setMessage("");
  }

  function reset() {
    setEditingId("");
    setName("");
    setParentCategoryId("");
  }

  const flatCategories = flattenCategories(categories);

  async function onSubmit(e) {
    e.preventDefault();
    setMessage("");
    try {
      const payload = { name, parentCategoryId: parentCategoryId || null };
      if (editingId) {
        await adminUpdateCategory(editingId, payload, token);
        setMessage("Оновлено");
      } else {
        await adminCreateCategory(payload, token);
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
      await adminDeleteCategory(id, token);
      await reload();
      setMessage("Видалено");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Помилка");
    }
  }

  if (isLoading) return <section className="card">Завантаження...</section>;

  return (
    <section className="stack">
      <div className="card">
        <h2>Категорії (Admin)</h2>
        {message ? <p>{message}</p> : null}
        <form className="form" onSubmit={onSubmit}>
          <label>
            Назва
            <input value={name} onChange={(e) => setName(e.target.value)} required />
          </label>
          <label>
            Батьківська категорія
            <select value={parentCategoryId} onChange={(e) => setParentCategoryId(e.target.value)}>
              <option value="">Без батьківської категорії</option>
              {flatCategories
                .filter((c) => c.id !== editingId)
                .map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.label}
                  </option>
                ))}
            </select>
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
        {categories.length === 0 ? (
          <p className="muted">Категорій поки що немає.</p>
        ) : (
          <ul className="list">
            {renderCategoryTree(categories, startEdit, onDelete)}
          </ul>
        )}
      </div>
    </section>
  );
}

function renderCategoryTree(categories, onEdit, onDelete, depth = 0) {
  return categories.map((category) => (
    <li key={category.id} className="list-item">
      <div className="row">
        <div>
          <strong>{"- ".repeat(depth)}{category.name}</strong>
          {category.parentCategoryId ? <div className="muted">Підкатегорія</div> : null}
        </div>
        <div className="action-row">
          <button type="button" onClick={() => onEdit(category)}>
            Редагувати
          </button>
          <button type="button" onClick={() => onDelete(category.id)}>
            Видалити
          </button>
        </div>
      </div>
      {category.children?.length ? (
        <ul className="list">{renderCategoryTree(category.children, onEdit, onDelete, depth + 1)}</ul>
      ) : null}
    </li>
  ));
}

function flattenCategories(categories, depth = 0) {
  return categories.flatMap((category) => [
    { id: category.id, label: `${"— ".repeat(depth)}${category.name}` },
    ...flattenCategories(category.children ?? [], depth + 1)
  ]);
}

