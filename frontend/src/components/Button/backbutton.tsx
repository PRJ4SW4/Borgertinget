import React from "react";
import { Link } from "react-router-dom";
import "./BackButton.css"; // Import the new CSS file

interface BackButtonProps {
  match: {
    path: string;
  };
  destination: string;
}

const BackButton: React.FC<BackButtonProps> = ({ match, destination }) => {
  let parentPath: string;
  if (match.path === "/") {
    parentPath = `/${destination}`;
  } else {
    const arr = match.path.split("/");
    const currPage = arr[arr.length - 1];
    parentPath = arr
      .filter((item: string) => {
        // Explicitly type 'item'
        return item !== currPage;
      })
      .join("/");
    // Ensure parentPath is not empty if all segments are filtered out,
    // which could happen if match.path was like "/somepage"
    if (!parentPath && arr.length > 0) {
      parentPath = "/"; // Default to root if path becomes empty
    }
  }
  return (
    <Link to={parentPath} className="back-arrow-link">
      &larr; {/* Use HTML arrow entity */}
    </Link>
  );
};

export default BackButton;
