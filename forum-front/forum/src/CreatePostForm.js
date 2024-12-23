import React, { useState } from "react";
import axios from "axios";

const CreatePostForm = ({ topicId }) => {
  const [title, setTitle] = useState("");
  const [body, setBody] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null); // Reset error state

    try {
      const response = await axios.post(
        `http://localhost:5052/api/topics/${topicId}/posts`,
        {
          title,
          body,
        },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
          },
        }
      );

      // Assuming the response returns the created post
      console.log("Post created:", response.data);
      setSuccess(true);
      setTitle("");
      setBody("");
    } catch (error) {
      console.error(
        "Failed to create post:",
        error.response ? error.response.data : error.message
      );
      setError("Failed to create post. Please try again.");
      setSuccess(false);
    }
  };

  return (
    <div className="create-post-form">
      <h2>Create a New Post</h2>
      {error && <p className="error">{error}</p>}
      {success && <p className="success">Post created successfully!</p>}
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
          <label htmlFor="body">Body:</label>
          <textarea
            id="body"
            value={body}
            onChange={(e) => setBody(e.target.value)}
            required
          ></textarea>
        </div>
        <button type="submit">Create Post</button>
      </form>
    </div>
  );
};

export default CreatePostForm;
