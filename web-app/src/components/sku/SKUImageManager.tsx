import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/use-toast';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Checkbox
} from '@/components/ui/checkbox';
import {
  Upload,
  Image as ImageIcon,
  Star,
  Trash2,
  Eye,
  Users,
  Download,
  Grid,
  List,
  Search,
  Filter,
  UserPlus
} from 'lucide-react';
import {
  skuImagesService,
  FileSys,
  SKUWithImages,
  ImageUploadRequest
} from '@/services/sku/sku-images.service';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { Skeleton } from '@/components/ui/skeleton';

interface SKUImageManagerProps {
  mode?: 'view' | 'manage';
  initialSKUUID?: string;
}

interface EmployeeUser {
  UID: string;
  Name: string;
  Email: string;
  Role: string;
}

export default function SKUImageManager({ mode = 'manage', initialSKUUID }: SKUImageManagerProps) {
  const { toast } = useToast();
  
  // State management
  const [skusWithImages, setSKUsWithImages] = useState<SKUWithImages[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  
  // Upload dialog state
  const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [selectedSKU, setSelectedSKU] = useState<any>(null);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadPreview, setUploadPreview] = useState<string>('');
  const [uploading, setUploading] = useState(false);
  
  // Assignment dialog state
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [selectedImages, setSelectedImages] = useState<Set<string>>(new Set());
  const [availableUsers, setAvailableUsers] = useState<EmployeeUser[]>([]);
  const [selectedUsers, setSelectedUsers] = useState<Set<string>>(new Set());
  const [assignmentType, setAssignmentType] = useState<'view' | 'edit' | 'manage'>('view');
  const [assigning, setAssigning] = useState(false);
  
  // Delete confirmation
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [imageToDelete, setImageToDelete] = useState<FileSys | null>(null);
  const [imageUrls, setImageUrls] = useState<Record<string, string>>({});
  const [deleting, setDeleting] = useState(false);

  // Fetch SKUs with images
  const fetchSKUsWithImages = async () => {
    setLoading(true);
    try {
      const result = await skuImagesService.getSKUsWithImages(
        currentPage,
        pageSize,
        searchTerm || undefined
      );
      setSKUsWithImages(result.skusWithImages);
      setTotalCount(result.totalCount);
      
      // Generate image URLs for all images
      const urls: Record<string, string> = {};
      for (const skuWithImages of result.skusWithImages) {
        for (const image of skuWithImages.images) {
          if (!urls[image.UID]) {
            urls[image.UID] = await skuImagesService.getImageBlob(image);
          }
        }
      }
      setImageUrls(urls);
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to fetch SKU images',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  // Load mock employee data (in real app, fetch from employee service)
  const loadEmployeeUsers = async () => {
    // Mock data - replace with actual employee service
    const mockUsers: EmployeeUser[] = [
      { UID: 'emp1', Name: 'John Smith', Email: 'john@example.com', Role: 'Sales Manager' },
      { UID: 'emp2', Name: 'Sarah Johnson', Email: 'sarah@example.com', Role: 'Product Manager' },
      { UID: 'emp3', Name: 'Mike Brown', Email: 'mike@example.com', Role: 'Sales Rep' },
      { UID: 'emp4', Name: 'Lisa Davis', Email: 'lisa@example.com', Role: 'Marketing' },
    ];
    setAvailableUsers(mockUsers);
  };

  useEffect(() => {
    fetchSKUsWithImages();
    loadEmployeeUsers();
  }, [currentPage, searchTerm, pageSize]);

  // Handle file upload
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (file.type.startsWith('image/')) {
        setUploadFile(file);
        const reader = new FileReader();
        reader.onload = (e) => {
          setUploadPreview(e.target?.result as string);
        };
        reader.readAsDataURL(file);
      } else {
        toast({
          title: 'Invalid File',
          description: 'Please select an image file.',
          variant: 'destructive',
        });
      }
    }
  };

  // Upload image
  const handleUpload = async () => {
    if (!uploadFile || !selectedSKU) return;

    setUploading(true);
    try {
      const base64Data = await skuImagesService.fileToBase64(uploadFile);
      
      const uploadRequest: ImageUploadRequest = {
        linkedItemType: 'SKU',
        linkedItemUID: selectedSKU.SKUUID || selectedSKU.UID,
        fileSysType: 'Image',
        fileData: base64Data,
        fileType: uploadFile.type,
        fileName: uploadFile.name,
        displayName: `${selectedSKU.SKUCode} - ${uploadFile.name}`,
        fileSize: uploadFile.size,
        isDefault: false, // Can be set to true if it's the first image
      };

      await skuImagesService.uploadSKUImage(uploadRequest);
      
      toast({
        title: 'Success',
        description: 'Image uploaded successfully',
      });
      
      // Reset upload state
      setUploadDialogOpen(false);
      setUploadFile(null);
      setUploadPreview('');
      setSelectedSKU(null);
      
      // Refresh data
      await fetchSKUsWithImages();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to upload image',
        variant: 'destructive',
      });
    } finally {
      setUploading(false);
    }
  };

  // Set default image
  const handleSetDefault = async (image: FileSys, skuUID: string) => {
    try {
      // Get all images for this SKU
      const skuWithImages = skusWithImages.find(
        swi => (swi.sku.SKUUID || swi.sku.UID) === skuUID
      );
      
      if (!skuWithImages) return;

      // Create update request
      const updates = skuWithImages.images.map(img => ({
        SKUUID: skuUID,
        FileSysUID: img.UID,
        IsDefault: img.UID === image.UID
      }));

      await skuImagesService.updateSKUImageDefault(updates);
      
      toast({
        title: 'Success',
        description: 'Default image updated successfully',
      });
      
      await fetchSKUsWithImages();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to update default image',
        variant: 'destructive',
      });
    }
  };

  // Delete image
  const handleDeleteImage = async () => {
    if (!imageToDelete) return;

    setDeleting(true);
    try {
      await skuImagesService.deleteSKUImage(imageToDelete.UID);
      
      toast({
        title: 'Success',
        description: 'Image deleted successfully',
      });
      
      setDeleteDialogOpen(false);
      setImageToDelete(null);
      await fetchSKUsWithImages();
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to delete image',
        variant: 'destructive',
      });
    } finally {
      setDeleting(false);
    }
  };

  // Handle image selection for assignment
  const toggleImageSelection = (imageUID: string) => {
    const newSelected = new Set(selectedImages);
    if (newSelected.has(imageUID)) {
      newSelected.delete(imageUID);
    } else {
      newSelected.add(imageUID);
    }
    setSelectedImages(newSelected);
  };

  // Handle user selection for assignment
  const toggleUserSelection = (userUID: string) => {
    const newSelected = new Set(selectedUsers);
    if (newSelected.has(userUID)) {
      newSelected.delete(userUID);
    } else {
      newSelected.add(userUID);
    }
    setSelectedUsers(newSelected);
  };

  // Assign images to users
  const handleAssignImages = async () => {
    if (selectedImages.size === 0 || selectedUsers.size === 0) {
      toast({
        title: 'Selection Required',
        description: 'Please select both images and users for assignment.',
        variant: 'destructive',
      });
      return;
    }

    setAssigning(true);
    try {
      await skuImagesService.assignImagesToUsers({
        fileSysUIDs: Array.from(selectedImages),
        userUIDs: Array.from(selectedUsers),
        assignmentType,
      });

      toast({
        title: 'Success',
        description: `${selectedImages.size} images assigned to ${selectedUsers.size} users successfully`,
      });

      // Reset assignment state
      setAssignDialogOpen(false);
      setSelectedImages(new Set());
      setSelectedUsers(new Set());
    } catch (error) {
      toast({
        title: 'Error',
        description: 'Failed to assign images to users. This feature may need backend implementation.',
        variant: 'destructive',
      });
    } finally {
      setAssigning(false);
    }
  };

  // Render image grid
  const renderImageGrid = () => (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {skusWithImages.map((skuWithImages) => (
        <Card key={skuWithImages.sku.SKUUID || skuWithImages.sku.UID} className="overflow-hidden">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium truncate">
              {skuWithImages.sku.SKUCode || skuWithImages.sku.Code}
            </CardTitle>
            <p className="text-xs text-muted-foreground truncate">
              {skuWithImages.sku.SKULongName || skuWithImages.sku.Name}
            </p>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="space-y-3">
              {/* Default image display */}
              <div className="relative aspect-square bg-gray-100 rounded-lg overflow-hidden">
                {skuWithImages.defaultImage ? (
                  <img
                    src={imageUrls[skuWithImages.defaultImage.UID] || "/placeholder-product.png"}
                    alt={skuWithImages.defaultImage.DisplayName}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      e.currentTarget.src = "/placeholder-product.png";
                    }}
                  />
                ) : (
                  <div className="w-full h-full flex items-center justify-center text-gray-400">
                    <ImageIcon className="h-12 w-12" />
                  </div>
                )}
                <div className="absolute top-2 right-2">
                  <Badge variant="secondary" className="text-xs">
                    {skuWithImages.images.length} images
                  </Badge>
                </div>
              </div>

              {/* Image thumbnails */}
              {skuWithImages.images.length > 0 && (
                <div className="flex gap-2 overflow-x-auto">
                  {skuWithImages.images.slice(0, 4).map((image) => (
                    <div
                      key={image.UID}
                      className="relative flex-shrink-0 w-12 h-12 bg-gray-100 rounded overflow-hidden cursor-pointer"
                      onClick={() => toggleImageSelection(image.UID)}
                    >
                      <img
                        src="/placeholder-product.png"
                        alt={image.DisplayName}
                        className="w-full h-full object-cover"
                      />
                      {image.IsDefault && (
                        <div className="absolute top-0 right-0 bg-yellow-500 text-white p-0.5">
                          <Star className="h-3 w-3" />
                        </div>
                      )}
                      {selectedImages.has(image.UID) && (
                        <div className="absolute inset-0 bg-blue-500 bg-opacity-50 flex items-center justify-center">
                          <Checkbox checked className="h-4 w-4" />
                        </div>
                      )}
                    </div>
                  ))}
                  {skuWithImages.images.length > 4 && (
                    <div className="flex-shrink-0 w-12 h-12 bg-gray-200 rounded flex items-center justify-center text-xs font-medium">
                      +{skuWithImages.images.length - 4}
                    </div>
                  )}
                </div>
              )}

              {/* Actions */}
              <div className="flex gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => {
                    setSelectedSKU(skuWithImages.sku);
                    setUploadDialogOpen(true);
                  }}
                  className="flex-1"
                >
                  <Upload className="h-3 w-3 mr-1" />
                  Add Image
                </Button>
                {skuWithImages.images.length > 0 && (
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      // Select all images for this SKU
                      const newSelected = new Set(selectedImages);
                      skuWithImages.images.forEach(img => newSelected.add(img.UID));
                      setSelectedImages(newSelected);
                    }}
                  >
                    <Eye className="h-3 w-3" />
                  </Button>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );

  // Render list view
  const renderListView = () => (
    <div className="border rounded-md">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-12">
              <Checkbox
                checked={selectedImages.size > 0}
                onCheckedChange={(checked) => {
                  if (checked) {
                    const allImageUIDs = new Set<string>();
                    skusWithImages.forEach(swi => 
                      swi.images.forEach(img => allImageUIDs.add(img.UID))
                    );
                    setSelectedImages(allImageUIDs);
                  } else {
                    setSelectedImages(new Set());
                  }
                }}
              />
            </TableHead>
            <TableHead>SKU Code</TableHead>
            <TableHead>Product Name</TableHead>
            <TableHead>Images</TableHead>
            <TableHead>Default Image</TableHead>
            <TableHead className="text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {skusWithImages.map((skuWithImages) => (
            <TableRow key={skuWithImages.sku.SKUUID || skuWithImages.sku.UID}>
              <TableCell>
                <Checkbox
                  checked={skuWithImages.images.some(img => selectedImages.has(img.UID))}
                  onCheckedChange={(checked) => {
                    const newSelected = new Set(selectedImages);
                    if (checked) {
                      skuWithImages.images.forEach(img => newSelected.add(img.UID));
                    } else {
                      skuWithImages.images.forEach(img => newSelected.delete(img.UID));
                    }
                    setSelectedImages(newSelected);
                  }}
                />
              </TableCell>
              <TableCell className="font-medium">
                {skuWithImages.sku.SKUCode || skuWithImages.sku.Code}
              </TableCell>
              <TableCell>
                {skuWithImages.sku.SKULongName || skuWithImages.sku.Name}
              </TableCell>
              <TableCell>
                <Badge variant="secondary">
                  {skuWithImages.images.length} images
                </Badge>
              </TableCell>
              <TableCell>
                {skuWithImages.defaultImage ? (
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 bg-gray-100 rounded overflow-hidden">
                      <img
                        src={imageUrls[skuWithImages.defaultImage.UID] || "/placeholder-product.png"}
                        alt="Default"
                        className="w-full h-full object-cover"
                        onError={(e) => {
                          e.currentTarget.src = "/placeholder-product.png";
                        }}
                      />
                    </div>
                    <span className="text-sm text-muted-foreground">
                      {skuWithImages.defaultImage.FileName}
                    </span>
                  </div>
                ) : (
                  <span className="text-sm text-muted-foreground">No images</span>
                )}
              </TableCell>
              <TableCell className="text-right">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => {
                    setSelectedSKU(skuWithImages.sku);
                    setUploadDialogOpen(true);
                  }}
                >
                  <Upload className="h-3 w-3 mr-1" />
                  Add Image
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">SKU Image Management</h2>
          <p className="text-muted-foreground">
            Manage product images and assign them to team members
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => setViewMode(viewMode === 'grid' ? 'list' : 'grid')}
          >
            {viewMode === 'grid' ? <List className="h-4 w-4 mr-2" /> : <Grid className="h-4 w-4 mr-2" />}
            {viewMode === 'grid' ? 'List View' : 'Grid View'}
          </Button>
          <Button
            variant="outline"
            onClick={() => setAssignDialogOpen(true)}
            disabled={selectedImages.size === 0}
          >
            <UserPlus className="h-4 w-4 mr-2" />
            Assign Images ({selectedImages.size})
          </Button>
        </div>
      </div>

      {/* Search and filters */}
      <Card className="p-4">
        <div className="flex gap-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search by SKU code or product name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>
      </Card>

      {/* Content */}
      <Card className="p-6">
        {loading ? (
          <div className="space-y-4">
            {[...Array(6)].map((_, i) => (
              <Skeleton key={i} className="h-48 w-full" />
            ))}
          </div>
        ) : skusWithImages.length === 0 ? (
          <div className="text-center py-8">
            <ImageIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No products found</h3>
            <p className="text-gray-500">Try adjusting your search criteria</p>
          </div>
        ) : (
          <>
            {viewMode === 'grid' ? renderImageGrid() : renderListView()}
            
            {totalCount > 0 && (
              <div className="mt-6">
                <PaginationControls
                  currentPage={currentPage}
                  totalCount={totalCount}
                  pageSize={pageSize}
                  onPageChange={setCurrentPage}
                  onPageSizeChange={(size) => {
                    setPageSize(size);
                    setCurrentPage(1);
                  }}
                  itemName="products"
                />
              </div>
            )}
          </>
        )}
      </Card>

      {/* Upload Dialog */}
      <Dialog open={uploadDialogOpen} onOpenChange={setUploadDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Upload Image</DialogTitle>
            <DialogDescription>
              Upload an image for {selectedSKU?.SKUCode || selectedSKU?.Code} - {selectedSKU?.SKULongName || selectedSKU?.Name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label htmlFor="imageFile">Select Image File</Label>
              <Input
                id="imageFile"
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="mt-1"
              />
            </div>
            {uploadPreview && (
              <div>
                <Label>Preview</Label>
                <div className="mt-2 w-full h-48 bg-gray-100 rounded-lg overflow-hidden">
                  <img
                    src={uploadPreview}
                    alt="Upload preview"
                    className="w-full h-full object-cover"
                  />
                </div>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setUploadDialogOpen(false);
                setUploadFile(null);
                setUploadPreview('');
              }}
              disabled={uploading}
            >
              Cancel
            </Button>
            <Button
              onClick={handleUpload}
              disabled={!uploadFile || uploading}
            >
              {uploading ? 'Uploading...' : 'Upload Image'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Assignment Dialog */}
      <Dialog open={assignDialogOpen} onOpenChange={setAssignDialogOpen}>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Assign Images to Users</DialogTitle>
            <DialogDescription>
              Assign selected images to team members with specific permissions
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>Assignment Type</Label>
              <Select value={assignmentType} onValueChange={(value: any) => setAssignmentType(value)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="view">View Only</SelectItem>
                  <SelectItem value="edit">View & Edit</SelectItem>
                  <SelectItem value="manage">Full Management</SelectItem>
                </SelectContent>
              </Select>
            </div>
            
            <div>
              <Label>Select Users ({selectedUsers.size} selected)</Label>
              <div className="mt-2 max-h-48 overflow-y-auto border rounded-md">
                {availableUsers.map((user) => (
                  <div
                    key={user.UID}
                    className="flex items-center space-x-3 p-3 border-b last:border-b-0 hover:bg-gray-50"
                  >
                    <Checkbox
                      checked={selectedUsers.has(user.UID)}
                      onCheckedChange={() => toggleUserSelection(user.UID)}
                    />
                    <div className="flex-1">
                      <p className="font-medium">{user.Name}</p>
                      <p className="text-sm text-muted-foreground">{user.Role} - {user.Email}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setAssignDialogOpen(false);
                setSelectedUsers(new Set());
              }}
              disabled={assigning}
            >
              Cancel
            </Button>
            <Button
              onClick={handleAssignImages}
              disabled={selectedImages.size === 0 || selectedUsers.size === 0 || assigning}
            >
              {assigning ? 'Assigning...' : `Assign ${selectedImages.size} Images`}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the image "{imageToDelete?.DisplayName}". 
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteImage}
              disabled={deleting}
              className="bg-red-600 hover:bg-red-700"
            >
              {deleting ? 'Deleting...' : 'Delete Image'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}