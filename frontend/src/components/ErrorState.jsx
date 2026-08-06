import { AlertTriangle, RefreshCw } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function ErrorState({
    title = "Something went wrong",
    description = "We couldn't load the requested data.",
    onRetry,
}) {
    return (
        <div className="flex min-h-[50vh] flex-col items-center justify-center text-center px-6">
            <div className="mb-4 rounded-full bg-red-100 p-4">
                <AlertTriangle className="h-8 w-8 text-red-600" />
            </div>

            <h2 className="text-xl font-semibold">
                {title}
            </h2>

            <p className="mt-2 max-w-md text-muted-foreground">
                {description}
            </p>

            {onRetry && (
                <Button
                    className="mt-6"
                    variant="outline"
                    onClick={onRetry}
                >
                    <RefreshCw className="mr-2 h-4 w-4" />
                    Try again
                </Button>
            )}
        </div>
    );
}