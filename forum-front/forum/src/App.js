import { useState, useEffect } from "react";
import axios from "axios";
import TopicList from "./TopicList";
import TopicDetail from "./TopicDetail";
import PostDetail from "./PostDetail";
import Register from "./Register";
import Login from "./Login";
import CreateTopicForm from "./CreateTopicForm";
import "./App.css";

export const refreshAccessToken = async () => {
  try {
    const response = await axios.post("http://localhost:5052/api/accessToken");
    return response.data.accessToken; // Return the new access token
  } catch (error) {
    console.error("Failed to refresh access token", error);
    throw error; // Propagate the error to be handled elsewhere
  }
};

export const logoutUser = async () => {
  try {
    // Log the access token to confirm it's being retrieved correctly
    const accessToken = localStorage.getItem("accessToken");
    console.log("Access Token:", accessToken);

    // Make the logout request with the correct configuration
    await axios.post(
      "http://localhost:5052/api/logout",
      {}, // Empty body since it's a logout request
      {
        headers: {
          Authorization: `Bearer ${accessToken}`, // Set the Authorization header
        },
        withCredentials: true, // Ensure cookies are sent with the request
      }
    );

    // Clear the access token from local storage upon successful logout
    localStorage.removeItem("accessToken");
    return true; // Indicate successful logout
  } catch (error) {
    console.error(
      "Logout failed",
      error.response ? error.response.data : error.message
    ); // Log error details
    throw error; // Propagate the error to be handled elsewhere
  }
};

