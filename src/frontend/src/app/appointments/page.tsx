"use client";
import { useState, useEffect, useCallback } from "react";
import { apiFetch } from "@/lib/api";

interface Appointment {
  id: string;
  patientId: string;
  branchId: string;
  startAt: string;
  createdAt: string;
}

interface Patient {
  id: string;
  firstName: string;
  lastName: string;
}

interface Branch {
  id: string;
  name: string;
}

export default function AppointmentsPage() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [branchFilter, setBranchFilter] = useState("");
  const [form, setForm] = useState({
    patientId: "",
    branchId: "",
    startAt: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const loadAppointments = useCallback(async () => {
    try {
      const query = branchFilter ? `?branchId=${branchFilter}` : "";
      const data = await apiFetch<Appointment[]>(
        `/api/appointments${query}`
      );
      setAppointments(data);
    } catch {
      /* handled */
    }
  }, [branchFilter]);

  useEffect(() => {
    loadAppointments();
    apiFetch<Patient[]>("/api/patients").then(setPatients).catch(() => {});
    apiFetch<Branch[]>("/api/branches").then(setBranches).catch(() => {});
  }, [loadAppointments]);

  async function handleCreate(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      await apiFetch("/api/appointments", {
        method: "POST",
        body: JSON.stringify({
          patientId: form.patientId,
          branchId: form.branchId,
          startAt: new Date(form.startAt).toISOString(),
        }),
      });
      setForm({ patientId: "", branchId: "", startAt: "" });
      setSuccess("Appointment created successfully!");
      loadAppointments();
      setTimeout(() => setSuccess(""), 3000);
    } catch (err: unknown) {
      setError(
        err instanceof Error ? err.message : "Failed to create appointment"
      );
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-800">Appointments</h1>

      {/* Create Form */}
      <form
        onSubmit={handleCreate}
        className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 space-y-4"
      >
        <h2 className="text-lg font-semibold text-gray-700">
          Create Appointment
        </h2>

        {error && (
          <p className="text-red-500 text-sm bg-red-50 p-2 rounded">{error}</p>
        )}
        {success && (
          <p className="text-green-600 text-sm bg-green-50 p-2 rounded">
            {success}
          </p>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <select
            value={form.patientId}
            onChange={(e) => setForm({ ...form, patientId: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          >
            <option value="">Select Patient *</option>
            {patients.map((p) => (
              <option key={p.id} value={p.id}>
                {p.firstName} {p.lastName}
              </option>
            ))}
          </select>

          <select
            value={form.branchId}
            onChange={(e) => setForm({ ...form, branchId: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          >
            <option value="">Select Branch *</option>
            {branches.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>

          <input
            type="datetime-local"
            value={form.startAt}
            onChange={(e) => setForm({ ...form, startAt: e.target.value })}
            className="border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded text-sm font-medium hover:bg-blue-700"
        >
          Create Appointment
        </button>
      </form>

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

      {/* Appointment List */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Patient
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Branch
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Start At
              </th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Created
              </th>
            </tr>
          </thead>
          <tbody>
            {appointments.length === 0 ? (
              <tr>
                <td
                  colSpan={4}
                  className="px-4 py-8 text-center text-gray-400"
                >
                  No appointments found
                </td>
              </tr>
            ) : (
              appointments.map((a) => (
                <tr
                  key={a.id}
                  className="border-b border-gray-100 hover:bg-gray-50"
                >
                  <td className="px-4 py-3">
                    {patients.find((p) => p.id === a.patientId)
                      ? `${patients.find((p) => p.id === a.patientId)!.firstName} ${patients.find((p) => p.id === a.patientId)!.lastName}`
                      : a.patientId.slice(0, 8)}
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {branches.find((b) => b.id === a.branchId)?.name ||
                      a.branchId.slice(0, 8)}
                  </td>
                  <td className="px-4 py-3">
                    {new Date(a.startAt).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-gray-400">
                    {new Date(a.createdAt).toLocaleString()}
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
