import { useNavigate, useLocation } from "react-router-dom";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import "./AdminPolls.css";
import BackButton from "../Button/backbutton";

export default function AdminPolls() {
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

      <h1>Administrer Polls</h1>
      <div className="button-group">
        <button onClick={() => navigate("/admin/Polls/addPoll")} className="Button">
          Opret ny Poll
        </button>
        <br />
        <button onClick={() => navigate("/admin/Polls/editPoll")} className="Button">
          Rediger Igangværende Poll
        </button>
        <br />
        <button onClick={() => navigate("/admin/Polls/deletePoll")} className="Button">
          Slet Poll
        </button>
      </div>
    </div>
  );
}