const App = () => {
  const [topics, setTopics] = useState([]);
  const [selectedTopicId, setSelectedTopicId] = useState(null);
  const [selectedPostId, setSelectedPostId] = useState(null);
  const [showRegister, setShowRegister] = useState(false);
  const [showLogin, setShowLogin] = useState(false);
  const [accessToken, setAccessToken] = useState(
    localStorage.getItem("accessToken")
  ); // Store access token in state
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [nextPageLink, setNextPageLink] = useState(null); // Store the next page link
  const [prevPageLink, setPrevPageLink] = useState(null); // Store the previous page link
  const [pageSize, setPageSize] = useState(4); // State for number of topics per page
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const loadTopics = async (pageSize, pageNumber) => {
      try {
        const response = await axios.get(
          `http://localhost:5052/api/topics?pageSize=${pageSize}&pageNumber=${pageNumber}`,
          {
            headers: { Authorization: `Bearer ${accessToken}` }, // Include access token in the request
          }
        );
        console.log(response.data);
        setTopics(response.data.resource);
        const links = response.data.links;
        setNextPageLink(
          links.find((link) => link.rel === "nextPage")?.href || null
        );
        setPrevPageLink(
          links.find((link) => link.rel === "prevPage")?.href || null
        );
      } catch (error) {
        console.error("Error loading topics:", error);
      }
    };

    // Function to refresh the access token
    const attemptTokenRefresh = async () => {
      try {
        console.log("before", accessToken);
        const newAccessToken = await refreshAccessToken();
        setAccessToken(newAccessToken);
        localStorage.setItem("accessToken", newAccessToken); // Update local storage
        console.log("after", accessToken);
      } catch (error) {
        console.error("Failed to refresh access token", error);
        // Optionally handle token refresh failure (e.g., log out the user)
      }
    };

    // Call loadTopics or refresh token here
    if (accessToken) {
      loadTopics(pageSize, currentPage);
    } else {
      attemptTokenRefresh(); // Try to refresh the token if it doesn't exist
    }
  }, [accessToken, pageSize]); // Dependency on accessToken

  const loadNextPage = async () => {
    if (nextPageLink) {
      const response = await axios.get(nextPageLink, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });
      setTopics(response.data.resource.map((item) => item.resource.title)); // Update topics
      const links = response.data.links; // Update pagination links
      setNextPageLink(
        links.find((link) => link.rel === "nextPage")?.href || null
      );
      setPrevPageLink(
        links.find((link) => link.rel === "prevPage")?.href || null
      );
    }
  };

  const handlePageSizeChange = (event) => {
    setPageSize(Number(event.target.value)); // Update page size
    setCurrentPage(1);
  };

  // Load previous page
  const loadPrevPage = async () => {
    if (prevPageLink) {
      const response = await axios.get(prevPageLink, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("accessToken")}`,
        },
      });
      setTopics(response.data.resource.map((item) => item.resource)); // Update topics
      const links = response.data.links; // Update pagination links
      setNextPageLink(
        links.find((link) => link.rel === "nextPage")?.href || null
      );
      setPrevPageLink(
        links.find((link) => link.rel === "prevPage")?.href || null
      );
    }
  };

  // Function to refresh the access token
  const attemptTokenRefresh = async () => {
    try {
      const newAccessToken = await refreshAccessToken();
      setAccessToken(newAccessToken);
      localStorage.setItem("accessToken", newAccessToken);
    } catch (error) {
      console.error("Failed to refresh access token", error);
      // Optionally handle token refresh failure (e.g., log out the user)
    }
  };

  const handleSelectTopic = (topic) => {
    console.log("Selected Topic ID:", topic.resource.id);
    setSelectedTopicId(topic.resource.id);
  };

  const handleSelectPost = (postId) => {
    console.log("Selected Post ID:", postId);
    setSelectedPostId(postId); // Set the selected post ID
  };

  const handleBackToTopics = () => {
    setSelectedTopicId(null);
    setSelectedPostId(null); // Reset selected post when going back
  };

  const handleBackToPosts = () => {
    setSelectedPostId(null); // Reset selected post
  };

  const toggleRegister = () => {
    setShowRegister((prev) => !prev);
  };

  const toggleLogin = () => {
    setShowLogin((prev) => !prev);
    setShowRegister(false); // Close register if opening login
  };
  const handleLoginSuccess = () => {
    console.log("Logged in");
    setShowLogin(false);
    // Optionally, you can load topics or perform other actions on login success
    handleBackToTopics(); // Reload topics or redirect to a different page
  };

  const handleLogout = async () => {
    try {
      await logoutUser(); // Call the logout function
      setAccessToken(null); // Clear access token from state
      localStorage.removeItem("accessToken"); // Remove from local storage
      setSelectedTopicId(null); // Reset any selected topics or posts
      setSelectedPostId(null);
      console.log("Logged out successfully");
    } catch (error) {
      console.error("Error during logout:", error);
    }
  };

  const toggleCreateForm = () => {
    setShowCreateForm(!showCreateForm);
  };

  return (
    <div>
      {showRegister ? (
        <Register />
      ) : showLogin ? (
        <Login onLoginSuccess={handleLoginSuccess} />
      ) : !selectedTopicId ? (
        <TopicList topics={topics} onSelectTopic={handleSelectTopic} />
      ) : selectedPostId ? (
        <PostDetail
          postId={selectedPostId}
          topicId={selectedTopicId}
          onBack={handleBackToPosts}
        />
      ) : (
        <TopicDetail
          topicId={selectedTopicId}
          onSelectPost={handleSelectPost}
          onBack={handleBackToTopics}
        />
      )}
      <label htmlFor="pageSize">Topics per page:</label>
      <select id="pageSize" value={pageSize} onChange={handlePageSizeChange}>
        <option value={4}>4</option>
        <option value={8}>8</option>
        <option value={12}>12</option>
      </select>
      {/* <div className="pagination">
        <button onClick={loadPrevPage} disabled={!prevPageLink}>
          Previous
        </button>
        <button onClick={loadNextPage} disabled={!nextPageLink}>
          Next
        </button>
      </div> */}
      <button onClick={toggleLogin}>
        {showLogin ? "Back to Forum" : "Login"}
      </button>
      <button onClick={toggleRegister}>
        {showRegister ? "Back to Forum" : "Register"}
      </button>
      <button onClick={handleLogout}>Logout</button>
      <button onClick={toggleCreateForm}>
        {showCreateForm ? "Cancel" : "Create New Topic"}
      </button>
      {showCreateForm && <CreateTopicForm />}
    </div>
  );
};

export default App;
