// components/ReportHeader.jsx
import { Calendar, Clipboard } from "lucide-react";
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select";
import { formatDateLabel, formatShortDate } from "@/lib/reportUtils";
import { Button } from "@/components/ui/button";
import { useAuth } from "../context/AuthContext";

export function ReportHeader({ summary, selectedDate, setSelectedDate, selectedMember, setSelectedMember, authors, onCopyReport, onLogout }) {
    const { user } = useAuth();
    return (
        <div className="mb-6 mt-16">
            <div className="flex items-start justify-between">
                <div>
                    <h1 className="text-3xl font-bold">Daily Report</h1>

                    <div className="mt-2 flex items-center gap-2 text-sm text-muted-foreground">
                        <Calendar className="w-4 h-4" />
                        {formatDateLabel(summary.date)}
                    </div>

                    <p className="text-sm text-muted-foreground mt-1">
                        Changes since {formatShortDate(summary.date)}
                    </p>
                </div>

                <div className="flex flex-col items-end gap-2">
                    <div className="text-right">
                        <p className="font-medium">{user?.username}</p>

                        <p className="text-sm text-muted-foreground">
                            {user?.role}
                        </p>
                    </div>

                    <Button
                        variant="outline"
                        size="sm"
                        onClick={onLogout}
                    >
                        Logout
                    </Button>
                </div>
            </div>

            <div className="mt-6 flex flex-wrap items-center gap-3">

                <Button className="h-10" onClick={onCopyReport}>
                    <Clipboard className="mr-2 h-4 w-4" />
                    Copy Report
                </Button>

                <input
                    type="date"
                    value={selectedDate}
                    className="h-10 rounded-md border px-3 text-sm"
                    onChange={(e) => setSelectedDate(e.target.value)}
                />

                {user?.role === "Leader" && (
                    <Select
                        value={selectedMember}
                        onValueChange={setSelectedMember}
                    >
                        <SelectTrigger className="h-10 w-[170px]">
                            <SelectValue />
                        </SelectTrigger>

                        <SelectContent>
                            <SelectItem value="All Members">
                                All Members
                            </SelectItem>

                            {authors.map(author => (
                                <SelectItem
                                    key={author}
                                    value={author}
                                >
                                    {author}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                )}

            </div>
        </div>
    );
}