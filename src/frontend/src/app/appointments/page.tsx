"use client";
import { useState, useEffect, useCallback } from "react";
import { apiFetch, getUserFromToken } from "@/lib/api";

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
  phoneNumber: string;
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
  const [selected, setSelected] = useState<Appointment | null>(null);
  const [form, setForm] = useState({
    patientId: "",
    branchId: "",
    startAt: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const user = getUserFromToken();
  const canCreate = user?.role !== "Viewer";

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

  function getPatientName(id: string) {
    const p = patients.find((p) => p.id === id);
    return p ? `${p.firstName} ${p.lastName}` : id.slice(0, 8);
  }

  function getPatientPhone(id: string) {
    return patients.find((p) => p.id === id)?.phoneNumber || "—";
  }

  function getBranchName(id: string) {
    return branches.find((b) => b.id === id)?.name || id.slice(0, 8);
  }

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

      {/* Create Form — hidden for Viewer */}
      {canCreate ? (
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
      ) : (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-sm text-yellow-700">
          You are logged in as <strong>Viewer</strong>. You can view appointments but cannot create them.
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
              <th className="text-left px-4 py-3 font-medium text-gray-600">
                Action
              </th>
            </tr>
          </thead>
          <tbody>
            {appointments.length === 0 ? (
              <tr>
                <td
                  colSpan={5}
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
                  <td className="px-4 py-3">{getPatientName(a.patientId)}</td>
                  <td className="px-4 py-3 text-gray-500">
                    {getBranchName(a.branchId)}
                  </td>
                  <td className="px-4 py-3">
                    {new Date(a.startAt).toLocaleString()}
                  </td>
                  <td className="px-4 py-3 text-gray-400">
                    {new Date(a.createdAt).toLocaleString()}
                  </td>
                  <td className="px-4 py-3">
                    <button
                      onClick={() => setSelected(a)}
                      className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                    >
                      Detail
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Detail Modal */}
      {selected && (
        <div
          className="fixed inset-0 bg-black/40 flex items-center justify-center z-50"
          onClick={() => setSelected(null)}
        >
          <div
            className="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="bg-blue-600 px-6 py-4">
              <h2 className="text-lg font-semibold text-white">
                Appointment Detail
              </h2>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Patient</p>
                  <p className="font-medium text-gray-800">{getPatientName(selected.patientId)}</p>
                </div>
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Phone</p>
                  <p className="font-medium text-gray-800">{getPatientPhone(selected.patientId)}</p>
                </div>
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Branch</p>
                  <p className="font-medium text-gray-800">{getBranchName(selected.branchId)}</p>
                </div>
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Start At</p>
                  <p className="font-medium text-gray-800">{new Date(selected.startAt).toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Created At</p>
                  <p className="font-medium text-gray-800">{new Date(selected.createdAt).toLocaleString()}</p>
                </div>
                <div>
                  <p className="text-gray-400 text-xs uppercase tracking-wide mb-1">Appointment ID</p>
                  <p className="font-mono text-xs text-gray-500">{selected.id}</p>
                </div>
              </div>
            </div>
            <div className="border-t border-gray-200 px-6 py-3 flex justify-end">
              <button
                onClick={() => setSelected(null)}
                className="bg-gray-100 text-gray-700 px-4 py-2 rounded text-sm font-medium hover:bg-gray-200"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
