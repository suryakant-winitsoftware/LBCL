/**
 * Database Query API Route
 * Executes direct database queries using the Python SQL tool
 */

import { NextRequest, NextResponse } from 'next/server';
import { exec } from 'child_process';
import { promisify } from 'util';

const execAsync = promisify(exec);

export async function POST(request: NextRequest) {
  try {
    const { query } = await request.json();

    if (!query || typeof query !== 'string') {
      return NextResponse.json(
        { error: 'Query is required and must be a string' },
        { status: 400 }
      );
    }

    // Path to the Python SQL tool
    const pythonToolPath = '/Users/suryakantkumar/Desktop/multiplex/Multiplex/backend/python-tools';
    const command = `cd "${pythonToolPath}" && python3 sql.py -q "${query.replace(/"/g, '\\"')}"`;

    console.log('🔍 Executing database query via Python tool...');
    
    // Execute the Python SQL tool
    const { stdout, stderr } = await execAsync(command, {
      timeout: 30000, // 30 second timeout
      maxBuffer: 1024 * 1024 // 1MB buffer
    });

    if (stderr) {
      console.error('❌ Python SQL tool stderr:', stderr);
      return NextResponse.json(
        { error: 'Database query failed', details: stderr },
        { status: 500 }
      );
    }

    // Parse the output from the Python tool
    console.log('📊 Python tool output:', stdout.substring(0, 500) + '...');

    // Extract the JSON results from the Python tool output
    // The Python tool outputs results in JSON format after "Results (X rows):"
    const lines = stdout.split('\n');
    let jsonStart = -1;
    let jsonEnd = -1;

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      if (line.startsWith('Results (') && line.includes('rows):')) {
        // Next line should be the JSON array start
        if (i + 1 < lines.length && lines[i + 1].trim() === '[') {
          jsonStart = i + 1;
        }
      }
      if (line === '[INFO] Database connection closed' && jsonStart !== -1) {
        jsonEnd = i - 1;
        break;
      }
    }

    if (jsonStart === -1) {
      console.error('❌ Could not find JSON results in Python tool output');
      return NextResponse.json(
        { error: 'Could not parse database results' },
        { status: 500 }
      );
    }

    // Extract and parse the JSON results
    const jsonLines = lines.slice(jsonStart, jsonEnd === -1 ? lines.length : jsonEnd + 1);
    const jsonString = jsonLines.join('\n');

    let results;
    try {
      results = JSON.parse(jsonString);
    } catch (parseError) {
      console.error('❌ Error parsing JSON results:', parseError);
      console.error('❌ JSON string:', jsonString);
      return NextResponse.json(
        { error: 'Failed to parse database results', details: parseError },
        { status: 500 }
      );
    }

    console.log('✅ Successfully executed database query, returned', results.length, 'rows');

    return NextResponse.json({
      success: true,
      results: results,
      rowCount: results.length
    });

  } catch (error) {
    console.error('❌ Database query API error:', error);
    
    return NextResponse.json(
      { 
        error: 'Internal server error', 
        details: error instanceof Error ? error.message : 'Unknown error'
      },
      { status: 500 }
    );
  }
}