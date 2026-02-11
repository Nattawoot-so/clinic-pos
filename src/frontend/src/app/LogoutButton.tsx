"use client";

export default function LogoutButton() {
  const handleLogout = () => {
    localStorage.removeItem("token");
    window.location.href = "/login";
  };

  return (
    <button
      onClick={handleLogout}
      className="text-red-500 hover:text-red-700 ml-2"
    >
      Logout
    </button>
  );
}
