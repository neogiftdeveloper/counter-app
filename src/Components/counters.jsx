import React, { Component } from "react";
import Counter from "./counter";

class Couters extends Component {
  state = {
    counters: [
      { id: 1, values: 0 },
      { id: 2, values: 0 },
      { id: 3, values: 0 },
      { id: 4, values: 0 }
    ]
  };
  render() {
    return (
      <div>
        {this.state.counters.map(counter => (
          <Counter key={counter.id} />
        ))}
      </div>
    );
  }
}

export default Couters;
