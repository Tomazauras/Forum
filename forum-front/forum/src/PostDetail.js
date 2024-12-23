// PostDetail.js
import React, { useEffect, useState } from "react";
import axios from "axios";
import CommentList from "./CommentList"; // You will create this component
import CreateCommentForm from "./CreateCommentForm";

const PostDetail = ({ postId, topicId, onBack }) => {
  const [comments, setComments] = useState([]);
  const [showCreateCommentForm, setShowCreateCommentForm] = useState(false);
  const toggleCreateCommentForm = () => {
    setShowCreateCommentForm(!showCreateCommentForm);
  };
  const handleCommentCreated = (newComment) => {
    // Optional: Add the new comment to the local state
    console.log("New comment created:", newComment);
  };

  const handleDeleteComment = async (commentId) => {
    const success = await deleteComment(topicId, postId, commentId);
    if (success) {
      setComments((prevComments) =>
        prevComments.filter((comment) => comment.resource.id !== commentId)
      );
      console.log(`Comment ${commentId} removed from state.`);
    }
  };

  const deleteComment = async (topicId, postId, commentId) => {
    try {
      await axios.delete(
        `http://localhost:5052/api/topics/${topicId}/posts/${postId}/comments/${commentId}`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
          },
        }
      );
      console.log(`Comment ${commentId} deleted successfully.`);
      return true;
    } catch (error) {
      console.error(
        `Failed to delete comment ${commentId}:`,
        error.response?.data || error.message
      );
      return false;
    }
  };

  useEffect(() => {
    const loadComments = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5052/api/topics/${topicId}/posts/${postId}/comments`
        );
        console.log(response.data); // Check the structure of the comments data
        setComments(response.data.resource); // Assuming comments are in response.data.resource
      } catch (error) {
        console.error("Error loading comments:", error);
      }
    };

    loadComments();
  }, [postId, topicId]);

  return (
    <div>
      <button onClick={onBack}>Back to Posts</button>
      <h2>Comments for Post {postId}</h2>
      <button onClick={toggleCreateCommentForm}>
        {showCreateCommentForm ? "Cancel" : "Add Comment"}
      </button>
      {showCreateCommentForm && (
        <CreateCommentForm
          topicId={topicId}
          postId={postId}
          onCommentCreated={handleCommentCreated}
        />
      )}
      <CommentList comments={comments} onDelete={handleDeleteComment} />
    </div>
  );
};

export default PostDetail;
