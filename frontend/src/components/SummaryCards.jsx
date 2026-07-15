// components/SummaryCards.jsx
import { PieChart, Pie, Sector } from "recharts";
import { TotalChangesIcon } from "@/components/icons/TotalChangesIcon";
import { CATEGORY_META, CATEGORY_ORDER } from "@/lib/reportUtils";

export function SummaryCards({ summary }) {
    return (
        <div className="mb-6 grid grid-cols-[1fr_1fr_1fr_1fr_140px] gap-4 items-stretch">
            <div className="bg-muted/40 rounded-lg p-4 border">
                <div className="flex items-center gap-x-2 mb-1 text-blue-600">
                    <TotalChangesIcon className="size-6" />
                    <p className="text-sm font-bold">Total changes</p>
                </div>
                <p className="text-2xl font-medium">{summary.total}</p>
            </div>

            {CATEGORY_ORDER.map(cat => {
                const Icon = CATEGORY_META[cat].icon;
                const value = summary[cat === "status_change" ? "status_changes" : cat === "new_task" ? "new_tasks" : "completed"];
                return (
                    <div key={cat} className="bg-muted/40 rounded-lg p-4 border">
                        <div className={`flex items-center gap-x-2 mb-1 ${CATEGORY_META[cat].color}`}>
                            <Icon className="size-6" />
                            <p className="text-sm font-bold">{CATEGORY_META[cat].label}</p>
                        </div>
                        <p className={`text-2xl font-medium ${CATEGORY_META[cat].color}`}>{value}</p>
                        <p className="text-xs text-muted-foreground">
                            {summary.total > 0 ? Math.round((value / summary.total) * 100) : 0}% of total
                        </p>
                    </div>
                );
            })}

            <div className="bg-muted/40 rounded-lg p-2 flex items-center justify-center border">
                <PieChart width={110} height={110}>
                    <Pie
                        data={[
                            { name: "Completed", value: summary.completed },
                            { name: "Status changes", value: summary.status_changes },
                            { name: "New tasks", value: summary.new_tasks },
                        ]}
                        dataKey="value"
                        innerRadius={30}
                        outerRadius={50}
                        shape={(props) => {
                            const colors = ["#16a34a", "#f59e0b", "#7c3aed"];
                            return <Sector {...props} fill={colors[props.index]} />;
                        }}
                    />
                </PieChart>
            </div>
        </div>
    );
}