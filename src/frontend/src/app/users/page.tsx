"use client";
import { useState, useEffect, useCallback } from "react";
import { apiFetch, getUserFromToken } from "@/lib/api";

interface User {
  id: string;
  username: string;
  role: string;
  tenantId: string;
}

const ROLES = ["Admin", "User", "Viewer"];

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [form, setForm] = useState({ username: "", password: "", role: "User" });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const currentUser = getUserFromToken();

  const loadUsers = useCallback(async () => {
    try {
      const data = await apiFetch<User[]>("/api/users");
      setUsers(data);
    } catch { /* handled */ }
  }, []);

  useEffect(() => {
    if (currentUser?.role !== "Admin") return;
    loadUsers();
  }, [loadUsers, currentUser?.role]);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      await apiFetch("/api/users", {
        method: "POST",
        body: JSON.stringify(form),
      });
      setForm({ username: "", password: "", role: "User" });
      setSuccess("User created successfully!");
      loadUsers();
      setTimeout(() => setSuccess(""), 3000);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to create user");
    }
  }

  async function handleRoleChange(userId: string, newRole: string) {
    try {
      await apiFetch(`/api/users/${userId}/role`, {
        method: "PUT",
        body: JSON.stringify({ role: newRole }),
      });
      loadUsers();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to update role");
    }
  }

  if (currentUser?.role !== "Admin") {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700">
        Access denied. Only <strong>Admin</strong> can manage users.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-800">User Management</h1>

      {/* Create User Form */}
      <form
        onSubmit={handleCreate}
        className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 space-y-4"
      >
        <h2 className="text-lg font-semibold text-gray-700">Create User</h2>

        {error && (
          <p className="text-red-500 text-sm bg-red-50 p-2 rounded">{error}</p>
        )}
        {success && (
          <p className="text-green-600 text-sm bg-green-50 p-2 rounded">{success}</p>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <input
            placeholder="Username *"
            value={form.username}
            onChange={(e) => setForm({ ...form, username: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
          <input
            type="password"
            placeholder="Password *"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
          <select
            value={form.role}
            onChange={(e) => setForm({ ...form, role: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            {ROLES.map((r) => (
              <option key={r} value={r}>{r}</option>
            ))}
          </select>
        </div>

        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded text-sm font-medium hover:bg-blue-700"
        >
          Create User
        </button>
      </form>

      {/* User List */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Username</th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Role</th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Change Role</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="border-b border-gray-100 hover:bg-gray-50">
                <td className="px-4 py-3">{u.username}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 rounded text-xs font-medium ${
                    u.role === "Admin" ? "bg-purple-100 text-purple-700" :
                    u.role === "User" ? "bg-blue-100 text-blue-700" :
                    "bg-gray-100 text-gray-600"
                  }`}>
                    {u.role}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <select
                    value={u.role}
                    onChange={(e) => handleRoleChange(u.id, e.target.value)}
                    className="border border-gray-300 rounded px-2 py-1 text-sm"
                  >
                    {ROLES.map((r) => (
                      <option key={r} value={r}>{r}</option>
                    ))}
                  </select>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
