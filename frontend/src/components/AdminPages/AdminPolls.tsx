import { useNavigate } from "react-router-dom";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import "./AdminPolls.css";

export default function AdminPolls() {
  const navigate = useNavigate();
  return (
    <div className="container">
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon"></img>
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
