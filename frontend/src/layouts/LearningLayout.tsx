// src/layouts/LearningLayout.tsx
import { Outlet } from 'react-router-dom';
import SideNav from '../components/SideNav';
import './LearningLayout.css';

// No props, return type is implicitly JSX.Element
function LearningLayout() {
  return (
    <div className="learning-layout">
      <aside className="learning-sidenav-container">
         <SideNav />
      </aside>
      <main className="learning-content-area">
        <Outlet /> {/* Renders the matched child route element */}
      </main>
    </div>
  );
}

export default LearningLayout;