#!/usr/bin/env python3
"""
MSSQL Professional Workbench
Advanced database management tool with auto-query generation, import/export, bulk operations
"""

from flask import Flask, render_template_string, request, jsonify, send_file
import pymssql
import json
import csv
import io
from datetime import datetime, date
from werkzeug.utils import secure_filename

app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 50 * 1024 * 1024  # 50MB max file size

# Database configuration
DB_CONFIG = {
    'server': '10.20.53.175',
    'database': 'LBSSFADev',
    'username': 'lbssfadev',
    'password': 'lbssfadev'
}

def get_connection():
    """Create and return database connection"""
    return pymssql.connect(
        server=DB_CONFIG['server'],
        user=DB_CONFIG['username'],
        password=DB_CONFIG['password'],
        database=DB_CONFIG['database'],
        as_dict=False
    )

def serialize_value(val):
    """Convert value to JSON-serializable format"""
    if isinstance(val, (datetime, date)):
        return val.isoformat()
    elif val is None:
        return None
    elif isinstance(val, (bytes, bytearray)):
        return val.decode('utf-8', errors='ignore')
    else:
        return str(val)

# HTML Template with enhanced features
HTML_TEMPLATE = """
<!DOCTYPE html>
<html>
<head>
    <title>MSSQL Professional Workbench</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #f5f5f7;
            color: #1d1d1f;
        }

        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px 30px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }

        .header h1 {
            font-size: 24px;
            font-weight: 600;
        }

        .header p {
            opacity: 0.9;
            margin-top: 5px;
            font-size: 14px;
        }

        .container {
            display: flex;
            height: calc(100vh - 100px);
        }

        .sidebar {
            width: 300px;
            background: white;
            border-right: 1px solid #e5e5e7;
            overflow-y: auto;
            padding: 20px;
        }

        .sidebar h3 {
            font-size: 14px;
            color: #86868b;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 15px;
        }

        .search-tables {
            width: 100%;
            padding: 8px 12px;
            border: 1px solid #d2d2d7;
            border-radius: 6px;
            margin-bottom: 15px;
            font-size: 13px;
        }

        .table-list {
            list-style: none;
        }

        .table-item {
            padding: 10px 15px;
            margin: 5px 0;
            cursor: pointer;
            border-radius: 8px;
            transition: all 0.2s;
            font-size: 14px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .table-item:hover {
            background: #f5f5f7;
        }

        .table-item.active {
            background: #007aff;
            color: white;
        }

        .table-count {
            font-size: 11px;
            background: rgba(0,0,0,0.1);
            padding: 2px 8px;
            border-radius: 12px;
        }

        .main-content {
            flex: 1;
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }

        .tabs {
            display: flex;
            background: white;
            border-bottom: 1px solid #e5e5e7;
            padding: 0 20px;
        }

        .tab {
            padding: 15px 25px;
            cursor: pointer;
            border-bottom: 3px solid transparent;
            transition: all 0.2s;
            font-size: 14px;
            font-weight: 500;
        }

        .tab:hover {
            background: #f5f5f7;
        }

        .tab.active {
            border-bottom-color: #007aff;
            color: #007aff;
        }

        .tab-content {
            display: none;
            padding: 20px;
            overflow: auto;
            flex: 1;
        }

        .tab-content.active {
            display: block;
        }

        .toolbar {
            background: white;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            display: flex;
            gap: 10px;
            flex-wrap: wrap;
            align-items: center;
        }

        .query-editor {
            margin-bottom: 15px;
        }

        textarea {
            width: 100%;
            min-height: 150px;
            padding: 15px;
            border: 1px solid #d2d2d7;
            border-radius: 8px;
            font-family: 'Monaco', 'Menlo', monospace;
            font-size: 13px;
            resize: vertical;
        }

        textarea:focus, input:focus, select:focus {
            outline: none;
            border-color: #007aff;
        }

        .btn {
            background: #007aff;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.2s;
            display: inline-flex;
            align-items: center;
            gap: 5px;
        }

        .btn:hover {
            background: #0051d5;
            transform: translateY(-1px);
        }

        .btn-danger {
            background: #ff3b30;
        }

        .btn-danger:hover {
            background: #d70015;
        }

        .btn-success {
            background: #34c759;
        }

        .btn-success:hover {
            background: #248a3d;
        }

        .btn-secondary {
            background: #8e8e93;
        }

        .btn-secondary:hover {
            background: #636366;
        }

        .btn-warning {
            background: #ff9500;
        }

        .btn-warning:hover {
            background: #cc7700;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }

        th {
            background: #f5f5f7;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            font-size: 13px;
            color: #1d1d1f;
            border-bottom: 2px solid #e5e5e7;
            position: sticky;
            top: 0;
        }

        td {
            padding: 12px;
            border-bottom: 1px solid #f5f5f7;
            font-size: 13px;
        }

        tr:hover {
            background: #fafafa;
        }

        .result-info {
            background: white;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-size: 13px;
            color: #86868b;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .error {
            background: #fff5f5;
            color: #ff3b30;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-size: 13px;
            border-left: 3px solid #ff3b30;
        }

        .success {
            background: #f0fdf4;
            color: #34c759;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-size: 13px;
            border-left: 3px solid #34c759;
        }

        .info {
            background: #f0f9ff;
            color: #007aff;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-size: 13px;
            border-left: 3px solid #007aff;
        }

        .action-btn {
            padding: 5px 10px;
            margin: 2px;
            font-size: 12px;
        }

        .pagination {
            display: flex;
            gap: 10px;
            margin-top: 15px;
            align-items: center;
        }

        .loading {
            text-align: center;
            padding: 40px;
            color: #86868b;
        }

        .modal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            z-index: 1000;
            align-items: center;
            justify-content: center;
        }

        .modal.active {
            display: flex;
        }

        .modal-content {
            background: white;
            padding: 30px;
            border-radius: 12px;
            max-width: 600px;
            width: 90%;
            max-height: 80vh;
            overflow-y: auto;
        }

        .modal-header {
            font-size: 20px;
            font-weight: 600;
            margin-bottom: 20px;
        }

        .form-group {
            margin-bottom: 15px;
        }

        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            font-size: 13px;
        }

        .form-group input, .form-group select, .form-group textarea {
            width: 100%;
            padding: 10px;
            border: 1px solid #d2d2d7;
            border-radius: 6px;
            font-size: 14px;
        }

        .checkbox-container {
            display: flex;
            align-items: center;
            gap: 5px;
        }

        .checkbox-container input[type="checkbox"] {
            width: auto;
        }

        .query-templates {
            background: white;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
        }

        .query-templates h4 {
            margin-bottom: 10px;
            font-size: 14px;
        }

        .template-btn {
            display: inline-block;
            padding: 8px 12px;
            margin: 5px;
            background: #f5f5f7;
            border-radius: 6px;
            cursor: pointer;
            font-size: 12px;
            transition: all 0.2s;
        }

        .template-btn:hover {
            background: #e5e5e7;
        }

        input[type="file"] {
            padding: 10px;
            border: 1px solid #d2d2d7;
            border-radius: 6px;
            font-size: 13px;
        }
    </style>
</head>
<body>
    <div class="header">
        <h1>🗄️ MSSQL Professional Workbench</h1>
        <p>Server: {{ config.server }} | Database: {{ config.database }}</p>
    </div>

    <div class="container">
        <div class="sidebar">
            <h3>Tables</h3>
            <input type="text" class="search-tables" id="searchTables" placeholder="Search tables..." onkeyup="filterTables()">
            <ul class="table-list" id="tableList">
                <li class="loading">Loading tables...</li>
            </ul>
        </div>

        <div class="main-content">
            <div class="tabs">
                <div class="tab active" onclick="switchTab('data')">📊 Browse Data</div>
                <div class="tab" onclick="switchTab('query')">⚡ Query Builder</div>
                <div class="tab" onclick="switchTab('structure')">🔧 Structure</div>
                <div class="tab" onclick="switchTab('import')">📥 Import/Export</div>
            </div>

            <!-- Browse Data Tab -->
            <div id="data-tab" class="tab-content active">
                <div class="toolbar">
                    <input type="text" id="searchInput" placeholder="Search..." style="flex: 1; padding: 10px; border: 1px solid #d2d2d7; border-radius: 6px;">
                    <button class="btn" onclick="loadTableData()">🔍 Search</button>
                    <button class="btn btn-success" onclick="showInsertModal()">➕ Insert Row</button>
                    <button class="btn btn-warning" onclick="showBulkDeleteModal()">🗑️ Bulk Delete</button>
                    <button class="btn btn-secondary" onclick="exportCurrentTable()">💾 Export CSV</button>
                    <button class="btn btn-secondary" onclick="refreshData()">🔄 Refresh</button>
                </div>

                <div id="dataResult"></div>

                <div class="pagination">
                    <button class="btn" onclick="firstPage()">⏮️ First</button>
                    <button class="btn" onclick="previousPage()">← Previous</button>
                    <span id="pageInfo">Page 1</span>
                    <button class="btn" onclick="nextPage()">Next →</button>
                    <button class="btn" onclick="lastPage()">Last ⏭️</button>
                    <select id="pageSizeSelect" onchange="changePageSize()" style="padding: 10px; border-radius: 6px; border: 1px solid #d2d2d7;">
                        <option value="25">25 rows</option>
                        <option value="50" selected>50 rows</option>
                        <option value="100">100 rows</option>
                        <option value="500">500 rows</option>
                    </select>
                </div>
            </div>

            <!-- Query Builder Tab -->
            <div id="query-tab" class="tab-content">
                <div class="query-templates">
                    <h4>📝 Quick Query Templates:</h4>
                    <span class="template-btn" onclick="generateSelectAll()">SELECT * FROM table</span>
                    <span class="template-btn" onclick="generateSelectWhere()">SELECT with WHERE</span>
                    <span class="template-btn" onclick="generateCount()">COUNT(*)</span>
                    <span class="template-btn" onclick="generateInsert()">INSERT INTO</span>
                    <span class="template-btn" onclick="generateUpdate()">UPDATE</span>
                    <span class="template-btn" onclick="generateDelete()">DELETE</span>
                    <span class="template-btn" onclick="generateJoin()">JOIN Tables</span>
                </div>

                <div class="query-editor">
                    <textarea id="queryInput" placeholder="Your SQL query will appear here...&#10;&#10;Or write your own custom query"></textarea>
                </div>
                <div style="margin-bottom: 15px;">
                    <button class="btn" onclick="executeQuery()">▶️ Execute Query</button>
                    <button class="btn btn-success" onclick="formatQuery()">✨ Format SQL</button>
                    <button class="btn btn-secondary" onclick="saveQuery()">💾 Save Query</button>
                    <button class="btn btn-danger" onclick="clearQuery()">🗑️ Clear</button>
                </div>

                <div id="queryResult"></div>
            </div>

            <!-- Structure Tab -->
            <div id="structure-tab" class="tab-content">
                <div id="structureResult"></div>
            </div>

            <!-- Import/Export Tab -->
            <div id="import-tab" class="tab-content">
                <div style="max-width: 800px;">
                    <h3 style="margin-bottom: 20px;">📥 Import Data from CSV</h3>
                    <div class="form-group">
                        <label>Select Table:</label>
                        <select id="importTable" style="padding: 10px; border-radius: 6px; border: 1px solid #d2d2d7; width: 100%;">
                            <option value="">Choose a table...</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>CSV File:</label>
                        <input type="file" id="csvFile" accept=".csv">
                    </div>
                    <button class="btn" onclick="importCSV()">📤 Import Data</button>

                    <hr style="margin: 40px 0; border: none; border-top: 1px solid #e5e5e7;">

                    <h3 style="margin-bottom: 20px;">💾 Export Data</h3>
                    <div class="form-group">
                        <label>Select Table to Export:</label>
                        <select id="exportTable" style="padding: 10px; border-radius: 6px; border: 1px solid #d2d2d7; width: 100%;">
                            <option value="">Choose a table...</option>
                        </select>
                    </div>
                    <button class="btn btn-success" onclick="exportTableToCSV()">💾 Export to CSV</button>
                    <button class="btn btn-secondary" onclick="exportTableToJSON()">💾 Export to JSON</button>

                    <div id="importResult"></div>
                </div>
            </div>
        </div>
    </div>

    <!-- Insert Modal -->
    <div id="insertModal" class="modal">
        <div class="modal-content">
            <div class="modal-header">➕ Insert New Row</div>
            <div id="insertForm"></div>
            <div style="display: flex; gap: 10px; margin-top: 20px;">
                <button class="btn" onclick="submitInsert()">Save</button>
                <button class="btn btn-secondary" onclick="closeModal('insertModal')">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Edit Modal -->
    <div id="editModal" class="modal">
        <div class="modal-content">
            <div class="modal-header">✏️ Edit Row</div>
            <div id="editForm"></div>
            <div style="display: flex; gap: 10px; margin-top: 20px;">
                <button class="btn" onclick="submitEdit()">Update</button>
                <button class="btn btn-secondary" onclick="closeModal('editModal')">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Bulk Delete Modal -->
    <div id="bulkDeleteModal" class="modal">
        <div class="modal-content">
            <div class="modal-header">🗑️ Bulk Delete Rows</div>
            <div class="form-group">
                <label>Enter DELETE query:</label>
                <textarea id="bulkDeleteQuery" placeholder="DELETE FROM table_name WHERE condition" style="min-height: 100px;"></textarea>
            </div>
            <div class="info">⚠️ Warning: This will permanently delete rows. Make sure your WHERE clause is correct!</div>
            <div style="display: flex; gap: 10px; margin-top: 20px;">
                <button class="btn btn-danger" onclick="submitBulkDelete()">Delete Rows</button>
                <button class="btn btn-secondary" onclick="closeModal('bulkDeleteModal')">Cancel</button>
            </div>
        </div>
    </div>

    <script>
        let currentTable = '';
        let currentPage = 1;
        let pageSize = 50;
        let totalPages = 1;
        let currentTableColumns = [];
        let currentEditRow = null;
        let allTables = [];

        window.onload = function() {
            loadTables();
        };

        function loadTables() {
            console.log('loadTables() called');
            fetch('/api/tables')
                .then(res => {
                    console.log('Response received:', res.status);
                    return res.json();
                })
                .then(data => {
                    console.log('Tables loaded:', data.tables.length);
                    allTables = data.tables;
                    renderTables(allTables);
                    populateTableSelects();
                })
                .catch(err => {
                    console.error('Error loading tables:', err);
                    document.getElementById('tableList').innerHTML = '<li class="error">Failed to load tables: ' + err.message + '</li>';
                });
        }

        function renderTables(tables) {
            const tableList = document.getElementById('tableList');
            tableList.innerHTML = '';

            tables.forEach(table => {
                const li = document.createElement('li');
                li.className = 'table-item';
                li.innerHTML = `<span>${table.name}</span><span class="table-count">${table.rows || '?'} rows</span>`;
                li.onclick = () => selectTable(table.name);
                tableList.appendChild(li);
            });
        }

        function filterTables() {
            const search = document.getElementById('searchTables').value.toLowerCase();
            const filtered = allTables.filter(t => t.name.toLowerCase().includes(search));
            renderTables(filtered);
        }

        function populateTableSelects() {
            try {
                const importSelect = document.getElementById('importTable');
                const exportSelect = document.getElementById('exportTable');

                if (!importSelect || !exportSelect) {
                    console.warn('Import/Export select elements not found');
                    return;
                }

                allTables.forEach(table => {
                    const option1 = document.createElement('option');
                    option1.value = table.name;
                    option1.textContent = table.name;
                    importSelect.appendChild(option1);

                    const option2 = document.createElement('option');
                    option2.value = table.name;
                    option2.textContent = table.name;
                    exportSelect.appendChild(option2);
                });
                console.log('Table selects populated');
            } catch (err) {
                console.error('Error in populateTableSelects:', err);
            }
        }

        function selectTable(tableName) {
            currentTable = tableName;
            currentPage = 1;

            document.querySelectorAll('.table-item').forEach(item => {
                item.classList.remove('active');
                if (item.textContent.includes(tableName)) {
                    item.classList.add('active');
                }
            });

            loadTableData();
            loadTableStructure();
        }

        function loadTableData() {
            if (!currentTable) return;

            const search = document.getElementById('searchInput').value;
            const url = `/api/table-data?table=${currentTable}&page=${currentPage}&size=${pageSize}&search=${encodeURIComponent(search)}`;

            document.getElementById('dataResult').innerHTML = '<div class="loading">Loading data...</div>';

            fetch(url)
                .then(res => res.json())
                .then(data => {
                    if (data.error) {
                        document.getElementById('dataResult').innerHTML = `<div class="error">${data.error}</div>`;
                        return;
                    }

                    currentTableColumns = data.columns;
                    totalPages = Math.ceil(data.total / pageSize);

                    let html = `<div class="result-info">
                        <span>Showing ${data.rows.length} of ${data.total} rows</span>
                        <span>Page ${currentPage} of ${totalPages}</span>
                    </div>`;

                    if (data.rows.length > 0) {
                        html += '<div style="overflow-x: auto;"><table><thead><tr>';
                        html += '<th><input type="checkbox" id="selectAll" onchange="toggleSelectAll()"></th>';
                        data.columns.forEach(col => {
                            html += `<th>${col}</th>`;
                        });
                        html += '<th>Actions</th></tr></thead><tbody>';

                        data.rows.forEach((row, idx) => {
                            html += '<tr>';
                            html += `<td><input type="checkbox" class="row-checkbox" data-row='${JSON.stringify(row)}'></td>`;
                            data.columns.forEach(col => {
                                let value = row[col];
                                if (value === null) value = '<em style="color: #86868b;">NULL</em>';
                                else if (typeof value === 'string' && value.length > 50) value = value.substring(0, 50) + '...';
                                html += `<td>${value}</td>`;
                            });
                            html += `<td style="white-space: nowrap;">
                                <button class="btn action-btn" onclick='viewRow(${JSON.stringify(row)})'>👁️ View</button>
                                <button class="btn action-btn" onclick='editRow(${JSON.stringify(row)})'>✏️ Edit</button>
                                <button class="btn action-btn" onclick='copyRow(${JSON.stringify(row)})'>📋 Copy</button>
                                <button class="btn btn-danger action-btn" onclick='deleteRow(${JSON.stringify(row)})'>🗑️</button>
                            </td>`;
                            html += '</tr>';
                        });

                        html += '</tbody></table></div>';
                    } else {
                        html += '<div class="result-info">No data found</div>';
                    }

                    document.getElementById('dataResult').innerHTML = html;
                    document.getElementById('pageInfo').textContent = `Page ${currentPage} of ${totalPages}`;
                })
                .catch(err => {
                    document.getElementById('dataResult').innerHTML = `<div class="error">Error: ${err.message}</div>`;
                });
        }

        function loadTableStructure() {
            if (!currentTable) return;

            fetch(`/api/table-structure?table=${currentTable}`)
                .then(res => res.json())
                .then(data => {
                    if (data.error) {
                        document.getElementById('structureResult').innerHTML = `<div class="error">${data.error}</div>`;
                        return;
                    }

                    let html = `<h3 style="margin-bottom: 20px;">Table: ${currentTable}</h3>`;
                    html += '<table><thead><tr><th>Column</th><th>Type</th><th>Nullable</th><th>Default</th><th>Key</th></tr></thead><tbody>';

                    data.columns.forEach(col => {
                        html += `<tr>
                            <td><strong>${col.column_name}</strong></td>
                            <td>${col.data_type}</td>
                            <td>${col.is_nullable}</td>
                            <td>${col.column_default || '<em style="color: #86868b;">NULL</em>'}</td>
                            <td>${col.is_identity ? '🔑 Identity' : ''}</td>
                        </tr>`;
                    });

                    html += '</tbody></table>';

                    html += `<div style="margin-top: 20px;">
                        <button class="btn" onclick="generateCreateTable()">📋 Copy CREATE TABLE</button>
                        <button class="btn btn-secondary" onclick="analyzeTable()">📊 Analyze Table</button>
                    </div>`;

                    document.getElementById('structureResult').innerHTML = html;
                })
                .catch(err => {
                    document.getElementById('structureResult').innerHTML = `<div class="error">Error: ${err.message}</div>`;
                });
        }

        function executeQuery() {
            const query = document.getElementById('queryInput').value.trim();
            if (!query) {
                alert('Please enter a query');
                return;
            }

            document.getElementById('queryResult').innerHTML = '<div class="loading">Executing query...</div>';

            fetch('/api/execute-query', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({query: query})
            })
                .then(res => res.json())
                .then(data => {
                    if (data.error) {
                        document.getElementById('queryResult').innerHTML = `<div class="error">${data.error}</div>`;
                        return;
                    }

                    let html = '';

                    if (data.message) {
                        html = `<div class="success">${data.message}</div>`;
                    } else if (data.rows && data.rows.length > 0) {
                        html = `<div class="result-info">
                            <span>${data.rows.length} rows returned</span>
                            <button class="btn btn-secondary" onclick="exportQueryResults()">💾 Export Results</button>
                        </div>`;
                        html += '<div style="overflow-x: auto;"><table><thead><tr>';

                        data.columns.forEach(col => {
                            html += `<th>${col}</th>`;
                        });

                        html += '</tr></thead><tbody>';

                        data.rows.forEach(row => {
                            html += '<tr>';
                            data.columns.forEach(col => {
                                let value = row[col];
                                if (value === null) value = '<em style="color: #86868b;">NULL</em>';
                                html += `<td>${value}</td>`;
                            });
                            html += '</tr>';
                        });

                        html += '</tbody></table></div>';
                    } else {
                        html = '<div class="result-info">Query executed successfully with no results</div>';
                    }

                    document.getElementById('queryResult').innerHTML = html;
                })
                .catch(err => {
                    document.getElementById('queryResult').innerHTML = `<div class="error">Error: ${err.message}</div>`;
                });
        }

        function deleteRow(row) {
            if (!confirm('Are you sure you want to delete this row?')) return;

            fetch('/api/delete-row', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    table: currentTable,
                    row: row
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        showNotification('Row deleted successfully!', 'success');
                        loadTableData();
                    } else {
                        showNotification('Error: ' + data.error, 'error');
                    }
                })
                .catch(err => showNotification('Error: ' + err.message, 'error'));
        }

        function viewRow(row) {
            let html = '<table style="width: 100%;"><tr><th>Column</th><th>Value</th></tr>';
            for (let key in row) {
                html += `<tr><td><strong>${key}</strong></td><td>${row[key] || '<em>NULL</em>'}</td></tr>`;
            }
            html += '</table>';

            document.getElementById('editModal').querySelector('.modal-header').textContent = '👁️ View Row';
            document.getElementById('editForm').innerHTML = html;
            document.getElementById('editModal').querySelector('.btn').style.display = 'none';
            openModal('editModal');
        }

        function copyRow(row) {
            const text = JSON.stringify(row, null, 2);
            navigator.clipboard.writeText(text).then(() => {
                showNotification('Row data copied to clipboard!', 'success');
            });
        }

        // Query Templates
        function generateSelectAll() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            document.getElementById('queryInput').value = `SELECT * FROM ${currentTable}`;
        }

        function generateSelectWhere() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            document.getElementById('queryInput').value = `SELECT * FROM ${currentTable}\nWHERE column_name = 'value'`;
        }

        function generateCount() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            document.getElementById('queryInput').value = `SELECT COUNT(*) as TotalRows FROM ${currentTable}`;
        }

        function generateInsert() {
            if (!currentTable || currentTableColumns.length === 0) {
                alert('Please select a table first');
                return;
            }
            const cols = currentTableColumns.join(', ');
            const vals = currentTableColumns.map(() => "'value'").join(', ');
            document.getElementById('queryInput').value = `INSERT INTO ${currentTable} (${cols})\nVALUES (${vals})`;
        }

        function generateUpdate() {
            if (!currentTable || currentTableColumns.length === 0) {
                alert('Please select a table first');
                return;
            }
            const sets = currentTableColumns.map(col => `${col} = 'value'`).join(',\\n       ');
            document.getElementById('queryInput').value = `UPDATE ${currentTable}\nSET    ${sets}\nWHERE  condition`;
        }

        function generateDelete() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            document.getElementById('queryInput').value = `DELETE FROM ${currentTable}\nWHERE condition`;
        }

        function generateJoin() {
            document.getElementById('queryInput').value = `SELECT t1.*, t2.*\nFROM table1 t1\nJOIN table2 t2 ON t1.id = t2.table1_id`;
        }

        function clearQuery() {
            document.getElementById('queryInput').value = '';
            document.getElementById('queryResult').innerHTML = '';
        }

        function formatQuery() {
            const query = document.getElementById('queryInput').value;
            // Basic SQL formatting
            const formatted = query
                .replace(/\\s+/g, ' ')
                .replace(/SELECT/gi, '\\nSELECT')
                .replace(/FROM/gi, '\\nFROM')
                .replace(/WHERE/gi, '\\nWHERE')
                .replace(/JOIN/gi, '\\nJOIN')
                .replace(/AND/gi, '\\n  AND')
                .replace(/OR/gi, '\\n  OR')
                .trim();
            document.getElementById('queryInput').value = formatted;
        }

        function saveQuery() {
            const query = document.getElementById('queryInput').value;
            const blob = new Blob([query], {type: 'text/plain'});
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `query_${new Date().getTime()}.sql`;
            a.click();
        }

        function switchTab(tab) {
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));

            event.target.classList.add('active');
            document.getElementById(tab + '-tab').classList.add('active');
        }

        function nextPage() {
            if (currentPage < totalPages) {
                currentPage++;
                loadTableData();
            }
        }

        function previousPage() {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        }

        function firstPage() {
            currentPage = 1;
            loadTableData();
        }

        function lastPage() {
            currentPage = totalPages;
            loadTableData();
        }

        function changePageSize() {
            pageSize = parseInt(document.getElementById('pageSizeSelect').value);
            currentPage = 1;
            loadTableData();
        }

        function refreshData() {
            loadTableData();
        }

        function exportCurrentTable() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            window.location.href = `/api/export-csv?table=${currentTable}`;
        }

        function exportTableToCSV() {
            const table = document.getElementById('exportTable').value;
            if (!table) {
                alert('Please select a table');
                return;
            }
            window.location.href = `/api/export-csv?table=${table}`;
        }

        function exportTableToJSON() {
            const table = document.getElementById('exportTable').value;
            if (!table) {
                alert('Please select a table');
                return;
            }
            window.location.href = `/api/export-json?table=${table}`;
        }

        function showInsertModal() {
            if (!currentTable || currentTableColumns.length === 0) {
                alert('Please select a table first');
                return;
            }

            let html = '';
            currentTableColumns.forEach(col => {
                html += `<div class="form-group">
                    <label>${col}</label>
                    <input type="text" id="insert_${col}" name="${col}">
                </div>`;
            });

            document.getElementById('insertForm').innerHTML = html;
            openModal('insertModal');
        }

        function showBulkDeleteModal() {
            if (!currentTable) {
                alert('Please select a table first');
                return;
            }
            document.getElementById('bulkDeleteQuery').value = `DELETE FROM ${currentTable}\nWHERE `;
            openModal('bulkDeleteModal');
        }

        function editRow(row) {
            currentEditRow = row;
            let html = '';
            for (let key in row) {
                html += `<div class="form-group">
                    <label>${key}</label>
                    <input type="text" id="edit_${key}" name="${key}" value="${row[key] || ''}">
                </div>`;
            }
            document.getElementById('editForm').innerHTML = html;
            document.getElementById('editModal').querySelector('.modal-header').textContent = '✏️ Edit Row';
            document.getElementById('editModal').querySelector('.btn').style.display = 'inline-flex';
            openModal('editModal');
        }

        function submitEdit() {
            const formData = {};
            const inputs = document.getElementById('editForm').querySelectorAll('input');
            inputs.forEach(input => {
                formData[input.name] = input.value;
            });

            fetch('/api/update-row', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    table: currentTable,
                    oldRow: currentEditRow,
                    newRow: formData
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        showNotification('Row updated successfully!', 'success');
                        closeModal('editModal');
                        loadTableData();
                    } else {
                        showNotification('Error: ' + data.error, 'error');
                    }
                })
                .catch(err => showNotification('Error: ' + err.message, 'error'));
        }

        function submitInsert() {
            const formData = {};
            const inputs = document.getElementById('insertForm').querySelectorAll('input');
            inputs.forEach(input => {
                if (input.value) {
                    formData[input.name] = input.value;
                }
            });

            fetch('/api/insert-row', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    table: currentTable,
                    data: formData
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        showNotification('Row inserted successfully!', 'success');
                        closeModal('insertModal');
                        loadTableData();
                    } else {
                        showNotification('Error: ' + data.error, 'error');
                    }
                })
                .catch(err => showNotification('Error: ' + err.message, 'error'));
        }

        function submitBulkDelete() {
            const query = document.getElementById('bulkDeleteQuery').value;
            if (!query.trim().toUpperCase().startsWith('DELETE')) {
                alert('Query must start with DELETE');
                return;
            }

            if (!confirm('Are you sure? This will permanently delete rows!')) return;

            fetch('/api/execute-query', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({query: query})
            })
                .then(res => res.json())
                .then(data => {
                    if (data.message) {
                        showNotification(data.message, 'success');
                        closeModal('bulkDeleteModal');
                        loadTableData();
                    } else {
                        showNotification('Error: ' + data.error, 'error');
                    }
                })
                .catch(err => showNotification('Error: ' + err.message, 'error'));
        }

        function openModal(modalId) {
            document.getElementById(modalId).classList.add('active');
        }

        function closeModal(modalId) {
            document.getElementById(modalId).classList.remove('active');
        }

        function showNotification(message, type) {
            alert(message);
        }

        function toggleSelectAll() {
            const checked = document.getElementById('selectAll').checked;
            document.querySelectorAll('.row-checkbox').forEach(cb => cb.checked = checked);
        }
    </script>
</body>
</html>
"""

