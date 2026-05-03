export function documentTitleForPath(pathname) {
  let p = pathname || "/";
  if (p.length > 1 && p.endsWith("/")) {
    p = p.slice(0, -1);
  }

  const exact = {
    "/": "Головна",
    "/catalog": "Каталог",
    "/checkout": "Кошик та оформлення",
    "/login": "Вхід",
    "/register": "Реєстрація",
    "/profile": "Мій кабінет",
    "/orders": "Мої замовлення",
    "/seller": "Кабінет продавця",
    "/seller/products": "Мої товари",
    "/seller/orders": "Замовлення продавця",
    "/seller/stats": "Статистика продажів",
    "/admin": "Адмін-панель",
    "/admin/categories": "Категорії",
    "/admin/users": "Користувачі",
    "/admin/orders": "Замовлення (адмін)",
    "/admin/products": "Товари (адмін)",
    "/admin/reviews": "Модерація відгуків",
  };

  const page = exact[p];
  if (page) {
    return `${page} — MarketplaceOOP`;
  }
  if (p.startsWith("/products/")) {
    return `Товар — MarketplaceOOP`;
  }
  return "MarketplaceOOP";
}
