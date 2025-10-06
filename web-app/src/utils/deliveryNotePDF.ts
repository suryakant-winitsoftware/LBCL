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

export const generateDeliveryNotePDF = (
  purchaseOrder: PurchaseOrder,
  orderLines: OrderLine[]
) => {
  const doc = new jsPDF();

  // Header with background color
  doc.setFillColor(160, 139, 92); // #A08B5C
  doc.rect(0, 0, 210, 40, "F");

  // Title
  doc.setTextColor(255, 255, 255);
  doc.setFontSize(24);
  doc.setFont("helvetica", "bold");
  doc.text("DELIVERY NOTE", 105, 20, { align: "center" });

  // Delivery Note Number
  const now = new Date();
  const orderNumber =
    purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || "PO";
  const timestamp = now.getTime().toString().slice(-6);
  const deliveryNoteNumber = `DN-${orderNumber}-${timestamp}`;

  doc.setFontSize(12);
  doc.text(deliveryNoteNumber, 105, 32, { align: "center" });

  // Reset text color
  doc.setTextColor(0, 0, 0);
  doc.setFont("helvetica", "normal");

  // Order Information Section
  let yPos = 50;
  doc.setFontSize(14);
  doc.setFont("helvetica", "bold");
  doc.text("Order Information", 14, yPos);

  yPos += 8;
  doc.setFontSize(10);
  doc.setFont("helvetica", "normal");
  doc.text(
    `Order Number: ${purchaseOrder?.OrderNumber || purchaseOrder?.orderNumber || "N/A"}`,
    14,
    yPos
  );
  yPos += 6;
  doc.text(
    `Order Date: ${
      purchaseOrder?.OrderDate || purchaseOrder?.orderDate
        ? new Date(
            purchaseOrder.OrderDate || purchaseOrder.orderDate
          ).toLocaleDateString()
        : "N/A"
    }`,
    14,
    yPos
  );
  yPos += 6;
  doc.text(`Delivery Date: ${now.toLocaleDateString()}`, 14, yPos);

  // Shipping Details Section
  yPos += 12;
  doc.setFontSize(14);
  doc.setFont("helvetica", "bold");
  doc.text("Shipping Details", 14, yPos);

  yPos += 8;
  doc.setFontSize(10);
  doc.setFont("helvetica", "normal");
  doc.text(
    `Ship To: ${purchaseOrder?.OrgName || purchaseOrder?.orgName || "N/A"}`,
    14,
    yPos
  );

  // Order Items Table
  yPos += 12;
  const tableData = orderLines.map((line: OrderLine, index: number) => [
    index + 1,
    line.SKUCode || line.skuCode || "N/A",
    line.SKUName || line.skuName || line.ProductName || "N/A",
    line.UOM || line.uom || "N/A",
    line.RequestedQty || line.requestedQty || 0,
  ]);

  autoTable(doc, {
    startY: yPos,
    head: [["#", "Product Code", "Description", "Unit", "Quantity"]],
    body: tableData,
    theme: "grid",
    headStyles: {
      fillColor: [160, 139, 92], // #A08B5C
      textColor: [255, 255, 255],
      fontStyle: "bold",
    },
    styles: {
      fontSize: 9,
      cellPadding: 3,
    },
    columnStyles: {
      0: { cellWidth: 15 },
      1: { cellWidth: 35 },
      2: { cellWidth: 70 },
      3: { cellWidth: 25 },
      4: { cellWidth: 30, halign: "right" },
    },
  });

  // Total Items
  const finalY = (doc as any).lastAutoTable.finalY || yPos + 50;
  const totalQty = orderLines.reduce(
    (sum: number, line: OrderLine) =>
      sum + (line.RequestedQty || line.requestedQty || 0),
    0
  );

  doc.setFont("helvetica", "bold");
  doc.text(`Total Items: ${totalQty}`, 14, finalY + 10);

  // Footer
  const pageHeight = doc.internal.pageSize.height;
  doc.setFontSize(8);
  doc.setFont("helvetica", "italic");
  doc.setTextColor(128, 128, 128);
  doc.text(
    "This is a computer-generated delivery note",
    105,
    pageHeight - 10,
    { align: "center" }
  );

  return doc;
};

export const openDeliveryNotePDFInNewTab = (
  purchaseOrder: PurchaseOrder,
  orderLines: OrderLine[]
) => {
  const doc = generateDeliveryNotePDF(purchaseOrder, orderLines);
  const pdfBlob = doc.output("blob");
  const url = URL.createObjectURL(pdfBlob);
  window.open(url, "_blank");
};
