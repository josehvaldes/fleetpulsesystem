

export function Login() {
    return (
        <div className="flex flex-col items-center justify-center h-screen">
            <h1 className="text-3xl font-bold mb-4">Login</h1>
            <form className="flex flex-col items-center">
                <input
                    type="text"
                    placeholder="Username"
                    className="border border-gray-300 rounded px-4 py-2 mb-4"
                />
                <input
                    type="password"
                    placeholder="Password"
                    className="border border-gray-300 rounded px-4 py-2 mb-4"
                />
                <button
                    type="submit"
                    className="bg-blue-500 text-white rounded px-4 py-2 hover:bg-blue-600"
                >
                    Login
                </button>
            </form>
        </div>
    );
}