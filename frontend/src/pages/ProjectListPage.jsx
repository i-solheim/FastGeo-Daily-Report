import { useState, useEffect, useCallback } from "react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router-dom"
import { getProjects } from "@/lib/dashboardApi";
import { useAuth } from "../context/AuthContext";
import ProjectCardSkeleton from "@/components/ProjectCardSkeleton";
import { FolderKanban } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import ErrorState from "@/components/ErrorState";
import EmptyState from "@/components/EmptyState";

function ProjectListPage() {
    const [projects, setProjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const loadProjects = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            const data = await getProjects();
            setProjects(data.projects);
        } catch (err) {
            console.error(err);
            setError(err);
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        loadProjects();
    }, [loadProjects]);

    if (loading) {
        return (
            <div className="max-w-4xl mx-auto py-16 px-6">
                <div className="mb-10">
                    <Skeleton className="h-10 w-80 mb-3" />
                    <Skeleton className="h-5 w-72" />
                </div>

                <div className="space-y-4">
                    {[...Array(3)].map((_, index) => (
                        <ProjectCardSkeleton key={index} />
                    ))}
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <ErrorState
                title="Couldn't load projects"
                description="Please check your connection and try again."
                onRetry={loadProjects}
            />
        );
    }

    if (projects.length === 0) {
        return (
            <EmptyState
                icon={FolderKanban}
                title="No projects found"
                description="There are no projects available for your account."
            />
        );
    }

    return <>
        <div className="max-w-4xl mx-auto py-16 px-6">
            <div className="mb-10">
                <h1 className="text-4xl font-bold">
                    Daily Report Dashboard
                </h1>

                <p className="text-muted-foreground mt-2">
                    Select a project to view its daily standup report.
                </p>
            </div>
            <div className="space-y-4">
                {projects.map(project => (
                    <Link
                        key={project.projectKey}
                        to={`/project/${project.projectKey}`}
                    >
                        <Card className="transition-all hover:shadow-md hover:border-primary/40">
                            <CardHeader>
                                <div className="flex items-center gap-3">
                                    <FolderKanban className="h-6 w-6 text-primary" />

                                    <div>
                                        <CardTitle>{project.projectKey}</CardTitle>

                                        <p className="text-sm text-muted-foreground">
                                            {project.role} · Open daily report
                                        </p>
                                    </div>
                                </div>
                            </CardHeader>
                        </Card>
                    </Link>
                ))}
            </div>
        </div>
    </>;
}

export default ProjectListPage;