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
        const data = await response.json();
        if (isRegistering) {
          alert("Uspešna registracija! Sada se uloguj.");
          setIsRegistering(false);
        } else {
          onLogin(data); 
        }
      } else {
        const err = await response.json();
        alert(err.message || "Greška na serveru");
      }
    } catch (error) {
      console.error("Greška:", error);
      alert("Proveri da li je Backend pokrenut!");
    }
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit}>
        <h2>{isRegistering ? 'Registracija' : 'Prijava'}</h2>
        <input 
          type="text" 
          placeholder="Korisničko ime" 
          value={username} 
          onChange={(e) => setUsername(e.target.value)} 
          required 
        />
        <input 
          type="password" 
          placeholder="Lozinka" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          required 
        />
        <button type="submit">{isRegistering ? 'Napravi nalog' : 'Uloguj se'}</button>
        <p onClick={() => setIsRegistering(!isRegistering)} style={{ cursor: 'pointer', color: 'blue' }}>
          {isRegistering ? 'Već imaš nalog? Prijavi se' : 'Nemaš nalog? Registruj se'}
        </p>
      </form>
    </div>
  );
};

export default Login;