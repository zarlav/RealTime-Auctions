import React, { useEffect, useState } from 'react';
import { connectToHub, subscribeToAuction, unsubscribeFromAuction } from '../Services/signalrService';
import AuctionCard from './AuctionCard';

const AuctionDashboard = ({ user, onLogout }: any) => {
  const [auctions, setAuctions] = useState<any[]>([]);
  const [name, setName] = useState("");
  const [price, setPrice] = useState(0);

  useEffect(() => {
    connectToHub();
    
    fetch('https://localhost:7068/api/auctions/active')
      .then(res => res.json())
      .then(data => setAuctions(data));

    subscribeToAuction((data: any, type: string) => {
      if (type === "DELETE") {
        setAuctions(prev => prev.filter(a => (a.Id || a.id) !== data));
      } else {
        setAuctions(prev => {
          const incomingId = data.Id || data.id;
          const exists = prev.find(a => (a.Id || a.id) === incomingId);
          
          if (exists) {
            return prev.map(a => (a.Id || a.id) === incomingId ? data : a);
          } else {
            return [data, ...prev];
          }
        });
      }
    });

    return () => unsubscribeFromAuction();
  }, []);

  const handleCreate = async () => {
    if (!name || price <= 0) {
      alert("Unesite naziv i pocetnu cenu!");
      return;
    }

    const newAuction = {
      ProductName: name,
      StartPrice: price,
      CurrentPrice: price,
      OwnerId: user.id,
      LeaderUserId: "Niko",
      EndAt: new Date(Date.now() + 5 * 60000).toISOString(),
      Status: 0 
    };

    try {
      const response = await fetch('https://localhost:7068/api/auctions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newAuction)
      });
      
      if (response.ok) {
        setName(""); 
        setPrice(0);
      }
    } catch (error) {
      console.error("Greska pri kreiranju:", error);
    }
  };

  return (
    <div className="dashboard">
      <header>
        <div>
          <h1>Aukcije</h1>
          <p>Ulogovan: <b>{user.username}</b></p>
        </div>
        <button onClick={onLogout} className="logout-btn">
          Odjavi se
        </button>
      </header>

      <div className="create-auction-form">
        <div className="input-group">
          <label>Naziv proizvoda</label>
          <input placeholder="npr. iPhone 15" value={name} onChange={e => setName(e.target.value)} />
        </div>
        <div className="input-group">
          <label>Pocetna cena (din)</label>
          <input type="number" value={price} onChange={e => setPrice(Number(e.target.value))} />
        </div>
        <button onClick={handleCreate}>Pokreni aukciju</button>
      </div>

      <div className="auction-grid">
        {auctions.length > 0 ? (
          auctions.map(a => (
            <AuctionCard 
              key={a.Id || a.id} 
              auction={a} 
              userId={user.username} 
              currentUserId={user.id} 
            />
          ))
        ) : (
          <p>Trenutno nema aktivnih aukcija.</p>
        )}
      </div>
    </div>
  );
};

export default AuctionDashboard;