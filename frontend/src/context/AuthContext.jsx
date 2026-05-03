import { createContext, useContext, useMemo, useState } from "react";

const AUTH_STORAGE_KEY = "marketplace_auth";
const AuthContext = createContext(null);

function loadAuthState() {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) {
    return { token: "", user: null, expiresAtUtc: "" };
  }

  try {
    return JSON.parse(raw);
  } catch {
    return { token: "", user: null, expiresAtUtc: "" };
  }
}

export function AuthProvider({ children }) {
  const [authState, setAuthState] = useState(loadAuthState);

  function saveAuth(data) {
    const next = {
      token: data?.accessToken ?? "",
      user: data?.user ?? null,
      expiresAtUtc: data?.expiresAtUtc ?? ""
    };
    setAuthState(next);
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(next));
  }

  function logout() {
    const next = { token: "", user: null, expiresAtUtc: "" };
    setAuthState(next);
    localStorage.removeItem(AUTH_STORAGE_KEY);
  }

  function updateUser(user) {
    const next = { ...authState, user: user ?? null };
    setAuthState(next);
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(next));
  }

  const value = useMemo(
    () => ({
      token: authState.token,
      user: authState.user,
      expiresAtUtc: authState.expiresAtUtc,
      isAuthenticated: Boolean(authState.token),
      saveAuth,
      logout,
      updateUser
    }),
    [authState]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider");
  }
  return context;
}
