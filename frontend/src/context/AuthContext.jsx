import { createContext, useContext, useState, useEffect, useCallback } from "react";
import { getCurrentUser, logoutUser } from "@/lib/authApi";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [status, setStatus] = useState("loading");

    useEffect(() => {
        async function loadUser() {
            try {
                const currentUser = await getCurrentUser();
                setUser(currentUser);
                setStatus("authenticated");
            } catch (err) {
                if (err.status === 401) {
                    setUser(null);
                    setStatus("unauthenticated");
                } else {
                    console.error(err);
                    setStatus("offline");
                    // Backend unavailable or another unexpected error.
                    // Don't log the user out here.
                }
            }
        }

        loadUser();
    }, []);

    const logout = useCallback(async () => {
        try {
            await logoutUser();
        } finally {
            setUser(null);
            setStatus("unauthenticated");
        }
    }, []);

    const value = {
        user,
        status,
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