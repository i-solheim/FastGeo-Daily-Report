import { useState, useEffect } from "react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Link } from "react-router-dom"
import { getProjects } from "@/lib/dashboardApi";

function ProjectListPage() {
    const [projects, setProjects] = useState([]);

    useEffect(() => {

        async function loadProjects() {

            try {
                const data = await getProjects();
                setProjects(data.projects);
            } catch (error) {
                console.error(error);
            }
        }

        loadProjects();
    }, []);


    return <>
        {projects.map(project => (
            <Link key={project} to={`/project/${project}`}>
                <Card key={project}>
                    <CardHeader>
                        <CardTitle>{project}</CardTitle>
                    </CardHeader>
                </Card>
            </Link>
        ))}
    </>;
}

export default ProjectListPage;