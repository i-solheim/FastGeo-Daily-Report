// components/ReportHeader.jsx
import { Calendar } from "lucide-react";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { formatDateLabel, formatShortDate } from "@/lib/reportUtils";

export function ReportHeader({ summary, selectedDate, setSelectedDate, selectedMember, setSelectedMember, authors }) {
    return (
        <div className="mb-6 mt-16 flex items-start justify-between">
            <div>
                <div className="flex items-center gap-2">
                    <h1 className="text-2xl font-bold">Daily Report</h1>
                    <span className="flex items-center gap-1 text-sm text-muted-foreground">
                        <Calendar className="w-4 h-4" />
                        {formatDateLabel(summary.date)}
                    </span>
                </div>
                <p className="text-sm text-muted-foreground mt-1">
                    Changes since {formatShortDate(summary.date)}
                </p>
            </div>

            <div className="flex gap-2">
                <input
                    type="date"
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                    className="flex items-center gap-2 border rounded-md px-3 py-2 text-sm"
                />
                <Select value={selectedMember} onValueChange={setSelectedMember}>
                    <SelectTrigger className="w-[160px] px-3 py-6">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent className="w-[160px]">
                        <SelectItem value="All Members">All Members</SelectItem>
                        {authors.map(author => (
                            <SelectItem key={author} value={author}>{author}</SelectItem>
                        ))}
                    </SelectContent>
                </Select>
            </div>
        </div>
    );
}