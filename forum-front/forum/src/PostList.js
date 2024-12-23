import React from "react";

const PostList = ({ posts, onSelectPost, topicId, onDelete }) => {
  return (
    <div className="post-list">
      {posts.length > 0 ? (
        posts.map((post) => (
          <div
            key={post.id}
            onClick={() => onSelectPost(post.resource.id)}
            className="post"
          >
            <h3 className="post-title">{post.resource.title}</h3>
            <p className="post-content">{post.resource.body}</p>
            <small>
              Created at: {new Date(post.resource.createdAt).toLocaleString()}
            </small>
            <button
              onClick={(e) => {
                e.stopPropagation(); // Prevent event bubbling if necessary
                onDelete(post.resource.id); // Call the delete function passed from parent
              }}
              className="delete-button"
            >
              Delete Post
            </button>
          </div>
        ))
      ) : (
        <p>No posts available for this topic.</p>
      )}
    </div>
  );
};

export default PostList;
