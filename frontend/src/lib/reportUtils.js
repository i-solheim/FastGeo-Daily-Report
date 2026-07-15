import { CheckIcon } from "@/components/icons/CheckIcon";
import { StatusChangeIcon } from "@/components/icons/StatusChangeIcon";
import { NewTaskIcon } from "@/components/icons/NewTaskIcon";

export const STATUS_CATEGORY = {
    "Backlog": "todo",
    "Selected for Development": "todo",
    "In Progress": "inprogress",
    "Completed Code": "inprogress",
    "Ready for Test": "inprogress",
    "Done": "done",
};

export function badgeClassFor(status) {
    return STATUS_CATEGORY[status] || "todo";
}

export const CATEGORY_META = {
    completed: { label: "Completed", icon: CheckIcon, color: "text-green-600" },
    status_change: { label: "Status Changes", icon: StatusChangeIcon, color: "text-amber-500" },
    new_task: { label: "New Tasks", icon: NewTaskIcon, color: "text-violet-600" },
};

export const CATEGORY_ORDER = ["completed", "status_change", "new_task"];

export function initials(name) {
    const parts = name.trim().split(" ").filter(Boolean);
    const first = parts[0]?.[0] ?? "";
    const last = parts.length > 1 ? parts[parts.length - 1][0] : "";
    return first + last;
}

export function formatTime(isoString) {
    return new Date(isoString).toLocaleTimeString([], { hour: "numeric", minute: "2-digit" });
}

export function groupByAuthorAndCategory(changes) {
    const grouped = {};
    for (const change of changes) {
        if (!grouped[change.author]) {
            grouped[change.author] = { completed: [], status_change: [], new_task: [] };
        }
        grouped[change.author][change.category].push(change);
    }
    return grouped;
}

export function countForAuthor(authorData) {
    return CATEGORY_ORDER.reduce((total, cat) => total + authorData[cat].length, 0);
}

export function formatDateLabel(dateString) {
    const date = new Date(dateString + "T00:00:00");
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);
    const formatted = date.toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" });
    if (date.getTime() === yesterday.getTime()) return `${formatted} (Yesterday)`;
    if (date.getTime() === today.getTime()) return `${formatted} (Today)`;
    return formatted;
}

export function formatShortDate(dateString) {
    const date = new Date(dateString + "T00:00:00");
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
}