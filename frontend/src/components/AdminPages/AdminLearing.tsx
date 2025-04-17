import { useState, useEffect } from 'react';
import { Routes, useParams, Route, useNavigate } from 'react-router-dom';
import './AdminLearing.css'

export default function AdminLaering() {
    const navigate = useNavigate();
    
    return (
        <div className='container'>
            <h1>Learing</h1>
            <div>
                <button 
                    onClick={ () => navigate("/admin/Laering/addflashcardcollection") }
                    className='Button'
                        >Opret ny Flashcard serie
                </button>
                <br/>
                <button className='Button'>
                    Rediger eksisterende flashcard serie
                </button>
            </div>
        </div>
        
        
    )
}