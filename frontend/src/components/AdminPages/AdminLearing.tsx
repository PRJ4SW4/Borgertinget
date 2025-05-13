import { useNavigate, useLocation } from "react-router-dom";
import "./AdminLearing.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

export default function AdminLaering() {
  const navigate = useNavigate();
  const location = useLocation();
  const matchProp = { path: location.pathname };

  return (
    <div className="container">
      <div style={{ position: "relative" }}>
        {" "}
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>

      <h1>Administrer Læringsområde</h1>
      <div className="button-group">
        <button onClick={() => navigate("/admin/Laering/addflashcardcollection")} className="Button">
          Opret ny Flashcard serie
        </button>
        <br />

        <button onClick={() => navigate("/admin/Laering/editflashcardcollection")} className="Button">
          Rediger flashcard serie
        </button>

        <br />
        <button onClick={() => navigate("/admin/Laering/deleteFlashcardCollection")} className="Button">
          Slet flashcard serie
        </button>

        <br />
        <button onClick={() => navigate("/admin/Laering/editcitatmode")} className="Button">
          Rediger Polidles Citat-Mode
        </button>

        <br />

        <button onClick={() => navigate("/admin/Laering/addLearningPage")} className="Button">
          Opret nyt læringsområde
        </button>
        <br />
        <button onClick={() => navigate("/admin/Laering/editLearningPage")} className="Button">
          Rediger læringsområde
        </button>
        <br />
        <button onClick={() => navigate("/admin/Laering/deleteLearningPage")} className="Button">
          Slet læringsområde
        </button>
      </div>
    </div>
  );
}
