import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection;

export const connectToHub = () => {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hubs/auction") // ASP.NET Core hub
    .withAutomaticReconnect()
    .build();

  connection.start().then(() => console.log("Connected to SignalR hub"))
    .catch(err => console.error(err));
};

export const subscribeToAuction = (auctionId: string, callback: (data: any) => void) => {
  if (!connection) return;
  connection.invoke("JoinAuction", auctionId)
    .catch(err => console.error(err));

  connection.on("NewBid", (data) => {
    if (data.AuctionId === auctionId) callback(data);
  });

  connection.on("AuctionEnded", (data) => {
    if (data.AuctionId === auctionId) alert(`Auction ended! Winner: ${data.WinnerUserId}, Bid: ${data.WinningBid}`);
  });
};

export const placeBid = (auctionId: string, userId: string, amount: number) => {
  fetch(`https://localhost:5001/api/auctions/${auctionId}/bid?userId=${userId}&amount=${amount}`, {
    method: 'POST'
  }).catch(err => console.error(err));
};
