import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const API_URL = import.meta.env.VITE_API_URL;

export default function LoginPage() {
    const { isAuthenticated } = useAuth();

    if (isAuthenticated) {
        return <Navigate to="/" replace />;
    }

    function handleLogin() {
        window.location.href = `${API_URL}/auth/github/login`;
    }

    return (
        <div className="min-h-screen flex items-center justify-center">
            <button onClick={handleLogin}>
                Login with GitHub
            </button>
        </div>
    );
}