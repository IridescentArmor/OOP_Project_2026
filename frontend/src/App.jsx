import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoute";
import AdminDashboardPage from "./pages/AdminDashboardPage";
import AdminCategoriesPage from "./pages/AdminCategoriesPage";
import AdminOrdersPage from "./pages/AdminOrdersPage";
import AdminProductsPage from "./pages/AdminProductsPage";
import AdminReviewsPage from "./pages/AdminReviewsPage";
import AdminUsersPage from "./pages/AdminUsersPage";
import CatalogPage from "./pages/CatalogPage";
import CheckoutPage from "./pages/CheckoutPage";
import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import ProfilePage from "./pages/ProfilePage";
import RegisterPage from "./pages/RegisterPage";
import SellerDashboardPage from "./pages/SellerDashboardPage";
import SellerOrdersPage from "./pages/SellerOrdersPage";
import SellerProductsPage from "./pages/SellerProductsPage";
import SellerStatsPage from "./pages/SellerStatsPage";
import OrdersPage from "./pages/OrdersPage";
import ProductDetailsPage from "./pages/ProductDetailsPage";

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />} path="/">
        <Route index element={<HomePage />} />
        <Route element={<CatalogPage />} path="catalog" />
        <Route element={<ProductDetailsPage />} path="products/:id" />
        <Route element={<LoginPage />} path="login" />
        <Route element={<RegisterPage />} path="register" />
        <Route element={<CheckoutPage />} path="checkout" />
        <Route
          element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          }
          path="profile"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Customer"]}>
              <OrdersPage />
            </ProtectedRoute>
          }
          path="orders"
        />

        <Route
          element={
            <ProtectedRoute allowedRoles={["Seller"]}>
              <SellerDashboardPage />
            </ProtectedRoute>
          }
          path="seller"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Seller"]}>
              <SellerProductsPage />
            </ProtectedRoute>
          }
          path="seller/products"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Seller"]}>
              <SellerOrdersPage />
            </ProtectedRoute>
          }
          path="seller/orders"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Seller"]}>
              <SellerStatsPage />
            </ProtectedRoute>
          }
          path="seller/stats"
        />

        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminDashboardPage />
            </ProtectedRoute>
          }
          path="admin"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminCategoriesPage />
            </ProtectedRoute>
          }
          path="admin/categories"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminUsersPage />
            </ProtectedRoute>
          }
          path="admin/users"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminReviewsPage />
            </ProtectedRoute>
          }
          path="admin/reviews"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminOrdersPage />
            </ProtectedRoute>
          }
          path="admin/orders"
        />
        <Route
          element={
            <ProtectedRoute allowedRoles={["Admin"]}>
              <AdminProductsPage />
            </ProtectedRoute>
          }
          path="admin/products"
        />
        <Route element={<Navigate replace to="/" />} path="*" />
      </Route>
    </Routes>
  );
}
