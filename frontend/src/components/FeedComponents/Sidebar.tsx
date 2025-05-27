import React from 'react';
import { PoliticianInfoDto } from '../../types/tweetTypes';
import './Sidebar.css';

interface SidebarProps {
  subscriptions: PoliticianInfoDto[];
  selectedPoliticianId: number | null;
  isLoading: boolean;
  onFilterChange: (id: number | null) => void;
  onBackClick: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({
  subscriptions,
  selectedPoliticianId,
  isLoading,
  onFilterChange,
  onBackClick
}) => {
  return (
    <div className="sidebar">
      <h2 className="sidebar-title">Følger</h2>
      <ul className="sidebar-nav-list">
        <li className={`sidebar-nav-item ${selectedPoliticianId === null ? 'active-filter' : ''}`}>
          <button 
            onClick={() => onFilterChange(null)} 
            disabled={isLoading || selectedPoliticianId === null} 
            className="sidebar-nav-button"
          >
            Alle Tweets
          </button>
        </li>
        
        {Array.isArray(subscriptions) && subscriptions.length > 0 ? (
          subscriptions.map((sub) => (
            <li key={sub.id} className={`sidebar-nav-item ${selectedPoliticianId === sub.id ? 'active-filter' : ''}`}>
              <button 
                onClick={() => onFilterChange(sub.id)} 
                disabled={isLoading || selectedPoliticianId === sub.id} 
                className="sidebar-nav-button"
              >
                {sub.name}
              </button>
            </li>
          ))
        ) : !isLoading ? (
          <li className="sidebar-nav-item-info">Du følger ingen.</li>
        ) : null}
      </ul>
      <button onClick={onBackClick} className="back-button sidebar-back-button">&larr; Tilbage til Home</button>
    </div>
  );
};

export default Sidebar;