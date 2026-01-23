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
  onBid: (auctionId: string) => void;
}

const AuctionList: React.FC<Props> = ({ auctions, onBid }) => {
  return (
    <div className="auction-list">
      {auctions.map(a => (
        <AuctionCard key={a.id} auction={a} onBid={onBid} />
      ))}
    </div>
  );
};

export default AuctionList;