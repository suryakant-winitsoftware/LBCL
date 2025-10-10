"use client";

import Link from "next/link";

export default function QuickTestPage() {
  return (
    <div style={{ padding: "40px", fontFamily: "system-ui" }}>
      <h1 style={{ fontSize: "32px", marginBottom: "20px" }}>Quick Test - Updated Features</h1>
      
      <div style={{ marginBottom: "40px", padding: "20px", backgroundColor: "#f0f0f0", borderRadius: "8px" }}>
        <p style={{ marginBottom: "10px" }}>
          <strong>Current URL:</strong> {typeof window !== "undefined" ? window.location.href : "Loading..."}
        </p>
        <p>
          <strong>Test Instructions:</strong> Click on the links below to test if the pages are accessible.
        </p>
      </div>

      <div style={{ display: "grid", gap: "20px" }}>
        <div>
          <h2 style={{ fontSize: "24px", marginBottom: "10px", color: "#2563eb" }}>Route Management</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "10px", paddingLeft: "20px" }}>
            <Link 
              href="/updatedfeatures/route-management/routes/manage" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Manage Routes
            </Link>
            <Link 
              href="/updatedfeatures/route-management/routes/create" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Create Route
            </Link>
            <Link 
              href="/updatedfeatures/route-management/routes/customer-mapping" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Customer Mapping (Store Assignment)
            </Link>
          </div>
        </div>

        <div>
          <h2 style={{ fontSize: "24px", marginBottom: "10px", color: "#7c3aed" }}>Journey Plan Management</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "10px", paddingLeft: "20px" }}>
            <Link 
              href="/updatedfeatures/journey-plan-management/view-plans" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → View Journey Plans
            </Link>
            <Link 
              href="/updatedfeatures/journey-plan-management/journey-plans/manage" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Manage Journey Plans
            </Link>
            <Link 
              href="/updatedfeatures/journey-plan-management/beat-history" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Beat History
            </Link>
          </div>
        </div>

        <div>
          <h2 style={{ fontSize: "24px", marginBottom: "10px", color: "#059669" }}>Settings</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "10px", paddingLeft: "20px" }}>
            <Link 
              href="/updatedfeatures/settings/holiday-management" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Holiday Management
            </Link>
          </div>
        </div>

        <div>
          <h2 style={{ fontSize: "24px", marginBottom: "10px", color: "#dc2626" }}>Other Links</h2>
          <div style={{ display: "flex", flexDirection: "column", gap: "10px", paddingLeft: "20px" }}>
            <Link 
              href="/dashboard" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Dashboard
            </Link>
            <Link 
              href="/updatedfeatures" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Updated Features Hub
            </Link>
            <Link 
              href="/test-routes" 
              style={{ color: "#3b82f6", textDecoration: "underline" }}
            >
              → Test Routes Page
            </Link>
          </div>
        </div>
      </div>

      <div style={{ marginTop: "40px", padding: "20px", backgroundColor: "#fef2f2", borderRadius: "8px", border: "1px solid #fecaca" }}>
        <h3 style={{ color: "#dc2626", marginBottom: "10px" }}>If pages are not loading:</h3>
        <ol style={{ paddingLeft: "20px", lineHeight: "1.8" }}>
          <li>Check browser console (F12) for errors</li>
          <li>Make sure Next.js dev server is running: <code>npm run dev</code></li>
          <li>Clear browser cache: Ctrl+Shift+R (Windows) or Cmd+Shift+R (Mac)</li>
          <li>Check if the page file exists in the correct directory</li>
          <li>Restart the Next.js dev server</li>
        </ol>
      </div>
    </div>
  );
}