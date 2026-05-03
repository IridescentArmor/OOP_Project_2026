import { useEffect, useState } from "react";
import {
  adminApproveSeller,
  adminBlockUser,
  adminDeleteUser,
  adminGrantAdminRole,
  adminGetUsers,
  adminRevokeAdminRole,
  adminUnblockUser,
  adminUpdateUser
} from "../api";
import { useAuth } from "../context/AuthContext";
import { validatePersonName, validatePhoneNumber } from "../utils/formHelpers";

export default function AdminUsersPage() {
  const { token, user: currentUser } = useAuth();
  const [users, setUsers] = useState([]);
  const [message, setMessage] = useState("");
  const [editingUserId, setEditingUserId] = useState("");
  const [editForm, setEditForm] = useState({ name: "", email: "", phoneNumber: "" });

  async function reload() {
    const data = await adminGetUsers(token);
    setUsers(data ?? []);
  }

  useEffect(() => {
    reload().catch((e) => setMessage(e instanceof Error ? e.message : "Помилка"));
  }, []);

  function beginEdit(user) {
    setEditingUserId(user.id);
    setEditForm({
      name: user.name ?? "",
      email: user.email ?? "",
      phoneNumber: user.phoneNumber ?? ""
    });
    setMessage("");
  }

  function cancelEdit() {
    setEditingUserId("");
    setEditForm({ name: "", email: "", phoneNumber: "" });
  }

  const nameError = editingUserId && editForm.name ? validatePersonName(editForm.name) : "";
  const phoneError =
    editingUserId && editForm.phoneNumber ? validatePhoneNumber(editForm.phoneNumber) : "";
  const isSuperAdmin = Number(currentUser?.adminAccessLevel ?? 0) >= 100;

  return (
    <section className="stack">
      <div className="card">
        <h2>Користувачі (Admin)</h2>
        <p className="muted">
          Тут можна редагувати контактні дані користувача, схвалювати продавців і керувати
          блокуванням.
        </p>
        {message ? <p>{message}</p> : null}
      </div>

      {users.map((u) => {
        const roles = u.roleNames ?? [];
        const isSeller = roles.includes("Seller");
        const isAdmin = roles.includes("Admin");
        const isTargetSuperAdmin = Number(u.adminAccessLevel ?? 0) >= 100;
        const canManageTarget = !isTargetSuperAdmin || currentUser?.id === u.id;
        const isEditing = editingUserId === u.id;

        return (
          <article key={u.id} className="card">
            <div className="row">
              <div>
                <h3>{u.name}</h3>
                <p className="muted">
                  {u.email} | {u.phoneNumber}
                </p>
                <p className="muted">Ролі: {roles.join(", ")}</p>
                {isTargetSuperAdmin ? <p className="muted">Рівень: Super Admin</p> : null}
                {typeof u.isSellerApproved === "boolean" ? (
                  <p className="muted">
                    Продавець: {u.isSellerApproved ? "схвалено" : "очікує модерації"}
                  </p>
                ) : null}
                {u.isBlocked ? (
                  <p className="error">Заблоковано: {u.blockReason || "без причини"}</p>
                ) : null}
              </div>
              <span className={`status-badge${u.isBlocked ? " danger" : ""}`}>
                {u.isBlocked ? "Blocked" : "Active"}
              </span>
            </div>

            {isEditing ? (
              <form
                className="form"
                onSubmit={async (e) => {
                  e.preventDefault();
                  if (nameError || phoneError) {
                    setMessage(nameError || phoneError);
                    return;
                  }

                  try {
                    await adminUpdateUser(u.id, editForm, token);
                    await reload();
                    cancelEdit();
                    setMessage("Дані користувача оновлено.");
                  } catch (e2) {
                    setMessage(e2 instanceof Error ? e2.message : "Помилка");
                  }
                }}
              >
                <label>
                  Ім&apos;я
                  <input
                    value={editForm.name}
                    onChange={(e) => setEditForm((prev) => ({ ...prev, name: e.target.value }))}
                  />
                </label>
                {nameError ? <p className="error form-hint">{nameError}</p> : null}
                <label>
                  Email
                  <input
                    type="email"
                    value={editForm.email}
                    onChange={(e) => setEditForm((prev) => ({ ...prev, email: e.target.value }))}
                  />
                </label>
                <label>
                  Телефон
                  <input
                    value={editForm.phoneNumber}
                    onChange={(e) =>
                      setEditForm((prev) => ({ ...prev, phoneNumber: e.target.value }))
                    }
                  />
                </label>
                {phoneError ? <p className="error form-hint">{phoneError}</p> : null}
                <div className="action-row">
                  <button type="submit">Зберегти</button>
                  <button className="secondary-button" type="button" onClick={cancelEdit}>
                    Скасувати
                  </button>
                </div>
              </form>
            ) : null}

            <div className="action-row">
              {canManageTarget ? (
                <button className="secondary-button" type="button" onClick={() => beginEdit(u)}>
                  Редагувати
                </button>
              ) : null}
              {canManageTarget && isSeller && !u.isSellerApproved ? (
                <button
                  type="button"
                  onClick={async () => {
                    try {
                      await adminApproveSeller(u.id, token);
                      await reload();
                      setMessage("Продавця схвалено.");
                    } catch (e) {
                      setMessage(e instanceof Error ? e.message : "Помилка");
                    }
                  }}
                >
                  Approve Seller
                </button>
              ) : null}
              {canManageTarget && !u.isBlocked ? (
                <InlineReasonButton
                  buttonClassName="danger"
                  buttonLabel="Block"
                  onSubmit={async (reason) => {
                    await adminBlockUser(u.id, reason, token);
                    await reload();
                    setMessage("Користувача заблоковано.");
                  }}
                  setMessage={setMessage}
                />
              ) : canManageTarget ? (
                <button
                  className="secondary-button"
                  type="button"
                  onClick={async () => {
                    try {
                      await adminUnblockUser(u.id, token);
                      await reload();
                      setMessage("Користувача розблоковано.");
                    } catch (e) {
                      setMessage(e instanceof Error ? e.message : "Помилка");
                    }
                  }}
                >
                  Unblock
                </button>
              ) : null}
              {canManageTarget ? (
                <button
                  className="danger"
                  type="button"
                  onClick={async () => {
                    if (!window.confirm("Видалити користувача повністю з системи?")) return;
                    try {
                      await adminDeleteUser(u.id, token);
                      await reload();
                      setMessage("Користувача повністю видалено.");
                    } catch (e) {
                      setMessage(e instanceof Error ? e.message : "Помилка");
                    }
                  }}
                >
                  Delete
                </button>
              ) : null}
              {isSuperAdmin ? (
                <button
                  className="secondary-button"
                  type="button"
                  onClick={async () => {
                    const raw = window.prompt("Вкажи рівень адмін-доступу (1..100):", "10");
                    if (raw == null) return;
                    const parsedLevel = Number(raw);
                    if (!Number.isInteger(parsedLevel) || parsedLevel < 1 || parsedLevel > 100) {
                      setMessage("Рівень доступу має бути цілим числом у межах 1..100.");
                      return;
                    }

                    try {
                      await adminGrantAdminRole(u.id, parsedLevel, token);
                      await reload();
                      setMessage("Адмін-права успішно видано.");
                    } catch (e) {
                      setMessage(e instanceof Error ? e.message : "Помилка");
                    }
                  }}
                >
                  Grant Admin
                </button>
              ) : null}
              {isSuperAdmin && isAdmin && !isTargetSuperAdmin ? (
                <button
                  className="secondary-button"
                  type="button"
                  onClick={async () => {
                    if (!window.confirm("Забрати роль Admin у цього користувача?")) return;
                    try {
                      await adminRevokeAdminRole(u.id, token);
                      await reload();
                      setMessage("Адмін-права забрано.");
                    } catch (e) {
                      setMessage(e instanceof Error ? e.message : "Помилка");
                    }
                  }}
                >
                  Revoke Admin
                </button>
              ) : null}
            </div>
          </article>
        );
      })}
    </section>
  );
}

function InlineReasonButton({ buttonClassName, buttonLabel, onSubmit, setMessage }) {
  const [isOpen, setIsOpen] = useState(false);
  const [reason, setReason] = useState("");

  return isOpen ? (
    <div className="inline-reason-box">
      <input
        value={reason}
        onChange={(e) => setReason(e.target.value)}
        placeholder="Вкажи причину"
      />
      <button
        className={buttonClassName}
        type="button"
        onClick={async () => {
          if (!reason.trim()) {
            setMessage("Причина обов'язкова.");
            return;
          }

          try {
            await onSubmit(reason.trim());
            setReason("");
            setIsOpen(false);
          } catch (e) {
            setMessage(e instanceof Error ? e.message : "Помилка");
          }
        }}
      >
        Підтвердити
      </button>
      <button className="secondary-button" type="button" onClick={() => setIsOpen(false)}>
        Скасувати
      </button>
    </div>
  ) : (
    <button className={buttonClassName} type="button" onClick={() => setIsOpen(true)}>
      {buttonLabel}
    </button>
  );
}

