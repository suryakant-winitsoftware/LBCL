"use client";

import { useState, useEffect, useRef, Fragment } from "react";
import { useRouter } from "next/navigation";
import { Check, ImageIcon, FileText, RefreshCw, UploadCloud, X, ChevronDown, ChevronUp } from "lucide-react";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { SignatureDialog } from "@/app/lbcl/components/signature-dialog";
import { useToast } from "@/hooks/use-toast";

type Product = {
  id: string;
  code: string;
  name: string;
  image: string;
  goodCollected: number;
  sampleGood: number;
  damageCollected: number;
};

type SampleItem = {
  sampleIndex: number;
  images: string[];
  textNote: string;
  documents: string[];
};

export function EmptiesPhysicalCountDetail() {
  const router = useRouter();
  const { toast } = useToast();
  const imageInputRefs = useRef<Record<string, HTMLInputElement | null>>({});
  const documentInputRefs = useRef<Record<string, HTMLInputElement | null>>({});

  const [activeTab, setActiveTab] = useState<
    "ALL" | "LION SCOUT" | "LION LAGER" | "CALSBURG" | "LUXURY BRAND"
  >("ALL");
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);
  const [showSuccessDialog, setShowSuccessDialog] = useState(false);
  const [agentSignature, setAgentSignature] = useState("");
  const [driverSignature, setDriverSignature] = useState("");
  const [notes, setNotes] = useState("");
  const [productData, setProductData] = useState<Record<string, { sampleGood: number | '' }>>({});
  const [elapsedTime, setElapsedTime] = useState(0);
  const [currentDate, setCurrentDate] = useState("");
  const [expandedProducts, setExpandedProducts] = useState<Set<string>>(new Set());
  const [sampleItems, setSampleItems] = useState<Record<string, SampleItem[]>>({});
  const [uploadingStates, setUploadingStates] = useState<Record<string, { images: boolean; documents: boolean }>>({});

  useEffect(() => {
    const date = new Date();
    const formatted = date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
    setCurrentDate(formatted);

    const timer = setInterval(() => {
      setElapsedTime(prev => prev + 1);
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')} Min`;
  };

  const products: Product[] = [
    {
      id: "1",
      code: "5213",
      name: "Short Quarter Keg 7.75 Galon Beers",
      image: "/amber-beer-bottle.png",
      goodCollected: 25,
      sampleGood: 0,
      damageCollected: 3
    },
    {
      id: "2",
      code: "5214",
      name: "Slim Quarter Keg 7.75 Galon",
      image: "/amber-beer-bottle.png",
      goodCollected: 10,
      sampleGood: 0,
      damageCollected: 2
    },
    {
      id: "3",
      code: "5216",
      name: "Lion Large Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      goodCollected: 20,
      sampleGood: 0,
      damageCollected: 2
    },
    {
      id: "4",
      code: "5210",
      name: "Lion Large Beer bottle 330ml",
      image: "/amber-beer-bottle.png",
      goodCollected: 5,
      sampleGood: 0,
      damageCollected: 1
    }
  ];

  const getValue = (productId: string, field: 'sampleGood', defaultValue: number) => {
    return productData[productId]?.[field] ?? defaultValue;
  };

  const handleValueChange = (productId: string, field: 'sampleGood', value: string, maxValue: number) => {
    if (value === '') {
      setProductData(prev => ({
        ...prev,
        [productId]: {
          ...prev[productId],
          [field]: '' as any
        }
      }));
      setSampleItems(prev => ({
        ...prev,
        [productId]: []
      }));
      return;
    }

    const numValue = parseInt(value) || 0;
    const clampedValue = Math.min(Math.max(0, numValue), maxValue);

    setProductData(prev => ({
      ...prev,
      [productId]: {
        ...prev[productId],
        [field]: clampedValue as any
      }
    }));

    setSampleItems(prev => {
      const existing = prev[productId] || [];
      const newItems: SampleItem[] = [];

      for (let i = 0; i < clampedValue; i++) {
        newItems.push(existing[i] || {
          sampleIndex: i + 1,
          images: [],
          textNote: '',
          documents: []
        });
      }

      return {
        ...prev,
        [productId]: newItems
      };
    });

    if (clampedValue > 0) {
      setExpandedProducts(prev => new Set(prev).add(productId));
    } else {
      setExpandedProducts(prev => {
        const newSet = new Set(prev);
        newSet.delete(productId);
        return newSet;
      });
    }
  };

  const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
    if (e.target.value === '0') {
      e.target.value = '';
    }
  };

  const handleBlur = (productId: string, field: 'sampleGood') => {
    const currentData = productData[productId];
    if (currentData && currentData[field] === '') {
      setProductData(prev => ({
        ...prev,
        [productId]: {
          ...prev[productId],
          [field]: 0 as any
        }
      }));
    }
  };

  const toggleProductExpansion = (productId: string) => {
    setExpandedProducts(prev => {
      const newSet = new Set(prev);
      if (newSet.has(productId)) {
        newSet.delete(productId);
      } else {
        newSet.add(productId);
      }
      return newSet;
    });
  };

  const handleSampleImageUpload = async (productId: string, sampleIndex: number, files: FileList) => {
    const key = `${productId}-${sampleIndex}`;
    try {
      setUploadingStates(prev => ({
        ...prev,
        [key]: { ...prev[key], images: true }
      }));

      const authToken = localStorage.getItem("auth_token");
      let empUID: string | null = null;
      try {
        const userInfoStr = localStorage.getItem("user_info");
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr);
          empUID = userInfo.uid || userInfo.id || userInfo.UID;
        }
      } catch (e) {
        console.error("Failed to parse user_info:", e);
      }

      if (!empUID) {
        throw new Error("User not authenticated");
      }

      const deliveryId = "EMT85444127121";
      const uploadedPaths: string[] = [];

      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const maxSize = 10 * 1024 * 1024;

        if (file.size > maxSize) {
          toast({
            title: "Error",
            description: `${file.name}: File size exceeds 10MB limit`,
            variant: "destructive"
          });
          continue;
        }

        if (!file.type.startsWith("image/")) {
          toast({
            title: "Error",
            description: `${file.name}: Not an image file`,
            variant: "destructive"
          });
          continue;
        }

        const formData = new FormData();
        formData.append("files", file, file.name);
        formData.append("folderPath", `empties-receiving/${deliveryId}/sample-${productId}-${sampleIndex}/images`);

        const uploadResponse = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileUpload/UploadFile`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : ""
            },
            body: formData
          }
        );

        if (!uploadResponse.ok) continue;

        const uploadResult = await uploadResponse.json();
        if (uploadResult.Status !== 1) continue;

        let relativePath = "";
        if (uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0) {
          relativePath = uploadResult.SavedImgsPath[0];
          if (relativePath.startsWith("Data/Data/")) {
            relativePath = relativePath.substring(5);
          }
        } else {
          relativePath = `Data/empties-receiving/${deliveryId}/sample-${productId}-${sampleIndex}/images/${file.name}`;
        }

        const uniqueUID = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          const r = (Math.random() * 16) | 0;
          const v = c === 'x' ? r : (r & 0x3) | 0x8;
          return v.toString(16);
        });

        const truncatedFileName = file.name.length > 50 ? file.name.substring(0, 46) + file.name.slice(-4) : file.name;

        const fileSysData = {
          UID: uniqueUID,
          SS: 1,
          CreatedBy: empUID,
          CreatedTime: new Date().toISOString(),
          ModifiedBy: empUID,
          ModifiedTime: new Date().toISOString(),
          LinkedItemType: "EmptiesReceiving",
          LinkedItemUID: deliveryId,
          FileSysType: `PhysicalCountSample-${productId}-${sampleIndex}-Image`,
          FileType: file.type,
          FileName: truncatedFileName,
          DisplayName: truncatedFileName,
          FileSize: file.size,
          IsDefault: uploadedPaths.length === 0,
          IsDirectory: false,
          RelativePath: relativePath,
          FileSysFileType: 1,
          CreatedByEmpUID: empUID
        };

        await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileSys/CUDFileSys`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : "",
              "Content-Type": "application/json"
            },
            body: JSON.stringify(fileSysData)
          }
        );

        uploadedPaths.push(relativePath);
      }

      if (uploadedPaths.length > 0) {
        setSampleItems(prev => {
          const productSamples = [...(prev[productId] || [])];
          const sampleIdx = sampleIndex - 1;
          if (productSamples[sampleIdx]) {
            productSamples[sampleIdx] = {
              ...productSamples[sampleIdx],
              images: [...productSamples[sampleIdx].images, ...uploadedPaths]
            };
          }
          return { ...prev, [productId]: productSamples };
        });

        toast({
          title: "Success",
          description: `${uploadedPaths.length} image${uploadedPaths.length > 1 ? 's' : ''} uploaded`
        });
      }
    } catch (error) {
      console.error("Error uploading images:", error);
      toast({
        title: "Error",
        description: "Failed to upload images",
        variant: "destructive"
      });
    } finally {
      setUploadingStates(prev => ({
        ...prev,
        [key]: { ...prev[key], images: false }
      }));
    }
  };

  const handleSampleDocumentUpload = async (productId: string, sampleIndex: number, files: FileList) => {
    const key = `${productId}-${sampleIndex}`;
    try {
      setUploadingStates(prev => ({
        ...prev,
        [key]: { ...prev[key], documents: true }
      }));

      const authToken = localStorage.getItem("auth_token");
      let empUID: string | null = null;
      try {
        const userInfoStr = localStorage.getItem("user_info");
        if (userInfoStr) {
          const userInfo = JSON.parse(userInfoStr);
          empUID = userInfo.uid || userInfo.id || userInfo.UID;
        }
      } catch (e) {
        console.error("Failed to parse user_info:", e);
      }

      if (!empUID) {
        throw new Error("User not authenticated");
      }

      const deliveryId = "EMT85444127121";
      const uploadedPaths: string[] = [];

      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const maxSize = 10 * 1024 * 1024;

        if (file.size > maxSize) {
          toast({
            title: "Error",
            description: `${file.name}: File size exceeds 10MB limit`,
            variant: "destructive"
          });
          continue;
        }

        const formData = new FormData();
        formData.append("files", file, file.name);
        formData.append("folderPath", `empties-receiving/${deliveryId}/sample-${productId}-${sampleIndex}/documents`);

        const uploadResponse = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileUpload/UploadFile`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : ""
            },
            body: formData
          }
        );

        if (!uploadResponse.ok) continue;

        const uploadResult = await uploadResponse.json();
        if (uploadResult.Status !== 1) continue;

        let relativePath = "";
        if (uploadResult.SavedImgsPath && uploadResult.SavedImgsPath.length > 0) {
          relativePath = uploadResult.SavedImgsPath[0];
          if (relativePath.startsWith("Data/Data/")) {
            relativePath = relativePath.substring(5);
          }
        } else {
          relativePath = `Data/empties-receiving/${deliveryId}/sample-${productId}-${sampleIndex}/documents/${file.name}`;
        }

        const uniqueUID = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          const r = (Math.random() * 16) | 0;
          const v = c === 'x' ? r : (r & 0x3) | 0x8;
          return v.toString(16);
        });

        const truncatedFileName = file.name.length > 50 ? file.name.substring(0, 46) + file.name.slice(-4) : file.name;

        const fileSysData = {
          UID: uniqueUID,
          SS: 1,
          CreatedBy: empUID,
          CreatedTime: new Date().toISOString(),
          ModifiedBy: empUID,
          ModifiedTime: new Date().toISOString(),
          LinkedItemType: "EmptiesReceiving",
          LinkedItemUID: deliveryId,
          FileSysType: `PhysicalCountSample-${productId}-${sampleIndex}-Document`,
          FileType: file.type,
          FileName: truncatedFileName,
          DisplayName: truncatedFileName,
          FileSize: file.size,
          IsDefault: uploadedPaths.length === 0,
          IsDirectory: false,
          RelativePath: relativePath,
          FileSysFileType: 2,
          CreatedByEmpUID: empUID
        };

        await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileSys/CUDFileSys`,
          {
            method: "POST",
            headers: {
              Authorization: authToken ? `Bearer ${authToken}` : "",
              "Content-Type": "application/json"
            },
            body: JSON.stringify(fileSysData)
          }
        );

        uploadedPaths.push(relativePath);
      }

      if (uploadedPaths.length > 0) {
        setSampleItems(prev => {
          const productSamples = [...(prev[productId] || [])];
          const sampleIdx = sampleIndex - 1;
          if (productSamples[sampleIdx]) {
            productSamples[sampleIdx] = {
              ...productSamples[sampleIdx],
              documents: [...productSamples[sampleIdx].documents, ...uploadedPaths]
            };
          }
          return { ...prev, [productId]: productSamples };
        });

        toast({
          title: "Success",
          description: `${uploadedPaths.length} document${uploadedPaths.length > 1 ? 's' : ''} uploaded`
        });
      }
    } catch (error) {
      console.error("Error uploading documents:", error);
      toast({
        title: "Error",
        description: "Failed to upload documents",
        variant: "destructive"
      });
    } finally {
      setUploadingStates(prev => ({
        ...prev,
        [key]: { ...prev[key], documents: false }
      }));
    }
  };

  const handleRemoveSampleImage = (productId: string, sampleIndex: number, imageIndex: number) => {
    setSampleItems(prev => {
      const productSamples = [...(prev[productId] || [])];
      const sampleIdx = sampleIndex - 1;
      if (productSamples[sampleIdx]) {
        productSamples[sampleIdx] = {
          ...productSamples[sampleIdx],
          images: productSamples[sampleIdx].images.filter((_, i) => i !== imageIndex)
        };
      }
      return { ...prev, [productId]: productSamples };
    });
  };

  const handleRemoveSampleDocument = (productId: string, sampleIndex: number, docIndex: number) => {
    setSampleItems(prev => {
      const productSamples = [...(prev[productId] || [])];
      const sampleIdx = sampleIndex - 1;
      if (productSamples[sampleIdx]) {
        productSamples[sampleIdx] = {
          ...productSamples[sampleIdx],
          documents: productSamples[sampleIdx].documents.filter((_, i) => i !== docIndex)
        };
      }
      return { ...prev, [productId]: productSamples };
    });
  };

  const handleSampleNoteChange = (productId: string, sampleIndex: number, note: string) => {
    setSampleItems(prev => {
      const productSamples = [...(prev[productId] || [])];
      const sampleIdx = sampleIndex - 1;
      if (productSamples[sampleIdx]) {
        productSamples[sampleIdx] = {
          ...productSamples[sampleIdx],
          textNote: note
        };
      }
      return { ...prev, [productId]: productSamples };
    });
  };

  const handleSubmit = () => {
    setShowSignatureDialog(true);
  };

  const handleSignatureSave = (logisticSig: string, driverSig: string, signatureNotes: string) => {
    setAgentSignature(logisticSig);
    setDriverSignature(driverSig);
    setNotes(signatureNotes);
    setShowSuccessDialog(true);
  };

  const handleSuccessDone = () => {
    setShowSuccessDialog(false);
    router.push("/lbcl/empties-receiving/activity-log");
  };

  return (
    <div className="min-h-screen bg-white">
      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="flex items-center justify-between mb-4">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 flex-1">
            <div>
              <div className="text-xs text-gray-600 mb-1">Agent Name</div>
              <div className="font-bold text-sm">R.T DISTRIBUTORS</div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-1">Empties Delivery No</div>
              <div className="font-bold text-sm">EMT85444127121</div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-1">Prime Mover</div>
              <div className="font-bold text-sm">LK1673 (U KUMAR)</div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-1">Date</div>
              <div className="font-bold text-sm">{currentDate}</div>
            </div>
          </div>
          {/* Timer Display */}
          <div className="ml-4 bg-white border-2 border-[#A08B5C] rounded-lg px-4 py-2 min-w-[120px]">
            <div className="text-xs text-gray-600 mb-1 text-center">Elapsed Time</div>
            <div className="font-bold text-lg text-[#A08B5C] text-center font-mono">
              {formatTime(elapsedTime)}
            </div>
          </div>
        </div>
        <button
          onClick={handleSubmit}
          className="w-full sm:w-auto bg-[#A08B5C] hover:bg-[#8A7549] text-white px-6 py-2 rounded-lg font-medium transition-colors"
        >
          Submit
        </button>
      </div>

      {/* Tabs */}
      <div className="bg-gray-50 border-b border-gray-200 mb-4">
        <div className="flex overflow-x-auto">
          {(
            [
              "ALL",
              "LION SCOUT",
              "LION LAGER",
              "CALSBURG",
              "LUXURY BRAND"
            ] as const
          ).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-6 py-4 text-sm font-semibold whitespace-nowrap transition-colors relative ${
                activeTab === tab
                  ? "text-[#A08B5C]"
                  : "text-gray-600 hover:text-gray-900"
              }`}
            >
              {tab}
              {activeTab === tab && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-[#A08B5C]" />
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Products Table */}
      <div className="overflow-x-auto px-4 pb-8">
        <table className="w-full border-collapse">
          <thead className="bg-[#F5E6D3] sticky top-0 z-20">
            <tr>
              <th className="text-left p-3 font-semibold text-sm border border-gray-300">
                Product Code/Description
              </th>
              <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                Good Empties<br />Collected Qty
              </th>
              <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                Sample<br />Good Qty
              </th>
              <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                Damage Empties<br />Collected Qty
              </th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => {
              const sampleQty = getValue(product.id, 'sampleGood', product.sampleGood);
              const hasSamples = typeof sampleQty === 'number' && sampleQty > 0;
              const isExpanded = expandedProducts.has(product.id);
              const samples = sampleItems[product.id] || [];

              return (
                <Fragment key={product.id}>
                  {/* Main Product Row */}
                  <tr className="border-b border-gray-300 hover:bg-gray-50">
                    <td className="p-3 border border-gray-300">
                      <div className="flex items-center gap-3">
                        {hasSamples && (
                          <button
                            onClick={() => toggleProductExpansion(product.id)}
                            className="p-1 hover:bg-gray-200 rounded transition-colors"
                          >
                            {isExpanded ? (
                              <ChevronUp className="h-4 w-4 text-gray-600" />
                            ) : (
                              <ChevronDown className="h-4 w-4 text-gray-600" />
                            )}
                          </button>
                        )}
                        <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center flex-shrink-0">
                          <span className="text-2xl">🍺</span>
                        </div>
                        <div>
                          <div className="font-semibold text-sm">{product.name}</div>
                          <div className="text-xs text-gray-600">{product.code}</div>
                        </div>
                      </div>
                    </td>
                    <td className="text-center p-3 border border-gray-300">
                      <div className="font-medium">{product.goodCollected}</div>
                    </td>
                    <td className="text-center p-3 border border-gray-300">
                      <input
                        type="number"
                        min="0"
                        max={product.goodCollected + product.damageCollected}
                        value={sampleQty}
                        onChange={(e) => handleValueChange(product.id, 'sampleGood', e.target.value, product.goodCollected + product.damageCollected)}
                        onFocus={handleFocus}
                        onBlur={() => handleBlur(product.id, 'sampleGood')}
                        className="w-24 mx-auto text-center px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
                      />
                    </td>
                    <td className="text-center p-3 border border-gray-300">
                      <div className="font-medium">{product.damageCollected}</div>
                    </td>
                  </tr>

                  {/* Sample Item Child Rows */}
                  {hasSamples && isExpanded && samples.map((sample) => {
                    const key = `${product.id}-${sample.sampleIndex}`;
                    const isUploadingImages = uploadingStates[key]?.images || false;
                    const isUploadingDocs = uploadingStates[key]?.documents || false;

                    return (
                      <tr key={key} className="bg-blue-50 border-b border-gray-200">
                        <td colSpan={4} className="p-4 border border-gray-300">
                          <div className="bg-white rounded-lg p-4 shadow-sm">
                            {/* Header */}
                            <div className="flex items-center justify-between mb-4 pb-3 border-b border-gray-200">
                              <div className="flex items-center gap-3">
                                <div className="bg-[#A08B5C] text-white px-3 py-1 rounded-md text-sm font-semibold">
                                  Sample {sample.sampleIndex}
                                </div>
                                <div className="text-sm text-gray-600">
                                  <span className="font-medium">{product.code}</span> - {product.name}
                                </div>
                              </div>
                            </div>

                            {/* Content Grid */}
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                              {/* Images Section */}
                              <div className="space-y-2">
                                <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                  <ImageIcon className="h-4 w-4" />
                                  Images
                                </label>
                                <input
                                  ref={(el) => imageInputRefs.current[key] = el}
                                  type="file"
                                  accept="image/*"
                                  multiple
                                  className="hidden"
                                  onChange={(e) => {
                                    const files = e.target.files;
                                    if (files && files.length > 0) {
                                      handleSampleImageUpload(product.id, sample.sampleIndex, files);
                                    }
                                  }}
                                />
                                <div className="flex flex-wrap gap-2">
                                  {sample.images.map((imageUrl, imgIndex) => (
                                    <div key={imgIndex} className="relative group">
                                      <a
                                        href={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="block"
                                      >
                                        <img
                                          src={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${imageUrl}`}
                                          alt={`Sample ${sample.sampleIndex} - ${imgIndex + 1}`}
                                          className="h-24 w-24 rounded-md object-contain border-2 border-gray-300 hover:border-[#A08B5C] transition-colors cursor-pointer bg-white"
                                          onError={(e) => {
                                            const target = e.currentTarget as HTMLImageElement;
                                            target.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96"><rect fill="%23ddd"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="%23999" font-size="12">Error</text></svg>';
                                          }}
                                        />
                                      </a>
                                      <div className="absolute -top-2 -right-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                        <button
                                          onClick={() => handleRemoveSampleImage(product.id, sample.sampleIndex, imgIndex)}
                                          className="h-6 w-6 bg-red-500 hover:bg-red-600 rounded-full flex items-center justify-center shadow-md"
                                          title="Remove Image"
                                        >
                                          <X className="h-3 w-3 text-white" />
                                        </button>
                                      </div>
                                    </div>
                                  ))}
                                  <button
                                    onClick={() => imageInputRefs.current[key]?.click()}
                                    disabled={isUploadingImages}
                                    className="h-24 w-24 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                                    title={sample.images.length > 0 ? "Add More Images" : "Upload Images"}
                                  >
                                    {isUploadingImages ? (
                                      <RefreshCw className="h-5 w-5 text-gray-400 animate-spin" />
                                    ) : (
                                      <>
                                        <UploadCloud className="h-5 w-5 text-gray-400 group-hover:text-[#A08B5C]" />
                                        <span className="text-[10px] text-gray-500 group-hover:text-[#A08B5C] font-medium mt-1">
                                          {sample.images.length > 0 ? "Add" : "Upload"}
                                        </span>
                                      </>
                                    )}
                                  </button>
                                </div>
                              </div>

                              {/* Notes Section */}
                              <div className="space-y-2">
                                <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                  <FileText className="h-4 w-4" />
                                  Notes
                                </label>
                                <textarea
                                  value={sample.textNote}
                                  onChange={(e) => handleSampleNoteChange(product.id, sample.sampleIndex, e.target.value)}
                                  placeholder="Enter notes for this sample..."
                                  className="w-full h-24 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[#A08B5C] resize-none text-sm"
                                />
                              </div>

                              {/* Documents Section */}
                              <div className="space-y-2">
                                <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                  <FileText className="h-4 w-4" />
                                  Documents
                                </label>
                                <input
                                  ref={(el) => documentInputRefs.current[key] = el}
                                  type="file"
                                  accept=".pdf,.doc,.docx,.txt"
                                  multiple
                                  className="hidden"
                                  onChange={(e) => {
                                    const files = e.target.files;
                                    if (files && files.length > 0) {
                                      handleSampleDocumentUpload(product.id, sample.sampleIndex, files);
                                    }
                                  }}
                                />
                                <div className="flex flex-wrap gap-2">
                                  {sample.documents.map((docUrl, docIndex) => {
                                    const fileName = docUrl.split('/').pop() || 'Document';
                                    const fileExtension = fileName.split('.').pop()?.toUpperCase() || 'FILE';
                                    return (
                                      <div key={docIndex} className="relative group">
                                        <a
                                          href={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000"}/${docUrl}`}
                                          target="_blank"
                                          rel="noopener noreferrer"
                                          className="h-20 w-20 px-2 rounded-md bg-blue-50 border-2 border-blue-200 flex flex-col items-center justify-center gap-1 hover:bg-blue-100 transition-colors"
                                          title={fileName}
                                        >
                                          <FileText className="h-6 w-6 text-blue-600" />
                                          <span className="text-[9px] text-blue-600 font-bold truncate max-w-full px-1">
                                            {fileExtension}
                                          </span>
                                        </a>
                                        <div className="absolute -top-2 -right-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                          <button
                                            onClick={() => handleRemoveSampleDocument(product.id, sample.sampleIndex, docIndex)}
                                            className="h-6 w-6 bg-red-500 hover:bg-red-600 rounded-full flex items-center justify-center shadow-md"
                                            title="Remove Document"
                                          >
                                            <X className="h-3 w-3 text-white" />
                                          </button>
                                        </div>
                                      </div>
                                    );
                                  })}
                                  <button
                                    onClick={() => documentInputRefs.current[key]?.click()}
                                    disabled={isUploadingDocs}
                                    className="h-20 w-20 rounded-md border-2 border-dashed border-gray-300 bg-gray-50 hover:bg-gray-100 hover:border-[#A08B5C]/50 transition-all flex flex-col items-center justify-center cursor-pointer group disabled:opacity-50 disabled:cursor-not-allowed"
                                    title={sample.documents.length > 0 ? "Add More Documents" : "Upload Documents"}
                                  >
                                    {isUploadingDocs ? (
                                      <RefreshCw className="h-5 w-5 text-gray-400 animate-spin" />
                                    ) : (
                                      <>
                                        <UploadCloud className="h-5 w-5 text-gray-400 group-hover:text-[#A08B5C]" />
                                        <span className="text-[10px] text-gray-500 group-hover:text-[#A08B5C] font-medium mt-1">
                                          {sample.documents.length > 0 ? "Add" : "Upload"}
                                        </span>
                                      </>
                                    )}
                                  </button>
                                </div>
                              </div>
                            </div>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </Fragment>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Signature Dialog */}
      <SignatureDialog
        open={showSignatureDialog}
        onOpenChange={setShowSignatureDialog}
        selectedDriverName="R.M.K.P. Rathnayake (U KUMAR)"
        organizationName="R.T DISTRIBUTORS"
        onSave={handleSignatureSave}
      />

      {/* Success Dialog */}
      <Dialog open={showSuccessDialog} onOpenChange={setShowSuccessDialog}>
        <DialogContent className="max-w-xl">
          <DialogTitle className="text-2xl font-bold text-gray-900 text-center">Success</DialogTitle>
          <div className="p-6 pt-0 text-center">
            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 bg-green-500 rounded-full flex items-center justify-center">
                <Check className="w-12 h-12 text-white" strokeWidth={3} />
              </div>
            </div>
            <h3 className="text-xl font-bold text-gray-900 mb-2">
              Physical Count & Audit Empties Successfully Completed
            </h3>
            <p className="text-gray-600 mb-8">
              Physical Count completed in{" "}
              <span className="font-bold text-[#A08B5C]">{formatTime(elapsedTime)}</span>
            </p>
            <div className="grid grid-cols-2 gap-4">
              <button className="py-4 bg-gray-400 text-white font-medium rounded-lg hover:bg-gray-500 transition-colors">
                PRINT
              </button>
              <button
                onClick={handleSuccessDone}
                className="py-4 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
              >
                DONE
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
