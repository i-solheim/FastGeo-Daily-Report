import { createContext, useContext, useState, useEffect, useCallback } from "react";
import { getCurrentUser, logoutUser } from "@/lib/authApi";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function loadUser() {
            try {
                const currentUser = await getCurrentUser();
                setUser(currentUser);
            } catch {
                setUser(null);
            } finally {
                setLoading(false);
            }
        }

        loadUser();
    }, []);

    const logout = useCallback(async () => {
    try {
        await logoutUser();
    } finally {
        setUser(null);
    }
}, []);

    const value = {
        user,
        loading,
        isAuthenticated: !!user,
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