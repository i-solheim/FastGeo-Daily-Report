import { Skeleton } from "@/components/ui/skeleton";

export default function ChangesTableSkeleton() {
    return (
        <div className="overflow-x-auto">
            <div className="border rounded-lg overflow-hidden min-w-[700px]">

                {/* Header */}
                <div className="grid grid-cols-[180px_1fr] bg-muted text-sm font-medium px-4 py-2">
                    <Skeleton className="h-4 w-24" />
                    <Skeleton className="h-4 w-20" />
                </div>

                {[...Array(5)].map((_, index) => (
                    <div
                        key={index}
                        className="grid grid-cols-[180px_1fr] border-t"
                    >
                        {/* Left column */}
                        <div className="flex items-center gap-3 px-4 py-3">

                            <Skeleton className="w-8 h-8 rounded-full" />

                            <div className="flex-1 space-y-2">
                                <Skeleton className="h-4 w-24" />
                                <Skeleton className="h-3 w-16" />
                            </div>

                            <Skeleton className="h-4 w-4" />
                        </div>

                        {/* Right column */}
                        <div className="px-4 py-3">
                            <div className="flex gap-4 pt-2">

                                {[...Array(3)].map((_, badge) => (
                                    <div
                                        key={badge}
                                        className="flex items-center gap-2"
                                    >
                                        <Skeleton className="h-4 w-4 rounded-full" />
                                        <Skeleton className="h-4 w-4" />
                                    </div>
                                ))}

                            </div>
                        </div>

                    </div>
                ))}

            </div>
        </div>
    );
}