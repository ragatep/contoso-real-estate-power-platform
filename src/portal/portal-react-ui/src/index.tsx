import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';

const rootEl = document.querySelector('#portal-ui-root');
if (!rootEl) throw new Error('Cannot find root element with that id');
ReactDOM.render(
    <React.StrictMode>
        <App />
    </React.StrictMode>,
    rootEl,
);
