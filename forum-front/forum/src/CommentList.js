// CommentList.js
import React from "react";

const CommentList = ({ comments, onDelete }) => {
  return (
    <div className="comment-list">
      {comments.length > 0 ? (
        comments.map((comment) => (
          <div key={comment.id} className="comment">
            {/* <h4>{comment.resource.User}</h4> */}
            <p className="comment-content">{comment.resource.content}</p>{" "}
            <button
              onClick={() => onDelete(comment.resource.id)}
              className="delete-button"
            >
              Delete
            </button>
          </div>
        ))
      ) : (
        <p>No comments available for this post.</p>
      )}
    </div>
  );
};

export default CommentList;
