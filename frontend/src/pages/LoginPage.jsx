import { Navigate } from "react-router-dom";
import { FaGithub } from "react-icons/fa";
import { useAuth } from "../context/AuthContext";
import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";

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
        <div className="min-h-screen flex items-center justify-center bg-muted/30 px-4">
            <Card className="w-full max-w-md shadow-lg">
                <CardHeader className="text-center">
                    <CardTitle className="text-3xl">
                        Project Dashboard
                    </CardTitle>

                    <CardDescription className="text-base">
                        View project progress and daily reports.
                    </CardDescription>
                </CardHeader>

                <CardContent className="space-y-6">
                    <Button
                        className="w-full h-11"
                        onClick={handleLogin}
                    >
                        <FaGithub className="mr-2 h-5 w-5" />
                        Continue with GitHub
                    </Button>

                    <p className="text-center text-sm text-muted-foreground">
                        Sign in using your company GitHub account.
                    </p>
                </CardContent>
            </Card>
        </div>
    );
}