@app.route('/')
def index():
    """Main page"""
    return render_template_string(HTML_TEMPLATE, config=DB_CONFIG)

@app.route('/api/tables')
def get_tables():
    """Get list of all tables"""
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute("""
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME
        """)

        tables = []
        for row in cursor.fetchall():
            tables.append({
                'name': row[0],
                'rows': None
            })

        cursor.close()
        conn.close()

        return jsonify({'tables': tables})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/table-data')
def get_table_data():
    """Get data from a specific table"""
    try:
        table_name = request.args.get('table')
        page = int(request.args.get('page', 1))
        size = int(request.args.get('size', 50))
        search = request.args.get('search', '')

        offset = (page - 1) * size

        conn = get_connection()
        cursor = conn.cursor()

        # Get total count
        count_query = f"SELECT COUNT(*) FROM {table_name}"
        if search:
            count_query += f" WHERE CAST((SELECT * FROM {table_name} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NVARCHAR(MAX)) LIKE ?"
            cursor.execute(count_query, (f'%{search}%',))
        else:
            cursor.execute(count_query)

        total = cursor.fetchone()[0]

        # Get data
        query = f"SELECT * FROM {table_name}"
        if search:
            query += f" WHERE CAST((SELECT * FROM {table_name} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NVARCHAR(MAX)) LIKE ?"
        query += f" ORDER BY 1 OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY"

        if search:
            cursor.execute(query, (f'%{search}%',))
        else:
            cursor.execute(query)

        columns = [column[0] for column in cursor.description]
        rows = []

        for row in cursor.fetchall():
            row_dict = {}
            for idx, col in enumerate(columns):
                row_dict[col] = serialize_value(row[idx])
            rows.append(row_dict)

        cursor.close()
        conn.close()

        return jsonify({
            'columns': columns,
            'rows': rows,
            'page': page,
            'size': size,
            'total': total
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/table-structure')
def get_table_structure():
    """Get structure of a table"""
    try:
        table_name = request.args.get('table')

        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute("""
            SELECT
                c.COLUMN_NAME as column_name,
                c.DATA_TYPE as data_type,
                c.IS_NULLABLE as is_nullable,
                c.COLUMN_DEFAULT as column_default,
                c.CHARACTER_MAXIMUM_LENGTH as max_length,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') as is_identity
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_NAME = ?
            ORDER BY c.ORDINAL_POSITION
        """, (table_name,))

        columns = []
        for row in cursor.fetchall():
            columns.append({
                'column_name': row[0],
                'data_type': row[1] + (f'({row[4]})' if row[4] else ''),
                'is_nullable': row[2],
                'column_default': row[3],
                'is_identity': row[5] == 1
            })

        cursor.close()
        conn.close()

        return jsonify({'columns': columns})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/execute-query', methods=['POST'])
def execute_query():
    """Execute a custom SQL query"""
    try:
        data = request.get_json()
        query = data.get('query', '').strip()

        if not query:
            return jsonify({'error': 'No query provided'}), 400

        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(query)

        if cursor.description:
            columns = [column[0] for column in cursor.description]
            rows = []

            for row in cursor.fetchall():
                row_dict = {}
                for idx, col in enumerate(columns):
                    row_dict[col] = serialize_value(row[idx])
                rows.append(row_dict)

            cursor.close()
            conn.close()

            return jsonify({
                'columns': columns,
                'rows': rows
            })
        else:
            conn.commit()
            affected = cursor.rowcount
            cursor.close()
            conn.close()

            return jsonify({
                'message': f'Query executed successfully. {affected} row(s) affected.'
            })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/delete-row', methods=['POST'])
def delete_row():
    """Delete a specific row"""
    try:
        data = request.get_json()
        table_name = data.get('table')
        row_data = data.get('row')

        where_parts = []
        params = []
        for key, value in row_data.items():
            if value is None or value == 'NULL':
                where_parts.append(f"{key} IS NULL")
            else:
                where_parts.append(f"{key} = ?")
                params.append(value)

        where_clause = " AND ".join(where_parts)
        query = f"DELETE FROM {table_name} WHERE {where_clause}"

        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(query, params)
        conn.commit()

        cursor.close()
        conn.close()

        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/insert-row', methods=['POST'])
def insert_row():
    """Insert a new row"""
    try:
        data = request.get_json()
        table_name = data.get('table')
        row_data = data.get('data')

        columns = ', '.join(row_data.keys())
        placeholders = ', '.join(['?' for _ in row_data])
        values = list(row_data.values())

        query = f"INSERT INTO {table_name} ({columns}) VALUES ({placeholders})"

        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(query, values)
        conn.commit()

        cursor.close()
        conn.close()

        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/update-row', methods=['POST'])
def update_row():
    """Update a row"""
    try:
        data = request.get_json()
        table_name = data.get('table')
        old_row = data.get('oldRow')
        new_row = data.get('newRow')

        # Build SET clause
        set_parts = []
        set_params = []
        for key, value in new_row.items():
            set_parts.append(f"{key} = ?")
            set_params.append(value)

        # Build WHERE clause
        where_parts = []
        where_params = []
        for key, value in old_row.items():
            if value is None or value == 'NULL':
                where_parts.append(f"{key} IS NULL")
            else:
                where_parts.append(f"{key} = ?")
                where_params.append(value)

        set_clause = ", ".join(set_parts)
        where_clause = " AND ".join(where_parts)

        query = f"UPDATE {table_name} SET {set_clause} WHERE {where_clause}"
        params = set_params + where_params

        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(query, params)
        conn.commit()

        cursor.close()
        conn.close()

        return jsonify({'success': True})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/export-csv')
def export_csv():
    """Export table to CSV"""
    try:
        table_name = request.args.get('table')

        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(f"SELECT * FROM {table_name}")

        columns = [column[0] for column in cursor.description]

        output = io.StringIO()
        writer = csv.writer(output)
        writer.writerow(columns)

        for row in cursor.fetchall():
            writer.writerow([serialize_value(val) for val in row])

        cursor.close()
        conn.close()

        output.seek(0)
        return send_file(
            io.BytesIO(output.getvalue().encode('utf-8')),
            mimetype='text/csv',
            as_attachment=True,
            download_name=f'{table_name}_{datetime.now().strftime("%Y%m%d_%H%M%S")}.csv'
        )
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/export-json')
def export_json():
    """Export table to JSON"""
    try:
        table_name = request.args.get('table')

        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(f"SELECT * FROM {table_name}")

        columns = [column[0] for column in cursor.description]
        rows = []

        for row in cursor.fetchall():
            row_dict = {}
            for idx, col in enumerate(columns):
                row_dict[col] = serialize_value(row[idx])
            rows.append(row_dict)

        cursor.close()
        conn.close()

        return send_file(
            io.BytesIO(json.dumps(rows, indent=2).encode('utf-8')),
            mimetype='application/json',
            as_attachment=True,
            download_name=f'{table_name}_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'
        )
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    print("=" * 80)
    print("🚀 MSSQL Professional Workbench Starting...")
    print("=" * 80)
    print(f"📊 Server: {DB_CONFIG['server']}")
    print(f"💾 Database: {DB_CONFIG['database']}")
    print("=" * 80)
    print("\n🌐 Open your browser: http://localhost:5001")
    print("\n⚡ Professional Features:")
    print("   • Auto-generate SELECT, INSERT, UPDATE, DELETE queries")
    print("   • Query templates and formatting")
    print("   • Import/Export CSV and JSON")
    print("   • Bulk delete operations")
    print("   • Edit and copy rows")
    print("   • Advanced search and filtering")
    print("   • Table structure analysis")
    print("\n🛑 Press Ctrl+C to stop")
    print("=" * 80 + "\n")

    app.run(debug=True, host='0.0.0.0', port=5001)
