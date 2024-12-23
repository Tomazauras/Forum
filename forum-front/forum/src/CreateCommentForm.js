import React, { useState } from "react";
import axios from "axios";

const CreateCommentForm = ({ topicId, postId, onCommentCreated }) => {
  const [content, setContent] = useState("");
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null); // Reset error state

    try {
      const response = await axios.post(
        `http://localhost:5052/api/topics/${topicId}/posts/${postId}/comments`,
        {
          content,
        },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
          },
        }
      );

      console.log("Comment created:", response.data);
      setSuccess(true);
      setContent("");

      if (onCommentCreated) {
        onCommentCreated(response.data.resource); // Notify parent component
      }
    } catch (error) {
      console.error(
        "Failed to create comment:",
        error.response ? error.response.data : error.message
      );
      setError("Failed to create comment. Please try again.");
      setSuccess(false);
    }
  };

  return (
    <div className="create-comment-form">
      <h3>Create a New Comment</h3>
      {error && <p className="error">{error}</p>}
      {success && <p className="success">Comment created successfully!</p>}
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="content">Comment:</label>
          <textarea
            id="content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            required
          ></textarea>
        </div>
        <button type="submit">Submit Comment</button>
      </form>
    </div>
  );
};

export default CreateCommentForm;
