'use client';

import React, { useState, useEffect } from 'react';
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, DragEndEvent } from '@dnd-kit/core';
import { arrayMove, SortableContext, sortableKeyboardCoordinates, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Separator } from '@/components/ui/separator';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { Skeleton } from '@/components/ui/skeleton';
import { GripVertical, Save, RotateCcw, Search, AlertCircle, CheckCircle2, Plus, X, RefreshCw, Eye, EyeOff, ChevronDown, ChevronUp, Loader2, Download, Upload, FileSpreadsheet, Zap } from 'lucide-react';
import { cn } from '@/lib/utils';
import skuSequenceService, { SkuSequenceUI } from '@/services/sku/sku-sequence.service';
import { skuService, SKUListView } from '@/services/sku/sku.service';
import { toast } from 'sonner';
import * as XLSX from 'xlsx';

// Sortable Item Component
interface SortableItemProps {
  sku: SkuSequenceUI;
  index: number;
  isSelected: boolean;
  onSelect: (uid: string, event: React.MouseEvent) => void;
  onCheckboxToggle: (uid: string) => void;
  onRemove?: (uid: string) => void;
  totalSelected: number;
  isDraggingAny: boolean;
  itemRef?: (el: HTMLDivElement | null, uid: string) => void;
}

function SortableItem({ sku, index, isSelected, onSelect, onCheckboxToggle, onRemove, totalSelected, isDraggingAny, itemRef }: SortableItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: sku.uid });

  const combinedRef = React.useCallback(
    (node: HTMLDivElement | null) => {
      setNodeRef(node);
      if (itemRef) {
        itemRef(node, sku.uid);
      }
    },
    [setNodeRef, itemRef, sku.uid]
  );

  const isMultiDrag = isSelected && totalSelected > 1;
  const showGroupBadge = isDragging && isMultiDrag;

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.9 : (isDraggingAny && isSelected && !isDragging) ? 0.5 : 1,
  };

  return (
    <div
      ref={combinedRef}
      style={style}
      className={cn(
        "group relative flex items-center gap-3 p-4 bg-white border rounded-lg transition-all",
        isDragging && isMultiDrag ? 'shadow-2xl ring-4 ring-blue-500 z-50 scale-105 bg-gradient-to-r from-blue-100 to-blue-50' : '',
        isDragging && !isMultiDrag ? 'shadow-xl ring-2 ring-primary z-50 scale-105' : '',
        !isDragging && isSelected && 'ring-2 ring-blue-500 bg-blue-50 border-blue-300',
        !isDragging && !isSelected && 'hover:shadow-md hover:border-gray-300',
        isDraggingAny && isSelected && !isDragging && 'ring-2 ring-blue-400 border-blue-400'
      )}
    >
      {showGroupBadge && (
        <div className="absolute -top-2 -right-2 z-10">
          <Badge className="bg-blue-600 text-white font-bold px-2 py-1 shadow-lg">
            {totalSelected} items
          </Badge>
        </div>
      )}

      {/* Larger clickable checkbox area */}
      <div
        className="flex-shrink-0 p-2 -m-2 hover:bg-blue-100 rounded cursor-pointer transition-colors"
        onClick={(e) => {
          e.stopPropagation();
          onCheckboxToggle(sku.uid);
        }}
      >
        <Checkbox
          checked={isSelected}
          className="h-5 w-5 pointer-events-none"
        />
      </div>

      <div
        {...attributes}
        {...listeners}
        className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-700 transition-colors"
        onClick={(e) => e.stopPropagation()}
      >
        <GripVertical className="h-5 w-5" />
      </div>

      <div className="flex-shrink-0 w-14 text-center">
        <Badge variant={isSelected ? "default" : "secondary"} className="font-mono font-semibold">
          #{index + 1}
        </Badge>
      </div>

      <div className="flex-1 grid grid-cols-2 gap-6">
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">SKU Code</p>
          <p className="font-semibold text-gray-900">{sku.skuCode}</p>
        </div>
        <div>
          <p className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">Product Name</p>
          <p className="font-medium text-gray-900">{sku.skuName}</p>
        </div>
      </div>

      {onRemove && (
        <Button
          variant="ghost"
          size="icon"
          onClick={(e) => {
            e.stopPropagation();
            onRemove(sku.uid);
          }}
          className="text-gray-400 hover:text-red-600 hover:bg-red-50 opacity-0 group-hover:opacity-100 transition-opacity"
        >
          <X className="h-4 w-4" />
        </Button>
      )}
    </div>
  );
}

