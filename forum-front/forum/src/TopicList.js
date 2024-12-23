// TopicList.js
import React from "react";

const TopicList = ({ topics, onSelectTopic }) => {
  return (
    <div className="topic-list">
      <h1>Topics</h1>
      {topics.length > 0 ? (
        topics.map((topic) => (
          <div
            key={topic.id}
            onClick={() => {
              onSelectTopic(topic);
            }}
            className="topic"
          >
            <h2>{topic.resource.title}</h2>
            <p>{topic.resource.description}</p>
          </div>
        ))
      ) : (
        <p>No topics available.</p>
      )}
    </div>
  );
};

export default TopicList;
