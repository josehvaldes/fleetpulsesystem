
import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';

export function Login() {
    const { login, isLoading } = useAuth();
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    
    const handleSubmit = async (e: React.SubmitEvent) => {
        e.preventDefault();
        setError('');
        try {
            await login(username, password);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Invalid credentials. Please try again.');
        }
    };


    return (
        <div className="flex flex-col items-center justify-center h-screen">
            <h1 className="text-3xl font-bold mb-4">Login</h1>
            <form className="flex flex-col items-center" onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Username"
                    className="border border-gray-300 rounded px-4 py-2 mb-4"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    autoFocus
                    required
                />
                <input
                    type="password"
                    placeholder="Password"
                    className="border border-gray-300 rounded px-4 py-2 mb-4"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
                {error && <div className="text-red-400 text-sm text-center">{error}</div>}


                <button
                    type="submit"
                    className="bg-blue-500 text-white rounded px-4 py-2 hover:bg-blue-600"
                    disabled={isLoading}
                >
                    {isLoading ? 'Authenticating...' : 'Sign In'}
                </button>
            </form>
        </div>
    );
}