import React, { useEffect, useState } from 'react';
import AuctionList from './components/AuctionList';
import { connectToHub, subscribeToAuction, placeBid } from './services/signalrService';

interface Auction {
  id: string;
  productName: string;
  currentPrice: number;
  leaderUserId: string;
}

const App: React.FC = () => {
  const [auctions, setAuctions] = useState<Auction[]>([]);

  useEffect(() => {
    // Poveži se na SignalR hub
    connectToHub();

    // Primer: učlani se u demo aukciju
    subscribeToAuction('demo-auction-1', (data: any) => {
      setAuctions((prev) => {
        const index = prev.findIndex(a => a.id === data.AuctionId);
        if (index >= 0) {
          const updated = [...prev];
          updated[index] = {
            ...updated[index],
            currentPrice: data.CurrentPrice,
            leaderUserId: data.LeaderUserId
          };
          return updated;
        } else {
          return [...prev, {
            id: data.AuctionId,
            productName: 'Demo Product',
            currentPrice: data.CurrentPrice,
            leaderUserId: data.LeaderUserId
          }];
        }
      });
    });
  }, []);

  const handleBid = (auctionId: string) => {
    const amount = prompt("Enter your bid amount:");
    if (!amount) return;
    placeBid(auctionId, "user-demo", parseFloat(amount));
  };

  return (
    <div className="app">
      <h1>Real-Time Auctions</h1>
      <AuctionList auctions={auctions} onBid={handleBid} />
    </div>
  );
};

export default App;
