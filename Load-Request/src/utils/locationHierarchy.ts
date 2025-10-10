import { Location, LocationType } from '@/services/locationService';

export interface LocationLevel {
  locationTypeUID: string;
  locationTypeName: string;
  locations: Location[];
  selectedLocationUID?: string;
  level: number;
  parentLocationUID?: string;
  dynamicLabel?: string;
}

export interface LocationHierarchyState {
  locationLevels: LocationLevel[];
  selectedLocations: string[];
}

export function buildLocationTree(locations: Location[]): any[] {
  const locationMap = new Map<string, any>();
  const rootNodes: any[] = [];
  
  locations.forEach(location => {
    locationMap.set(location.UID, {
      ...location,
      children: []
    });
  });
  
  locations.forEach(location => {
    const node = locationMap.get(location.UID);
    if (location.ParentUID && locationMap.has(location.ParentUID)) {
      const parent = locationMap.get(location.ParentUID);
      parent.children.push(node);
    } else {
      rootNodes.push(node);
    }
  });
  
  return rootNodes;
}

function generateDynamicLabel(typeName: string): string {
  return typeName;
}

export function initializeLocationHierarchy(
  locations: Location[],
  locationTypes: LocationType[]
): LocationLevel[] {
  console.log("🔍 Initializing progressive location hierarchy (like org hierarchy)...");
  
  // Find root locations (those without parents or with null/empty ParentUID)
  const rootLocations = locations.filter(location => 
    !location.ParentUID || location.ParentUID === ''
  );
  
  console.log(`🌱 Found ${rootLocations.length} root locations:`, 
    rootLocations.map(l => `${l.Name}(${l.LocationTypeName})`));
  
  // Only show the first level initially - like org hierarchy
  if (rootLocations.length > 0) {
    const rootType = rootLocations[0].LocationTypeUID;
    const rootTypeName = rootLocations[0].LocationTypeName || rootType;
    
    const levels: LocationLevel[] = [{
      locationTypeUID: rootType,
      locationTypeName: rootTypeName,
      locations: rootLocations.filter(location => location.LocationTypeUID === rootType),
      level: 0,
      dynamicLabel: generateDynamicLabel(rootTypeName)
    }];
    
    console.log(`📍 Created initial level: ${rootTypeName} with ${levels[0].locations.length} locations`);
    return levels;
  }
  
  return [];
}

export function handleLocationSelection(
  levelIndex: number,
  selectedLocationUID: string,
  currentLevels: LocationLevel[],
  selectedLocations: string[],
  allLocations: Location[],
  locationTypes: LocationType[]
): { updatedLevels: LocationLevel[]; updatedSelectedLocations: string[] } {
  const updatedLevels = [...currentLevels];
  const updatedSelectedLocations = [...selectedLocations];
  
  // Set selection for current level
  updatedLevels[levelIndex].selectedLocationUID = selectedLocationUID;
  
  // Update selected locations array
  if (updatedSelectedLocations.length > levelIndex) {
    updatedSelectedLocations[levelIndex] = selectedLocationUID;
    updatedSelectedLocations.splice(levelIndex + 1);
  } else {
    updatedSelectedLocations.push(selectedLocationUID);
  }
  
  // Remove all levels after the current selection (progressive display like org hierarchy)
  updatedLevels.splice(levelIndex + 1);
  
  // Find direct children of the selected location
  const directChildren = allLocations.filter(location => 
    location.ParentUID === selectedLocationUID
  );
  
  console.log(`🔍 Found ${directChildren.length} direct children of "${selectedLocationUID}":`, 
    directChildren.map(l => `${l.Name}(${l.LocationTypeName})`));
  
  // If there are children, create a new level for them (like org hierarchy)
  if (directChildren.length > 0) {
    // Group children by type to handle multiple child types
    const childrenByType = new Map<string, Location[]>();
    
    directChildren.forEach(child => {
      if (child.LocationTypeUID) {
        if (!childrenByType.has(child.LocationTypeUID)) {
          childrenByType.set(child.LocationTypeUID, []);
        }
        childrenByType.get(child.LocationTypeUID)!.push(child);
      }
    });
    
    // For now, use the first child type found (most common case)
    // In future, could support multiple child types
    const childTypes = Array.from(childrenByType.entries());
    if (childTypes.length > 0) {
      const [childTypeUID, childLocations] = childTypes[0];
      const childTypeName = childLocations[0].LocationTypeName || childTypeUID;
      
      // Create new level for children
      const newLevel: LocationLevel = {
        locationTypeUID: childTypeUID,
        locationTypeName: childTypeName,
        locations: childLocations,
        level: levelIndex + 1,
        parentLocationUID: selectedLocationUID,
        dynamicLabel: generateDynamicLabel(childTypeName)
      };
      
      updatedLevels.push(newLevel);
      
      console.log(`✅ Added new level ${levelIndex + 1}: "${childTypeName}" with ${childLocations.length} locations`);
      
      // If there are multiple child types, log them for awareness
      if (childTypes.length > 1) {
        console.log(`ℹ️ Multiple child types found. Showing "${childTypeName}". Other types:`, 
          childTypes.slice(1).map(([uid, locs]) => `${locs[0].LocationTypeName}(${locs.length})`));
      }
    }
  } else {
    console.log(`🏁 No children found for "${selectedLocationUID}" - this is a leaf location`);
  }
  
  return { updatedLevels, updatedSelectedLocations };
}

export function resetLocationHierarchy(
  allLocations: Location[],
  locationTypes: LocationType[]
): { resetLevels: LocationLevel[]; resetSelectedLocations: string[] } {
  const resetLevels = initializeLocationHierarchy(allLocations, locationTypes);
  return { resetLevels, resetSelectedLocations: [] };
}

export function getFinalSelectedLocation(selectedLocations: string[]): string | null {
  return selectedLocations.length > 0 ? selectedLocations[selectedLocations.length - 1] : null;
}

export function getLocationDisplayName(location: Location): string {
  return location.Name;
}