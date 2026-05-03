import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProtectedRoute({ children, allowedRoles }) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate replace to="/login" />;
  }

  if (Array.isArray(allowedRoles) && allowedRoles.length > 0) {
    const roleNames = user?.roleNames ?? [];
    const hasAny = allowedRoles.some((r) => roleNames.includes(r));
    if (!hasAny) {
      return <Navigate replace to="/profile" />;
    }
  }

  return children;
}
