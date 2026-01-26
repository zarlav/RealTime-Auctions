import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export const connectToHub = () => {
  if (connection && connection.state === signalR.HubConnectionState.Connected) return;

  connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7068/hubs/auction")
    .withAutomaticReconnect()
    .build();

  connection.start().catch(err => console.error("SignalR Error: ", err));
};

export const subscribeToAuction = (callback: (data: any, type: string) => void) => {
  if (!connection) return;

  connection.on("NewBid", (data) => callback(data, "UPDATE"));
  connection.on("NewAuctionCreated", (data) => callback(data, "UPDATE"));
  connection.on("AuctionEnded", (data) => callback(data, "UPDATE"));
  
  connection.on("AuctionDeleted", (auctionId) => callback(auctionId, "DELETE"));
};

export const unsubscribeFromAuction = () => {
  if (connection) {
    connection.off("NewBid");
    connection.off("NewAuctionCreated");
    connection.off("AuctionEnded");
    connection.off("AuctionDeleted");
  }
};

export const placeBid = (auctionId: string, userId: string, amount: number) => {
  fetch(`https://localhost:7068/api/auctions/${auctionId}/bid?userId=${userId}&amount=${amount}`, {
    method: 'POST'
  });
};