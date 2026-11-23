import { useEffect } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useCart } from "../context/CartContext";
import { documentTitleForPath } from "../utils/pageTitle";

function navClassName({ isActive }) {
  return `nav-link${isActive ? " active" : ""}`;
}

export default function Layout() {
  const location = useLocation();
  const { isAuthenticated, logout, user } = useAuth();
  const { items } = useCart();

  useEffect(() => {
    document.title = documentTitleForPath(location.pathname);
  }, [location.pathname]);
  const totalItems = Object.values(items).reduce((sum, count) => sum + count, 0);
  const roles = user?.roleNames ?? [];
  const isCustomer = roles.includes("Customer");
  const isSeller = roles.includes("Seller");
  const isAdmin = roles.includes("Admin");
  const adminOnly = isAdmin && !isCustomer && !isSeller;

  return (
    <div className="app-shell">
      <header className="header">
        <h1>MarketplaceOOP</h1>
        <nav className="nav">
          {!adminOnly ? (
            <>
              <NavLink className={navClassName} to="/">
                Головна
              </NavLink>
              <NavLink className={navClassName} to="/catalog">
                Каталог
              </NavLink>
              <NavLink className={navClassName} to="/checkout">
                Кошик ({totalItems})
              </NavLink>
            </>
          ) : null}
          <NavLink className={navClassName} to="/profile">
            Мій кабінет
          </NavLink>
          {isAuthenticated && isCustomer ? (
            <NavLink className={navClassName} to="/orders">
              Замовлення
            </NavLink>
          ) : null}
          {isAuthenticated && isSeller ? (
            <NavLink className={navClassName} to="/seller">
              Кабінет продавця
            </NavLink>
          ) : null}
          {isAuthenticated && isAdmin ? (
            <NavLink className={navClassName} to="/admin">
              Адмін панель
            </NavLink>
          ) : null}
          {isAuthenticated ? (
            <button className="link-button" onClick={logout} type="button">
              Вийти
            </button>
          ) : (
            <>
              <NavLink className={navClassName} to="/login">
                Увійти
              </NavLink>
            </>
          )}
        </nav>
      </header>
      {isAuthenticated && user?.isBlocked ? (
        <div className="card">
          <p className="error">
            Акаунт заблоковано. Причина: {user.blockReason || "не вказано"}.
          </p>
        </div>
      ) : null}
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