export default function SkuSequencingPage() {
  const [sequenceType, setSequenceType] = useState<string>('SalesOrder');
  const [skuSequences, setSkuSequences] = useState<SkuSequenceUI[]>([]);
  const [filteredSequences, setFilteredSequences] = useState<SkuSequenceUI[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);
  const [originalSequences, setOriginalSequences] = useState<SkuSequenceUI[]>([]);
  const [deletedItemUIDs, setDeletedItemUIDs] = useState<string[]>([]); // Track deleted items

  // Add Products Popover
  const [addPopoverOpen, setAddPopoverOpen] = useState(false);
  const [availableProducts, setAvailableProducts] = useState<SKUListView[]>([]);
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [productSearchQuery, setProductSearchQuery] = useState('');
  const [selectedProductsForAdd, setSelectedProductsForAdd] = useState<SKUListView[]>([]);
  const [productPage, setProductPage] = useState(1);
  const [hasMoreProducts, setHasMoreProducts] = useState(true);
  const [hideSelectedItems, setHideSelectedItems] = useState(false);
  const productSearchDebounceTimer = React.useRef<NodeJS.Timeout>();

  // Multi-select state
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [lastSelectedIndex, setLastSelectedIndex] = useState<number | null>(null);
  const [isDragging, setIsDragging] = useState(false);

  // Quick Jump state
  const [jumpToSKU, setJumpToSKU] = useState('');
  const [moveToPosition, setMoveToPosition] = useState('');
  const [showQuickActions, setShowQuickActions] = useState(false);
  const itemRefs = React.useRef<{ [key: string]: HTMLDivElement | null }>({});

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Load sequences on mount and when sequence type changes
  useEffect(() => {
    loadSequences();
  }, [sequenceType]);

  // Filter sequences based on search query
  useEffect(() => {
    if (searchQuery.trim() === '') {
      setFilteredSequences(skuSequences);
    } else {
      const query = searchQuery.toLowerCase();
      const filtered = skuSequences.filter(
        (seq) =>
          seq.skuCode.toLowerCase().includes(query) ||
          seq.skuName.toLowerCase().includes(query)
      );
      setFilteredSequences(filtered);
    }
  }, [searchQuery, skuSequences]);

  const loadSequences = async () => {
    setLoading(true);
    try {
      console.log('🔍 Loading sequences for type:', sequenceType);

      const response = await skuSequenceService.getSkuSequences(sequenceType, {
        pageNumber: 1,
        pageSize: 1000, // Load all for sequencing
        isCountRequired: false,
        sortCriterias: [
          {
            sortParameter: 'serial_no',
            direction: 'Asc',
          },
        ],
      });

      console.log('📦 Raw API Response:', response);
      console.log('📊 Paged Data:', response.pagedData);

      const sequences = response.pagedData || [];

      console.log('✅ Loaded sequences:', sequences);
      console.log('📝 First sequence sample:', sequences[0]);

      setSkuSequences(sequences);
      setFilteredSequences(sequences);
      setOriginalSequences(JSON.parse(JSON.stringify(sequences))); // Deep copy
      setHasChanges(false);

      if (sequences.length > 0) {
        toast.success(`Loaded ${sequences.length} product${sequences.length !== 1 ? 's' : ''}`);
      }
    } catch (error: any) {
      console.error('❌ Error loading sequences:', error);
      // Don't show error toast for empty sequences (common scenario)
      if (error.status !== 404) {
        toast.error(error.message || 'Failed to load SKU sequences');
      }
      // Set empty arrays on error
      setSkuSequences([]);
      setFilteredSequences([]);
      setOriginalSequences([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDragStart = (event: any) => {
    const draggedItemId = event.active.id as string;
    console.log('🎯 Drag started:', draggedItemId);
    console.log('📦 Selected items:', Array.from(selectedItems));
    console.log('🔢 Total selected:', selectedItems.size);
    setIsDragging(true);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    setIsDragging(false);
    const { active, over } = event;

    console.log('🏁 Drag ended - Active:', active?.id, 'Over:', over?.id);

    if (!over || active.id === over.id) {
      console.log('⚠️ Invalid drop - same position or no target');
      return;
    }

    const draggedItemId = active.id as string;
    const targetItemId = over.id as string;

    // Check if the dragged item is part of a multi-selection
    const isDraggingSelected = selectedItems.has(draggedItemId);
    const isMultiDrag = isDraggingSelected && selectedItems.size > 1;

    console.log('🔍 Is multi-drag?', isMultiDrag, '| Selected count:', selectedItems.size);

    if (isMultiDrag) {
      // Multi-item drag: Move all selected items together
      console.log('🔄 Starting multi-item drag with', selectedItems.size, 'items');

      setSkuSequences((currentItems) => {
        console.log('📋 Current items count:', currentItems.length);

        // Preserve the order of selected items based on current position
        const selectedItemsArray = currentItems
          .filter(item => selectedItems.has(item.uid))
          .sort((a, b) => {
            const aIndex = currentItems.findIndex(x => x.uid === a.uid);
            const bIndex = currentItems.findIndex(x => x.uid === b.uid);
            return aIndex - bIndex;
          });

        console.log('✅ Selected items for move:', selectedItemsArray.map(i => i.skuCode));

        const nonSelectedItems = currentItems.filter(item => !selectedItems.has(item.uid));
        console.log('📦 Non-selected items:', nonSelectedItems.length);

        // Find the target position in the CURRENT items array
        const targetIndex = currentItems.findIndex((item) => item.uid === targetItemId);
        const targetIsSelected = selectedItems.has(targetItemId);

        console.log('🎯 Target index:', targetIndex, '| Target is selected?', targetIsSelected);

        // If dropping on a selected item, don't move
        if (targetIsSelected) {
          console.log('⚠️ Cannot drop on selected item - aborting');
          toast.warning('Cannot drop on selected item');
          return currentItems;
        }

        // Better approach: Find position of target in the final array
        // Step 1: Get the first selected item's current position
        const firstSelectedIndex = currentItems.findIndex(item => item.uid === selectedItemsArray[0].uid);
        const isDraggingDown = targetIndex > firstSelectedIndex;

        console.log('🔽 Dragging down?', isDraggingDown, '| First selected at:', firstSelectedIndex, '| Target at:', targetIndex);

        // Step 2: Find where the target item will be in the non-selected array
        const targetItemInNonSelected = nonSelectedItems.findIndex(item => item.uid === targetItemId);

        console.log('🎯 Target position in non-selected array:', targetItemInNonSelected);

        let insertPosition: number;

        if (targetItemInNonSelected === -1) {
          // Target is selected (shouldn't happen, but handle it)
          console.log('⚠️ Target not found in non-selected items');
          insertPosition = 0;
        } else {
          if (isDraggingDown) {
            // When dragging down, insert AFTER the target
            insertPosition = targetItemInNonSelected + 1;
            console.log('⬇️ Inserting AFTER position', targetItemInNonSelected, '=> position', insertPosition);
          } else {
            // When dragging up, insert BEFORE the target
            insertPosition = targetItemInNonSelected;
            console.log('⬆️ Inserting BEFORE position', targetItemInNonSelected, '=> position', insertPosition);
          }
        }

        console.log('📍 Final insert position:', insertPosition, '(in non-selected array of', nonSelectedItems.length, 'items)');

        // Insert selected items at the calculated position
        const newItems = [...nonSelectedItems];
        newItems.splice(insertPosition, 0, ...selectedItemsArray);

        console.log('📊 Final order with positions:');
        newItems.forEach((item, idx) => {
          const wasSelected = selectedItems.has(item.uid);
          console.log(`  #${idx + 1}: ${item.skuCode}${wasSelected ? ' ✓ (moved)' : ''}`);
        });

        // Update serial numbers
        const reordered = newItems.map((item, index) => ({
          ...item,
          serialNo: index + 1,
        }));

        setHasChanges(true);
        console.log('✅ Successfully moved', selectedItems.size, 'items to position', insertPosition);
        toast.success(`Moved ${selectedItems.size} item${selectedItems.size !== 1 ? 's' : ''} together`, {
          description: `Items repositioned to #${insertPosition}`
        });
        return reordered;
      });
    } else {
      // Single item drag: Original behavior
      console.log('🔄 Single item drag');

      setSkuSequences((currentItems) => {
        const oldIndex = currentItems.findIndex((item) => item.uid === draggedItemId);
        const newIndex = currentItems.findIndex((item) => item.uid === targetItemId);

        console.log('📍 Moving from', oldIndex, 'to', newIndex);

        const reordered = arrayMove(currentItems, oldIndex, newIndex);

        // Update serial numbers
        const updated = reordered.map((item, index) => ({
          ...item,
          serialNo: index + 1,
        }));

        setHasChanges(true);
        console.log('✅ Single item moved');
        return updated;
      });
    }
  };

  const handleSave = async () => {
    if (!hasChanges) {
      toast.info('No changes to save');
      return;
    }

    setSaving(true);
    try {
      console.log('💾 Saving sequences...');
      console.log('📦 Current sequences:', skuSequences.length);
      console.log('🗑️ Deleted items:', deletedItemUIDs.length);
      console.log('🔄 Has changes:', hasChanges);

      // Step 1: Delete removed items if any
      if (deletedItemUIDs.length > 0) {
        console.log('🗑️ Deleting', deletedItemUIDs.length, 'items from backend');
        await skuSequenceService.deleteSkuSequences(deletedItemUIDs);
        console.log('✅ Deletion complete');
      }

      // Step 2: Save/Update remaining sequences
      if (skuSequences.length > 0) {
        console.log('💾 Saving', skuSequences.length, 'sequences');
        const updates = await skuSequenceService.reorderSequences(
          skuSequences,
          sequenceType
        );
        console.log('📤 Sending updates to backend:', updates);
        await skuSequenceService.saveSkuSequences(updates);
        console.log('✅ Save complete');
      }

      toast.success('SKU sequence saved successfully!');
      setHasChanges(false);
      setOriginalSequences(JSON.parse(JSON.stringify(skuSequences)));
      setDeletedItemUIDs([]); // Clear deleted items tracker

      // Clear all selections after save
      setSelectedItems(new Set());
      setLastSelectedIndex(null);
    } catch (error: any) {
      console.error('❌ Save error:', error);
      console.error('❌ Error details:', error.response?.data || error);
      toast.error(error.message || 'Failed to save SKU sequence');
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    setSkuSequences(JSON.parse(JSON.stringify(originalSequences)));
    setHasChanges(false);

    // Clear all selections when resetting
    setSelectedItems(new Set());
    setLastSelectedIndex(null);

    toast.info('Changes reset to last saved state');
  };

  // Load available products with pagination
  const loadAvailableProducts = async (page: number = 1, append: boolean = false) => {
    if (loadingProducts) return;

    try {
      setLoadingProducts(true);

      const pageSize = 50; // Load 50 products at a time
      const requestBody = {
        PageNumber: page,
        PageSize: pageSize,
        IsCountRequired: page === 1,
        FilterCriterias: productSearchQuery
          ? [{ Name: 'skucodeandname', Value: productSearchQuery }]
          : [],
        SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
      };

      const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api'}/SKU/SelectAllSKUDetailsWebView`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
            'Content-Type': 'application/json',
            Accept: 'application/json',
          },
          body: JSON.stringify(requestBody),
        }
      );

      if (response.ok) {
        const result = await response.json();

        if (result.IsSuccess && result.Data) {
          const data = result.Data;
          let skuData: any[] = [];

          // Handle PagedResponse structure from backend
          if (data.PagedData && Array.isArray(data.PagedData)) {
            skuData = data.PagedData;
          } else if (Array.isArray(data)) {
            skuData = data;
          }

          // Transform SKU data to match expected format
          const products: SKUListView[] = skuData
            .map((sku: any) => ({
              UID: sku.SKUUID || sku.UID || sku.Id,
              SKUUID: sku.SKUUID || sku.UID || sku.Id,
              Code: sku.SKUCode || sku.Code,
              SKUCode: sku.SKUCode || sku.Code,
              Name: sku.SKULongName || sku.Name || sku.LongName,
              LongName: sku.LongName || sku.Name || sku.AliasName,
              SKULongName: sku.SKULongName || sku.Name || sku.LongName,
              IsActive: sku.IsActive !== false,
            }))
            .filter((sku: SKUListView) => sku.Code && sku.Name);

          if (append) {
            setAvailableProducts((prev) => [...prev, ...products]);
          } else {
            setAvailableProducts(products);
          }

          // Check if there are more products to load
          setHasMoreProducts(products.length === pageSize);
        } else {
          console.error('API returned success=false or no data');
          if (!append) {
            toast.error('Failed to load products');
            setAvailableProducts([]);
          }
        }
      } else {
        console.error('Failed to load products:', response.status);
        if (!append) {
          toast.error('Failed to load products from server');
          setAvailableProducts([]);
        }
      }
    } catch (error: any) {
      console.error('Error loading products:', error);
      if (!append) {
        toast.error(error.message || 'Failed to load products');
        setAvailableProducts([]);
      }
    } finally {
      setLoadingProducts(false);
    }
  };

  // Load more products when scrolling
  const loadMoreProducts = () => {
    if (!loadingProducts && hasMoreProducts) {
      const nextPage = productPage + 1;
      setProductPage(nextPage);
      loadAvailableProducts(nextPage, true);
    }
  };

  // Auto-load more when hiding selected items
  React.useEffect(() => {
    if (hideSelectedItems && !loadingProducts && hasMoreProducts && addPopoverOpen) {
      const existingSkuUIDs = new Set(skuSequences.map(s => s.skuUID));
      const selectedUIDs = new Set(selectedProductsForAdd.map(p => p.UID || p.SKUUID || ''));
      const visibleItems = availableProducts.filter((p) => {
        const skuUID = p.UID || p.SKUUID || '';
        const isExisting = existingSkuUIDs.has(skuUID);
        const isSelected = selectedUIDs.has(skuUID);
        return !isExisting && !isSelected;
      });

      if (visibleItems.length < 5) {
        setTimeout(() => {
          loadMoreProducts();
        }, 100);
      }
    }
  }, [hideSelectedItems, availableProducts, selectedProductsForAdd, skuSequences, loadingProducts, hasMoreProducts, addPopoverOpen]);

  const handleAddProducts = () => {
    if (selectedProductsForAdd.length === 0) {
      toast.warning('Please select at least one product');
      return;
    }

    const newSequences: SkuSequenceUI[] = [];
    let currentMaxSerial = skuSequences.length > 0
      ? Math.max(...skuSequences.map(s => s.serialNo))
      : 0;

    selectedProductsForAdd.forEach((product) => {
      currentMaxSerial++;
      newSequences.push({
        uid: crypto.randomUUID(),
        skuUID: product.UID || product.SKUUID || '',
        skuCode: product.Code || product.SKUCode || '',
        skuName: product.LongName || product.Name || product.SKULongName || '',
        seqType: sequenceType,
        serialNo: currentMaxSerial,
        buOrgUID: '2d893d92-dc1b-5904-934c-621103a900e3', // Default BU Org
        franchiseeOrgUID: 'WINIT',
      });
    });

    setSkuSequences([...skuSequences, ...newSequences]);
    setHasChanges(true);
    setAddPopoverOpen(false);
    setSelectedProductsForAdd([]);
    toast.success(`Added ${newSequences.length} product(s) to sequence`);
  };

  const toggleProductSelection = (product: SKUListView) => {
    const skuUID = product.UID || product.SKUUID || '';
    const isSelected = selectedProductsForAdd.some(p => (p.UID || p.SKUUID) === skuUID);

    if (isSelected) {
      setSelectedProductsForAdd(selectedProductsForAdd.filter(p => (p.UID || p.SKUUID) !== skuUID));
    } else {
      setSelectedProductsForAdd([...selectedProductsForAdd, product]);
    }
  };

  const handleRemoveProduct = (uid: string) => {
    // Track deleted item UID for backend deletion
    setDeletedItemUIDs(prev => [...prev, uid]);

    const updatedSequences = skuSequences.filter(s => s.uid !== uid);
    // Reorder serial numbers
    const reordered = updatedSequences.map((item, index) => ({
      ...item,
      serialNo: index + 1,
    }));
    setSkuSequences(reordered);
    setSelectedItems(new Set()); // Clear selection
    setHasChanges(true);
    toast.success('Product removed from sequence');
  };

  // Handle item selection - simplified for easier multi-select
  const handleItemSelect = (uid: string, event: React.MouseEvent) => {
    const currentIndex = filteredSequences.findIndex(s => s.uid === uid);

    if (event.shiftKey && lastSelectedIndex !== null) {
      // Shift + Click: Select range
      const start = Math.min(lastSelectedIndex, currentIndex);
      const end = Math.max(lastSelectedIndex, currentIndex);
      const newSelection = new Set(selectedItems);
      for (let i = start; i <= end; i++) {
        newSelection.add(filteredSequences[i].uid);
      }
      setSelectedItems(newSelection);
    } else {
      // Click on checkbox or row: Toggle selection (keep other selections)
      const newSelection = new Set(selectedItems);
      if (newSelection.has(uid)) {
        newSelection.delete(uid);
      } else {
        newSelection.add(uid);
      }
      setSelectedItems(newSelection);
      setLastSelectedIndex(currentIndex);
    }
  };

  // Handle checkbox only click - simple toggle
  const handleCheckboxToggle = (uid: string) => {
    const newSelection = new Set(selectedItems);
    if (newSelection.has(uid)) {
      newSelection.delete(uid);
    } else {
      newSelection.add(uid);
    }
    setSelectedItems(newSelection);
    const currentIndex = filteredSequences.findIndex(s => s.uid === uid);
    setLastSelectedIndex(currentIndex);
  };

  // Export current sequence to Excel
  const handleExport = () => {
    try {
      const exportData = skuSequences.map((seq, index) => ({
        'Serial No': index + 1,
        'SKU Code': seq.skuCode,
        'Product Name': seq.skuName,
      }));

      const worksheet = XLSX.utils.json_to_sheet(exportData);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'SKU Sequence');

      // Auto-size columns
      const maxWidth = 50;
      const colWidths = [
        { wch: 10 }, // Serial No
        { wch: 20 }, // SKU Code
        { wch: maxWidth }, // Product Name
      ];
      worksheet['!cols'] = colWidths;

      const fileName = `SKU_Sequence_${sequenceType}_${new Date().toISOString().split('T')[0]}.xlsx`;
      XLSX.writeFile(workbook, fileName);
      toast.success('Sequence exported successfully!');
    } catch (error: any) {
      console.error('Export error:', error);
      toast.error('Failed to export sequence');
    }
  };

  // Download Excel template
  const handleDownloadTemplate = () => {
    try {
      const templateData = [
        {
          'Serial No': 1,
          'SKU Code': 'EXAMPLE001',
        },
        {
          'Serial No': 2,
          'SKU Code': 'EXAMPLE002',
        },
      ];

      const worksheet = XLSX.utils.json_to_sheet(templateData);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'SKU Sequence');

      // Add instructions in a separate sheet
      const instructions = [
        { Instructions: 'How to use this template:' },
        { Instructions: '1. Fill in the Serial No (sequence order)' },
        { Instructions: '2. Enter the SKU Code (must match existing products)' },
        { Instructions: '3. Save the file and import it back' },
        { Instructions: '' },
        { Instructions: 'Note: Serial No determines the display order (1 = first, 2 = second, etc.)' },
        { Instructions: 'The system will validate SKU Codes against your product database' },
      ];
      const instructionSheet = XLSX.utils.json_to_sheet(instructions);
      XLSX.utils.book_append_sheet(workbook, instructionSheet, 'Instructions');

      // Auto-size columns
      worksheet['!cols'] = [{ wch: 10 }, { wch: 20 }];
      instructionSheet['!cols'] = [{ wch: 80 }];

      XLSX.writeFile(workbook, 'SKU_Sequence_Template.xlsx');
      toast.success('Template downloaded successfully!');
    } catch (error: any) {
      console.error('Template download error:', error);
      toast.error('Failed to download template');
    }
  };

  // Import sequence from Excel
  const handleImport = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = async (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: 'array' });
        const worksheet = workbook.Sheets[workbook.SheetNames[0]];
        const jsonData = XLSX.utils.sheet_to_json(worksheet) as any[];

        console.log('📥 Imported data:', jsonData);

        if (jsonData.length === 0) {
          toast.error('The Excel file is empty');
          return;
        }

        // Validate and map imported data
        const importedItems: { skuCode: string; serialNo: number }[] = [];
        const errors: string[] = [];

        jsonData.forEach((row, index) => {
          const skuCode = row['SKU Code']?.toString().trim();
          const serialNo = parseInt(row['Serial No']?.toString() || '0');

          if (!skuCode) {
            errors.push(`Row ${index + 2}: Missing SKU Code`);
            return;
          }

          if (!serialNo || serialNo <= 0) {
            errors.push(`Row ${index + 2}: Invalid Serial No`);
            return;
          }

          importedItems.push({ skuCode, serialNo });
        });

        if (errors.length > 0) {
          toast.error(`Import validation failed:\n${errors.slice(0, 5).join('\n')}`);
          return;
        }

        // Fetch full SKU details for the imported codes
        toast.info('Validating SKU codes...');
        const skuCodes = importedItems.map(item => item.skuCode);

        // Fetch all SKUs to validate
        const requestBody = {
          PageNumber: 1,
          PageSize: 10000,
          IsCountRequired: false,
          FilterCriterias: [],
          SortCriterias: [{ SortParameter: 'SKUCode', Direction: 'Asc' }],
        };

        const response = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api'}/SKU/SelectAllSKUDetailsWebView`,
          {
            method: 'POST',
            headers: {
              Authorization: `Bearer ${localStorage.getItem('auth_token')}`,
              'Content-Type': 'application/json',
              Accept: 'application/json',
            },
            body: JSON.stringify(requestBody),
          }
        );

        if (!response.ok) {
          throw new Error('Failed to fetch SKU details');
        }

        const result = await response.json();
        const allSkus = result.Data?.PagedData || [];

        // Map imported items to SKU sequences
        const newSequences: SkuSequenceUI[] = [];
        const notFoundSkus: string[] = [];

        importedItems.forEach(item => {
          const sku = allSkus.find((s: any) =>
            (s.SKUCode || s.Code)?.toString().trim().toLowerCase() === item.skuCode.toLowerCase()
          );

          if (sku) {
            newSequences.push({
              uid: crypto.randomUUID(),
              skuUID: sku.SKUUID || sku.UID || sku.Id,
              skuCode: sku.SKUCode || sku.Code,
              skuName: sku.SKULongName || sku.Name || sku.LongName,
              seqType: sequenceType,
              serialNo: item.serialNo,
              buOrgUID: '',
              franchiseeOrgUID: '',
            });
          } else {
            notFoundSkus.push(item.skuCode);
          }
        });

        if (notFoundSkus.length > 0) {
          toast.error(`SKU codes not found: ${notFoundSkus.slice(0, 5).join(', ')}${notFoundSkus.length > 5 ? '...' : ''}`);
          return;
        }

        // Sort by serial number and update sequence
        newSequences.sort((a, b) => a.serialNo - b.serialNo);
        const reorderedSequences = newSequences.map((seq, index) => ({
          ...seq,
          serialNo: index + 1,
        }));

        setSkuSequences(reorderedSequences);
        setHasChanges(true);
        toast.success(`Successfully imported ${reorderedSequences.length} products`);
      } catch (error: any) {
        console.error('Import error:', error);
        toast.error(error.message || 'Failed to import sequence');
      }
    };

    reader.readAsArrayBuffer(file);
    // Reset input value to allow re-importing the same file
    event.target.value = '';
  };

  // Select all items
  const handleSelectAll = () => {
    const allUIDs = new Set(filteredSequences.map(s => s.uid));
    setSelectedItems(allUIDs);
  };

  // Quick Jump to SKU Code
  const handleJumpToSKU = () => {
    if (!jumpToSKU.trim()) {
      toast.error('Please enter a SKU code');
      return;
    }

    const query = jumpToSKU.trim().toLowerCase();
    const targetIndex = filteredSequences.findIndex(
      (seq) => seq.skuCode.toLowerCase().includes(query)
    );

    if (targetIndex === -1) {
      toast.error(`SKU code "${jumpToSKU}" not found in sequence`);
      return;
    }

    const targetItem = filteredSequences[targetIndex];
    if (targetItem && itemRefs.current[targetItem.uid]) {
      itemRefs.current[targetItem.uid]?.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      });
      // Highlight the item (navigation only - doesn't change sequence)
      setSelectedItems(new Set([targetItem.uid]));
      setLastSelectedIndex(targetIndex);
      toast.success(`Found SKU: ${targetItem.skuCode}`);
      setJumpToSKU('');
      console.log('🎯 Quick Jump - Navigation only, hasChanges:', hasChanges);
    }
  };

  // Move Selected Items to Specific Position
  const handleMoveToPosition = () => {
    if (selectedItems.size === 0) {
      toast.error('Please select at least one item to move');
      return;
    }

    const targetPosition = parseInt(moveToPosition);
    if (isNaN(targetPosition) || targetPosition < 1 || targetPosition > skuSequences.length) {
      toast.error(`Please enter a valid position between 1 and ${skuSequences.length}`);
      return;
    }

    try {
      // Get selected and non-selected items
      const selectedSeqs = skuSequences.filter(seq => selectedItems.has(seq.uid));
      const nonSelectedSeqs = skuSequences.filter(seq => !selectedItems.has(seq.uid));

      // Insert selected items at target position (convert to 0-based index)
      const insertIndex = targetPosition - 1;
      const reordered = [
        ...nonSelectedSeqs.slice(0, insertIndex),
        ...selectedSeqs,
        ...nonSelectedSeqs.slice(insertIndex)
      ];

      // Update serial numbers
      const updated = reordered.map((item, index) => ({
        ...item,
        serialNo: index + 1,
      }));

      setSkuSequences(updated);
      setHasChanges(true);

      // Scroll to first moved item
      setTimeout(() => {
        if (selectedSeqs[0] && itemRefs.current[selectedSeqs[0].uid]) {
          itemRefs.current[selectedSeqs[0].uid]?.scrollIntoView({
            behavior: 'smooth',
            block: 'center',
          });
        }
      }, 100);

      toast.success(`Moved ${selectedItems.size} item${selectedItems.size !== 1 ? 's' : ''} to position ${targetPosition}`);
      setMoveToPosition('');
      console.log('✅ Moved items to position:', targetPosition);
    } catch (error: any) {
      console.error('❌ Error moving items:', error);
      toast.error('Failed to move items');
    }
  };

  // Clear selection
  const handleClearSelection = () => {
    setSelectedItems(new Set());
    setLastSelectedIndex(null);
  };

  // Delete selected items
  const handleDeleteSelected = () => {
    if (selectedItems.size === 0) return;

    // Track all deleted item UIDs for backend deletion
    const deletedUIDs = Array.from(selectedItems);
    setDeletedItemUIDs(prev => [...prev, ...deletedUIDs]);

    const updatedSequences = skuSequences.filter(s => !selectedItems.has(s.uid));
    // Reorder serial numbers
    const reordered = updatedSequences.map((item, index) => ({
      ...item,
      serialNo: index + 1,
    }));
    setSkuSequences(reordered);
    setSelectedItems(new Set());
    setLastSelectedIndex(null);
    setHasChanges(true);
    toast.success(`Removed ${deletedUIDs.length} product${deletedUIDs.length !== 1 ? 's' : ''} from sequence`);
  };

  const sequenceTypes = [
    { label: 'Sales Order', value: 'SalesOrder' },
    { label: 'Return Order', value: 'ReturnOrder' },
    { label: 'Store Check', value: 'StoreCheck' },
  ];

  return (
    <div className="container mx-auto p-6 max-w-7xl">
      <Card className="shadow-sm">
        <CardHeader className="border-b bg-gradient-to-r from-gray-50 to-white">
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl font-bold text-gray-900">
                Product Sequencing
              </CardTitle>
              <p className="text-sm text-gray-600 mt-1">
                Manage the order in which products appear in different contexts
              </p>
            </div>
            {hasChanges && (
              <Badge variant="destructive" className="text-sm px-3 py-1">
                <AlertCircle className="h-3 w-3 mr-1" />
                Unsaved Changes
              </Badge>
            )}
          </div>
        </CardHeader>

        <CardContent className="space-y-6 pt-6">
          {/* Controls */}
          <div className="flex flex-col sm:flex-row items-start sm:items-end justify-between gap-4 bg-gray-50 p-4 rounded-lg border">
            <div className="w-full sm:w-64">
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Sequence Type
              </label>
              <Select value={sequenceType} onValueChange={setSequenceType}>
                <SelectTrigger className="bg-white">
                  <SelectValue placeholder="Select sequence type" />
                </SelectTrigger>
                <SelectContent>
                  {sequenceTypes.map((type) => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Import/Export Buttons */}
            <div className="flex gap-2 flex-wrap items-center">
              <Button
                variant="outline"
                size="sm"
                onClick={handleDownloadTemplate}
                className="whitespace-nowrap"
              >
                <FileSpreadsheet className="h-4 w-4 mr-2" />
                Template
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={handleExport}
                disabled={skuSequences.length === 0}
                className="whitespace-nowrap"
              >
                <Download className="h-4 w-4 mr-2" />
                Export
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => document.getElementById('excel-import')?.click()}
                className="whitespace-nowrap"
              >
                <Upload className="h-4 w-4 mr-2" />
                Import
              </Button>
              <input
                id="excel-import"
                type="file"
                accept=".xlsx,.xls"
                onChange={handleImport}
                className="hidden"
              />
            </div>

          </div>

          {/* Info Alerts */}
          {!loading && selectedItems.size > 0 && (
            <Alert className="border-green-200 bg-green-50">
              <CheckCircle2 className="h-4 w-4 text-green-600" />
              <AlertDescription className="text-green-800">
                <strong>{selectedItems.size} item{selectedItems.size !== 1 ? 's' : ''} selected.</strong>
                {selectedItems.size > 1 && (
                  <span className="ml-1">
                    Drag any selected item to move all {selectedItems.size} items together as a group!
                  </span>
                )}
              </AlertDescription>
            </Alert>
          )}

          {/* Search and Actions */}
          <div className="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center">
            {/* Search Bar */}
            <div className="flex-1 max-w-md">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  type="text"
                  placeholder="Search products in sequence..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10"
                />
                {searchQuery && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="absolute right-1 top-1/2 transform -translate-y-1/2 h-7 w-7"
                    onClick={() => setSearchQuery('')}
                  >
                    <X className="h-3 w-3" />
                  </Button>
                )}
              </div>
            </div>

            {/* Add Products Dropdown */}
            <Popover
              open={addPopoverOpen}
              onOpenChange={(open) => {
                setAddPopoverOpen(open);
                if (open) {
                  setProductPage(1);
                  setHasMoreProducts(true);
                  setProductSearchQuery('');
                  loadAvailableProducts(1, false);
                }
              }}
            >
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  role="combobox"
                  aria-expanded={addPopoverOpen}
                  className={cn(
                    "w-[400px] justify-between",
                    selectedProductsForAdd.length > 0 && "border-primary"
                  )}
                >
                  <span className={selectedProductsForAdd.length > 0 ? 'font-medium' : 'text-muted-foreground'}>
                    {selectedProductsForAdd.length > 0
                      ? `${selectedProductsForAdd.length} product${selectedProductsForAdd.length !== 1 ? 's' : ''} selected`
                      : 'Select products to add...'}
                  </span>
                  <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-[500px] p-0" align="start">
                <div className="p-2">
                  <Input
                    placeholder="Search products..."
                    value={productSearchQuery}
                    onChange={(e) => {
                      setProductSearchQuery(e.target.value);
                      if (productSearchDebounceTimer.current) {
                        clearTimeout(productSearchDebounceTimer.current);
                      }
                      productSearchDebounceTimer.current = setTimeout(() => {
                        setProductPage(1);
                        setHasMoreProducts(true);
                        loadAvailableProducts(1, false);
                      }, 300);
                    }}
                    className="mb-2"
                  />
                  <div className="flex items-center justify-between mb-2 px-2">
                    <span className="text-sm font-medium">
                      {selectedProductsForAdd.length} selected | {availableProducts.length} loaded
                    </span>
                    <div className="flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setHideSelectedItems(!hideSelectedItems)}
                        className="h-7"
                      >
                        {hideSelectedItems ? (
                          <>
                            <Eye className="h-3 w-3 mr-1" />
                            Show All
                          </>
                        ) : (
                          <>
                            <EyeOff className="h-3 w-3 mr-1" />
                            Hide Selected
                          </>
                        )}
                      </Button>
                      {selectedProductsForAdd.length > 0 && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => setSelectedProductsForAdd([])}
                        >
                          Clear all
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
                <Separator />
                <div
                  className="max-h-[300px] overflow-y-auto p-2"
                  onScroll={(e) => {
                    const target = e.target as HTMLDivElement;
                    const bottom = target.scrollHeight - target.scrollTop === target.clientHeight;
                    if (bottom && !loadingProducts && hasMoreProducts) {
                      loadMoreProducts();
                    }
                  }}
                >
                  {availableProducts
                    .filter(p => {
                      const skuUID = p.UID || p.SKUUID || '';
                      const existingSkuUIDs = new Set(skuSequences.map(s => s.skuUID));
                      const isExisting = existingSkuUIDs.has(skuUID);
                      const selectedUIDs = new Set(selectedProductsForAdd.map(sp => sp.UID || sp.SKUUID));
                      const isSelected = selectedUIDs.has(skuUID);

                      if (isExisting) return false;
                      if (hideSelectedItems && isSelected) return false;

                      return true;
                    })
                    .map((product) => {
                      const skuUID = product.UID || product.SKUUID || '';
                      const selectedUIDs = new Set(selectedProductsForAdd.map(sp => sp.UID || sp.SKUUID));
                      const isSelected = selectedUIDs.has(skuUID);

                      return (
                        <div
                          key={skuUID}
                          className={cn(
                            'flex items-center space-x-2 p-2 rounded cursor-pointer select-none',
                            isSelected && 'bg-accent/50 hover:bg-accent/70'
                          )}
                          onClick={() => toggleProductSelection(product)}
                        >
                          <Checkbox
                            checked={isSelected}
                            onCheckedChange={() => toggleProductSelection(product)}
                          />
                          <div className="flex-1">
                            <div className="font-medium">{product.Code || product.SKUCode}</div>
                            <div className="text-xs text-muted-foreground">
                              {product.LongName || product.Name || product.SKULongName}
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  {availableProducts.filter(p => {
                    const skuUID = p.UID || p.SKUUID || '';
                    const existingSkuUIDs = new Set(skuSequences.map(s => s.skuUID));
                    const isExisting = existingSkuUIDs.has(skuUID);
                    const selectedUIDs = new Set(selectedProductsForAdd.map(sp => sp.UID || sp.SKUUID));
                    const isSelected = selectedUIDs.has(skuUID);
                    if (isExisting) return false;
                    if (hideSelectedItems && isSelected) return false;
                    return true;
                  }).length === 0 && (
                    <div className="text-center py-4">
                      <p className="text-sm text-muted-foreground">
                        {hideSelectedItems
                          ? loadingProducts
                            ? 'Loading more unselected items...'
                            : hasMoreProducts
                            ? 'Loading more items automatically...'
                            : 'No more unselected items available'
                          : productSearchQuery
                          ? 'No products found matching your search'
                          : loadingProducts
                          ? 'Loading items...'
                          : 'All products are already in the sequence'}
                      </p>
                    </div>
                  )}
                  {loadingProducts && (
                    <div className="flex items-center justify-center py-2">
                      <RefreshCw className="h-4 w-4 animate-spin mr-2" />
                      <span className="text-sm text-muted-foreground">Loading more products...</span>
                    </div>
                  )}
                  {!loadingProducts && hasMoreProducts && availableProducts.length > 0 && (
                    <div className="text-center py-2">
                      <span className="text-xs text-muted-foreground">
                        {hideSelectedItems ? 'Auto-loading more...' : 'Scroll for more'}
                      </span>
                    </div>
                  )}
                </div>
                <Separator />
                <div className="p-2">
                  <Button
                    onClick={handleAddProducts}
                    disabled={selectedProductsForAdd.length === 0}
                    className="w-full"
                  >
                    Add {selectedProductsForAdd.length > 0 && `(${selectedProductsForAdd.length})`} Product{selectedProductsForAdd.length !== 1 ? 's' : ''}
                  </Button>
                </div>
              </PopoverContent>
            </Popover>

            {/* Action Buttons */}
            <div className="flex gap-2">
              <Button
                onClick={handleSave}
                disabled={!hasChanges || saving}
                className="flex items-center gap-2"
              >
                {saving ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="h-4 w-4" />
                    Save
                  </>
                )}
              </Button>

              <Button
                variant="outline"
                onClick={handleReset}
                disabled={!hasChanges || saving}
                className="flex items-center gap-2"
              >
                <RotateCcw className="h-4 w-4" />
                Reset
              </Button>
            </div>
          </div>

          {/* Quick Actions Toggle & Section */}
          {!loading && filteredSequences.length > 0 && (
            <div className="border rounded-lg bg-blue-50/50">
              {/* Toggle Button */}
              <button
                onClick={() => setShowQuickActions(!showQuickActions)}
                className="w-full px-4 py-3 flex items-center justify-between hover:bg-blue-50 transition-colors"
                type="button"
              >
                <div className="flex items-center gap-2">
                  <Zap className="h-4 w-4 text-blue-600" />
                  <span className="font-medium text-sm">Quick Actions</span>
                  {selectedItems.size > 0 && (
                    <Badge variant="secondary" className="text-xs">
                      {selectedItems.size} selected
                    </Badge>
                  )}
                </div>
                {showQuickActions ? (
                  <ChevronUp className="h-4 w-4 text-gray-500" />
                ) : (
                  <ChevronDown className="h-4 w-4 text-gray-500" />
                )}
              </button>

              {/* Quick Actions Panel */}
              {showQuickActions && (
                <div className="px-4 pb-4 border-t">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Find SKU Code
                      </label>
                      <div className="flex gap-2">
                        <Input
                          type="text"
                          placeholder="Search by SKU code..."
                          value={jumpToSKU}
                          onChange={(e) => setJumpToSKU(e.target.value)}
                          onKeyPress={(e) => e.key === 'Enter' && handleJumpToSKU()}
                          className="flex-1 bg-white"
                        />
                        <Button
                          onClick={handleJumpToSKU}
                          disabled={!jumpToSKU.trim()}
                          variant="secondary"
                          size="sm"
                        >
                          <Search className="h-4 w-4 mr-1" />
                          Find
                        </Button>
                      </div>
                      <p className="text-xs text-gray-500 mt-1">Scroll to and select a product by SKU code</p>
                    </div>
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Move Selected Items to Position
                      </label>
                      <div className="flex gap-2">
                        <Input
                          type="number"
                          min="1"
                          max={skuSequences.length}
                          placeholder={`1-${skuSequences.length}`}
                          value={moveToPosition}
                          onChange={(e) => setMoveToPosition(e.target.value)}
                          onKeyPress={(e) => e.key === 'Enter' && handleMoveToPosition()}
                          className="w-32 bg-white"
                        />
                        <Button
                          onClick={handleMoveToPosition}
                          disabled={!moveToPosition || selectedItems.size === 0}
                          variant="default"
                          size="sm"
                        >
                          <GripVertical className="h-4 w-4 mr-1" />
                          Move
                        </Button>
                      </div>
                      {selectedItems.size > 0 ? (
                        <p className="text-xs text-green-600 mt-1 font-medium">
                          ✓ {selectedItems.size} item{selectedItems.size !== 1 ? 's' : ''} selected
                        </p>
                      ) : (
                        <p className="text-xs text-gray-500 mt-1">Select items first, then enter target position</p>
                      )}
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* SKU List */}
          {loading ? (
            <div className="space-y-3">
              <div className="flex items-center justify-between text-sm text-gray-500 pb-2 border-b">
                <Skeleton className="h-4 w-24" />
              </div>
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="flex items-center gap-3 p-4 bg-white border rounded-lg">
                  <Skeleton className="h-5 w-5" />
                  <Skeleton className="h-6 w-12" />
                  <div className="flex-1 grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Skeleton className="h-3 w-16" />
                      <Skeleton className="h-5 w-24" />
                    </div>
                    <div className="space-y-2">
                      <Skeleton className="h-3 w-16" />
                      <Skeleton className="h-5 w-32" />
                    </div>
                  </div>
                  <Skeleton className="h-8 w-8" />
                </div>
              ))}
            </div>
          ) : filteredSequences.length === 0 ? (
            <div className="text-center py-16 bg-gray-50 rounded-lg border-2 border-dashed">
              <div className="flex flex-col items-center gap-3">
                <div className="bg-gray-100 p-4 rounded-full">
                  {searchQuery ? (
                    <Search className="h-8 w-8 text-gray-400" />
                  ) : (
                    <AlertCircle className="h-8 w-8 text-gray-400" />
                  )}
                </div>
                <h3 className="text-lg font-semibold text-gray-900">
                  {searchQuery ? 'No products found' : 'No products in sequence'}
                </h3>
                <p className="text-sm text-gray-600 max-w-md">
                  {searchQuery
                    ? `No products match "${searchQuery}". Try different search terms or clear the search.`
                    : `No products have been added to the ${sequenceType} sequence yet. Use the dropdown above to add products.`}
                </p>
                {searchQuery && skuSequences.length > 0 && (
                  <Button
                    variant="outline"
                    onClick={() => setSearchQuery('')}
                    className="mt-2"
                  >
                    <X className="h-4 w-4 mr-2" />
                    Clear Search
                  </Button>
                )}
                {!searchQuery && (
                  <p className="text-xs text-gray-500 mt-2">
                    💡 Tip: Select products from the dropdown to build your sequence
                  </p>
                )}
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center justify-between pb-3 border-b">
                <div className="flex items-center gap-3">
                  <span className="text-sm font-semibold text-gray-700">
                    {filteredSequences.length} product{filteredSequences.length !== 1 ? 's' : ''}
                  </span>
                  {searchQuery && (
                    <Badge variant="secondary" className="text-xs">
                      Showing {filteredSequences.length} of {skuSequences.length}
                    </Badge>
                  )}
                  {selectedItems.size > 0 && (
                    <Badge variant="default" className="text-xs">
                      {selectedItems.size} selected
                    </Badge>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  {selectedItems.size > 0 ? (
                    <>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={handleClearSelection}
                        className="h-8 text-xs"
                      >
                        Clear Selection
                      </Button>
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={handleDeleteSelected}
                        className="h-8 text-xs"
                      >
                        <X className="h-3 w-3 mr-1" />
                        Delete Selected
                      </Button>
                    </>
                  ) : (
                    <>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={handleSelectAll}
                        className="h-8 text-xs"
                      >
                        Select All
                      </Button>
                      <div className="flex items-center gap-2 text-xs text-gray-500 ml-2">
                        <GripVertical className="h-4 w-4" />
                        <span>Select items • Drag to move together</span>
                      </div>
                    </>
                  )}
                </div>
              </div>

              <DndContext
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragStart={handleDragStart}
                onDragEnd={handleDragEnd}
              >
                <SortableContext
                  items={filteredSequences.map((s) => s.uid).filter(Boolean)}
                  strategy={verticalListSortingStrategy}
                >
                  <div className="max-h-[600px] overflow-y-auto pr-2 space-y-2 scroll-smooth">
                    {filteredSequences.map((sku, idx) => (
                      <SortableItem
                        key={`${sku.uid}-${idx}`}
                        sku={sku}
                        index={idx}
                        isSelected={selectedItems.has(sku.uid)}
                        onSelect={handleItemSelect}
                        onCheckboxToggle={handleCheckboxToggle}
                        onRemove={handleRemoveProduct}
                        totalSelected={selectedItems.size}
                        isDraggingAny={isDragging}
                        itemRef={(el, uid) => {
                          if (el) {
                            itemRefs.current[uid] = el;
                          } else {
                            delete itemRefs.current[uid];
                          }
                        }}
                      />
                    ))}
                  </div>
                </SortableContext>
              </DndContext>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
