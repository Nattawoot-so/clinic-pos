"use client";

import { useEffect, useState } from "react";
import { getUserFromToken } from "@/lib/api";

export default function NavBar() {
  const [mounted, setMounted] = useState(false);
  const [user, setUser] = useState<{ role: string; username: string } | null>(null);

  useEffect(() => {
    setUser(getUserFromToken());
    setMounted(true);
  }, []);

  // Don't render nav links until client-side hydration is complete
  if (!mounted) {
    return (
      <nav className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
          <a href="/patients" className="text-lg font-bold text-blue-600">
            Clinic POS
          </a>
          <div className="flex gap-4 text-sm items-center">
            <span className="text-gray-400">Loading...</span>
          </div>
        </div>
      </nav>
    );
  }

  const handleLogout = () => {
    localStorage.removeItem("token");
    window.location.href = "/login";
  };

  return (
    <nav className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
        <a href="/patients" className="text-lg font-bold text-blue-600">
          Clinic POS
        </a>
        <div className="flex gap-4 text-sm items-center">
          <a href="/patients" className="text-gray-600 hover:text-blue-600">
            Patients
          </a>
          <a href="/appointments" className="text-gray-600 hover:text-blue-600">
            Appointments
          </a>
          {user?.role === "Admin" && (
            <a href="/users" className="text-gray-600 hover:text-blue-600">
              Users
            </a>
          )}
          {user && (
            <span className="text-xs bg-gray-100 text-gray-500 px-2 py-1 rounded">
              {user.username || "user"} ({user.role})
            </span>
          )}
          <button
            onClick={handleLogout}
            className="text-red-500 hover:text-red-700 ml-1"
          >
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
}
