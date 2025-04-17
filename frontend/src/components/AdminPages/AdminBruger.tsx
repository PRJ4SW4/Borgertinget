import { useState, useEffect } from 'react';
import { Routes, useParams } from 'react-router-dom';
import axios from "axios";
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
        <div>
            <h1>Ændre Brugernavn</h1>

            <br/>

            
            <input
                type='text'
                placeholder='Gammel brugernavn'
                value={oldUsername}
                onChange={(o) => setOldUsername(o.target.value)}
            />

            <input
                type='text'
                placeholder='Ny brugernavn'
                value={newUsername}
                onChange={(n) => setNewUsername(n.target.value)}
            />

            <br/>

            <button className='Button'
                onClick={handleChange}
                >Ændrer brugernavn</button>
        </div>
    )
}