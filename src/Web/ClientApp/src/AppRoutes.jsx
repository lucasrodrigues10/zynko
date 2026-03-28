import { HomePage } from "./pages/HomePage";
import { LobbyPage } from "./pages/LobbyPage";
import { GamePage } from "./pages/GamePage";
import { ResultsPage } from "./pages/ResultsPage";

const AppRoutes = [
  { index: true, element: <HomePage /> },
  { path: '/lobby/:code', element: <LobbyPage /> },
  { path: '/game/:code', element: <GamePage /> },
  { path: '/results/:code', element: <ResultsPage /> },
];

export default AppRoutes;
