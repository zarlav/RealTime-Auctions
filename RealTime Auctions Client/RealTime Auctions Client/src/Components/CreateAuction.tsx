import React, { useState } from 'react';

const CreateAuction = ({ userId }: { userId: string }) => {
    const [name, setName] = useState('');
    const [price, setPrice] = useState(0);

    const handleCreate = async () => {
        const newAuction = {
            productName: name,
            currentPrice: price,
            creatorId: userId,
            endTime: new Date(Date.now() + 5 * 60000).toISOString() 
        };

        await fetch('https://localhost:7068/api/auctions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newAuction)
        });
    };

    return (
        <div style={{ border: '1px solid black', padding: '10px', marginBottom: '20px' }}>
            <h3>Pokreni novu aukciju</h3>
            <input type="text" placeholder="Naziv proizvoda" onChange={e => setName(e.target.value)} />
            <input type="number" placeholder="Početna cijena" onChange={e => setPrice(Number(e.target.value))} />
            <button onClick={handleCreate}>Objavi aukciju</button>
        </div>
    );
};

export default CreateAuction;