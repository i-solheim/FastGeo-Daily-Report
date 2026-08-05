import { useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

export default function AuthCallbackPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();

    useEffect(() => {
        const token = searchParams.get("token");

        if (!token) {
            navigate("/");
            return;
        }

        localStorage.setItem("token", token);

        navigate("/");
    }, [navigate, searchParams]);

    return <p>Signing you in...</p>;
}