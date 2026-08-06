import { Button } from "@/components/ui/button";

export default function EmptyState({
    icon: Icon,
    title,
    description,
    actionLabel,
    onAction,
}) {
    return (
        <div className="flex min-h-[50vh] items-center justify-center">
            <div className="max-w-md text-center">
                {Icon && (
                    <Icon className="mx-auto mb-5 h-12 w-12 text-muted-foreground" />
                )}

                <h2 className="text-2xl font-semibold">
                    {title}
                </h2>

                <p className="mt-2 text-muted-foreground">
                    {description}
                </p>

                {actionLabel && onAction && (
                    <Button
                        className="mt-6"
                        onClick={onAction}
                    >
                        {actionLabel}
                    </Button>
                )}
            </div>
        </div>
    );
}