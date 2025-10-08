import { NextRequest, NextResponse } from "next/server";
import * as XLSX from "xlsx";
import * as fs from "fs";
import * as path from "path";

// Helper function to convert Excel serial date to JavaScript Date
function excelDateToJSDate(serial: number): Date {
  const utc_days = Math.floor(serial - 25569);
  const utc_value = utc_days * 86400;
  const date_info = new Date(utc_value * 1000);
  return new Date(date_info.getFullYear(), date_info.getMonth(), date_info.getDate());
}

// Helper function to format date as DD-MMM-YYYY
function formatDate(date: Date): string {
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const day = date.getDate();
  const month = months[date.getMonth()];
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
}

export async function GET(request: NextRequest) {
  try {
    // Read the Excel file from the Desktop location
    const filePath = "/Users/suryakantkumar/Desktop/LBCL/FSM_Itinerary Template2.xlsx";

    if (!fs.existsSync(filePath)) {
      return NextResponse.json(
        { success: false, message: "Excel file not found" },
        { status: 404 }
      );
    }

    // Read the file
    const fileBuffer = fs.readFileSync(filePath);
    const workbook = XLSX.read(fileBuffer, { type: "buffer", cellDates: true });

    // Read the Worksheet sheet
    const worksheetName = "Worksheet";
    const worksheet = workbook.Sheets[worksheetName];

    if (!worksheet) {
      return NextResponse.json(
        { success: false, message: "Worksheet not found" },
        { status: 404 }
      );
    }

    // Convert to JSON
    const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

    // Parse the data structure
    // Row 0-1: Empty/Header info
    // Row 2: Name
    // Row 3: Cluster
    // Row 4: Month
    // Row 5: Date
    // Row 6-7: Column headers
    // Row 8+: Data rows

    const nameRow = jsonData[1] as any[];
    const clusterRow = jsonData[2] as any[];
    const monthRow = jsonData[3] as any[];
    const dateRow = jsonData[4] as any[];

    const name = nameRow[1] || "";
    const cluster = clusterRow[1] || "";
    const month = monthRow[1] || "";
    const date = dateRow[1] || "";

    // Get column headers
    const headerRow1 = jsonData[5] as any[];
    const headerRow2 = jsonData[6] as any[];

    // Parse data rows (starting from row 8)
    const entries = [];
    const summaryData: any = {
      focusAreas: new Set(),
      markets: new Set(),
      channels: new Set(),
      accompaniedBy: new Set(),
    };

    for (let i = 8; i < jsonData.length; i++) {
      const row = jsonData[i] as any[];
      if (!row || !row[1] || row[1] === null) continue; // Skip empty rows

      // Convert date if it's a number (Excel serial date)
      let dateValue = row[2] || "";
      if (typeof row[2] === 'number') {
        const jsDate = excelDateToJSDate(row[2]);
        dateValue = formatDate(jsDate);
      } else if (row[2] instanceof Date) {
        dateValue = formatDate(row[2]);
      }

      const entry = {
        focusArea: row[1] || "",
        date: dateValue,
        day: row[3] || "",
        kraPlan: row[4] || "",
        morningMeeting: row[5] || "NA",
        timeFrom: row[6] || "",
        timeTo: row[7] || "",
        market: row[8] || "",
        channel: row[9] || "NA",
        routeNo: row[10] || "NA",
        outletNo: row[11] || "NA",
        accompaniedBy: row[12] || "",
        nightOut: row[13] || "",
        noOfDays: row[14] || 1,
        daysPercent: row[15] || 0,
        plannedMileage: row[16] || 0,
        kraReview: row[17] || "",
        comments: row[18] || "",
      };

      entries.push(entry);

      // Collect unique values for filters
      if (entry.focusArea) summaryData.focusAreas.add(entry.focusArea);
      if (entry.market) summaryData.markets.add(entry.market);
      if (entry.channel) summaryData.channels.add(entry.channel);
      if (entry.accompaniedBy) summaryData.accompaniedBy.add(entry.accompaniedBy);
    }

    // Read Summary sheet
    const summarySheet = workbook.Sheets["Summery"];
    let summary = {};
    if (summarySheet) {
      const summaryJson = XLSX.utils.sheet_to_json(summarySheet, { header: 1 });
      summary = {
        meetingsTrainings: summaryJson[5]?.[1] || 0,
        coaching: summaryJson[7]?.[1] || 0,
        event: summaryJson[8]?.[1] || 0,
      };
    }

    return NextResponse.json({
      success: true,
      data: {
        metadata: {
          name,
          cluster,
          month,
          date,
        },
        entries,
        filters: {
          focusAreas: Array.from(summaryData.focusAreas).sort(),
          markets: Array.from(summaryData.markets).sort(),
          channels: Array.from(summaryData.channels).sort(),
          accompaniedBy: Array.from(summaryData.accompaniedBy).sort(),
        },
        summary,
      },
    });
  } catch (error) {
    console.error("Error reading Excel file:", error);
    return NextResponse.json(
      { success: false, message: "Failed to read Excel file", error: String(error) },
      { status: 500 }
    );
  }
}
