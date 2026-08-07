import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import PageLoader from "@/components/PageLoader";
import ErrorState from "@/components/ErrorState";

export default function ProtectedRoute({ children }) {
    const { status } = useAuth();

    if (status === "loading") {
        return <PageLoader />;
    }

    if (status === "unauthenticated") {
        return <Navigate to="/login" replace />;
    }

    if (status === "offline") {
        return (
            <div className="flex min-h-screen items-center justify-center">
                <ErrorState
                    title="Server unavailable"
                    description="Can't connect to the backend."
                    onRetry={() => window.location.reload()}
                />
            </div>
        );
    }

    return children;
}