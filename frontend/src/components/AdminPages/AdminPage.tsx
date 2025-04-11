import { useState, useEffect } from 'react';
import { Routes, useParams, Route, useNavigate } from 'react-router-dom';
import './AdminPage.css'


// Import other pages
import Bruger from './AdminBruger'
import AdminIndhold from './AdminIndhold'
import AdminLaering from './AdminLearing'
import AdminPolls from './AdminPolls'


// Import logos from images
import AdminBrugerLogo from "../../images/BrugerLogo.png" 
import AdminIndholdLogo from "../../images/InholdLogo.png"
import AdminLaeringLogo from "../../images/AdminLaeringLogo.png" 
import AdminPollsLogo from "../../images/AdminPollsLogo.png"
import BorgertingetIcon from "../../images/BorgertingetIcon.png"



function AdminPage() {
    const navigate = useNavigate();

    // Define the button data
    type buttonData = {
        icon: string; // Path to image
        label: string;
        route: string; // path to another page
    }

    // Define an array of the relevant buttons 
    const buttons: buttonData[] = [
        {icon: AdminBrugerLogo, label: "Administrer brugere", route: "/admin/Bruger" },
        {icon: AdminIndholdLogo, label: "Administrer indhold", route: "/admin/Indhold" },
        {icon: AdminLaeringLogo, label: "Administrer læringsområde", route: "/admin/Laering" },
        {icon: AdminPollsLogo, label: "Administrer polls", route: "/admin/Polls" }
    ]
    


    return (
    <div>
        <div><img src={BorgertingetIcon} className='Borgertinget-Icon'></img></div>

        <div className='top-red-line'></div>

        <div className="button-container">
                {/* buttons.map works like a foreach(btn in buttons). idx is a key that react needs */} 
                {buttons.map((btn, idx) => ( 
                    <div key={idx} className='button-wrapper' onClick={() => navigate(btn.route)}>
                        <div className='circle-button'> 
                            <img src={btn.icon} alt={btn.label} className='icon-image'/>
                        </div>
                        <span className='button-label'>{btn.label}</span>
                    </div>
                ))}
        </div>
    </div>
    ) 
}


export default AdminPage;