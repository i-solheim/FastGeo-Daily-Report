import { useState, useEffect, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Info, RefreshCw, Diamond, ArrowLeft } from "lucide-react";
import { ReportHeader } from "@/components/ReportHeader";
import { SummaryCards } from "@/components/SummaryCards";
import { ChangesTable } from "@/components/ChangesTable";
import { toast } from "@/components/ui/toast";
import {
    groupByAuthorAndCategory,
    formatShortDate
} from "@/lib/reportUtils";
import {
    getChanges,
    getSummary,
    getReport,
    getProjectMembership
} from "@/lib/dashboardApi";
import { useAuth } from "../context/AuthContext";
import ReportHeaderSkeleton from "@/components/ReportHeaderSkeleton";
import SummaryCardsSkeleton from "@/components/SummaryCardsSkeleton";
import ChangesTableSkeleton from "@/components/ChangesTableSkeleton";
import ErrorState from "@/components/ErrorState";

function ProjectPage() {
    const { projectKey } = useParams();
    const { logout, user } = useAuth();
    const navigate = useNavigate();

    const [changes, setChanges] = useState({});
    const [expandedAuthors, setExpandedAuthors] = useState({});
    const [summary, setSummary] = useState(null);
    const [membership, setMembership] = useState(null);

    const [selectedDate, setSelectedDate] = useState(() => {
        const yesterday = new Date();
        yesterday.setDate(yesterday.getDate() - 1);

        return yesterday.toISOString().split("T")[0];
    });

    const [selectedMember, setSelectedMember] =
        useState("All Members");

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const isAdmin = user?.isAdmin === true;
    const isLeader = membership?.role === "Leader";

    const canManageMembers = isAdmin || isLeader;

    const loadData = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const [
                changesData,
                summaryData,
                membershipData
            ] = await Promise.all([
                getChanges(projectKey, selectedDate),
                getSummary(projectKey, selectedDate),
                getProjectMembership(projectKey)
            ]);

            setMembership(membershipData);

            setChanges(
                groupByAuthorAndCategory(
                    changesData.changes
                )
            );

            setSummary(summaryData);
        } catch (err) {
            console.error(err);
            setError(err);
        } finally {
            setLoading(false);
        }
    }, [projectKey, selectedDate]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    if (loading) {
        return (
            <div className="max-w-[60%] mx-auto">
                <ReportHeaderSkeleton />
                <SummaryCardsSkeleton />
                <ChangesTableSkeleton />
            </div>
        );
    }

    if (error) {
        return (
            <ErrorState
                title="Couldn't load report"
                description="Please check your connection and try again."
                onRetry={loadData}
            />
        );
    }

    function toggleAuthor(author) {
        setExpandedAuthors(prev => ({
            ...prev,
            [author]: !prev[author]
        }));
    }

    async function handleCopyReport() {
        const report = await getReport(
            projectKey,
            selectedDate
        );

        await navigator.clipboard.writeText(report);

        toast.add({
            title: "Report copied",
            description: "Standup report copied to clipboard.",
            type: "success",
            duration: 2000
        });
    }

    async function handleLogout() {
        await logout();
        navigate("/", { replace: true });
    }

    return (
        <div className="max-w-[60%] mx-auto pt-8">
            <button
                type="button"
                onClick={() => navigate("/")}
                className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground mb-6"
            >
                <ArrowLeft className="w-4 h-4" />
                Back to projects
            </button>
            {summary && (
                <ReportHeader
                    summary={summary}
                    role={membership?.role}
                    selectedDate={selectedDate}
                    setSelectedDate={setSelectedDate}
                    selectedMember={selectedMember}
                    setSelectedMember={setSelectedMember}
                    authors={Object.keys(changes)}
                    onCopyReport={handleCopyReport}
                    onLogout={handleLogout}
                    canManageMembers={canManageMembers}
                    onManageMembers={() =>
                        navigate(`/project/${projectKey}/members`)}
                />
            )}

            {summary && (
                <SummaryCards summary={summary} />
            )}

            <ChangesTable
                changes={changes}
                selectedMember={selectedMember}
                expandedAuthors={expandedAuthors}
                toggleAuthor={toggleAuthor}
            />

            <div className="mt-4 bg-muted/40 rounded-lg p-4 flex items-start justify-between gap-6">
                <div className="flex items-start gap-2">
                    <Info className="w-4 h-4 text-blue-500 mt-0.5 shrink-0" />

                    <div>
                        <p className="text-sm font-medium">
                            About this report
                        </p>

                        <p className="text-xs text-muted-foreground mt-1">
                            This report shows tasks that were created,
                            completed, or had status changes during the
                            selected time period.
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-2 text-xs text-muted-foreground shrink-0">
                    <Diamond className="w-3.5 h-3.5" />
                    <span>Data source: GitHub Projects</span>
                </div>
            </div>
        </div>
    );
}

export default ProjectPage;