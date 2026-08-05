import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function AuthCallbackPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const { login } = useAuth();

    useEffect(() => {
        const token = searchParams.get("token");

        if (!token) {
            navigate("/");
            return;
        }

        login(token);

        navigate("/", { replace: true });
    }, [login, navigate, searchParams]);

    return <p>Signing you in...</p>;
}