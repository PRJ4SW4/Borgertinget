import { Routes, useParams, Route, useNavigate } from 'react-router-dom';
import './AdminLearing.css'
import BorgertingetIcon from "../../images/BorgertingetIcon.png"


export default function AdminLaering() {
    const navigate = useNavigate();
    
    return (
        <div className='container'>

            
            <div><img src={BorgertingetIcon} className='Borgertinget-Icon'></img></div>
            <div className='top-red-line'></div>

            <h1>Administrer Læringsområde</h1>
            <div className="button-group">
                <button 
                    onClick={ () => navigate("/admin/Laering/addflashcardcollection") }
                    className='Button'
                        >Opret ny Flashcard serie
                </button>
                <br/>
                <button 
                    onClick={() => navigate("/admin/Laering/editflashcardcollection")}
                    className='Button'>
                    Rediger flashcard serie
                </button>
                <br/>
                <button
                    onClick={() => navigate("/admin/Laering/editcitatmode")}
                    className='Button'>
                    Rediger Polidles Citat-Mode

                </button>

            </div>
        </div>
        
        
    )
}