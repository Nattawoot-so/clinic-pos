"use client";
import { useState, useEffect, useCallback } from "react";
import { apiFetch, getUserFromToken } from "@/lib/api";

interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  primaryBranchId: string | null;
  createdAt: string;
}

interface Branch {
  id: string;
  name: string;
}

export default function PatientsPage() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [branchFilter, setBranchFilter] = useState("");
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    phoneNumber: "",
    primaryBranchId: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const user = getUserFromToken();
  const canCreate = user?.role !== "Viewer";

  const loadPatients = useCallback(async () => {
    try {
      const query = branchFilter ? `?branchId=${branchFilter}` : "";
      const data = await apiFetch<Patient[]>(`/api/patients${query}`);
      setPatients(data);
    } catch {
      /* redirect handled by apiFetch */
    }
  }, [branchFilter]);

  useEffect(() => {
    loadPatients();
    apiFetch<Branch[]>("/api/branches").then(setBranches).catch(() => {});
  }, [loadPatients]);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      await apiFetch("/api/patients", {
        method: "POST",
        body: JSON.stringify({
          firstName: form.firstName,
          lastName: form.lastName,
          phoneNumber: form.phoneNumber,
          primaryBranchId: form.primaryBranchId || null,
        }),
      });
      setForm({
        firstName: "",
        lastName: "",
        phoneNumber: "",
        primaryBranchId: "",
      });
      setSuccess("Patient created successfully!");
      loadPatients();
      setTimeout(() => setSuccess(""), 3000);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Failed to create patient");
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-800">Patients</h1>

      {/* Create Form — hidden for Viewer */}
      {canCreate ? (
        <form
          onSubmit={handleCreate}
          className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 space-y-4"
        >
          <h2 className="text-lg font-semibold text-gray-700">
            Create Patient
          </h2>

          {error && (
            <p className="text-red-500 text-sm bg-red-50 p-2 rounded">{error}</p>
          )}
          {success && (
            <p className="text-green-600 text-sm bg-green-50 p-2 rounded">
              {success}
            </p>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <input
              placeholder="First Name *"
              value={form.firstName}
              onChange={(e) => setForm({ ...form, firstName: e.target.value })}
              className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
            <input
              placeholder="Last Name *"
              value={form.lastName}
              onChange={(e) => setForm({ ...form, lastName: e.target.value })}
              className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
            <input
              placeholder="Phone Number *"
              value={form.phoneNumber}
              onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })}
              className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
            <select
              value={form.primaryBranchId}
              onChange={(e) =>
                setForm({ ...form, primaryBranchId: e.target.value })
              }
              className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">No Primary Branch</option>
              {branches.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </select>
          </div>

          <button
            type="submit"
            className="bg-blue-600 text-white px-4 py-2 rounded text-sm font-medium hover:bg-blue-700"
          >
            Create Patient
          </button>
        </form>
      ) : (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-sm text-yellow-700">
          You are logged in as <strong>Viewer</strong>. You can view data but cannot create patients.
        </div>
      )}

      {/* Filter */}
      <div className="flex items-center gap-3">
        <label className="text-sm font-medium text-gray-600">
          Filter by Branch:
        </label>
        <select
          value={branchFilter}
          onChange={(e) => setBranchFilter(e.target.value)}
          className="border border-gray-300 rounded px-3 py-1.5 text-sm"
        >
          <option value="">All Branches</option>
          {branches.map((b) => (
            <option key={b.id} value={b.id}>
              {b.name}
            </option>
          ))}
        </select>
      </div>

      {/* Patient List */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Name
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Phone
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Branch
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Created
              </th>
            </tr>
          </thead>
          <tbody>
            {patients.length === 0 ? (
              <tr>
                <td
                  colSpan={4}
                  className="px-4 py-8 text-center text-gray-400"
                >
                  No patients found
                </td>
              </tr>
            ) : (
              patients.map((p) => (
                <tr
                  key={p.id}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="px-4 py-3">
                    {p.firstName} {p.lastName}
                  </td>
                  <td className="px-4 py-3 text-gray-500">{p.phoneNumber}</td>
                  <td className="px-4 py-3 text-gray-500">
                    {branches.find((b) => b.id === p.primaryBranchId)?.name ||
                      "—"}
                  </td>
                  <td className="px-4 py-3 text-gray-400">
                    {new Date(p.createdAt).toLocaleString()}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
