import { Skeleton } from "@/components/ui/skeleton";

export default function SummaryCardsSkeleton() {
    return (
        <div className="mb-6 grid grid-cols-[1fr_1fr_1fr_1fr_140px] gap-4 items-stretch">

            {[...Array(4)].map((_, index) => (
                <div
                    key={index}
                    className="bg-muted/40 rounded-lg p-4 border"
                >
                    <div className="flex items-center gap-x-2 min-h-[40px] mb-1">
                        <Skeleton className="size-6 rounded-full" />
                        <Skeleton className="h-4 w-24" />
                    </div>

                    <Skeleton className="h-8 w-12 mb-2" />

                    <Skeleton className="h-3 w-20" />
                </div>
            ))}

            <div className="bg-muted/40 rounded-lg p-2 border flex items-center justify-center">
                <Skeleton className="h-[100px] w-[100px] rounded-full" />
            </div>

        </div>
    );
}