#!/usr/bin/env python3
"""
MSSQL Database Workbench
A web-based tool to manage MSSQL database - browse tables, run queries, insert, update, delete data
"""

from flask import Flask, render_template_string, request, jsonify
import pymssql
import json
from datetime import datetime, date

app = Flask(__name__)

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
    else:
        return str(val)

# HTML Template
HTML_TEMPLATE = """
<!DOCTYPE html>
<html>
<head>
    <title>MSSQL Database Workbench</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
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
        }

        .table-item:hover {
            background: #f5f5f7;
        }

        .table-item.active {
            background: #007aff;
            color: white;
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

        textarea:focus {
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
        }

        .btn:hover {
            background: #0051d5;
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

        .action-btn {
            padding: 5px 10px;
            margin: 2px;
            font-size: 12px;
        }

        .filter-box {
            background: white;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
        }

        .filter-box input {
            padding: 8px 12px;
            border: 1px solid #d2d2d7;
            border-radius: 6px;
            font-size: 13px;
            margin-right: 10px;
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
    </style>
</head>
<body>
    <div class="header">
        <h1>🗄️ MSSQL Database Workbench</h1>
        <p>Server: {{ config.server }} | Database: {{ config.database }}</p>
    </div>

    <div class="container">
        <div class="sidebar">
            <h3>Tables</h3>
            <ul class="table-list" id="tableList">
                <li class="loading">Loading tables...</li>
            </ul>
        </div>

        <div class="main-content">
            <div class="tabs">
                <div class="tab active" onclick="switchTab('data')">📊 Browse Data</div>
                <div class="tab" onclick="switchTab('query')">⚡ Query Editor</div>
                <div class="tab" onclick="switchTab('structure')">🔧 Table Structure</div>
            </div>

            <div id="data-tab" class="tab-content active">
                <div class="filter-box">
                    <input type="text" id="searchInput" placeholder="Search...">
                    <button class="btn" onclick="loadTableData()">🔍 Search</button>
                    <button class="btn btn-success" onclick="showInsertForm()">➕ Insert Row</button>
                </div>

                <div id="dataResult"></div>

                <div class="pagination">
                    <button class="btn" onclick="previousPage()">← Previous</button>
                    <span id="pageInfo">Page 1</span>
                    <button class="btn" onclick="nextPage()">Next →</button>
                </div>
            </div>

            <div id="query-tab" class="tab-content">
                <div class="query-editor">
                    <textarea id="queryInput" placeholder="Enter your SQL query here...&#10;&#10;Example:&#10;SELECT * FROM emp WHERE status = 'Active'"></textarea>
                </div>
                <button class="btn" onclick="executeQuery()">▶️ Execute Query</button>
                <button class="btn btn-danger" onclick="clearQuery()">🗑️ Clear</button>

                <div id="queryResult"></div>
            </div>

            <div id="structure-tab" class="tab-content">
                <div id="structureResult"></div>
            </div>
        </div>
    </div>

    <script>
        let currentTable = '';
        let currentPage = 1;
        let pageSize = 50;

        // Load tables on page load
        window.onload = function() {
            loadTables();
        };

        function loadTables() {
            fetch('/api/tables')
                .then(res => res.json())
                .then(data => {
                    const tableList = document.getElementById('tableList');
                    tableList.innerHTML = '';

                    data.tables.forEach(table => {
                        const li = document.createElement('li');
                        li.className = 'table-item';
                        li.textContent = table;
                        li.onclick = () => selectTable(table);
                        tableList.appendChild(li);
                    });
                })
                .catch(err => {
                    console.error('Error loading tables:', err);
                    document.getElementById('tableList').innerHTML = '<li class="error">Failed to load tables</li>';
                });
        }

        function selectTable(tableName) {
            currentTable = tableName;
            currentPage = 1;

            // Update active state
            document.querySelectorAll('.table-item').forEach(item => {
                item.classList.remove('active');
                if (item.textContent === tableName) {
                    item.classList.add('active');
                }
            });

            loadTableData();
            loadTableStructure();
        }

        function loadTableData() {
            if (!currentTable) return;

            const search = document.getElementById('searchInput').value;
            const url = `/api/table-data?table=${currentTable}&page=${currentPage}&size=${pageSize}&search=${search}`;

            document.getElementById('dataResult').innerHTML = '<div class="loading">Loading data...</div>';

            fetch(url)
                .then(res => res.json())
                .then(data => {
                    if (data.error) {
                        document.getElementById('dataResult').innerHTML = `<div class="error">${data.error}</div>`;
                        return;
                    }

                    let html = `<div class="result-info">Showing ${data.rows.length} rows</div>`;

                    if (data.rows.length > 0) {
                        html += '<table><thead><tr>';
                        data.columns.forEach(col => {
                            html += `<th>${col}</th>`;
                        });
                        html += '<th>Actions</th></tr></thead><tbody>';

                        data.rows.forEach((row, idx) => {
                            html += '<tr>';
                            data.columns.forEach(col => {
                                let value = row[col];
                                if (value === null) value = '<em style="color: #86868b;">NULL</em>';
                                html += `<td>${value}</td>`;
                            });
                            html += `<td>
                                <button class="btn action-btn" onclick='editRow(${JSON.stringify(row)})'>✏️ Edit</button>
                                <button class="btn btn-danger action-btn" onclick='deleteRow(${JSON.stringify(row)})'>🗑️ Delete</button>
                            </td>`;
                            html += '</tr>';
                        });

                        html += '</tbody></table>';
                    } else {
                        html += '<div class="result-info">No data found</div>';
                    }

                    document.getElementById('dataResult').innerHTML = html;
                    document.getElementById('pageInfo').textContent = `Page ${currentPage}`;
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

                    let html = '<table><thead><tr><th>Column</th><th>Type</th><th>Nullable</th><th>Default</th></tr></thead><tbody>';

                    data.columns.forEach(col => {
                        html += `<tr>
                            <td><strong>${col.column_name}</strong></td>
                            <td>${col.data_type}</td>
                            <td>${col.is_nullable}</td>
                            <td>${col.column_default || '<em style="color: #86868b;">NULL</em>'}</td>
                        </tr>`;
                    });

                    html += '</tbody></table>';
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
                        html = `<div class="result-info">${data.rows.length} rows returned</div>`;
                        html += '<table><thead><tr>';

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

                        html += '</tbody></table>';
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
                        alert('Row deleted successfully!');
                        loadTableData();
                    } else {
                        alert('Error: ' + data.error);
                    }
                })
                .catch(err => alert('Error: ' + err.message));
        }

        function clearQuery() {
            document.getElementById('queryInput').value = '';
            document.getElementById('queryResult').innerHTML = '';
        }

        function switchTab(tab) {
            document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));

            event.target.classList.add('active');
            document.getElementById(tab + '-tab').classList.add('active');
        }

        function nextPage() {
            currentPage++;
            loadTableData();
        }

        function previousPage() {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        }

        function showInsertForm() {
            alert('Insert form feature - coming soon! Use Query Editor for now.');
        }

        function editRow(row) {
            alert('Edit form feature - coming soon! Use Query Editor for now.');
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

        tables = [row[0] for row in cursor.fetchall()]

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

        # Build query
        if search:
            query = f"SELECT TOP {size} * FROM {table_name} WHERE CAST({table_name} AS NVARCHAR(MAX)) LIKE ? ORDER BY 1 OFFSET {offset} ROWS"
            cursor.execute(query, (f'%{search}%',))
        else:
            query = f"SELECT * FROM {table_name} ORDER BY 1 OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY"
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
            'size': size
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
                COLUMN_NAME as column_name,
                DATA_TYPE as data_type,
                IS_NULLABLE as is_nullable,
                COLUMN_DEFAULT as column_default,
                CHARACTER_MAXIMUM_LENGTH as max_length
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = ?
            ORDER BY ORDINAL_POSITION
        """, (table_name,))

        columns = []
        for row in cursor.fetchall():
            columns.append({
                'column_name': row[0],
                'data_type': row[1] + (f'({row[4]})' if row[4] else ''),
                'is_nullable': row[2],
                'column_default': row[3]
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

        # Check if query returns results
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
            # Query doesn't return results (INSERT, UPDATE, DELETE, etc.)
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

        # Build WHERE clause from row data
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

if __name__ == '__main__':
    print("=" * 80)
    print("🚀 MSSQL Database Workbench Starting...")
    print("=" * 80)
    print(f"📊 Server: {DB_CONFIG['server']}")
    print(f"💾 Database: {DB_CONFIG['database']}")
    print("=" * 80)
    print("\n🌐 Open your browser and go to: http://localhost:5001")
    print("\n⚡ Features:")
    print("   • Browse all tables")
    print("   • View table structure")
    print("   • Execute custom SQL queries")
    print("   • Search and filter data")
    print("   • Delete rows")
    print("\n🛑 Press Ctrl+C to stop the server")
    print("=" * 80 + "\n")

    app.run(debug=True, host='0.0.0.0', port=5001)
