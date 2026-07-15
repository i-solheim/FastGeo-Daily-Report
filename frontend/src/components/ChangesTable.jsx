// components/ChangesTable.jsx
import { Badge } from "@/components/ui/badge";
import { badgeClassFor, CATEGORY_META, CATEGORY_ORDER, initials, formatTime, countForAuthor } from "@/lib/reportUtils";

const JIRA_BASE_URL = import.meta.env.VITE_JIRA_BASE_URL;

export function ChangesTable({ changes, selectedMember, expandedAuthors, toggleAuthor }) {
    return (
        <div className="overflow-x-auto">
            <div className="border rounded-lg overflow-hidden min-w-[700px]">
                <div className="grid grid-cols-[200px_minmax(160,1fr)] bg-muted text-sm font-medium px-4 py-2">
                    <div>Team Member</div>
                    <div>Changes</div>
                </div>

                {Object.keys(changes)
                    .filter(author => selectedMember === "All Members" || author === selectedMember)
                    .map(author => {
                        const authorData = changes[author];
                        const total = countForAuthor(authorData);
                        const isExpanded = !!expandedAuthors[author];

                        return (
                            <div key={author} className="grid grid-cols-[200px_minmax(160,1fr)] border-t">
                                <div
                                    onClick={() => toggleAuthor(author)}
                                    className="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-muted/50"
                                >
                                    <div className="w-8 h-8 rounded-full bg-violet-100 text-violet-700 flex items-center justify-center text-xs font-medium shrink-0">
                                        {initials(author)}
                                    </div>
                                    <div className="flex-1">
                                        <div className="text-sm font-medium">{author}</div>
                                        <div className="text-xs text-muted-foreground">{total} change{total !== 1 ? "s" : ""}</div>
                                    </div>
                                    <span className={`text-muted-foreground transition-transform ${isExpanded ? "rotate-90" : ""}`}>
                                        ›
                                    </span>
                                </div>

                                <div className="px-4 py-3">
                                    {!isExpanded && (
                                        <div className="flex gap-4 text-sm text-muted-foreground items-center pt-2">
                                            {CATEGORY_ORDER.map(cat => {
                                                const Icon = CATEGORY_META[cat].icon;
                                                return (
                                                    <span key={cat} className={`flex items-center gap-1 ${CATEGORY_META[cat].color}`}>
                                                        <Icon className="size-4" />
                                                        {authorData[cat].length}
                                                    </span>
                                                );
                                            })}
                                        </div>
                                    )}

                                    {isExpanded && (
                                        <div className="flex flex-col">
                                            {CATEGORY_ORDER.map(cat => {
                                                const Icon = CATEGORY_META[cat].icon;
                                                return (
                                                    authorData[cat].length > 0 && (
                                                        <div key={cat}>
                                                            <div className={`flex items-center gap-2 pb-4 pt-6 text-sm font-medium mb-2 ${CATEGORY_META[cat].color}`}>
                                                                <Icon className="w-4 h-4" />
                                                                <span>{CATEGORY_META[cat].label} ({authorData[cat].length})</span>
                                                            </div>
                                                            <div className="flex flex-col">
                                                                {authorData[cat].map(issue => (
                                                                    <div
                                                                        key={`${issue.issue_key}-${issue.changed_at}-${issue.to_status}`}
                                                                        className="grid grid-cols-[90px_minmax(0,1fr)_220px_70px_28px] items-start gap-3 text-sm border-b last:border-b-0 py-4"
                                                                    >
                                                                        <a href={`${JIRA_BASE_URL}${issue.issue_key}`} target="_blank" rel="noreferrer"
                                                                            className="text-blue-600 hover:underline pl-4">
                                                                            {issue.issue_key}
                                                                        </a>
                                                                        <span className="min-w-0 break-words">{issue.issue_title}</span>
                                                                        <div className="flex items-center gap-2 flex-wrap">
                                                                            <Badge variant={badgeClassFor(issue.from_status)}>{issue.from_status}</Badge>
                                                                            <span className="text-muted-foreground">→</span>
                                                                            <Badge variant={badgeClassFor(issue.to_status)}>{issue.to_status}</Badge>
                                                                        </div>
                                                                        <span className="text-xs text-muted-foreground text-right">
                                                                            {formatTime(issue.changed_at)}
                                                                        </span>
                                                                        <a href={`${JIRA_BASE_URL}${issue.issue_key}`} target="_blank" rel="noreferrer"
                                                                            className="text-muted-foreground hover:text-foreground">
                                                                            ↗
                                                                        </a>
                                                                    </div>
                                                                ))}
                                                            </div>
                                                        </div>
                                                    )
                                                );
                                            })}
                                        </div>
                                    )}
                                </div>
                            </div>
                        );
                    })}
            </div>
        </div>
    );
}