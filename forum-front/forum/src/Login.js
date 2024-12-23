import React, { useState } from "react";
import axios from "axios";

const Login = ({ onLoginSuccess }) => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(""); // Clear previous errors

    try {
      const response = await axios.post("http://localhost:5052/api/login", {
        userName: username,
        password,
      });

      if (response.status === 200) {
        // Save the JWT token (response.data.accessToken) in localStorage or a context/state
        localStorage.setItem("accessToken", response.data.accessToken);
        console.log(response.data.accessToken);
        setSuccess(true);
        onLoginSuccess(); // Notify parent component of login success
      }
    } catch (err) {
      // Handle errors (e.g., user does not exist, password incorrect)
      if (err.response && err.response.status === 422) {
        setError(err.response.data || "Login failed. Please try again.");
      } else {
        setError("An unexpected error occurred. Please try again.");
      }
    }
  };

  return (
    <div>
      <h1>Login</h1>
      {success && <p>Login successful!</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit">Login</button>
      </form>
    </div>
  );
};

export default Login;
