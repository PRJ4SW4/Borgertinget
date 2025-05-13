import { useNavigate } from "react-router-dom";
import { jwtDecode } from "jwt-decode";

// Function to check if "Admin" is inside the role claim
function isAdmin(token: string | null): boolean {
  if (!token) return false;

  try {
    const decoded = jwtDecode<any>(token); // Decode the JWT token
    console.log("Decoded JWT:", decoded);

    // Correct claim key for role found in the JWT token
    const roleClaimKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    let roles: string[] = [];

    if (typeof decoded[roleClaimKey] === "string") {
      // If it's a single role as string, wrap it into an array
      roles = [decoded[roleClaimKey]];
    } else if (Array.isArray(decoded[roleClaimKey])) {
      // If it's already an array of roles
      roles = decoded[roleClaimKey];
    } else {
      return false;
    }

    // Check if the user has the "Admin" role
    return roles.includes("Admin");
  } catch (err) {
    console.error("Invalid token", err);
    return false;
  }
}

// AdminButton component
export default function AdminButton() {
  const navigate = useNavigate();
  const token = localStorage.getItem("jwt");

  if (!isAdmin(token)) {
    return null; // Don't render anything if the user is not an admin
  }

  return (
    <button className="Button" onClick={() => navigate("/admin")}>
      Admin Panel
    </button>
  );
}
