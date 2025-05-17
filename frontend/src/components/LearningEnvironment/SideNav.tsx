import React, { useState, useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { fetchPagesStructure } from '../../services/ApiService';
import type { PageSummaryDto, PageNode } from '../../types/pageTypes';
import './SideNav.css';

// --- Helper function buildTree ---
const buildTree = (list: PageSummaryDto[] | undefined | null): PageNode[] => {
    const map: { [key: number]: PageNode } = {};
    const roots: PageNode[] = [];
    if (!list) return roots;
    list.forEach(item => {
        map[item.id] = { ...item, children: [] };
    });
    list.forEach(item => {
        const node = map[item.id];
        if (item.parentPageId && map[item.parentPageId]) {
            map[item.parentPageId].children.push(node);
            map[item.parentPageId].children.sort((a, b) => a.displayOrder - b.displayOrder);
        } else if (item.parentPageId === null || item.parentPageId === undefined) {
            if (!roots.some(root => root.id === node.id)) {
                 roots.push(node);
            }
        } else {
              console.warn(`Orphan page detected: ${item.title} (ID: ${item.id}) with missing parent ID: ${item.parentPageId}`);
        }
    });
    roots.sort((a, b) => a.displayOrder - b.displayOrder);
    return roots;
};


// --- NavItem Component ---
interface NavItemProps {
  item: PageNode;
}

const NavItem: React.FC<NavItemProps> = ({ item }) => {
    const [isOpen, setIsOpen] = useState<boolean>(false);
    const hasChildren = item.children && item.children.length > 0;

    // Define the handler for clicks *only* on the arrow
    const handleArrowClick = (e: React.MouseEvent<HTMLSpanElement>) => {
        // Prevent the click event from bubbling up to the NavLink parent
        e.stopPropagation();
        // Prevent any potential default behavior of the span click
        e.preventDefault();
        // Toggle the open state for the sub-menu
        setIsOpen(prevIsOpen => !prevIsOpen);
    };

    const linkDestination = `/learning/${item.id}`;

    return (
        <li>
            {/* NavLink only handles navigation */}
            <NavLink
               to={linkDestination}
               className={({ isActive }): string =>
                   `nav-item-link ${isActive ? 'active-nav-link' : ''} ${hasChildren ? 'has-children' : ''}`
                }
            >
                {/* Display the page title */}
                {item.title}

                {/* Render arrow only if item has children */}
                {hasChildren && (
                    // Attach the specific click handler ONLY to the arrow span
                    <span
                        className={`arrow ${isOpen ? 'open' : ''}`}
                        onClick={handleArrowClick} // <-- Click handler is ONLY here
                        // Add aria attributes for accessibility
                        role="button"
                        aria-expanded={isOpen}
                        aria-label={isOpen ? `Collapse ${item.title}` : `Expand ${item.title}`}
                    >
                         {/* Characters representing state - adjust padding in CSS for easier clicking */}
                         {isOpen ? ' ▲' : ' ▼'}
                    </span>
                )}
            </NavLink>

            {/* Conditionally render the sub-menu based on isOpen state */}
            {hasChildren && isOpen && (
                <ul className="sub-menu">
                    {item.children.map(child => (
                        <NavItem key={child.id} item={child} />
                    ))}
                </ul>
            )}
        </li>
    );
};


// --- SideNav Component ---
function SideNav() {
  const [navTree, setNavTree] = useState<PageNode[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadStructure = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const flatList = await fetchPagesStructure();
        const tree = buildTree(flatList);
        setNavTree(tree);
      } catch (err) {
         if (err instanceof Error) {
             setError(err.message);
         } else {
             setError("An unknown error occurred");
         }
        console.error("Failed to load nav structure:", err);
      } finally {
        setIsLoading(false);
      }
    };
    loadStructure();
  }, []);

  if (isLoading) return <nav className="side-nav loading">Indlæser navigation...</nav>;
  if (error) return <nav className="side-nav error">Fejl ved indlæsning: {error}</nav>;
  if (navTree.length === 0) return <nav className="side-nav empty">Ingen sider fundet.</nav>;

  return (
    <nav className="side-nav">
      <ul>
        {navTree.map(rootItem => (
          <NavItem key={rootItem.id} item={rootItem} />
        ))}
      </ul>
    </nav>
  );
}

export default SideNav;