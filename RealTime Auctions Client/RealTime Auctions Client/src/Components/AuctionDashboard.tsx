import React, { useEffect, useState } from 'react';
import { connectToHub, subscribeToAuction, unsubscribeFromAuction } from '../Services/signalrService';
import AuctionCard from './AuctionCard';

const AuctionDashboard = ({ user }: any) => {
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
        setAuctions(prev => prev.filter(a => (a.id || a.Id) !== data));
      } else {
        setAuctions(prev => {
          const id = data.id || data.Id;
          const exists = prev.find(a => (a.id || a.Id) === id);
          if (exists) return prev.map(a => (a.id || a.Id) === id ? data : a);
          return [...prev, data];
        });
      }
    });

    return () => unsubscribeFromAuction();
  }, []);

  const handleCreate = async () => {
    if (!name) return;
    await fetch('https://localhost:7068/api/auctions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ProductName: name,
        StartPrice: price,
        CurrentPrice: price,
        OwnerId: user.id,
        LeaderUserId: "Niko",
        EndAt: new Date(Date.now() + 5 * 60000).toISOString(),
        Status: "active"
      })
    });
    setName(""); setPrice(0);
  };

  return (
    <div className="dashboard">
      <header>
        <h1>Aukcije</h1>
        <p>Ulogovan: <b>{user.username}</b></p>
      </header>
      <div className="create-auction-form">
        <input placeholder="Proizvod" value={name} onChange={e => setName(e.target.value)} />
        <input type="number" value={price} onChange={e => setPrice(Number(e.target.value))} />
        <button onClick={handleCreate}>Pokreni</button>
      </div>
      <div className="auction-grid">
        {auctions.map(a => (
          <AuctionCard key={a.id || a.Id} auction={a} userId={user.username} currentUserId={user.id} />
        ))}
      </div>
    </div>
  );
};

export default AuctionDashboard;