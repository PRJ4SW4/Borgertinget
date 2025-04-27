import { useState } from 'react';
import axios from "axios";
import BorgertingetIcon from "../../images/BorgertingetIcon.png"
import './AdminBruger.css'

export default function AdminBruger() {
    const [oldUsername, setOldUsername] = useState<string>("");
    const [newUsername, setNewUsername] = useState<string>("");

    const handleChange = async () => {

        try {
            // Lookup  the user ID by the old username
            const getRes = await axios.get<number>(
                `/api/administrator/username`,
                { params: {username: oldUsername} }
            );
            const userId = getRes.data;
            
            // Call put to update username 
            await axios.put(
                `/api/administrator/${userId}`,
                {userName: newUsername},
                { headers: { "Content-Type": "application/json" } }
            );

            alert("Brugernavn opdateret!");
            // Reset inputs
            setOldUsername("");
            setNewUsername("");
        } catch (err: any) {
            console.error(err);
            if (err.response?.status === 404) {
                alert("Bruger ikke fundet");
            } else {
                alert("Fejl under opdatering af brugernavn")
            }
        }
    }

    return (
        <div className="admin-bruger-container">

          <div><img src={BorgertingetIcon} className='Borgertinget-Icon'></img></div>
          <div className='top-red-line'></div>

          <h1>Ændre Brugernavn</h1>
    
          <div className="input-group">
            <input
              type="text"
              placeholder="Gammel brugernavn"
              value={oldUsername}
              onChange={(e) => setOldUsername(e.target.value)}
            />
    
            <input
              type="text"
              placeholder="Ny brugernavn"
              value={newUsername}
              onChange={(e) => setNewUsername(e.target.value)}
            />
          </div>
    
          <button className="Button" onClick={handleChange}>
            Ændrer brugernavn
          </button>
        </div>
      );
}