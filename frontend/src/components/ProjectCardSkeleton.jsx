import {
    Card,
    CardHeader,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

export default function ProjectCardSkeleton() {
    return (
        <Card>
            <CardHeader className="space-y-2">
                <Skeleton className="h-6 w-40" />
            </CardHeader>
        </Card>
    );
}