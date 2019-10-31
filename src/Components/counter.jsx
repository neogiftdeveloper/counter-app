import React, { Component } from "react";

class Counter extends Component {
  state = {
    count: 0,
    tags: []
  };

  constructor() {
    super();
    this.handleIncrement = this.handleIncrement.bind(this);
  }

  render() {
    return <div>{this.eventHandling()}</div>;
  }

  renderTags() {
    if (this.state.tags.length === 0) {
      return <p>There are no tags</p>;
    } else {
      return (
        <ul>
          {this.state.tags.map(tag => (
            <li key={tag}>{tag}</li>
          ))}
        </ul>
      );
    }
  }

  eventHandling() {
    return (
      <div>
        <span className={this.getBadgeClasses()}> {this.formatCount()} </span>
        <button
          onClick={this.handleIncrement}
          className={this.getButtonClasses()}
        >
          Increment
        </button>
      </div>
    );
  }

  handleIncrement() {
    this.setState({ count: this.state.count + 1 });
  }

  formatCount() {
    const { count } = this.state;

    return count === 0 ? "Zero" : count;
  }

  getButtonClasses() {
    let classes = "btn btn-secondary btn-sm";

    return classes;
  }

  getBadgeClasses() {
    let classes = "Badge m-2 badge-";

    classes += this.state.count === 0 ? "warning" : "primary";

    return classes;
  }
}

export default Counter;
