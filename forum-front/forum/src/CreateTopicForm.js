// CreateTopicForm.js
import React, { useState } from "react";
import axios from "axios";

const CreateTopicForm = () => {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null); // Reset error state

    try {
      const response = await axios.post(
        "http://localhost:5052/api/topics",
        {
          title,
          description,
        },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
          },
        }
      );

      // Assuming the response returns the created topic
      console.log("Topic created:", response.data);
      setSuccess(true);
      setTitle("");
      setDescription("");
    } catch (error) {
      console.error(
        "Failed to create topic:",
        error.response ? error.response.data : error.message
      );
      setError("Failed to create topic. Please try again.");
      setSuccess(false);
    }
  };

  return (
    <div className="create-topic-form">
      <h2>Create a New Topic</h2>
      {error && <p className="error">{error}</p>}
      {success && <p className="success">Topic created successfully!</p>}
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="title">Title:</label>
          <input
            type="text"
            id="title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="description">Description:</label>
          <textarea
            id="description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
          ></textarea>
        </div>
        <button type="submit">Create Topic</button>
      </form>
    </div>
  );
};

export default CreateTopicForm;
