import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { createOrderWithDelivery, getProducts } from "../api";
import { useAuth } from "../context/AuthContext";
import { useCart } from "../context/CartContext";
import { validatePersonName, validatePhoneNumber } from "../utils/formHelpers";

export default function CheckoutPage() {
  const { token, isAuthenticated, user } = useAuth();
  const { items, add, remove, clear } = useCart();
  const [products, setProducts] = useState([]);
  const [message, setMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [recipientName, setRecipientName] = useState("");
  const [recipientPhone, setRecipientPhone] = useState("+380");
  const [shippingAddress, setShippingAddress] = useState("");
  const nameError = recipientName ? validatePersonName(recipientName) : "";
  const phoneError = recipientPhone ? validatePhoneNumber(recipientPhone) : "";

  useEffect(() => {
    getProducts().then((data) => setProducts(data ?? []));
  }, []);

  useEffect(() => {
    if (!isAuthenticated || !user) return;
    setRecipientName((v) => v || user.name || "");
    setRecipientPhone((v) => (v === "+380" || !v ? user.phoneNumber || "+380" : v));
  }, [isAuthenticated, user]);

  const cartRows = useMemo(() => {
    return products
      .filter((product) => (items[product.id] ?? 0) > 0)
      .map((product) => {
        const qty = items[product.id];
        return {
          product,
          qty,
          subtotal: qty * Number(product.price)
        };
      });
  }, [items, products]);

  const total = cartRows.reduce((sum, row) => sum + row.subtotal, 0);

  async function submitOrder() {
    if (!isAuthenticated) {
      setMessage("Щоб оформити замовлення, спочатку увійди.");
      return;
    }

    if (Object.keys(items).length === 0) {
      setMessage("Кошик порожній.");
      return;
    }

    if (nameError) {
      setMessage(nameError);
      return;
    }

    if (phoneError) {
      setMessage(phoneError);
      return;
    }

    if (!shippingAddress.trim() || shippingAddress.trim().length < 5) {
      setMessage("Вкажи адресу доставки (мін. 5 символів).");
      return;
    }

    setIsSubmitting(true);
    setMessage("");

    try {
      const result = await createOrderWithDelivery(
        {
          items,
          recipientName,
          recipientPhone,
          shippingAddress
        },
        token
      );
      clear();
      setMessage(
        `Замовлення створено! № ${result.id}, статус: ${result.status}, сума: ${result.total}`
      );
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Не вдалося створити замовлення");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="card">
      <h2>Кошик та оформлення</h2>
      {cartRows.length === 0 ? (
        <p>
          Кошик порожній. Перейди у <Link to="/catalog">каталог</Link>, щоб
          додати товари.
        </p>
      ) : (
        <>
          <div className="card subtle">
            <h3>Дані для доставки</h3>
            <div className="form">
              <label>
                Отримувач
                <input
                  value={recipientName}
                  onChange={(e) => setRecipientName(e.target.value)}
                  required
                />
              </label>
              {nameError ? <p className="error form-hint">{nameError}</p> : null}
              <label>
                Телефон
                <input
                  value={recipientPhone}
                  onChange={(e) => setRecipientPhone(e.target.value)}
                  required
                />
              </label>
              {phoneError ? <p className="error form-hint">{phoneError}</p> : null}
              <label>
                Адреса доставки
                <input
                  value={shippingAddress}
                  onChange={(e) => setShippingAddress(e.target.value)}
                  required
                />
              </label>
            </div>
          </div>

          <ul className="list">
            {cartRows.map((row) => (
              <li className="list-item" key={row.product.id}>
                <div>
                  <strong>{row.product.title}</strong> — {row.product.price} $ x{" "}
                  {row.qty} = {row.subtotal} $
                </div>
                <div className="qty-controls">
                  <button className="icon-button" onClick={() => remove(row.product.id)} type="button">
                    -
                  </button>
                  <button className="icon-button" onClick={() => add(row.product.id)} type="button">
                    +
                  </button>
                </div>
              </li>
            ))}
          </ul>
          <p>
            <strong>Загалом: {total} $</strong>
          </p>
        </>
      )}

      <button disabled={isSubmitting} onClick={submitOrder} type="button">
        {isSubmitting ? "Оформлюємо..." : "Оформити замовлення"}
      </button>
      {message ? <p>{message}</p> : null}
    </section>
  );
}
