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
  const endAt = auction.EndAt || auction.endAt;

  useEffect(() => {
    setBidAmount(currentPrice + 10);
  }, [currentPrice]);

  useEffect(() => {
    const timer = setInterval(() => {
      const diff = new Date(endAt).getTime() - new Date().getTime();
      if (diff <= 0) {
        setTimeLeft("ZAVRSENO");
        clearInterval(timer);
      } else {
        const mins = Math.floor(diff / 60000);
        const secs = Math.floor((diff % 60000) / 1000);
        setTimeLeft(`${mins}m ${secs}s`);
      }
    }, 1000);
    return () => clearInterval(timer);
  }, [endAt]);

  const handleDelete = () => {
    if (window.confirm("Obrisati aukciju?")) {
      fetch(`https://localhost:7068/api/auctions/${id}`, { method: 'DELETE' });
    }
  };

  const handleSendBid = async () => {
    if (bidAmount < currentPrice + 10) {
      alert(`Minimalna ponuda mora biti barem ${currentPrice + 10} din`);
      return;
    }
    await placeBid(id, userId, bidAmount);
  };

  const isEnded = timeLeft === "ZAVRSENO" || auction.Status === 1 || auction.status === 1;

  return (
    <div className={`auction-card ${isEnded ? 'ended' : ''}`}>
      <div className="card-header">
        <h4>{productName}</h4>
        {ownerId === currentUserId && (
          <button onClick={handleDelete} className="delete-btn">×</button>
        )}
      </div>
      
      <div className="price-box">
        <span className="current-price">{currentPrice} din</span>
        <p className="leader-name">Vodi: <b>{leader && leader !== "Niko" ? leader : "Nema ponuda"}</b></p>
      </div>

      <div className={`timer ${isEnded ? 'ended-timer' : ''}`}>
         {isEnded ? "Aukcija zavrsena" : `Preostalo: ${timeLeft}`}
      </div>

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
    </div>
  );
};

export default AuctionCard;