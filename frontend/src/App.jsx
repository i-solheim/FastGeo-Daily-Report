import ProjectListPage from "./pages/ProjectListPage";
import ProjectPage from "./pages/ProjectPage";
import { Route, Routes } from "react-router-dom"
import { Toaster } from "@/components/ui/toast";

function App() {
  return (
    <>
    <Toaster>
      <Routes>
        <Route path="/" element={<ProjectListPage />} />
        <Route path="/project/:projectKey" element={<ProjectPage />} />
      </Routes>
    </Toaster>
    </>
  );
}

export default App;