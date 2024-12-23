// TopicDetail.js
import React, { useEffect, useState } from "react";
import axios from "axios";
import PostList from "./PostList";
import CreatePostForm from "./CreatePostForm";

const TopicDetail = ({ topicId, onBack, onSelectPost }) => {
  const [posts, setPosts] = useState([]);
  const [showCreatePostForm, setShowCreatePostForm] = useState(false);
  const handleDelete = () => {
    if (
      window.confirm(
        "Are you sure you want to delete this topic? This action cannot be undone."
      )
    ) {
      deleteTopic(topicId); // Call the delete function
    }
  };

  const deleteTopic = async (topicId) => {
    try {
      await axios.delete(`http://localhost:5052/api/topics/${topicId}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
        },
      });
      console.log("Topic deleted successfully.");
      // Optionally, you might want to refresh the topics or redirect the user
    } catch (error) {
      console.error(
        "Failed to delete topic:",
        error.response ? error.response.data : error.message
      );
    }
  };

  useEffect(() => {
    const loadPosts = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5052/api/topics/${topicId}/posts`
        );
        console.log(response.data); // Inspect the data structure
        setPosts(response.data.resource); // Assuming posts are in response.data.resource
      } catch (error) {
        console.error("Error loading posts:", error);
      }
    };

    loadPosts();
  }, [topicId]);

  const toggleCreatePostForm = () => {
    setShowCreatePostForm(!showCreatePostForm);
  };
  const deletePost = async (topicId, postId) => {
    try {
      await axios.delete(
        `http://localhost:5052/api/topics/${topicId}/posts/${postId}`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("accessToken")}`, // Include the access token
          },
        }
      );
      console.log(`Post ${postId} deleted successfully.`);
      return true;
    } catch (error) {
      console.error(
        `Failed to delete post ${postId}:`,
        error.response?.data || error.message
      );
      return false;
    }
  };

  const handleDeletePost = async (postId) => {
    const success = await deletePost(topicId, postId);
    if (success) {
      setPosts((prevPosts) =>
        prevPosts.filter((post) => post.resource.id !== postId)
      );
      console.log(`Post ${postId} removed from state.`);
    }
  };

  return (
    <div>
      <button onClick={onBack}>Back to Topics</button>
      <h2>Posts for Topic {topicId}</h2>
      <button onClick={toggleCreatePostForm}>
        {showCreatePostForm ? "Cancel" : "Create New Post"}
      </button>
      {showCreatePostForm && <CreatePostForm topicId={topicId} />}
      <PostList
        posts={posts}
        onSelectPost={onSelectPost}
        topicId={topicId}
        onDelete={handleDeletePost}
      />
      <button onClick={handleDelete}>Delete Topic</button>
    </div>
  );
};

export default TopicDetail;
