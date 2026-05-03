import { createContext, useContext, useMemo, useState } from "react";

const CART_STORAGE_KEY = "marketplace_cart";
const CartContext = createContext(null);

function loadCart() {
  const raw = localStorage.getItem(CART_STORAGE_KEY);
  if (!raw) {
    return {};
  }

  try {
    return JSON.parse(raw);
  } catch {
    return {};
  }
}

export function CartProvider({ children }) {
  const [items, setItems] = useState(loadCart);

  function persist(next) {
    setItems(next);
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(next));
  }

  function add(productId) {
    const next = {
      ...items,
      [productId]: (items[productId] ?? 0) + 1
    };
    persist(next);
  }

  function remove(productId) {
    const current = items[productId] ?? 0;
    if (current <= 1) {
      const { [productId]: _, ...rest } = items;
      persist(rest);
      return;
    }

    persist({
      ...items,
      [productId]: current - 1
    });
  }

  function clear() {
    persist({});
  }

  const value = useMemo(
    () => ({
      items,
      add,
      remove,
      clear
    }),
    [items]
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart must be used inside CartProvider");
  }
  return context;
}
