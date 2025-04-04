import React from "react";
import "./Infobox.css";

const Infobox: React.FC = () => {
  return (
    <div className="infobox">
      <div className="item">
        <div className="indikator korrekt"></div>
        <div className="label">Korrekt</div>
      </div>
      <div className="item">
        <div className="indikator forkert"></div>
        <div className="label">Forkert</div>
      </div>
      <div className="item">
        <div className="indikator hoejere"></div>
        <div className="label">Højere</div>
      </div>
      <div className="item">
        <div className="indikator lavere"></div>
        <div className="label">Lavere</div>
      </div>
    </div>
  );
};

export default Infobox;
