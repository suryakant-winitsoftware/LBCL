import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";

interface OrderLine {
  SKUCode?: string;
  skuCode?: string;
  SKUName?: string;
  skuName?: string;
  ProductName?: string;
  UOM?: string;
  uom?: string;
  RequestedQty?: number;
  requestedQty?: number;
}

interface PurchaseOrder {
  OrderNumber?: string;
  orderNumber?: string;
  OrderDate?: string;
  orderDate?: string;
  OrgName?: string;
  orgName?: string;
  WarehouseName?: string;
  warehouseName?: string;
}

export const generatePickListPDF = (
  purchaseOrder: PurchaseOrder,
  orderLines: OrderLine[]
) => {
  console.log("📋 Pick List PDF - Order Lines:", orderLines);
  console.log("🏷️ Pick List PDF - First line SKUName:", orderLines[0]?.SKUName);
  console.log("🏷️ Pick List PDF - First line skuName:", orderLines[0]?.skuName);
  console.log("🏷️ Pick List PDF - First line ProductName:", orderLines[0]?.ProductName);

  const doc = new jsPDF();

  // Header with background color
  doc.setFillColor(160, 139, 92); // #A08B5C
  doc.rect(0, 0, 210, 35, "F");

  // Title
  doc.setTextColor(255, 255, 255);
  doc.setFontSize(22);
  doc.setFont("helvetica", "bold");
  doc.text("PICK LIST", 105, 15, { align: "center" });

  // Order Number
  const orderNumber =
    purchaseOrder.OrderNumber || purchaseOrder.orderNumber || "N/A";
  doc.setFontSize(12);
  doc.text(`Order: ${orderNumber}`, 105, 25, { align: "center" });

  // Reset text color
  doc.setTextColor(0, 0, 0);
  doc.setFont("helvetica", "normal");

  // Order Information Section
  let yPos = 45;
  doc.setFontSize(10);

  // Create info grid
  const orderDate = purchaseOrder.OrderDate || purchaseOrder.orderDate;
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    }).toUpperCase();
  };

  doc.text(`Order Date: ${formatDate(orderDate || "")}`, 14, yPos);
  doc.text(`Print Date: ${formatDate(new Date().toISOString())}`, 105, yPos);
  yPos += 6;
  doc.text(
    `Warehouse: ${purchaseOrder.WarehouseName || purchaseOrder.warehouseName || "N/A"}`,
    14,
    yPos
  );
  doc.text(`Total Lines: ${orderLines.length}`, 105, yPos);

  // Order Items Table
  yPos += 10;
  const tableData = orderLines.map((line: OrderLine, index: number) => [
    index + 1,
    line.SKUCode || line.skuCode || "N/A",
    line.SKUName || line.skuName || line.ProductName || "N/A",
    line.UOM || line.uom || "N/A",
    line.RequestedQty || line.requestedQty || 0,
  ]);

  autoTable(doc, {
    startY: yPos,
    head: [["#", "SKU Code", "Description", "UOM", "Qty"]],
    body: tableData,
    theme: "grid",
    headStyles: {
      fillColor: [160, 139, 92], // #A08B5C
      textColor: [255, 255, 255],
      fontStyle: "bold",
      fontSize: 10,
    },
    styles: {
      fontSize: 9,
      cellPadding: 4,
    },
    columnStyles: {
      0: { cellWidth: 15, halign: "center" },
      1: { cellWidth: 35 },
      2: { cellWidth: 80 },
      3: { cellWidth: 25, halign: "center" },
      4: { cellWidth: 25, halign: "right" },
    },
    foot: [
      [
        "",
        "",
        "",
        "Total:",
        orderLines
          .reduce(
            (sum: number, line: OrderLine) =>
              sum + (line.RequestedQty || line.requestedQty || 0),
            0
          )
          .toString(),
      ],
    ],
    footStyles: {
      fillColor: [245, 245, 245],
      textColor: [0, 0, 0],
      fontStyle: "bold",
      fontSize: 10,
    },
  });

  // Footer
  const pageHeight = doc.internal.pageSize.height;
  doc.setFontSize(8);
  doc.setFont("helvetica", "italic");
  doc.setTextColor(128, 128, 128);
  doc.text(
    "This is a computer-generated pick list",
    105,
    pageHeight - 10,
    { align: "center" }
  );

  return doc;
};

export const openPickListPDFInNewTab = (
  purchaseOrder: PurchaseOrder,
  orderLines: OrderLine[]
) => {
  const doc = generatePickListPDF(purchaseOrder, orderLines);
  const pdfBlob = doc.output("blob");
  const url = URL.createObjectURL(pdfBlob);
  window.open(url, "_blank");
};
