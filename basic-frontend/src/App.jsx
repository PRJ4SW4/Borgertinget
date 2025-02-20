import { useState, useEffect } from "react";

function App() {
  const [message, setMessage] = useState("");

  useEffect(() => {
    fetch("http://localhost:5000/api/hello")
      .then((response) => response.json())
      .then((data) => setMessage(data.message))
      .catch((error) => console.error("Error fetching API:", error));
  }, []);

  return (
    <div>
      <h1>Frontend to Backend Test</h1>
      <p>{message}</p>
    </div>
  );
}

export default App;
