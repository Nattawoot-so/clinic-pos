import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Clinic POS",
  description: "Clinic POS Platform",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="bg-gray-50 min-h-screen">
        <nav className="bg-white shadow-sm border-b border-gray-200">
          <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
            <a href="/patients" className="text-lg font-bold text-blue-600">
              Clinic POS
            </a>
            <div className="flex gap-4 text-sm">
              <a href="/patients" className="text-gray-600 hover:text-blue-600">
                Patients
              </a>
              <a
                href="/appointments"
                className="text-gray-600 hover:text-blue-600"
              >
                Appointments
              </a>
            </div>
          </div>
        </nav>
        <main className="max-w-5xl mx-auto px-4 py-6">{children}</main>
      </body>
    </html>
  );
}
