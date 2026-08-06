import {
    Card,
    CardHeader,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function ProjectCardSkeleton() {
    return (
        <Card>
            <CardHeader>
                <div className="flex items-center gap-3">
                    <Skeleton className="h-6 w-6 rounded" />

                    <div className="space-y-2 flex-1">
                        <Skeleton className="h-5 w-40" />
                        <Skeleton className="h-4 w-28" />
                    </div>
                </div>
            </CardHeader>
        </Card>
    );
}