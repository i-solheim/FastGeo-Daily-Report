import { Skeleton } from "@/components/ui/skeleton";

export default function ReportHeaderSkeleton() {
    return (
        <div className="mb-6 mt-16">
            <div>
                <Skeleton className="h-9 w-56" />

                <div className="mt-2">
                    <Skeleton className="h-4 w-40" />
                </div>

                <div className="mt-2">
                    <Skeleton className="h-4 w-52" />
                </div>
            </div>

            <div className="mt-6 flex flex-wrap items-center gap-3">
                <Skeleton className="h-10 w-36 rounded-md" />
                <Skeleton className="h-10 w-40 rounded-md" />
                <Skeleton className="h-10 w-44 rounded-md" />
            </div>
        </div>
    );
}