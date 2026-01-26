import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import Login from './Components/Login';
import AuctionDashboard from './Components/AuctionDashboard';

function App() {
  const [user, setUser] = useState<any>(null);
  const navigate = useNavigate();

  const handleLogin = (userData: any) => {
    setUser(userData);
    localStorage.setItem('user', JSON.stringify(userData)); 
    navigate('/auctions');
  };

  return (
    <Routes>
      <Route path="/" element={user ? <Navigate to="/auctions" /> : <Login onLogin={handleLogin} />} />
      <Route path="/auctions" element={user ? <AuctionDashboard user={user} /> : <Navigate to="/" />} />
    </Routes>
  );
}

export default App;