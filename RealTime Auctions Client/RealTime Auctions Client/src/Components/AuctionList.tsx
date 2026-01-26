import React from 'react';
import AuctionCard from './AuctionCard';

interface Auction {
  id: string;
  productName: string;
  currentPrice: number;
  leaderUserId: string;
}

interface Props {
  auctions: Auction[];
  userId: string; 
}

const AuctionList: React.FC<Props> = ({ auctions, userId }) => {
  return (
    <div className="auction-list">
      {auctions.map(a => (
        <AuctionCard key={a.id} auction={a} userId={userId} />
      ))}
    </div>
  );
};

export default AuctionList;