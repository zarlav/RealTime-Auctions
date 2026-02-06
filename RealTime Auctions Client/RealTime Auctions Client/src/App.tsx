import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import Login from './Components/Login';
import AuctionDashboard from './Components/AuctionDashboard';

function App() {
  const [user, setUser] = useState<any>(() => {
    const savedUser = localStorage.getItem('user');
    return savedUser ? JSON.parse(savedUser) : null;
  });
  
  const navigate = useNavigate();

  const handleLogin = (userData: any) => {
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData)); 
    navigate('/auctions');
  };

  const handleLogout = () => {
    setUser(null);
    localStorage.removeItem('user');
    navigate('/');
  };

  return (
    <Routes>
      <Route path="/" element={user ? <Navigate to="/auctions" /> : <Login onLogin={handleLogin} />} />
      <Route 
        path="/auctions" 
        element={user ? <AuctionDashboard user={user} onLogout={handleLogout} /> : <Navigate to="/" />} 
      />
    </Routes>
  );
}

export default App;