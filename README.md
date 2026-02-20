Naziv aplikacije:RealTime-Auctions
Koriscene tehnologije:
  -Backend: ASP .Net Core
  -Frontend: React, TypeScript, Vite, SignalR
  -Baza: Redis

1.Pokrenuti redis-server (preuzeto sa cs-a)
2.Pokrenuti redis-cli (preuzeto sa cs-a)
Pokretanje backend-a:cd ..RealTime-Auctions\RealTime Auctions   dotnet build -> dotnet run  (Preduslov: .Net 8 SDK)
Pokretanje frontend-a: cd ..RealTime-Auctions\RealTime Auctions Client    npm install  -> npm run dev  (Preduslov: Node.js v18+)

Potrebno je kreirati korisnika, zatim se ulogovati. Korisnik ce moci da postavi aukciju koja ce biti aktivna 5min. Minimalna ponuda za aukciju je 10 rsd. Kad istekne vreme, pobednik aukcije ce biti proglasen i zatvara se aukcija.
