import React, { useState } from 'react';

interface Props {
  onLogin: (user: any) => void;
}

const Login: React.FC<Props> = ({ onLogin }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isRegistering, setIsRegistering] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const endpoint = isRegistering ? '/api/users/register' : '/api/auth/login';
    const url = `https://localhost:7068${endpoint}`;

    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, email: `${username}@example.com` })
      });

      if (response.ok) {
        if (isRegistering) {
          alert("Uspesna registracija! Sada se ulogujte.");
          setIsRegistering(false);
          setUsername('');
          setPassword('');
        } else {
          const data = await response.json();
          onLogin(data); 
        }
      } else {
        const err = await response.json();
        if (isRegistering && response.status === 409) {
          alert("Vec imate nalog! Molimo vas da se prijavite.");
          setIsRegistering(false); 
        } else {
          alert(err.message || "Greska na serveru");
        }
      }
    } catch (error) {
      alert("Greska u komunikaciji sa serverom.");
    }
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit}>
        <h2>{isRegistering ? 'Registracija' : 'Prijava'}</h2>
        <input type="text" placeholder="Korisnicko ime" value={username} onChange={(e) => setUsername(e.target.value)} required />
        <input type="password" placeholder="Lozinka" value={password} onChange={(e) => setPassword(e.target.value)} required />
        <button type="submit">{isRegistering ? 'Napravite nalog' : 'Ulogujte se'}</button>
        <p onClick={() => setIsRegistering(!isRegistering)} style={{ cursor: 'pointer', color: 'blue' }}>
          {isRegistering ? 'Vec imate nalog? Prijavite se' : 'Nemate nalog? Registrujte se'}
        </p>
      </form>
    </div>
  );
};

export default Login;