import { createContext, useContext, useState, useCallback } from "react";
import { jwtDecode } from "jwt-decode";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [token, setToken] = useState(
        () => localStorage.getItem("token")
    );

    const [user, setUser] = useState(
        () => parseUser(localStorage.getItem("token"))
    );

    const login = useCallback((newToken) => {
        localStorage.setItem("token", newToken);
        setToken(newToken);
        setUser(parseUser(newToken));
    }, []);

    const logout = useCallback(() => {
        localStorage.removeItem("token");
        setToken(null);
        setUser(null);
    }, []);

    const value = {
        token,
        user,
        isAuthenticated: !!token,
        login,
        logout,
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error("useAuth must be used inside an AuthProvider");
    }

    return context;
}

function parseUser(token) {
    if (!token) return null;

    const claims = jwtDecode(token);

    return {
        id: claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
        username: claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        role: claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    };
}