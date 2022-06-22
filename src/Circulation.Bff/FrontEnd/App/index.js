import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';

function render() {
  const rootElement = document.getElementById('app');
  ReactDOM.render(<App />, rootElement);
}

window.onload = render;

if (module.hot) {
  // support hot reloading
  module.hot.accept('./App', render);
}
