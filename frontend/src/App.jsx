import ProjectListPage from "./pages/ProjectListPage";
import ProjectPage from "./pages/ProjectPage";
import { Route, Routes } from "react-router-dom"

function App() {
  return (
    <>
      <Routes>
        <Route path="/" element={<ProjectListPage />} />
        <Route path="/project/:projectKey" element={<ProjectPage />} />
      </Routes>
    </>
  );
}

export default App;