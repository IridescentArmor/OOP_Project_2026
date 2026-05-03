const defaultHeaders = {
  "Content-Type": "application/json"
};

function extractValidationMessage(data) {
  const validationErrors = data?.errors;
  if (!validationErrors || typeof validationErrors !== "object") {
    return "";
  }

  const firstEntry = Object.values(validationErrors)[0];
  if (!Array.isArray(firstEntry) || firstEntry.length === 0) {
    return "";
  }

  return String(firstEntry[0]);
}

async function parseResponse(response) {
  const rawText = await response.text();
  let data = null;
  if (rawText) {
    data = JSON.parse(rawText);
  }

  if (!response.ok) {
    const message =
      extractValidationMessage(data) ||
      data?.detail ||
      data?.message ||
      data?.title ||
      (rawText && rawText.length < 240 ? rawText : "") ||
      `HTTP ${response.status}: запит не виконано`;
    throw new Error(message);
  }

  return data;
}

async function request(url, options) {
  try {
    const response = await fetch(url, options);
    return await parseResponse(response);
  } catch (error) {
    if (error instanceof SyntaxError) {
      throw new Error("Сервер повернув некоректну відповідь.");
    }

    if (error instanceof TypeError) {
      throw new Error(
        "Немає з'єднання з API. Перевір, що бекенд запущений на http://localhost:5077."
      );
    }

    throw error;
  }
}

export async function getProducts() {
  return request("/api/products");
}

export async function getProductById(id) {
  return request(`/api/products/${id}`);
}

export async function getProductReviews(productId, token) {
  return request(`/api/products/${productId}/reviews`, {
    method: "GET",
    headers: token
      ? {
          ...defaultHeaders,
          Authorization: `Bearer ${token}`
        }
      : defaultHeaders
  });
}

export async function createProductReview(productId, payload, token) {
  return request(`/api/products/${productId}/reviews`, {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function getCategories() {
  return request("/api/categories");
}

export async function login(email, password) {
  return request("/api/auth/login", {
    method: "POST",
    headers: defaultHeaders,
    body: JSON.stringify({ email, password })
  });
}

export async function register(payload) {
  return request("/api/auth/register", {
    method: "POST",
    headers: defaultHeaders,
    body: JSON.stringify(payload)
  });
}

export async function createOrder(itemsMap, token) {
  throw new Error("createOrder: use createOrderWithDelivery(payload, token) instead");
}

export async function createOrderWithDelivery(payload, token) {
  return request("/api/orders", {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function getMyOrders(token) {
  return request("/api/orders/my", {
    method: "GET",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function cancelMyOrder(orderId, token) {
  return request(`/api/orders/${orderId}/cancel`, {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function getSellerOrders(token) {
  return request("/api/orders/seller", {
    method: "GET",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function getSellerStats(token, periodDays = 7) {
  return request(`/api/orders/seller/stats?periodDays=${periodDays}`, {
    method: "GET",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function changeSellerOrderStatus(orderId, payload, token) {
  return request(`/api/orders/${orderId}/status`, {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function createMyProduct(payload, token) {
  return request("/api/products/my", {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function updateMyProduct(productId, payload, token) {
  return request(`/api/products/my/${productId}`, {
    method: "PUT",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function deleteMyProduct(productId, token) {
  return request(`/api/products/my/${productId}`, {
    method: "DELETE",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function adminCreateCategory(payload, token) {
  return request("/api/categories", {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function adminUpdateCategory(categoryId, payload, token) {
  return request(`/api/categories/${categoryId}`, {
    method: "PUT",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function adminDeleteCategory(categoryId, token) {
  return request(`/api/categories/${categoryId}`, {
    method: "DELETE",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function updateMyProfile(payload, token) {
  return request("/api/users/me", {
    method: "PUT",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function changeMyPassword(payload, token) {
  return request("/api/users/me/change-password", {
    method: "POST",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });
}

export async function deleteMyAccount(token) {
  return request("/api/users/me", {
    method: "DELETE",
    headers: {
      ...defaultHeaders,
      Authorization: `Bearer ${token}`
    }
  });
}

export async function adminGetUsers(token) {
  return request("/api/admin/users", {
    method: "GET",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminApproveSeller(userId, token) {
  return request(`/api/admin/sellers/${userId}/approve`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminUpdateUser(userId, payload, token) {
  return request(`/api/admin/users/${userId}`, {
    method: "PUT",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify(payload)
  });
}

export async function adminBlockUser(userId, reason, token) {
  return request(`/api/admin/users/${userId}/block`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify({ reason })
  });
}

export async function adminUnblockUser(userId, token) {
  return request(`/api/admin/users/${userId}/unblock`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminDeleteUser(userId, token) {
  return request(`/api/admin/users/${userId}`, {
    method: "DELETE",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminGrantAdminRole(userId, accessLevel, token) {
  return request(`/api/admin/users/${userId}/grant-admin`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify({ accessLevel })
  });
}

export async function adminRevokeAdminRole(userId, token) {
  return request(`/api/admin/users/${userId}/revoke-admin`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminGetReviews(token) {
  return request("/api/admin/reviews", {
    method: "GET",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminUnhideReview(productId, reviewId, token) {
  return request(`/api/products/${productId}/reviews/${reviewId}/unhide`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminHideReview(productId, reviewId, reason, token) {
  return request(`/api/products/${productId}/reviews/${reviewId}/hide`, {
    method: "POST",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify({ reason })
  });
}

export async function adminDeleteReview(productId, reviewId, reason, token) {
  return request(`/api/products/${productId}/reviews/${reviewId}`, {
    method: "DELETE",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify({ reason })
  });
}

export async function adminGetOrders(token) {
  return request("/api/admin/orders", {
    method: "GET",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminDeleteOrder(orderId, token) {
  return request(`/api/admin/orders/${orderId}`, {
    method: "DELETE",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminGetProducts(token) {
  return request("/api/admin/products", {
    method: "GET",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminUpdateProduct(productId, payload, token) {
  return request(`/api/admin/products/${productId}`, {
    method: "PUT",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` },
    body: JSON.stringify(payload)
  });
}

export async function adminDeleteProduct(productId, token) {
  return request(`/api/admin/products/${productId}`, {
    method: "DELETE",
    headers: { ...defaultHeaders, Authorization: `Bearer ${token}` }
  });
}

export async function adminExportProductsJson(token) {
  const response = await fetch("/api/admin/products/export-json", {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error(`Не вдалося експортувати JSON (HTTP ${response.status})`);
  }

  return response.blob();
}

export async function adminImportProductsJson(file, token) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch("/api/admin/products/import-json", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`
    },
    body: formData
  });

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;
  if (!response.ok) {
    throw new Error(data?.detail || data?.title || "Не вдалося імпортувати JSON");
  }

  return data;
}
