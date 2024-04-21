import logo from './logo.svg';
import './App.css';
import React, { useEffect, useState } from 'react';


function App() {
    const [socket, setSocket] = useState(null);

    const connectWs = () => {
      const s = new WebSocket('ws://localhost:5163/ws');

      // Connection opened
      s.addEventListener('open', (event) => {
        socket.send('Hello Server!');
      });
  
      // Listen for messages
      s.addEventListener('message', (event) => {
        console.log('Message from server: ', event.data);
      });
  
      setSocket(s);

    }

  return (
    <div className="App">
      <button onClick={connectWs}>Connect</button>

      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

export default App;
