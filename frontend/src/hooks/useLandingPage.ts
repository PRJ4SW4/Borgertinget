import { useCallback, useEffect, useRef } from "react";

export interface FeatureItem {
  title: string;
  image: string;
  link: string;
}

export const useLandingPageLogic = () => {
  const missionText = `
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor 
    incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud 
    exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute 
    irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla 
    pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia 
    deserunt mollit anim id est laborum."
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor 
    incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud 
    exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute 
    irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla 
    pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia 
    deserunt mollit anim id est laborum."
  `;

  const navRef = useRef<HTMLElement>(null);

  const smoothScroll = useCallback((targetId: string) => {
    const targetElement = document.getElementById(targetId);
    if (targetElement) {
      window.scrollTo({ top: targetElement.offsetTop, behavior: "smooth" });
    }
  }, []);

  const handleAnchorClick = useCallback(
    (targetId: string) => {
      smoothScroll(targetId);
    },
    [smoothScroll]
  );

  useEffect(() => {
    const currentNavRef = navRef.current;
    if (currentNavRef) {
      const anchorLinks =
        currentNavRef.querySelectorAll<HTMLAnchorElement>('a[href^="#"]');
      const listeners: {
        element: HTMLAnchorElement;
        handler: (event: MouseEvent) => void;
      }[] = [];

      anchorLinks.forEach((link) => {
        const eventHandler = (e: MouseEvent) => {
          e.preventDefault();
          const targetId = (e.currentTarget as HTMLAnchorElement)
            ?.getAttribute("href")
            ?.substring(1);
          if (targetId) handleAnchorClick(targetId);
        };
        link.addEventListener("click", eventHandler);
        listeners.push({ element: link, handler: eventHandler });
      });

      return () => {
        listeners.forEach(({ element, handler }) => {
          element.removeEventListener("click", handler);
        });
      };
    }
  }, [handleAnchorClick]);

  return {
    navRef,
    missionText,
  };
};
