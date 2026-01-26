import React, { useState, useEffect } from 'react';
import { placeBid } from '../Services/signalrService';

const AuctionCard = ({ auction, userId, currentUserId }: any) => {
  const [timeLeft, setTimeLeft] = useState("");
  const [bidAmount, setBidAmount] = useState<number>(0);

  const id = auction.Id || auction.id;
  const ownerId = auction.OwnerId || auction.ownerId;
  const currentPrice = auction.CurrentPrice ?? auction.currentPrice;
  const productName = auction.ProductName || auction.productName;
  const leader = auction.LeaderUserId || auction.leaderUserId;

  useEffect(() => {
    setBidAmount(currentPrice + 10);
  }, [currentPrice]);

  useEffect(() => {
    const timer = setInterval(() => {
      const diff = new Date(auction.EndAt || auction.endAt).getTime() - new Date().getTime();
      if (diff <= 0) {
        setTimeLeft("ZAVRŠENO");
        clearInterval(timer);
      } else {
        const mins = Math.floor(diff / 60000);
        const secs = Math.floor((diff % 60000) / 1000);
        setTimeLeft(`${mins}m ${secs}s`);
      }
    }, 1000);
    return () => clearInterval(timer);
  }, [auction]);

  const handleDelete = () => {
    if (window.confirm("Obrisati aukciju?")) {
      fetch(`https://localhost:7068/api/auctions/${id}`, { method: 'DELETE' });
    }
  };

  const handleSendBid = () => {
    if (bidAmount < currentPrice + 10) {
      alert(`Minimalna ponuda mora biti barem ${currentPrice + 10} din (Trenutna + 10 din)`);
      return;
    }
    placeBid(id, userId, bidAmount);
  };

  const isEnded = timeLeft === "ZAVRŠENO";

  return (
    <div className={`auction-card ${isEnded ? 'ended' : ''}`}>
      {ownerId === currentUserId && (
        <button onClick={handleDelete} className="delete-btn">Obriši aukciju</button>
      )}
      
      <h4>{productName}</h4>
      
      <div className="price-box">
        <span className="current-price">{currentPrice} din</span>
        <p className="leader-name">Vodi: <b>{leader || "Niko"}</b></p>
      </div>

      <div className="timer">Preostalo: {timeLeft}</div>

      {!isEnded && (
        <div className="bid-controls">
          <input 
            type="number" 
            value={bidAmount} 
            onChange={(e) => setBidAmount(Number(e.target.value))}
            min={currentPrice + 10}
            className="bid-input"
          />
          <button onClick={handleSendBid} className="bid-btn">Ponudi</button>
        </div>
      )}
      
      {isEnded && <p className="ended-msg">Aukcija je zatvorena</p>}
    </div>
  );
};

export default AuctionCard;